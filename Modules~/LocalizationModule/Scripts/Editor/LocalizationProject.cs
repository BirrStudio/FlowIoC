#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using Modules.LocalizationModule.Constants;
using Modules.LocalizationModule.Data.UnityObjects;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.Google;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Modules.LocalizationModule.Editor
{
    /// <summary>
    /// Where the project's localization lives and how it is made. Set up writes into
    /// Assets/Localization rather than the module's folder: the tables are the game's texts, and a
    /// module that is updated or deleted must never take a translation with it.
    /// </summary>
    internal class LocalizationProject
    {
        public const string FOLDER = "Assets/Localization";

        private const string SETTINGS_PATH = FOLDER + "/LocalizationSettings.asset";
        private const string LOCALES_FOLDER = FOLDER + "/Locales";
        private const string TABLES_FOLDER = FOLDER + "/Tables";
        private const string PROVIDER_PATH = FOLDER + "/SheetsServiceProvider.asset";
        private const string SHEET_PATH = FOLDER + "/ED_LocalizationSheet.asset";

        private readonly string[] _firstLanguages = { "en", "tr" };

        public bool IsSetUp => LocalizationEditorSettings.ActiveLocalizationSettings != null && Collection != null;

        public StringTableCollection Collection => LocalizationEditorSettings.GetStringTableCollection(LocalizationConstants.TABLE);

        // Found once and kept: the panel asks for them on every repaint. A deleted asset reads as
        // null through Unity's equality and is looked for again.
        private SheetsServiceProvider _provider;
        private ED_LocalizationSheet _sheet;

        public SheetsServiceProvider Provider => _provider != null ? _provider : _provider = Find<SheetsServiceProvider>();

        public ED_LocalizationSheet Sheet => _sheet != null ? _sheet : _sheet = Find<ED_LocalizationSheet>();

        /// <summary>
        /// An OAuth client on the provider. Every sheet call the panel makes writes or reads a
        /// private sheet, and an API key only reads a public one, so nothing less will do.
        /// </summary>
        public bool HasCredentials =>
            Provider != null
            && Provider.Authentication == AuthenticationType.OAuth
            && !string.IsNullOrEmpty(Provider.ClientId)
            && !string.IsNullOrEmpty(Provider.ClientSecret);

        /// <summary>
        /// The languages the Game table has, the source language first. A locale with no table in
        /// Game - one made in Unity's Locale Generator - is left out until Add language gives it one,
        /// so nothing is read from or written to a table that is not there.
        /// </summary>
        public List<string> Languages()
        {
            var codes = new List<string>();
            StringTableCollection collection = Collection;

            if (collection == null)
                return codes;

            foreach (Locale locale in LocalizationEditorSettings.GetLocales())
            {
                if (collection.ContainsTable(locale.Identifier))
                    codes.Add(locale.Identifier.Code);
            }

            codes.Sort((a, b) => a == LocalizationConstants.SOURCE_LANGUAGE ? -1 : b == LocalizationConstants.SOURCE_LANGUAGE ? 1 : string.CompareOrdinal(a, b));

            return codes;
        }

        /// <summary>
        /// Makes whatever is missing and leaves what exists alone, so pressing it twice, or on a
        /// project that already localizes, changes nothing that was there.
        /// </summary>
        public void SetUp()
        {
            Directory.CreateDirectory(LOCALES_FOLDER);
            Directory.CreateDirectory(TABLES_FOLDER);
            AssetDatabase.Refresh();

            LocalizationSettings settings = LocalizationEditorSettings.ActiveLocalizationSettings;

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                AssetDatabase.CreateAsset(settings, SETTINGS_PATH);
                LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            }

            foreach (string code in _firstLanguages)
                AddLanguage(code);

            SetStartup(settings);

            if (Collection == null)
                LocalizationEditorSettings.CreateStringTableCollection(LocalizationConstants.TABLE, TABLES_FOLDER);

            // The texts are read synchronously once Initialize is over, so the table is in memory by then.
            Collection.SetPreloadTableFlag(true);

            if (Provider == null)
            {
                var provider = ScriptableObject.CreateInstance<SheetsServiceProvider>();
                provider.ApplicationName = Application.productName;
                AssetDatabase.CreateAsset(provider, PROVIDER_PATH);
            }

            if (Sheet == null)
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<ED_LocalizationSheet>(), SHEET_PATH);

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        /// <summary>A locale for the code, a table for it in Game, preloaded. False when the code names no culture.</summary>
        public bool AddLanguage(string code)
        {
            Locale locale = LocalizationEditorSettings.GetLocale(new LocaleIdentifier(code));

            if (locale == null)
            {
                var identifier = new LocaleIdentifier(code);

                if (identifier.CultureInfo == null)
                    return false;

                Directory.CreateDirectory(LOCALES_FOLDER);
                locale = Locale.CreateLocale(identifier);
                AssetDatabase.CreateAsset(locale, $"{LOCALES_FOLDER}/{locale.LocaleName}.asset");
                LocalizationEditorSettings.AddLocale(locale);
            }

            StringTableCollection collection = Collection;

            if (collection != null && !collection.ContainsTable(locale.Identifier))
            {
                collection.AddNewTable(locale.Identifier);
                collection.SetPreloadTableFlag(true);
            }

            return true;
        }

        /// <summary>
        /// The order a start picks the language in: the player's saved choice, then the device's
        /// language, then English. The saved choice is the selector's own PlayerPrefs key, which
        /// it also writes whenever the language changes.
        /// </summary>
        private void SetStartup(LocalizationSettings settings)
        {
            List<IStartupLocaleSelector> selectors = settings.GetStartupLocaleSelectors();
            selectors.Clear();
            selectors.Add(new PlayerPrefLocaleSelector { PlayerPreferenceKey = LocalizationConstants.PREFS_KEY });
            selectors.Add(new SystemLocaleSelector());
            selectors.Add(new SpecificLocaleSelector { LocaleId = new LocaleIdentifier(LocalizationConstants.SOURCE_LANGUAGE) });
        }

        private T Find<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);

            return guids.Length == 0 ? null : AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}

#endif
