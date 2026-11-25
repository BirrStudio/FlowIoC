using Modules.ConsentModule.Shared.Enums;

namespace Modules.ConsentModule.Shared.Data.ValueObjects
{
    /// <summary>
    /// The resolved answer: the consent form's, the tracking prompt's, whether the game must offer a
    /// privacy options button, and - for a TCF form - the purposes the player consented to. The
    /// readings below are what an ad or analytics SDK is handed; for a TCF answer they follow
    /// Google's consent mode mapping of the TCF purposes.
    /// </summary>
    public readonly struct ConsentVO
    {
        public readonly ConsentStatus Consent;
        public readonly TrackingStatus Tracking;
        public readonly bool PrivacyOptionsRequired;

        /// <summary>IABTCF_PurposeConsents as the TCF form wrote it: '1' at index n is consent to purpose n+1. Empty without a TCF form.</summary>
        public readonly string TcfPurposeConsents;

        public ConsentVO(ConsentStatus consent, TrackingStatus tracking, bool privacyOptionsRequired,
            string tcfPurposeConsents = null)
        {
            Consent = consent;
            Tracking = tracking;
            PrivacyOptionsRequired = privacyOptionsRequired;
            TcfPurposeConsents = tcfPurposeConsents ?? string.Empty;
        }

        /// <summary>
        /// The answer lives in the IAB TCF string, which the ad SDKs read on their own: hand them no
        /// consent flag, which would override it.
        /// </summary>
        public bool IsInTcfString => Consent == ConsentStatus.Recorded;

        /// <summary>Storing on and reading from the device for ads. TCF: purpose 1.</summary>
        public bool AdStorage => IsInTcfString ? HasPurpose(1) : IsGranted;

        /// <summary>Sending user data to Google for ads. TCF: purposes 1 and 7.</summary>
        public bool AdUserData => IsInTcfString ? HasPurpose(1) && HasPurpose(7) : IsGranted;

        /// <summary>
        /// Consent to personalised ads. TCF: purposes 3 and 4. It carries no ATT answer: hand an
        /// analytics SDK's ad_personalization CanTrack, which adds it.
        /// </summary>
        public bool AdPersonalization => IsInTcfString ? HasPurpose(3) && HasPurpose(4) : IsGranted;

        /// <summary>Storing analytics data on the device. TCF has no analytics purpose of its own: purpose 1.</summary>
        public bool AnalyticsStorage => IsInTcfString ? HasPurpose(1) : IsGranted;

        /// <summary>Personalised ads and data use are allowed: storage and personalisation both granted.</summary>
        public bool CanPersonalise => AdStorage && AdPersonalization;

        /// <summary>Cross-app tracking is allowed: personalised, and authorized or not asked on this platform.</summary>
        public bool CanTrack =>
            CanPersonalise && (Tracking == TrackingStatus.Authorized || Tracking == TrackingStatus.NotApplicable);

        /// <summary>The player consented to this TCF purpose (1-based).</summary>
        public bool HasPurpose(int purpose) =>
            purpose >= 1 && purpose <= TcfPurposeConsents.Length && TcfPurposeConsents[purpose - 1] == '1';

        private bool IsGranted => Consent == ConsentStatus.Granted || Consent == ConsentStatus.NotRequired;

        public override string ToString() =>
            $"consent={Consent} tracking={Tracking} privacy_options={(PrivacyOptionsRequired ? "required" : "no")}"
            + (IsInTcfString ? $" tcf_purposes={TcfPurposeConsents}" : string.Empty);
    }
}
