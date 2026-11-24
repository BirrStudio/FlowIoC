using System;
using System.Collections.Generic;
using Modules.LocalizationModule.Constants;
using Modules.LocalizationModule.Data.UnityObjects;
using Modules.LocalizationModule.Data.ValueObjects;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Modules.LocalizationModule.Remote
{
    /// <summary>
    /// Puts the corrected texts a phone last downloaded over the built Game table, as each
    /// language's table loads. It is saved on the settings asset rather than assigned in code:
    /// Unity Localization starts the first time anything touches it - a LocalizedText at scene
    /// load - and the preloaded table can be in before any of the module's code has run.
    ///
    /// It applies nothing in the Editor: there a table is the asset itself, a patched entry could
    /// be saved into Assets/Localization, and the next Push would send the download back to the
    /// sheet as if somebody had typed it.
    /// </summary>
    [Serializable]
    public class RemoteTextsPostprocessor : ITablePostprocessor
    {
        /// <summary>The settings phones read the sheet with; it travels with the settings asset into the build.</summary>
        public CD_LocalizationRemote Config;

        [NonSerialized] private RemoteTextsCacheVO _cache;
        [NonSerialized] private bool _isRead;
        [NonSerialized] private readonly RemoteTextsValidator _validator = new();

        public void PostprocessTable(LocalizationTable table)
        {
            if (Application.isEditor || table is not StringTable strings || strings.TableCollectionName != LocalizationConstants.TABLE)
                return;

            if (!_isRead)
            {
                _cache = new RemoteTextsCache(RemoteTextsCache.DefaultPath(), Application.version).Read();
                _isRead = true;
            }

            if (_cache != null)
                Apply(strings, _cache.Texts);
        }

        /// <summary>The patch itself: each text of the table's language whose key the build has and whose placeholders match.</summary>
        internal void Apply(StringTable table, IEnumerable<RemoteTextVO> texts)
        {
            string language = table.LocaleIdentifier.Code;

            foreach (RemoteTextVO text in texts)
            {
                if (text.Language != language)
                    continue;

                StringTableEntry entry = table.GetEntry(text.Key);

                if (entry != null && _validator.IsApplicable(entry.Value, text.Text))
                    entry.Value = text.Text;
            }
        }

        /// <summary>Drops the cache read so far, so tables loaded after a new download read the new file.</summary>
        internal void Forget()
        {
            _cache = null;
            _isRead = false;
        }

        /// <summary>The settings asset's postprocessor when it is this one, or null when phones do not read the sheet.</summary>
        internal static RemoteTextsPostprocessor Current() =>
            LocalizationSettings.HasSettings ? LocalizationSettings.StringDatabase?.TablePostprocessor as RemoteTextsPostprocessor : null;
    }
}
