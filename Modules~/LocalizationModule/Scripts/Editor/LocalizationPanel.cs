#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using FlowIoC.BaseModule.Attributes;
using FlowIoC.ConsoleModule;
using FlowIoC.Editor.ModulePanels;
using Modules.LocalizationModule.Constants;
using Modules.LocalizationModule.Data.UnityObjects;
using UnityEditor;
using UnityEngine;

namespace Modules.LocalizationModule.Editor
{
    /// <summary>
    /// The game's texts as the developer works on them: every key of the Game table in every
    /// language, editable in place, with what is missing and what a machine translated and nobody
    /// has read yet picked out; the Google Sheet they sync with, and the Pull and Push that do it.
    ///
    /// Editing the table is authoring, the way Unity's own table window edits it - the assets, with
    /// Undo. The running service is never touched.
    /// </summary>
    internal class LocalizationPanel : ModulePanel
    {
        private const string MENU_PATH = "Tools/FlowIoC-Modules/Localization/Panel";

        /// <summary>
        /// Between Tools/FlowIoC, whose items sit around -1300 to -1100, and Tools/FlowIoC-dev at
        /// -1050: a submenu takes its place from its lowest item, so this is what keeps
        /// FlowIoC-Modules the second entry under Tools.
        /// </summary>
        private const int MENU_PRIORITY = -1080;

        private const float ROW_HEIGHT = 20f;
        private const float KEY_SHARE = 0.3f;
        private const float GAP = 4f;
        private const float REMOVE_WIDTH = 20f;

        /// <summary>The painter's help gutter, so the key column starts where every label above it does.</summary>
        private const float LABEL_INSET = 16f;

        private readonly string[] _filters = { "All", "Missing", "Unchecked" };
        private readonly Color _missingTint = new(1f, 0.45f, 0.4f, 0.25f);
        private readonly Color _uncheckedTint = new(1f, 0.8f, 0.3f, 0.3f);

        private readonly LocalizationProject _project = new();
        private readonly LocalizationKeysWriter _keys = new();
        private readonly LocalizationUsageScan _scan = new();
        private readonly Queue<Action> _pending = new();
        private readonly LocalizationTableTools _tables;
        private readonly LocalizationSheetSync _sync;

        private string _search = string.Empty;
        private int _filter;
        private string _newKey = string.Empty;
        private string _newLanguage = string.Empty;
        private string _address = string.Empty;
        private string _report = string.Empty;
        private List<string> _unused;
        private HashSet<(string key, string language)> _unchecked = new();

        public LocalizationPanel()
        {
            _tables = new LocalizationTableTools(_project);
            _sync = new LocalizationSheetSync(_project, _tables);
        }

        [MenuItem(MENU_PATH, false, MENU_PRIORITY)]
        private static void Open() => ModulePanelWindow.Open<LocalizationPanel>();

        public override string Title => "Localization";

        public override string Module => "LocalizationModule";

        public override string Subtitle => "The game's texts in every language, and the Google Sheet they sync with";

        public override FlowRole Role => FlowRole.Service;

        public override string HelpPage => "Localization Module";

        public override void Draw(ModulePanelPainter painter)
        {
            if (!_project.IsSetUp)
            {
                painter.Heading("Set up");
                painter.Warning(
                    "The project has no localization yet. Set up makes Assets/Localization: the settings, "
                    + "English and Turkish, the Game table, and the files the sheet sync needs.");
                painter.Actions(new ModulePanelAction("Set up", () => Defer(SetUp), ModulePanelActionKind.Confirm));
                Flush();
                return;
            }

            List<string> languages = _project.Languages();
            TextGrid grid = _tables.Read();
            List<string> keys = _tables.Keys();
            _unchecked = _sync.Unchecked();

            DrawLanguages(painter, languages, grid, keys);
            DrawSheet(painter);
            DrawTexts(painter, languages, grid, keys);

            Flush();
        }

        // --- languages ---

        private void DrawLanguages(ModulePanelPainter painter, List<string> languages, TextGrid grid, List<string> keys)
        {
            painter.Heading("Languages");

            foreach (string language in languages)
            {
                int missing = 0;

                foreach (string key in keys)
                {
                    if (grid.Get(key, language).Length == 0)
                        missing++;
                }

                string source = language == LocalizationConstants.SOURCE_LANGUAGE ? "source - " : string.Empty;
                painter.Field(language, missing == 0 ? source + "complete" : $"{source}{missing} of {keys.Count} missing");
            }

            _newLanguage = painter.TextField("Add language", _newLanguage,
                help: "A locale code: de, ja, ar, zh-Hans. It gets a table in Game; the next Push adds its column to the sheet and asks the sheet to translate it.");
            painter.Actions(new ModulePanelAction("Add language", () => Defer(AddLanguage), ModulePanelActionKind.Add, _newLanguage.Trim().Length > 0));
        }

