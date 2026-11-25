using System;
using Modules.ConsentModule.Shared.Enums;

namespace Modules.ConsentModule.Services
{
    /// <summary>
    /// What a release build falls back to for an empty socket: it asks nothing and answers what
    /// CD_Consent.ReleaseConsentWithoutPlug says - Denied unless the developer chose otherwise, so
    /// nothing personal is used without consent by default, and a tester build can stay measured.
    /// Tracking is Denied on iOS, where no prompt was shown, and NotApplicable elsewhere. The module
    /// reports a missing plug before it falls back.
    /// </summary>
    internal class FallbackConsentProvider : IConsentProvider, ITrackingProvider
    {
        private readonly ConsentStatus _answer;

        public FallbackConsentProvider(ConsentStatus answer) => _answer = answer;

        public string Name => "Fallback";

        public void RequestConsent(Action<ConsentStatus, bool> done) => done(_answer, false);

#if UNITY_IOS
        public void RequestTracking(Action<TrackingStatus> done) => done(TrackingStatus.Denied);
#else
        public void RequestTracking(Action<TrackingStatus> done) => done(TrackingStatus.NotApplicable);
#endif

        public void ShowPrivacyOptions(Action<ConsentStatus> done) => done(_answer);
    }
}
