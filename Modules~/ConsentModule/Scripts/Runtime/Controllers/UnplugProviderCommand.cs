using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.ConsentModule.Models;
using Modules.ConsentModule.Services;

namespace Modules.ConsentModule.Controllers
{
    /// <summary>Unplugs only the provider that is plugged; any other is ignored.</summary>
    internal class UnplugProviderCommand : Command
    {
        [Inject] private IConsentModel _model { get; set; }

        [SignalParam] private IConsentProvider _provider { get; set; }

        public override void Execute()
        {
            if (_provider == null || _model.Provider != _provider)
                return;

            _model.Unplug();
            FlowLogger.Log($"Unplug - {_provider.Name}");
        }
    }
}
