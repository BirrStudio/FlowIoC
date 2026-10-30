#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Modules.LocalizationModule.Constants;
using Modules.LocalizationModule.Data.UnityObjects;
using Modules.LocalizationModule.Data.ValueObjects;
using UnityEditor;
using UnityEditor.Localization.Plugins.Google;

namespace Modules.LocalizationModule.Editor
{
    internal enum ConflictChoice
    {
        Project = 0,
        Sheet = 1,
        Skip = 2
    }

    /// <summary>
    /// Pull, Push and Approve against the Google Sheet. The sheet's first row names the columns -
    /// Key, then one language code each - and every row under it is a key. Both operations read
    /// the whole sheet first and compare it with the table through <see cref="LocalizationSheetMerge"/>:
    /// Pull brings the sheet's changes in, Push sends the table's out, and a text changed on both
    /// sides is settled the same way in either, on both sides at once, so the next comparison
    /// does not ask again.
    ///
    /// Machine translation is the sheet's own: Push writes <c>=GOOGLETRANSLATE</c> into a cell
    /// that is empty on both sides, the sheet fills it, and the next Pull brings the words in
    /// marked unchecked - because the cell still holds the formula - until Approve writes the
    /// words over it or somebody edits them.
    ///
    /// Three things the sheet can hold that are not a text are never read as an empty one: a
    /// language with no column, a formula still loading or failed, and a second row for a key.
    /// Pull writes nothing to the sheet, so somebody with view access can pull.
    /// </summary>
    internal class LocalizationSheetSync
    {
        private const string KEY_HEADER = "Key";
        private const string FORMULA = "=GOOGLETRANSLATE(";

        private readonly LocalizationProject _project;
        private readonly LocalizationTableTools _tables;
        private readonly LocalizationSheetMerge _merge = new();
        private readonly SheetAddress _address = new();

        public LocalizationSheetSync(LocalizationProject project, LocalizationTableTools tables)
        {
            _project = project;
            _tables = tables;
        }

        /// <summary>What a pass did, for the panel to say.</summary>
        public string Report { get; private set; } = string.Empty;

        /// <summary>A new spreadsheet with one tab named after the table, linked; the first Push fills it.</summary>
        public void Create()
        {
            ED_LocalizationSheet sheet = _project.Sheet;
            (string spreadsheetId, int sheetId) = new GoogleSheets(_project.Provider)
                .CreateSpreadsheet($"{UnityEngine.Application.productName} Localization", LocalizationConstants.TABLE, new NewSheetProperties());

            Undo.RecordObject(sheet, "Localization sheet");
            sheet.SpreadsheetId = spreadsheetId;
            sheet.SheetId = sheetId;
            sheet.Rows.Clear();
            Save(sheet);
            Report = "Created the sheet. Push fills it.";
        }

        /// <summary>
        /// Links the sheet a browser address names - its id after /d/, its tab after gid=. A new
        /// link forgets what the old sheet agreed on, so the first Pull or Push compares from
        /// nothing and asks about every text the two hold differently.
        /// </summary>
        public bool Link(string address)
        {
            Match id = Regex.Match(address ?? string.Empty, "/d/([a-zA-Z0-9-_]+)");
            Match gid = Regex.Match(address ?? string.Empty, "gid=([0-9]+)");

            if (!id.Success)
                return false;

            ED_LocalizationSheet sheet = _project.Sheet;
            Undo.RecordObject(sheet, "Localization sheet");
            sheet.SpreadsheetId = id.Groups[1].Value;
            sheet.SheetId = gid.Success ? int.Parse(gid.Groups[1].Value) : 0;
            sheet.Rows.Clear();
            Save(sheet);

            return true;
        }

        public void Pull(Func<SheetChange, ConflictChoice> choose) => Run(choose, pull: true);

        public void Push(Func<SheetChange, ConflictChoice> choose) => Run(choose, pull: false);

        /// <summary>Every cell a machine translated and nobody has read, as of the last Pull or Push.</summary>
        public HashSet<(string key, string language)> Unchecked()
        {
            var cells = new HashSet<(string, string)>();
            ED_LocalizationSheet sheet = _project.Sheet;

            if (sheet == null)
                return cells;

            foreach (SheetRowEVO row in sheet.Rows)
            {
                foreach (SheetCellEVO cell in row.Cells)
                {
                    if (cell.Unchecked)
                        cells.Add((row.Key, cell.Language));
                }
            }

            return cells;
        }

