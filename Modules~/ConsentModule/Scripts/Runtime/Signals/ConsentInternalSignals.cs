using FlowIoC.BaseModule.Signals;
using Modules.ConsentModule.Services;

namespace Modules.ConsentModule.Signals
{
    internal class ConsentInternalSignals : ISignalHolder
    {
        public Signal<IConsentProvider> Plug = new();
        public Signal<IConsentProvider> Unplug = new();
        public Signal<ITrackingProvider> PlugTracking = new();
        public Signal<ITrackingProvider> UnplugTracking = new();
        public Signal Launched = new();
        public Signal Request = new();
        public Signal ShowPrivacyOptions = new();
    }
}
