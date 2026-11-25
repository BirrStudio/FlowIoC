using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.ConsentModule.Models;
using Modules.ConsentModule.Shared.Data.ValueObjects;
using Modules.ConsentModule.Signals;

namespace Modules.ConsentModule.Services
{
    public class ConsentService : IConsentService
    {
        [Inject] private IConsentModel _model { get; set; }
        [InjectSignal] private ConsentInternalSignals _signals { get; set; }

        public bool IsResolved => _model.IsResolved;

        public ConsentVO Current => _model.Current;

        public string ProviderName => _model.Provider?.Name ?? string.Empty;

        public void Request() => _signals.Request.Dispatch();

        public void ShowPrivacyOptions() => _signals.ShowPrivacyOptions.Dispatch();

        public void Plug(IConsentProvider provider) => _signals.Plug.Dispatch(provider);

        public void Unplug(IConsentProvider provider) => _signals.Unplug.Dispatch(provider);
    }
}
