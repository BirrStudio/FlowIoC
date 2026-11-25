using Modules.ConsentModule.Data.ValueObjects;
using Modules.ConsentModule.Services;
using Modules.ConsentModule.Shared.Data.ValueObjects;
using Modules.ConsentModule.Shared.Enums;

namespace Modules.ConsentModule.Models
{
    /// <summary>
    /// The plugged provider, the answers as they come in, and the published result. No decisions;
    /// the commands take them.
    /// </summary>
    public interface IConsentModel
    {
        ConsentSettingsCVO Settings { get; }

        IConsentProvider Provider { get; }

        /// <summary>A request or the privacy options form is waiting on the provider.</summary>
        bool IsAsking { get; }

        bool IsResolved { get; }

        ConsentVO Current { get; }

        void Plug(IConsentProvider provider);

        void Unplug();

        void SetAsking(bool asking);

        void SetConsent(ConsentStatus consent, bool privacyOptionsRequired);

        void SetConsent(ConsentStatus consent);

        void SetTracking(TrackingStatus tracking);

        /// <summary>Takes the answers so far as the result and writes it into RD_Consent.</summary>
        ConsentVO Publish();
    }
}