        /// <summary>
        /// Writes the words of every unchecked cell over its formula, so they are the sheet's own. A
        /// formula still loading or failed is left alone: there are no words yet to keep.
        /// </summary>
        public void ApproveAll()
        {
            ED_LocalizationSheet sheet = _project.Sheet;
            RemoteSheet remote = Read(sheet);
            var writes = new List<ValueRange>();

            foreach ((string key, string language) in Unchecked())
            {
                if (remote.Pending.Contains((key, language))
                    || !remote.Rows.TryGetValue(key, out int row)
                    || !remote.Columns.TryGetValue(language, out int column))
                    continue;

                writes.Add(Cell(remote.Title, row, column, remote.Grid.Get(key, language)));
                SetUnchecked(sheet, key, language, false);
            }

            Write(sheet, writes, "RAW");
            Save(sheet);
            Report = $"Approved {writes.Count} text(s).";
        }

        private void Run(Func<SheetChange, ConflictChoice> choose, bool pull)
        {
            ED_LocalizationSheet sheet = _project.Sheet;
            List<string> languages = _project.Languages();
            RemoteSheet remote = Read(sheet);
            TextGrid local = _tables.Read();
            TextGrid synced = Synced(sheet);

            // A formula with no words yet says nothing about the text: it reads as what the table holds.
            foreach ((string key, string language) in remote.Pending)
                remote.Grid.Set(key, language, local.Get(key, language));

            // A language with no column on the sheet is not compared: its texts are not empty there,
            // they are not there at all. Push adds the column and fills it.
            List<string> present = languages.FindAll(language => remote.Columns.ContainsKey(language));
            List<string> missing = languages.FindAll(language => !remote.Columns.ContainsKey(language));

            // Every answer is collected before anything is written, so the rows read above are the
            // rows written to - unless the sheet moved while a question was open, which is checked.
            var decided = new List<(SheetChange change, SheetChangeKind kind)>();
            int asked = 0;

            foreach (SheetChange change in _merge.Compare(synced, local, remote.Grid, present))
            {
                SheetChangeKind kind = change.Kind;

                if (kind == SheetChangeKind.Conflict)
                {
                    asked++;
                    ConflictChoice choice = choose(change);

                    if (choice == ConflictChoice.Skip)
                        continue;

                    kind = choice == ConflictChoice.Sheet
                        ? change.Remote == null ? SheetChangeKind.RemoveLocal : SheetChangeKind.ToLocal
                        : change.Local == null ? SheetChangeKind.RemoveRemote : SheetChangeKind.ToRemote;
                }
                else if (pull != (kind == SheetChangeKind.ToLocal || kind == SheetChangeKind.RemoveLocal))
                {
                    // The other direction's change: the other button carries it.
                    continue;
                }

                decided.Add((change, kind));
            }

            if (asked > 0 && !Same(remote, Read(sheet)))
            {
                Report = "The sheet changed while you were choosing. Nothing was written; run it again.";
                return;
            }

            bool writesSheet = !pull || decided.Exists(d => d.kind == SheetChangeKind.ToRemote || d.kind == SheetChangeKind.RemoveRemote);
            var raw = new List<ValueRange>();
            var formulas = new List<ValueRange>();
            var deletes = new List<int>();
            int toLocal = 0, toRemote = 0;

            // Pull adds no column: one it made would stay empty until a Push filled it.
            if (writesSheet)
                AddHeader(remote, pull ? new List<string>() : missing, raw);

            foreach ((SheetChange change, SheetChangeKind kind) in decided)
            {
                switch (kind)
                {
                    case SheetChangeKind.ToLocal:
                        foreach (string language in change.Language == null ? present : new List<string> { change.Language })
                            WriteLocal(change.Key, language, remote.Grid.Get(change.Key, language), local);
                        toLocal++;
                        break;

                    case SheetChangeKind.RemoveLocal:
                        _tables.Remove(change.Key);
                        local.Remove(change.Key);
                        toLocal++;
                        break;

                    case SheetChangeKind.ToRemote:
                        foreach (string language in change.Language == null ? present : new List<string> { change.Language })
                            WriteRemote(remote, change.Key, language, local.Get(change.Key, language), raw);
                        toRemote++;
                        break;

                    case SheetChangeKind.RemoveRemote:
                        if (remote.Rows.TryGetValue(change.Key, out int doomed))
                            deletes.Add(doomed);
                        if (remote.Copies.TryGetValue(change.Key, out List<int> copies))
                            deletes.AddRange(copies);
                        remote.Grid.Remove(change.Key);
                        toRemote++;
                        break;
                }
            }

            if (!pull)
            {
                // A new column holds the table's texts from the start, then counts as agreed.
                foreach (string language in missing)
                {
                    foreach (string key in new List<string>(remote.Rows.Keys))
                        WriteRemote(remote, key, language, local.Get(key, language), raw);
                }

                AddTranslations(remote, local, languages, formulas);
            }

            Grow(sheet, remote);
            Write(sheet, raw, "RAW");
            Write(sheet, formulas, "USER_ENTERED");
            DeleteRows(sheet, deletes);

            Remember(sheet, synced, local, remote, pull ? present : languages);
            _tables.Save();
            Save(sheet);

            Report = pull
                ? $"Pulled {toLocal} change(s) into the table" + (asked > 0 ? $", {asked} asked about." : ".")
                : $"Pushed {toRemote} change(s) to the sheet, {formulas.Count} sent for machine translation" + (asked > 0 ? $", {asked} asked about." : ".");
        }

