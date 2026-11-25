using System;
using Modules.ConsentModule.Shared.Enums;
using UnityEngine;

namespace Modules.ConsentModule.Data.ValueObjects
{
    /// <summary>The knobs CD_Consent carries. The defaults are what a game gets without touching the asset.</summary>
    [Serializable]
    public class ConsentSettingsCVO
    {
        [Tooltip("Ask when the Root launches. Off: the game binds IConsentService.Commands.Request where its flow wants the form.")]
        public bool RequestOnLaunch = true;

        [Tooltip("Release build with no consent provider plugged: the answer, reported as an error. Denied keeps a store build from using personal data; NotRequired keeps a tester build measured before the consent SDK is in.")]
        public ConsentStatus ReleaseConsentWithoutPlug = ConsentStatus.Denied;

        [Tooltip("Editor and Development Build with no provider plugged: what the stand-in answers for the consent form.")]
        public ConsentStatus DevelopmentConsent = ConsentStatus.Granted;

        [Tooltip("Editor and Development Build with no provider plugged, on iOS: what the stand-in answers for the tracking prompt.")]
        public TrackingStatus DevelopmentTracking = TrackingStatus.Authorized;
    }
}
