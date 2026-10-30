#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Localization;
using UnityEngine.Localization.Tables;

namespace Modules.LocalizationModule.Editor
{
    /// <summary>
    /// The keys nothing uses: no prefab, scene or asset holds the key's id - which is what a
    /// LocalizedText serializes - and no script names the key or its constant. A text read from a
    /// string the code builds at runtime is invisible to this, so the list is where to look, not
    /// what to delete.
    /// </summary>
    internal class LocalizationUsageScan
    {
        private readonly string[] _assets = { "*.prefab", "*.unity", "*.asset" };

        private readonly LocalizationKeysWriter _keys = new();

        public List<string> Unused(StringTableCollection collection)
        {
            var unused = new List<string>();

            if (collection == null)
                return unused;

            string serialized = Gather(_assets, skipTables: true);
            string code = Gather(new[] { "*.cs" }, skipTables: false);

            foreach (SharedTableData.SharedTableEntry entry in collection.SharedData.Entries)
            {
                bool inAssets = Regex.IsMatch(serialized, $@"m_KeyId: {entry.Id}\b");
                bool inCode = code.Contains("\"" + entry.Key + "\"") || Regex.IsMatch(code, $@"\bLocalizationKeys\.{_keys.ConstantOf(entry.Key)}\b");

                if (!inAssets && !inCode)
                    unused.Add(entry.Key);
            }

            return unused;
        }

        /// <summary>Every matching file under Assets, read once. The tables themselves hold every id, so they are left out.</summary>
        private string Gather(string[] patterns, bool skipTables)
        {
            var all = new System.Text.StringBuilder();

            foreach (string pattern in patterns)
            {
                foreach (string path in Directory.EnumerateFiles("Assets", pattern, SearchOption.AllDirectories))
                {
                    string normal = path.Replace('\\', '/');

                    if (skipTables && normal.StartsWith(LocalizationProject.FOLDER + "/"))
                        continue;

                    if (normal.EndsWith("/LocalizationKeys.cs"))
                        continue;

                    all.AppendLine(File.ReadAllText(path));
                }
            }

            return all.ToString();
        }
    }
}

#endif
