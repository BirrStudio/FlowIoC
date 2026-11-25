using FlowIoC.BaseModule.Signals;
using Modules.ConsentModule.Shared.Data.ValueObjects;

namespace Modules.ConsentModule.Signals
{
    /// <summary>
    /// Outgoing only: the way into a Service is its interface. A Connector hangs the hand-over to
    /// the ad and analytics SDKs on Resolved.
    /// </summary>
    public class ConsentSignals : ISignalHolder
    {
        public ConsentSignalsOutgoing Outgoing = new();
    }

    public class ConsentSignalsOutgoing
    {
        /// <summary>Consent is known - at boot, and again whenever the privacy options form answers.</summary>
        public Signal<ConsentVO> Resolved = new();
    }
}
