using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.ConsentModule.Models;
using Modules.ConsentModule.Services;

namespace Modules.ConsentModule.Controllers
{
    /// <summary>One tracking prompt provider per game: a second is refused with an error, the first stays.</summary>
    internal class PlugTrackingProviderCommand : Command
    {
        [Inject] private IConsentModel _model { get; set; }

        [SignalParam] private ITrackingProvider _provider { get; set; }

        public override void Execute()
        {
            if (_provider == null)
            {
                FlowLogger.LogError("PlugTracking - a null provider was handed in; nothing plugged.");
                return;
            }

            if (_model.TrackingProvider != null)
            {
                FlowLogger.LogError(
                    $"PlugTracking - '{_model.TrackingProvider.Name}' is already plugged; '{_provider.Name}' refused. One tracking provider per game: list one tracking plug on ConsentServiceRoot.");
                return;
            }

            _model.PlugTracking(_provider);
            FlowLogger.Log($"PlugTracking - {_provider.Name}");
        }
    }
}
