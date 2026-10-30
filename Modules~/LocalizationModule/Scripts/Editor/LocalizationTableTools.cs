#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;

namespace Modules.LocalizationModule.Editor
{
    /// <summary>
    /// Reads and writes the Game table's texts by key and language code. Every write goes through
    /// Undo and dirties the asset it touched, the way an edit in Unity's own table window does.
    /// </summary>
    internal class LocalizationTableTools
    {
        private readonly LocalizationProject _project;

        public LocalizationTableTools(LocalizationProject project) => _project = project;

        /// <summary>Every key in the table's order, with its text in each language.</summary>
        public TextGrid Read()
        {
            var grid = new TextGrid();
            StringTableCollection collection = _project.Collection;

            if (collection == null)
                return grid;

            foreach (SharedTableData.SharedTableEntry shared in collection.SharedData.Entries)
            {
                foreach (StringTable table in collection.StringTables)
                {
                    StringTableEntry entry = table.GetEntry(shared.Id);
                    grid.Set(shared.Key, table.LocaleIdentifier.Code, entry?.Value ?? string.Empty);
                }
            }

            return grid;
        }

        public List<string> Keys()
        {
            var keys = new List<string>();
            StringTableCollection collection = _project.Collection;

            if (collection == null)
                return keys;

            foreach (SharedTableData.SharedTableEntry shared in collection.SharedData.Entries)
                keys.Add(shared.Key);

            return keys;
        }

        public bool Has(string key) => _project.Collection?.SharedData.Contains(key) ?? false;

        /// <summary>
        /// One text. A text with a placeholder is a Smart String, so <c>{0}</c> and plural forms
        /// are filled when it is read; one without stays plain and costs nothing to read.
        /// </summary>
        public void Write(string key, string language, string text)
        {
            StringTable table = Table(language);

            if (table == null)
                return;

            Undo.RecordObjects(new UnityEngine.Object[] { table, table.SharedData }, "Localization text");

            StringTableEntry entry = table.GetEntry(key) ?? table.AddEntry(key, string.Empty);
            entry.Value = text ?? string.Empty;
            entry.IsSmart = entry.Value.Contains("{");

            EditorUtility.SetDirty(table);
            EditorUtility.SetDirty(table.SharedData);
        }

        public void Remove(string key)
        {
            StringTableCollection collection = _project.Collection;

            if (collection == null || !collection.SharedData.Contains(key))
                return;

            var touched = new List<UnityEngine.Object> { collection.SharedData };
            foreach (StringTable table in collection.StringTables)
                touched.Add(table);

            Undo.RecordObjects(touched.ToArray(), "Localization key removed");
            collection.RemoveEntry(key);

            foreach (UnityEngine.Object asset in touched)
                EditorUtility.SetDirty(asset);
        }

        public void Save() => AssetDatabase.SaveAssets();

        private StringTable Table(string language)
        {
            StringTableCollection collection = _project.Collection;

            if (collection == null)
                return null;

            foreach (StringTable table in collection.StringTables)
            {
                if (table.LocaleIdentifier.Code == language)
                    return table;
            }

            return null;
        }
    }
}

#endif
