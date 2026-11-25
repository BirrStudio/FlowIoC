using System;
using Modules.ConsentModule.Shared.Enums;

namespace Modules.ConsentModule.Services
{
    /// <summary>
    /// The socket the consent form's SDK plugs into - Google UMP, or your own. A provider only
    /// asks and answers; the order, the publishing and what happens without a provider are the
    /// module's. Each callback is answered exactly once, on the main thread.
    /// </summary>
    public interface IConsentProvider
    {
        string Name { get; }

        /// <summary>Shows the consent form where the player's region needs one, then answers it and whether privacy options must be offered.</summary>
        void RequestConsent(Action<ConsentStatus, bool> done);

        /// <summary>Shows the privacy options form again, from a settings button, then answers the new consent.</summary>
        void ShowPrivacyOptions(Action<ConsentStatus> done);
    }
}
