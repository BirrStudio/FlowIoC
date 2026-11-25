using System;
using System.Threading;
using FlowIoC.ConsoleModule;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
using Modules.ConsentModule.Services;
using ConsentStatus = Modules.ConsentModule.Shared.Enums.ConsentStatus;
using UmpStatus = GoogleMobileAds.Ump.Api.ConsentStatus;

namespace Modules.ConsentModule.UmpConsentModule.Services
{
    /// <summary>
    /// Google UMP behind ITcfConsentProvider. A form the player answered is Recorded, not Granted:
    /// what they chose, purpose by purpose, is in the IAB TCF string the ad SDKs read themselves, and
    /// a consent flag handed to them would override it; the purposes are read back for the module
    /// to map onto the analytics flags. NotRequired passes as it is; a form that could not be shown
    /// is Denied. Every answer is returned on the main thread it was asked from.
    /// </summary>
    public class UmpConsentProvider : ITcfConsentProvider
    {
        public string Name => "UMP";

        public void RequestConsent(Action<ConsentStatus, bool> done)
        {
            SynchronizationContext main = SynchronizationContext.Current;

            ConsentInformation.Update(new ConsentRequestParameters(), updateError =>
            {
                if (updateError != null)
                {
                    FlowLogger.LogWarning($"RequestConsent - UMP could not update the consent information: {updateError.Message}");
                    Answer(main, () => done(Read(), IsPrivacyOptionsRequired()));
                    return;
                }

                ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
                {
                    if (formError != null)
                        FlowLogger.LogWarning($"RequestConsent - UMP could not show the consent form: {formError.Message}");

                    Answer(main, () => done(Read(), IsPrivacyOptionsRequired()));
                });
            });
        }

        public void ShowPrivacyOptions(Action<ConsentStatus> done)
        {
            SynchronizationContext main = SynchronizationContext.Current;

            ConsentForm.ShowPrivacyOptionsForm(formError =>
            {
                if (formError != null)
                    FlowLogger.LogWarning($"ShowPrivacyOptions - UMP could not show the privacy options form: {formError.Message}");

                Answer(main, () => done(Read()));
            });
        }

        /// <summary>UMP writes it where each platform's TCF string lives; GMA's ApplicationPreferences reads both.</summary>
        public string ReadPurposeConsents() => ApplicationPreferences.GetString("IABTCF_PurposeConsents") ?? string.Empty;

        private static ConsentStatus Read()
        {
            switch (ConsentInformation.ConsentStatus)
            {
                case UmpStatus.Obtained:
                    return ConsentStatus.Recorded;
                case UmpStatus.NotRequired:
                    return ConsentStatus.NotRequired;
                default:
                    return ConsentStatus.Denied;
            }
        }

        private static bool IsPrivacyOptionsRequired() =>
            ConsentInformation.PrivacyOptionsRequirementStatus == PrivacyOptionsRequirementStatus.Required;

        private static void Answer(SynchronizationContext main, Action answer)
        {
            if (main == null || SynchronizationContext.Current == main)
                answer();
            else
                main.Post(_ => answer(), null);
        }
    }
}
