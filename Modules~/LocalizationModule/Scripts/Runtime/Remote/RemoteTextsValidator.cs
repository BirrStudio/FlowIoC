using System;
using System.Collections.Generic;
using Modules.LocalizationModule.Data.ValueObjects;

namespace Modules.LocalizationModule.Remote
{
    /// <summary>
    /// Decides what a phone may take from the published sheet. The download checks run before the
    /// cache is written: the shape of the tab and the Status column, which says which languages of
    /// a row are still the sheet's unread machine translation. The build check runs as a table
    /// loads, against the built text: a corrected text must keep the built text's placeholders, or
    /// formatting it would throw or show a raw brace.
    /// </summary>
    internal class RemoteTextsValidator
    {
        public const string KEY_HEADER = "Key";
        public const string STATUS_HEADER = "Status";

        /// <summary>
        /// The header cell that carries the revision inside the Game tab itself - "Revision 12". The
        /// Version tab and the Game tab are published apart and may lag each other, so a phone keeps
        /// a download only when both say the same number.
        /// </summary>
        public const string REVISION_HEADER = "Revision ";

        /// <summary>A formula's published value while it has no words yet: still loading, or an error such as #N/A.</summary>
        private const string LOADING = "Loading...";

        /// <summary>Stands in for a brace with no partner, so a text holding one never matches a sound one.</summary>
        private const string UNBALANCED = "{unbalanced}";

        public bool TryReadDownload(List<List<string>> rows, out List<RemoteTextVO> texts, out string reason)
        {
            texts = new List<RemoteTextVO>();
            reason = string.Empty;

            if (rows == null || rows.Count == 0)
            {
                reason = "the sheet is empty";
                return false;
            }

            List<string> header = rows[0];

            if (header.Count == 0 || header[0].Trim() != KEY_HEADER)
            {
                reason = "the first header is not Key";
                return false;
            }

            int status = header.FindIndex(cell => cell.Trim() == STATUS_HEADER);

            if (status < 0)
            {
                // A sheet pushed before phones could read it: nothing tells a checked text from an unread one.
                reason = "the sheet has no Status column; Push it once from the Editor";
                return false;
            }

            for (int r = 1; r < rows.Count; r++)
            {
                List<string> row = rows[r];
                string key = Cell(row, 0).Trim();

                if (key.Length == 0)
                    continue;

                HashSet<string> unread = Languages(Cell(row, status));

                for (int c = 1; c < header.Count; c++)
                {
                    string language = header[c].Trim();
                    string text = Cell(row, c);

                    if (c == status || language.Length == 0 || language.StartsWith(REVISION_HEADER, StringComparison.Ordinal)
                        || text.Length == 0 || unread.Contains(language) || IsFormulaPlaceholder(text))
                        continue;

                    texts.Add(new RemoteTextVO { Key = key, Language = language, Text = text });
                }
            }

            return true;
        }

        /// <summary>The number in the Game tab's "Revision N" header cell, or null when it has none.</summary>
        public string TableRevision(List<List<string>> rows)
        {
            if (rows == null || rows.Count == 0)
                return null;

            foreach (string cell in rows[0])
            {
                string text = cell.Trim();

                if (text.StartsWith(REVISION_HEADER, StringComparison.Ordinal))
                    return text.Substring(REVISION_HEADER.Length).Trim();
            }

            return null;
        }

        public bool IsApplicable(string builtText, string remoteText) =>
            Placeholders(builtText).SetEquals(Placeholders(remoteText));

        /// <summary>
        /// Every top-level placeholder as its name and formatter - {0} is "0", {count:plural:...} is
        /// "count:plural" - so a renamed value, a dropped plural or a swapped formatter all differ. A
        /// brace with no partner is counted too: Smart Strings would throw on it.
        /// </summary>
        public HashSet<string> Placeholders(string text)
        {
            var names = new HashSet<string>();

            if (string.IsNullOrEmpty(text))
                return names;

            int depth = 0;
            int start = -1;
            int first = -1;
            int second = -1;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (depth == 0 && i + 1 < text.Length && text[i + 1] == c && (c == '{' || c == '}'))
                {
                    i++;
                    continue;
                }

                if (c == '{')
                {
                    if (depth == 0)
                    {
                        start = i + 1;
                        first = -1;
                        second = -1;
                    }

                    depth++;
                }
                else if (c == ':' && depth == 1)
                {
                    if (first < 0)
                        first = i;
                    else if (second < 0)
                        second = i;
                }
                else if (c == '}')
                {
                    if (depth == 0)
                    {
                        names.Add(UNBALANCED);
                        continue;
                    }

                    depth--;

                    if (depth == 0)
                        names.Add(Signature(text, start, first, second, i));
                }
            }

            if (depth != 0)
                names.Add(UNBALANCED);

            return names;
        }

        private string Signature(string text, int start, int first, int second, int end)
        {
            if (first < 0)
                return text.Substring(start, end - start).Trim();

            string name = text.Substring(start, first - start).Trim();
            int formatterEnd = second < 0 ? end : second;
            string formatter = text.Substring(first + 1, formatterEnd - first - 1).Trim();

            // A plain format string - {0:N2} - has no second colon; its formatter is what it is.
            return name + ":" + formatter;
        }

        private bool IsFormulaPlaceholder(string text) => text == LOADING || text.StartsWith("#", StringComparison.Ordinal);

        private HashSet<string> Languages(string status)
        {
            var languages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string code in status.Split(','))
            {
                if (code.Trim().Length > 0)
                    languages.Add(code.Trim());
            }

            return languages;
        }

        private string Cell(List<string> row, int column) => column < row.Count ? row[column] ?? string.Empty : string.Empty;
    }
}
