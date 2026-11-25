using Modules.ConsentModule.Shared.Data.ValueObjects;

namespace Modules.ConsentModule.Services
{
    /// <summary>
    /// The consent module's surface. The answer itself goes out on ConsentSignals.Outgoing.Resolved
    /// and into RD_Consent; this interface asks, reads, and takes the two plugs - one for the
    /// consent form, one for the tracking prompt.
    /// </summary>
    public partial interface IConsentService
    {
        bool IsResolved { get; }

        ConsentVO Current { get; }

        string ProviderName { get; }

        string TrackingProviderName { get; }

        /// <summary>Runs the form and the tracking prompt. Only needed with CD_Consent.RequestOnLaunch off.</summary>
        void Request();

        /// <summary>Shows the privacy options form; refused before consent is first resolved.</summary>
        void ShowPrivacyOptions();

        void Plug(IConsentProvider provider);

        void Unplug(IConsentProvider provider);

        void Plug(ITrackingProvider provider);

        void Unplug(ITrackingProvider provider);
    }
}
