using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.ConsentModule.Models;
using Modules.ConsentModule.Shared.Data.ValueObjects;
using Modules.ConsentModule.Signals;

namespace Modules.ConsentModule.Controllers
{
    /// <summary>The answers so far become the result: written into RD_Consent and announced on Outgoing.Resolved.</summary>
    internal class PublishConsentCommand : Command
    {
        [Inject] private IConsentModel _model { get; set; }
        [InjectSignal] private ConsentSignals _signals { get; set; }

        public override void Execute()
        {
            _model.SetAsking(false);
            ConsentVO consent = _model.Publish();

            FlowLogger.Log($"Resolved - {consent}");
            _signals.Outgoing.Resolved.Dispatch(consent);
        }
    }
}
