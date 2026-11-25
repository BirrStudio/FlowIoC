using System;
using Modules.ConsentModule.Shared.Enums;

namespace Modules.ConsentModule.Services
{
    /// <summary>
    /// What a release build falls back to for an empty socket: it asks nothing and answers denied -
    /// tracking NotApplicable off iOS, where there is no prompt - so ads still run, non-personalised,
    /// and nothing personal is used without consent. The module reports a missing plug before it
    /// falls back.
    /// </summary>
    internal class DeniedConsentProvider : IConsentProvider, ITrackingProvider
    {
        public string Name => "Denied";

        public void RequestConsent(Action<ConsentStatus, bool> done) => done(ConsentStatus.Denied, false);

#if UNITY_IOS
        public void RequestTracking(Action<TrackingStatus> done) => done(TrackingStatus.Denied);
#else
        public void RequestTracking(Action<TrackingStatus> done) => done(TrackingStatus.NotApplicable);
#endif

        public void ShowPrivacyOptions(Action<ConsentStatus> done) => done(ConsentStatus.Denied);
    }
}
