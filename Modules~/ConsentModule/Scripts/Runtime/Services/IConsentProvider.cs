using System;
using Modules.ConsentModule.Shared.Enums;

namespace Modules.ConsentModule.Services
{
    /// <summary>
    /// The socket a consent SDK plugs into - Google UMP for the form, Apple's ATT for the tracking
    /// prompt, or one SDK doing both. A provider only asks and answers; the order, the publishing
    /// and what happens without a provider are the module's. Each callback is answered exactly
    /// once, on the main thread.
    /// </summary>
    public interface IConsentProvider
    {
        string Name { get; }

        /// <summary>Shows the consent form where the player's region needs one, then answers it and whether privacy options must be offered.</summary>
        void RequestConsent(Action<ConsentStatus, bool> done);

        /// <summary>Shows the tracking prompt where the platform has one, then answers; NotApplicable elsewhere.</summary>
        void RequestTracking(Action<TrackingStatus> done);

        /// <summary>Shows the privacy options form again, from a settings button, then answers the new consent.</summary>
        void ShowPrivacyOptions(Action<ConsentStatus> done);
    }
}