        /// <summary>
        /// A cell empty on both sides whose key has words in the source language is handed to the
        /// sheet's translator. Next Pull brings the words back, unchecked.
        /// </summary>
        private void AddTranslations(RemoteSheet remote, TextGrid local, List<string> languages, List<ValueRange> formulas)
        {
            int source = remote.Columns[LocalizationConstants.SOURCE_LANGUAGE];

            foreach (KeyValuePair<string, int> row in remote.Rows)
            {
                if (!local.ContainsKey(row.Key) || local.Get(row.Key, LocalizationConstants.SOURCE_LANGUAGE).Length == 0)
                    continue;

                foreach (string language in languages)
                {
                    if (language == LocalizationConstants.SOURCE_LANGUAGE
                        || local.Get(row.Key, language).Length > 0
                        || remote.Grid.Get(row.Key, language).Length > 0
                        || remote.Formulas.Contains((row.Key, language)))
                        continue;

                    string formula = $"{FORMULA}{_address.Column(source)}{row.Value + 1},\"{_address.TranslateCode(LocalizationConstants.SOURCE_LANGUAGE)}\",\"{_address.TranslateCode(language)}\")";
                    formulas.Add(Cell(remote.Title, row.Value, remote.Columns[language], formula));
                }
            }
        }

        private void WriteLocal(string key, string language, string text, TextGrid local)
        {
            _tables.Write(key, language, text);
            local.Set(key, language, text);
        }

        /// <summary>One text to the sheet, on the key's row - a new row at the bottom for a new key.</summary>
        private void WriteRemote(RemoteSheet remote, string key, string language, string text, List<ValueRange> raw)
        {
            if (!remote.Rows.TryGetValue(key, out int row))
            {
                row = remote.RowCount++;
                remote.Rows[key] = row;
                raw.Add(Cell(remote.Title, row, 0, key));
            }

            raw.Add(Cell(remote.Title, row, remote.Columns[language], text));
            remote.Grid.Set(key, language, text);
            remote.Formulas.Remove((key, language));
            remote.Pending.Remove((key, language));
        }

        /// <summary>
        /// The new agreement: every text both sides now hold alike, in the languages this pass
        /// compared. A text they still differ on - skipped, or waiting for the other button - and
        /// a language this pass did not compare keep their old agreement, so a side's change is
        /// still seen as that side's.
        /// </summary>
        private void Remember(ED_LocalizationSheet sheet, TextGrid synced, TextGrid local, RemoteSheet remote, List<string> compared)
        {
            var rows = new List<SheetRowEVO>();
            List<string> languages = _project.Languages();
            var old = new Dictionary<string, SheetRowEVO>();

            foreach (SheetRowEVO row in sheet.Rows)
                old[row.Key] = row;

            foreach (string key in local.Keys)
            {
                if (!remote.Grid.ContainsKey(key) && !synced.ContainsKey(key))
                    continue;

                old.TryGetValue(key, out SheetRowEVO before);
                var row = new SheetRowEVO { Key = key };

                foreach (string language in languages)
                {
                    SheetCellEVO was = before?.Cell(language);
                    string mine = local.Get(key, language);
                    bool agreed = compared.Contains(language) && remote.Grid.ContainsKey(key)
                        && remote.Grid.Get(key, language) == mine && !remote.Pending.Contains((key, language));

                    row.Cells.Add(agreed
                        ? new SheetCellEVO { Language = language, Value = mine, Unchecked = remote.Formulas.Contains((key, language)) }
                        : new SheetCellEVO { Language = language, Value = was?.Value ?? string.Empty, Unchecked = was?.Unchecked ?? false });
                }

                rows.Add(row);
            }

            // Deleted here but still on the sheet: the agreement stays, so the next Push still
            // reads the key as deleted rather than as new on the sheet.
            foreach (SheetRowEVO kept in sheet.Rows)
            {
                if (!local.ContainsKey(kept.Key) && remote.Grid.ContainsKey(kept.Key))
                    rows.Add(kept);
            }

            Undo.RecordObject(sheet, "Localization sync");
            sheet.Rows = rows;
        }

