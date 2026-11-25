using System;
using FlowIoC.BaseModule.Attributes;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.ConsentModule.Models;
using Modules.ConsentModule.Services;
using Modules.ConsentModule.Shared.Enums;

namespace Modules.ConsentModule.Controllers
{
    /// <summary>
    /// Asks the provider for the consent form and waits for the player, however long that takes.
    /// A request while another is waiting stops here. A provider that throws counts as Denied and
    /// the flow goes on; a second answer from a provider is ignored.
    /// </summary>
    [LongRetain]
    internal class RequestConsentCommand : Command
    {
        [Inject] private IConsentModel _model { get; set; }

        public override void Execute()
        {
            Retain();

            if (_model.IsAsking)
            {
                FlowLogger.Log("Request - already asking; this request stops.");
                Stop();
                return;
            }

            IConsentModel model = _model;
            IConsentProvider provider = model.Provider;
            bool answered = false;
            model.SetAsking(true);

            try
            {
                provider.RequestConsent((status, privacyOptionsRequired) =>
                {
                    if (answered) return;
                    answered = true;

                    FlowLogger.Log($"Request - consent {status} from '{provider.Name}'");
                    model.SetConsent(status, privacyOptionsRequired);
                    Release();
                });
            }
            catch (Exception exception)
            {
                if (answered) return;
                answered = true;

                FlowLogger.LogError($"Request - '{provider.Name}' threw on RequestConsent; consent counts as Denied: {exception.Message}");
                model.SetConsent(ConsentStatus.Denied, false);
                Release();
            }
        }
    }
}
