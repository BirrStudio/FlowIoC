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
    /// Asks the provider for the tracking prompt after the consent form, and waits for the player.
    /// A provider that throws counts as Denied and the flow goes on.
    /// </summary>
    [LongRetain]
    internal class RequestTrackingCommand : Command
    {
        [Inject] private IConsentModel _model { get; set; }

        public override void Execute()
        {
            Retain();

            IConsentModel model = _model;
            IConsentProvider provider = model.Provider;
            bool answered = false;

            try
            {
                provider.RequestTracking(status =>
                {
                    if (answered) return;
                    answered = true;

                    FlowLogger.Log($"Request - tracking {status} from '{provider.Name}'");
                    model.SetTracking(status);
                    Release();
                });
            }
            catch (Exception exception)
            {
                if (answered) return;
                answered = true;

                FlowLogger.LogError($"Request - '{provider.Name}' threw on RequestTracking; tracking counts as Denied: {exception.Message}");
                model.SetTracking(TrackingStatus.Denied);
                Release();
            }
        }
    }
}
