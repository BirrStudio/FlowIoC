using System;
using Modules.ConsentModule.Shared.Enums;

namespace Modules.ConsentModule.Services
{
    /// <summary>
    /// The socket the tracking prompt plugs into - Apple's App Tracking Transparency, or your own.
    /// Asked after the consent form; answered exactly once, on the main thread.
    /// </summary>
    public interface ITrackingProvider
    {
        string Name { get; }

        /// <summary>Shows the tracking prompt where the platform has one, then answers; NotApplicable elsewhere.</summary>
        void RequestTracking(Action<TrackingStatus> done);
    }
}
