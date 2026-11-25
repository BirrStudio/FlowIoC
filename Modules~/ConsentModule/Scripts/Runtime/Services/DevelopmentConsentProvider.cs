#if UNITY_EDITOR || DEVELOPMENT_BUILD

using System;
using Modules.ConsentModule.Data.ValueObjects;
using Modules.ConsentModule.Shared.Enums;

namespace Modules.ConsentModule.Services
{
    /// <summary>
    /// The stand-in the module plugs itself in the Editor and in a Development Build when nothing
    /// else is plugged. It shows nothing and answers at once from CD_Consent, so the flow after
    /// consent runs before an SDK is chosen. A release build does not have it.
    /// </summary>
    internal class DevelopmentConsentProvider : IConsentProvider
    {
        private readonly ConsentSettingsCVO _settings;

        public DevelopmentConsentProvider(ConsentSettingsCVO settings) => _settings = settings;

        public string Name => "Development";

        public void RequestConsent(Action<ConsentStatus, bool> done) => done(_settings.DevelopmentConsent, true);

#if UNITY_IOS
        public void RequestTracking(Action<TrackingStatus> done) => done(_settings.DevelopmentTracking);
#else
        public void RequestTracking(Action<TrackingStatus> done) => done(TrackingStatus.NotApplicable);
#endif

        public void ShowPrivacyOptions(Action<ConsentStatus> done) => done(_settings.DevelopmentConsent);
    }
}

#endif
