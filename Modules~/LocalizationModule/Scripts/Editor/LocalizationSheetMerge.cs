#if UNITY_EDITOR

using System.Collections.Generic;

namespace Modules.LocalizationModule.Editor
{
    /// <summary>key → language → text. A key that is absent has no row; an empty text is a row with nothing in that cell.</summary>
    internal class TextGrid : Dictionary<string, Dictionary<string, string>>
    {
        public string Get(string key, string language) =>
            TryGetValue(key, out Dictionary<string, string> row) && row.TryGetValue(language, out string text) ? text ?? string.Empty : string.Empty;

        public void Set(string key, string language, string text)
        {
            if (!TryGetValue(key, out Dictionary<string, string> row))
            {
                row = new Dictionary<string, string>();
                this[key] = row;
            }

            row[language] = text ?? string.Empty;
        }
    }

    internal enum SheetChangeKind
    {
        /// <summary>The sheet changed the text; the table takes it.</summary>
        ToLocal = 0,

        /// <summary>The table changed the text; the sheet takes it.</summary>
        ToRemote = 1,

        /// <summary>Both changed it since they last agreed; somebody has to choose.</summary>
        Conflict = 2,

        /// <summary>The sheet deleted the key and the table had not touched it.</summary>
        RemoveLocal = 3,

        /// <summary>The table deleted the key and the sheet had not touched it.</summary>
        RemoveRemote = 4
    }

    /// <summary>
    /// One difference between the table and the sheet. Language is null for a whole key - one
    /// deleted on a side, or deleted on one side and edited on the other, which is a conflict with
    /// a null text on the side that deleted it.
    /// </summary>
    internal readonly struct SheetChange
    {
        public readonly string Key;
        public readonly string Language;
        public readonly SheetChangeKind Kind;
        public readonly string Local;
        public readonly string Remote;

        public SheetChange(string key, string language, SheetChangeKind kind, string local, string remote)
        {
            Key = key;
            Language = language;
            Kind = kind;
            Local = local;
            Remote = remote;
        }

        public override string ToString() => $"{Kind} {Key}/{Language ?? "*"}: '{Local}' | '{Remote}'";
    }

    /// <summary>
    /// The three-way comparison behind Pull and Push. What the two sides last agreed on is the
    /// base; a side that differs from the base changed, and the side that did not follows it.
    /// Only a text both changed - to different words - is a conflict, so editing in the panel and
    /// in the sheet at once loses nothing and asks only where it must.
    /// </summary>
    internal class LocalizationSheetMerge
    {
        public List<SheetChange> Compare(TextGrid synced, TextGrid local, TextGrid remote, IReadOnlyList<string> languages)
        {
            var changes = new List<SheetChange>();
            var keys = new SortedSet<string>(synced.Keys);
            keys.UnionWith(local.Keys);
            keys.UnionWith(remote.Keys);

            foreach (string key in keys)
            {
                bool wasSynced = synced.ContainsKey(key);
                bool inLocal = local.ContainsKey(key);
                bool inRemote = remote.ContainsKey(key);

                if (inLocal && inRemote)
                {
                    CompareCells(key, synced, local, remote, languages, changes);
                    continue;
                }

                if (!wasSynced)
                {
                    // New on one side: every text of it goes to the other.
                    if (inLocal)
                        AddRow(key, local, languages, SheetChangeKind.ToRemote, changes);
                    else if (inRemote)
                        AddRow(key, remote, languages, SheetChangeKind.ToLocal, changes);

                    continue;
                }

                if (!inLocal && !inRemote)
                    continue;

                // Deleted on one side. Untouched on the other, it goes; edited there, somebody chooses.
                TextGrid kept = inLocal ? local : remote;
                bool keptChanged = RowChanged(key, synced, kept, languages);

                if (keptChanged)
                    changes.Add(new SheetChange(key, null, SheetChangeKind.Conflict, inLocal ? "" : null, inRemote ? "" : null));
                else
                    changes.Add(new SheetChange(key, null, inLocal ? SheetChangeKind.RemoveLocal : SheetChangeKind.RemoveRemote, null, null));
            }

            return changes;
        }

        private void CompareCells(string key, TextGrid synced, TextGrid local, TextGrid remote, IReadOnlyList<string> languages, List<SheetChange> changes)
        {
            foreach (string language in languages)
            {
                string mine = local.Get(key, language);
                string theirs = remote.Get(key, language);

                if (mine == theirs)
                    continue;

                string agreed = synced.Get(key, language);

                if (mine == agreed)
                    changes.Add(new SheetChange(key, language, SheetChangeKind.ToLocal, mine, theirs));
                else if (theirs == agreed)
                    changes.Add(new SheetChange(key, language, SheetChangeKind.ToRemote, mine, theirs));
                else
                    changes.Add(new SheetChange(key, language, SheetChangeKind.Conflict, mine, theirs));
            }
        }

        private void AddRow(string key, TextGrid from, IReadOnlyList<string> languages, SheetChangeKind kind, List<SheetChange> changes)
        {
            foreach (string language in languages)
            {
                string text = from.Get(key, language);
                bool toLocal = kind == SheetChangeKind.ToLocal;
                changes.Add(new SheetChange(key, language, kind, toLocal ? null : text, toLocal ? text : null));
            }
        }

        private bool RowChanged(string key, TextGrid synced, TextGrid side, IReadOnlyList<string> languages)
        {
            foreach (string language in languages)
            {
                if (synced.Get(key, language) != side.Get(key, language))
                    return true;
            }

            return false;
        }
    }
}

#endif