        private TextGrid Synced(ED_LocalizationSheet sheet)
        {
            var grid = new TextGrid();

            foreach (SheetRowEVO row in sheet.Rows)
            {
                grid[row.Key] = new Dictionary<string, string>();

                foreach (SheetCellEVO cell in row.Cells)
                    grid.Set(row.Key, cell.Language, cell.Value);
            }

            return grid;
        }

        private void SetUnchecked(ED_LocalizationSheet sheet, string key, string language, bool value)
        {
            foreach (SheetRowEVO row in sheet.Rows)
            {
                SheetCellEVO cell = row.Key == key ? row.Cell(language) : null;

                if (cell != null)
                    cell.Unchecked = value;
            }
        }

        private void Save(ED_LocalizationSheet sheet)
        {
            EditorUtility.SetDirty(sheet);
            AssetDatabase.SaveAssets();
        }

        /// <summary>The sheet as it is now: its words, which cells hold a translate formula, and where each key and language sit.</summary>
        private RemoteSheet Read(ED_LocalizationSheet sheet)
        {
            SheetsService service = Service();
            Sheet tab = TabOf(service, sheet);
            string title = tab.Properties.Title;

            SpreadsheetsResource.ValuesResource.GetRequest words = service.Spreadsheets.Values.Get(sheet.SpreadsheetId, Quote(title));
            words.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMATTEDVALUE;
            IList<IList<object>> values = words.Execute().Values ?? new List<IList<object>>();

            SpreadsheetsResource.ValuesResource.GetRequest formulas = service.Spreadsheets.Values.Get(sheet.SpreadsheetId, Quote(title));
            formulas.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMULA;
            IList<IList<object>> cells = formulas.Execute().Values ?? new List<IList<object>>();

            var remote = new RemoteSheet
            {
                Title = title,
                RowCount = Math.Max(values.Count, 1),
                GridRows = tab.Properties.GridProperties?.RowCount ?? 0,
                GridColumns = tab.Properties.GridProperties?.ColumnCount ?? 0
            };

            IList<object> header = values.Count > 0 ? values[0] : new List<object>();

            for (int column = 1; column < header.Count; column++)
            {
                string code = header[column]?.ToString().Trim();

                if (!string.IsNullOrEmpty(code) && !remote.Columns.ContainsKey(code))
                    remote.Columns[code] = column;
            }

            // A new column goes after the widest row, so it never lands on notes with no header.
            foreach (IList<object> row in values)
                remote.Width = Math.Max(remote.Width, row.Count);

            remote.Width = Math.Max(remote.Width, 1);

            for (int row = 1; row < values.Count; row++)
            {
                string key = At(values[row], 0).Trim();

                if (key.Length == 0)
                    continue;

                if (remote.Rows.ContainsKey(key))
                {
                    // A copied row: read once, deleted with the key.
                    if (!remote.Copies.TryGetValue(key, out List<int> copies))
                        remote.Copies[key] = copies = new List<int>();
                    copies.Add(row);
                    continue;
                }

                remote.Rows[key] = row;
                remote.Grid[key] = new Dictionary<string, string>();

                foreach (KeyValuePair<string, int> column in remote.Columns)
                {
                    string formula = row < cells.Count ? At(cells[row], column.Value) : string.Empty;
                    bool translated = formula.StartsWith(FORMULA, StringComparison.OrdinalIgnoreCase);
                    string text = At(values[row], column.Value);

                    remote.Grid.Set(key, column.Key, text);

                    if (!translated)
                        continue;

                    remote.Formulas.Add((key, column.Key));

                    if (text.Length == 0 || text == "Loading..." || text.StartsWith("#"))
                        remote.Pending.Add((key, column.Key));
                }
            }

            return remote;
        }

        /// <summary>Whether two reads found the same keys in the same rows with the same words.</summary>
        private bool Same(RemoteSheet a, RemoteSheet b)
        {
            if (a.Rows.Count != b.Rows.Count || a.Columns.Count != b.Columns.Count)
                return false;

            foreach (KeyValuePair<string, int> row in a.Rows)
            {
                if (!b.Rows.TryGetValue(row.Key, out int other) || other != row.Value)
                    return false;

                foreach (string language in a.Columns.Keys)
                {
                    if (!b.Columns.ContainsKey(language)
                        || (!a.Pending.Contains((row.Key, language)) && a.Grid.Get(row.Key, language) != b.Grid.Get(row.Key, language)))
                        return false;
                }
            }

            return true;
        }