        private void AddLanguage()
        {
            string code = _newLanguage.Trim();

            if (_project.AddLanguage(code))
            {
                AssetDatabase.SaveAssets();
                _newLanguage = string.Empty;
                _report = $"Added {code}.";
            }
            else
            {
                _report = $"{code} is not a locale code Unity knows.";
            }
        }

        // --- the sheet ---

        private void DrawSheet(ModulePanelPainter painter)
        {
            ED_LocalizationSheet sheet = _project.Sheet;
            bool linked = sheet != null && sheet.SpreadsheetId.Length > 0;
            bool signedIn = _project.HasCredentials;

            painter.Heading("Google Sheet");

            if (!signedIn)
                painter.Warning(
                    "No OAuth client yet. Press Settings, give SheetsServiceProvider a Google Cloud OAuth client's id and "
                    + "secret, and Authorize once - the Help page has the steps. Create sheet, Pull and Push wait for it.");

            painter.Field("Linked", linked ? $"{sheet.SpreadsheetId}  (tab {sheet.SheetId})" : "none",
                "The sheet's credentials are on SheetsServiceProvider in Assets/Localization: a Google Cloud OAuth "
                + "client's id and secret, and Authorize once. The Help page has the steps.");

            _address = painter.TextField("Sheet address", _address,
                help: "Paste the sheet's address from the browser, with the tab open - its #gid= says which tab.");

            painter.Actions(
                new ModulePanelAction("Link", () => Defer(Link), ModulePanelActionKind.Plain, _address.Trim().Length > 0),
                new ModulePanelAction("Create sheet", () => Defer(() => Run(_sync.Create)), ModulePanelActionKind.Plain, signedIn && !linked),
                new ModulePanelAction("Open sheet", () => Application.OpenURL($"https://docs.google.com/spreadsheets/d/{sheet.SpreadsheetId}/edit#gid={sheet.SheetId}"), ModulePanelActionKind.Plain, linked),
                new ModulePanelAction("Settings", () => Selection.activeObject = _project.Provider, ModulePanelActionKind.Plain, _project.Provider != null));

            painter.Actions(
                "Pull brings the sheet's changes into the table. Push sends the table's changes to the sheet and asks it to "
                + "translate every empty cell. A text changed on both sides is asked about. Approve writes the machine's words "
                + "over their formulas, so they stop being unchecked.",
                new ModulePanelAction("Pull", () => Defer(() => Run(() => _sync.Pull(Choose))), ModulePanelActionKind.Confirm, signedIn && linked),
                new ModulePanelAction("Push", () => Defer(() => Run(() => _sync.Push(Choose))), ModulePanelActionKind.Caution, signedIn && linked),
                new ModulePanelAction("Approve all", () => Defer(() => Run(_sync.ApproveAll)), ModulePanelActionKind.Plain, signedIn && linked));

            if (_report.Length > 0)
                painter.Note(_report);
        }

        private void Link()
        {
            if (_sync.Link(_address.Trim()))
            {
                _address = string.Empty;
                _report = "Linked. The first Pull or Push compares from nothing.";
            }
            else
            {
                _report = "That is not a Google Sheets address - it has no /d/<id>/ in it.";
            }
        }

        /// <summary>Asked once per text both sides changed.</summary>
        private ConflictChoice Choose(SheetChange change)
        {
            string where = change.Language == null ? change.Key : $"{change.Key} ({change.Language})";
            string mine = change.Local == null ? "deleted" : $"\"{change.Local}\"";
            string theirs = change.Remote == null ? "deleted" : $"\"{change.Remote}\"";

            int answer = EditorUtility.DisplayDialogComplex(
                "Changed on both sides",
                $"{where}\n\nProject: {mine}\nSheet: {theirs}",
                "Keep project",
                "Skip",
                "Keep sheet");

            return answer == 0 ? ConflictChoice.Project : answer == 2 ? ConflictChoice.Sheet : ConflictChoice.Skip;
        }

        /// <summary>A sheet call that failed is reported, not thrown into the window: a wrong id or an expired sign-in is a sentence.</summary>
        private void Run(Action action)
        {
            try
            {
                action();
                _report = _sync.Report;
                _keys.Write(_tables.Keys());
            }
            catch (Exception e)
            {
                _report = "The sheet refused: " + e.Message;
                FlowLogger.LogError("Localization sheet - " + e.Message);
            }
        }

        // --- the texts ---

