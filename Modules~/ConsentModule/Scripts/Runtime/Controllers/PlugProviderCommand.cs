using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.ConsentModule.Models;
using Modules.ConsentModule.Services;

namespace Modules.ConsentModule.Controllers
{
    /// <summary>One provider per game: a second is refused with an error, the first stays.</summary>
    internal class PlugProviderCommand : Command
    {
        [Inject] private IConsentModel _model { get; set; }

        [SignalParam] private IConsentProvider _provider { get; set; }

        public override void Execute()
        {
            if (_provider == null)
            {
                FlowLogger.LogError("Plug - a null provider was handed in; nothing plugged.");
                return;
            }

            if (_model.Provider != null)
            {
                FlowLogger.LogError(
                    $"Plug - '{_model.Provider.Name}' is already plugged; '{_provider.Name}' refused. One consent provider per game: list one plug on ConsentServiceRoot.");
                return;
            }

            _model.Plug(_provider);
            FlowLogger.Log($"Plug - {_provider.Name}");
        }
    }
}
