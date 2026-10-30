using System;
using System.Collections.Generic;
using System.Globalization;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.LocalizationModule.Constants;
using Modules.LocalizationModule.Data.ValueObjects;
using Modules.LocalizationModule.Models;
using Modules.LocalizationModule.Signals;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Modules.LocalizationModule.Services
{
    /// <summary>
    /// Reads go straight to Unity Localization; the two operations that wait are handed to
    /// commands through the internal signals, so each is a step the Flow Console shows and the
    /// decisions - an unknown code, a failed load - are taken in a Command.
    /// </summary>
    public class LocalizationService : ILocalizationService
    {
        [Inject] private ILocalizationModel _model { get; set; }
        [InjectSignal] private LocalizationInternalSignals _signals { get; set; }

        public bool IsReady => _model.IsReady;

        public string Language => LocalizationSettings.SelectedLocale?.Identifier.Code;

        public IReadOnlyList<LanguageVO> Languages
        {
            get
            {
                var languages = new List<LanguageVO>();

                foreach (Locale locale in LocalizationSettings.AvailableLocales.Locales)
                    languages.Add(new LanguageVO(locale.Identifier.Code, NameOf(locale)));

                return languages;
            }
        }

        public string Get(string key) =>
            LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationConstants.TABLE, key);

        public string Get(string key, params object[] arguments) =>
            LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationConstants.TABLE, key, arguments);

        public void Initialize(Action done) => _signals.Initialize.Dispatch(done);

        public void SetLanguage(string code, Action done) => _signals.SetLanguage.Dispatch(code, done);

        /// <summary>The language's name in itself, capitalised the way its own culture does it.</summary>
        private string NameOf(Locale locale)
        {
            CultureInfo culture = locale.Identifier.CultureInfo;

            if (culture == null)
                return locale.LocaleName;

            string name = culture.NativeName;

            return name.Length == 0 ? name : culture.TextInfo.ToUpper(name[0]) + name.Substring(1);
        }
    }
}