        private void DrawTexts(ModulePanelPainter painter, List<string> languages, TextGrid grid, List<string> keys)
        {
            painter.Heading("Texts");

            _search = painter.TextField("Search", _search, help: "Matches the key and the text in any language.");
            _filter = painter.Popup("Show", _filter, _filters,
                "Missing: a key with a language still empty. Unchecked: a text the sheet's machine translation wrote and nobody has read.");

            _newKey = painter.TextField("New key", _newKey,
                help: "screen.element - popup.ad.watch. A LocalizedText's Create key does the same from the label it sits on.");

            painter.Actions(
                new ModulePanelAction("Add key", () => Defer(AddKey), ModulePanelActionKind.Add, _newKey.Trim().Length > 0 && !_tables.Has(_newKey.Trim())));

            painter.Actions(
                "Generate keys writes LocalizationKeys.cs, one constant per key, so code reads a text by a name the compiler checks. "
                + "Pull, Push and Add key write it too. Find unused lists the keys no prefab, scene or script mentions.",
                new ModulePanelAction("Generate keys", () => Defer(() => _keys.Write(_tables.Keys()))),
                new ModulePanelAction("Find unused", () => Defer(() => _unused = _scan.Unused(_project.Collection))));

            if (_unused != null)
            {
                if (_unused.Count == 0)
                    painter.Note("Every key is used.");
                else
                    painter.Warning("Unused: " + string.Join(", ", _unused));
            }

            painter.Custom(ROW_HEIGHT, rect => DrawHeader(rect, languages));

            foreach (string key in keys)
            {
                if (!Shown(key, languages, grid))
                    continue;

                painter.Custom(ROW_HEIGHT, rect => DrawRow(rect, key, languages, grid));
            }
        }

        private bool Shown(string key, List<string> languages, TextGrid grid)
        {
            bool missing = false, unchecked_ = false, matches = _search.Length == 0 || key.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0;

            foreach (string language in languages)
            {
                string text = grid.Get(key, language);
                missing |= text.Length == 0;
                unchecked_ |= _unchecked.Contains((key, language));
                matches |= _search.Length > 0 && text.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return matches && (_filter == 0 || (_filter == 1 && missing) || (_filter == 2 && unchecked_));
        }

        private void DrawHeader(Rect rect, List<string> languages)
        {
            (Rect keyRect, List<Rect> cells) = Columns(rect, languages.Count);
            GUI.Label(keyRect, "Key", EditorStyles.boldLabel);

            for (int i = 0; i < languages.Count; i++)
                GUI.Label(cells[i], languages[i], EditorStyles.boldLabel);
        }

        private void DrawRow(Rect rect, string key, List<string> languages, TextGrid grid)
        {
            (Rect keyRect, List<Rect> cells) = Columns(rect, languages.Count);
            EditorGUI.SelectableLabel(keyRect, key);

            for (int i = 0; i < languages.Count; i++)
            {
                string language = languages[i];
                string text = grid.Get(key, language);
                Rect cell = cells[i];

                if (text.Length == 0)
                    EditorGUI.DrawRect(cell, _missingTint);
                else if (_unchecked.Contains((key, language)))
                    EditorGUI.DrawRect(cell, _uncheckedTint);

                // Committed when the field loses focus or Enter is pressed, never per keystroke.
                string typed = EditorGUI.DelayedTextField(cell, text);

                if (typed != text)
                    Defer(() => Edit(key, language, typed));
            }

            Rect remove = new(rect.xMax - REMOVE_WIDTH, rect.y + 1f, REMOVE_WIDTH, rect.height - 2f);

            if (GUI.Button(remove, "x", EditorStyles.miniButton)
                && EditorUtility.DisplayDialog("Remove key", $"Remove {key} from every language?", "Remove", "Cancel"))
                Defer(() => RemoveKey(key));
        }

        private (Rect key, List<Rect> cells) Columns(Rect rect, int count)
        {
            float x = rect.x + LABEL_INSET;
            float width = rect.width - LABEL_INSET - REMOVE_WIDTH - GAP;
            float keyWidth = width * KEY_SHARE;
            float cellWidth = (width - keyWidth) / Mathf.Max(count, 1);
            var cells = new List<Rect>();

            for (int i = 0; i < count; i++)
                cells.Add(new Rect(x + keyWidth + i * cellWidth + GAP, rect.y + 1f, cellWidth - GAP, rect.height - 2f));

            return (new Rect(x, rect.y + 1f, keyWidth, rect.height - 2f), cells);
        }

        private void Edit(string key, string language, string text)
        {
            _tables.Write(key, language, text);
            _tables.Save();
        }

        private void AddKey()
        {
            string key = _newKey.Trim();
            _tables.Write(key, LocalizationConstants.SOURCE_LANGUAGE, string.Empty);
            _tables.Save();
            _keys.Write(_tables.Keys());
            _newKey = string.Empty;
        }

        private void RemoveKey(string key)
        {
            _tables.Remove(key);
            _tables.Save();
            _keys.Write(_tables.Keys());
        }

        private void SetUp()
        {
            _project.SetUp();
            _report = "Set up. Link a sheet, or Create sheet, to sync.";
        }

        private void Defer(Action action) => _pending.Enqueue(action);

        private void Flush()
        {
            while (_pending.Count > 0)
                _pending.Dequeue()();
        }
    }
}

#endif
