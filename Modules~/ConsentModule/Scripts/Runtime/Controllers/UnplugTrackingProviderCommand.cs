using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.ConsentModule.Models;
using Modules.ConsentModule.Services;

namespace Modules.ConsentModule.Controllers
{
    /// <summary>Unplugs only the tracking provider that is plugged; any other is ignored.</summary>
    internal class UnplugTrackingProviderCommand : Command
    {
        [Inject] private IConsentModel _model { get; set; }

        [SignalParam] private ITrackingProvider _provider { get; set; }

        public override void Execute()
        {
            if (_provider == null || _model.TrackingProvider != _provider)
                return;

            _model.UnplugTracking();
            FlowLogger.Log($"UnplugTracking - {_provider.Name}");
        }
    }
}
