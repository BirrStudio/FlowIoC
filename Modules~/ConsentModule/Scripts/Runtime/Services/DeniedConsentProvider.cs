using System;
using Modules.ConsentModule.Shared.Enums;

namespace Modules.ConsentModule.Services
{
    /// <summary>
    /// What a release build falls back to when no provider is plugged: it asks nothing and answers
    /// denied, so ads still run, non-personalised, and nothing personal is used without consent.
    /// The module reports the missing plug as an error before it falls back.
    /// </summary>
    internal class DeniedConsentProvider : IConsentProvider
    {
        public string Name => "Denied";

        public void RequestConsent(Action<ConsentStatus, bool> done) => done(ConsentStatus.Denied, false);

        public void RequestTracking(Action<TrackingStatus> done) => done(TrackingStatus.NotApplicable);

        public void ShowPrivacyOptions(Action<ConsentStatus> done) => done(ConsentStatus.Denied);
    }
}
