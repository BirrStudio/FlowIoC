using Modules.ConsentModule.Shared.Enums;

namespace Modules.ConsentModule.Shared.Data.ValueObjects
{
    /// <summary>
    /// The resolved answer: the consent form's, the tracking prompt's, and whether the game must
    /// offer a privacy options button. The two readings below are what an ad or analytics SDK is
    /// handed; the SDK's own flags are built by whoever hands them on.
    /// </summary>
    public readonly struct ConsentVO
    {
        public readonly ConsentStatus Consent;
        public readonly TrackingStatus Tracking;
        public readonly bool PrivacyOptionsRequired;

        public ConsentVO(ConsentStatus consent, TrackingStatus tracking, bool privacyOptionsRequired)
        {
            Consent = consent;
            Tracking = tracking;
            PrivacyOptionsRequired = privacyOptionsRequired;
        }

        /// <summary>Personalised ads and data use are allowed: granted, or not required where the player is.</summary>
        public bool CanPersonalise => Consent == ConsentStatus.Granted || Consent == ConsentStatus.NotRequired;

        /// <summary>Cross-app tracking is allowed: personalised, and authorized or not asked on this platform.</summary>
        public bool CanTrack =>
            CanPersonalise && (Tracking == TrackingStatus.Authorized || Tracking == TrackingStatus.NotApplicable);

        public override string ToString() =>
            $"consent={Consent} tracking={Tracking} privacy_options={(PrivacyOptionsRequired ? "required" : "no")}";
    }
}