        /// <summary>Key in A1, and a header for every language the sheet has no column for, after its widest row.</summary>
        private void AddHeader(RemoteSheet remote, List<string> missing, List<ValueRange> raw)
        {
            raw.Add(Cell(remote.Title, 0, 0, KEY_HEADER));

            foreach (string language in missing)
            {
                int column = remote.Width++;
                remote.Columns[language] = column;
                raw.Add(Cell(remote.Title, 0, column, language));
            }
        }

        /// <summary>A tab is a fixed grid - 1000 by 26 when made - and a write past its edge is refused, so it grows first.</summary>
        private void Grow(ED_LocalizationSheet sheet, RemoteSheet remote)
        {
            var requests = new List<Request>();

            if (remote.RowCount > remote.GridRows)
                requests.Add(Append("ROWS", remote.RowCount - remote.GridRows + 100, sheet.SheetId));

            if (remote.Width > remote.GridColumns)
                requests.Add(Append("COLUMNS", remote.Width - remote.GridColumns, sheet.SheetId));

            if (requests.Count > 0)
                Service().Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest { Requests = requests }, sheet.SpreadsheetId).Execute();
        }

        private Request Append(string dimension, int length, int sheetId) =>
            new() { AppendDimension = new AppendDimensionRequest { SheetId = sheetId, Dimension = dimension, Length = length } };

        private void Write(ED_LocalizationSheet sheet, List<ValueRange> writes, string input)
        {
            if (writes.Count == 0)
                return;

            var request = new BatchUpdateValuesRequest { ValueInputOption = input, Data = writes };
            Service().Spreadsheets.Values.BatchUpdate(request, sheet.SpreadsheetId).Execute();
        }

        /// <summary>Bottom first, so a deleted row never moves one still to be deleted.</summary>
        private void DeleteRows(ED_LocalizationSheet sheet, List<int> rows)
        {
            if (rows.Count == 0)
                return;

            rows.Sort((a, b) => b.CompareTo(a));
            var requests = new List<Request>();

            foreach (int row in rows)
            {
                requests.Add(new Request
                {
                    DeleteDimension = new DeleteDimensionRequest
                    {
                        Range = new DimensionRange { SheetId = sheet.SheetId, Dimension = "ROWS", StartIndex = row, EndIndex = row + 1 }
                    }
                });
            }

            Service().Spreadsheets.BatchUpdate(new BatchUpdateSpreadsheetRequest { Requests = requests }, sheet.SpreadsheetId).Execute();
        }

        private SheetsService Service()
        {
            if (_project.Provider == null)
                throw new InvalidOperationException("No SheetsServiceProvider in the project. Press Set up.");

            return _project.Provider.Service;
        }

        private Sheet TabOf(SheetsService service, ED_LocalizationSheet sheet)
        {
            if (string.IsNullOrEmpty(sheet.SpreadsheetId))
                throw new InvalidOperationException("No sheet is linked. Paste its address in Sheet or press Create sheet.");

            foreach (Sheet tab in service.Spreadsheets.Get(sheet.SpreadsheetId).Execute().Sheets)
            {
                if (tab.Properties.SheetId == sheet.SheetId)
                    return tab;
            }

            throw new InvalidOperationException($"The sheet has no tab with gid {sheet.SheetId}.");
        }

        private ValueRange Cell(string title, int row, int column, string text) =>
            new() { Range = $"{Quote(title)}!{_address.Column(column)}{row + 1}", Values = new List<IList<object>> { new List<object> { text ?? string.Empty } } };

        private string Quote(string title) => "'" + title.Replace("'", "''") + "'";

        private string At(IList<object> row, int column) => column < row.Count ? row[column]?.ToString() ?? string.Empty : string.Empty;

        private class RemoteSheet
        {
            public string Title;
            public int RowCount;
            public int Width;
            public int GridRows;
            public int GridColumns;
            public readonly Dictionary<string, int> Columns = new();
            public readonly Dictionary<string, int> Rows = new();
            public readonly Dictionary<string, List<int>> Copies = new();
            public readonly TextGrid Grid = new();
            public readonly HashSet<(string key, string language)> Formulas = new();
            public readonly HashSet<(string key, string language)> Pending = new();
        }
    }
}

#endif
