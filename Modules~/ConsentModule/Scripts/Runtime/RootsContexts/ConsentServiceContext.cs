using FlowIoC.BaseModule.Contexts;
using Modules.ConsentModule.Controllers;
using Modules.ConsentModule.Models;
using Modules.ConsentModule.Services;
using Modules.ConsentModule.Signals;

namespace Modules.ConsentModule.RootsContexts
{
    /// <summary>
    /// Two sockets, one for the consent form and one for the tracking prompt: the plugs plug in
    /// Setup, and Launch asks - the form, then the tracking prompt, then the answer published -
    /// unless CD_Consent says the game asks.
    /// </summary>
    public class ConsentServiceContext : Context
    {
        private ConsentSignals _signals;
        private ConsentInternalSignals _internalSignals;

        public override void SignalBindings()
        {
            base.SignalBindings();
            _internalSignals = InjectionBinder.Bind<ConsentInternalSignals>();
            _signals = InjectionBinderCrossContext.Bind<ConsentSignals>();
        }

        public override void InjectionBindings()
        {
            base.InjectionBindings();

            InjectionBinder.Bind<IConsentModel, ConsentModel>();

            // The one type other modules reference directly, which is what makes this a Service.
            InjectionBinderCrossContext.Bind<IConsentService, ConsentService>();
        }

        public override void CommandBindings()
        {
            base.CommandBindings();

            CommandBinder.Bind(_internalSignals.Plug).ToSequence<PlugProviderCommand>();

            CommandBinder.Bind(_internalSignals.Unplug).ToSequence<UnplugProviderCommand>();

            CommandBinder.Bind(_internalSignals.PlugTracking).ToSequence<PlugTrackingProviderCommand>();

            CommandBinder.Bind(_internalSignals.UnplugTracking).ToSequence<UnplugTrackingProviderCommand>();

            CommandBinder.Bind(_internalSignals.Launched)
                .ToSequence<RequestOnLaunchCommand>()
                .ToSequence<EnsureProviderCommand>()
                .ToSequence<RequestConsentCommand>()
                .ToSequence<RequestTrackingCommand>()
                .ToSequence<PublishConsentCommand>();

            CommandBinder.Bind(_internalSignals.Request)
                .ToSequence<EnsureProviderCommand>()
                .ToSequence<RequestConsentCommand>()
                .ToSequence<RequestTrackingCommand>()
                .ToSequence<PublishConsentCommand>();

            CommandBinder.Bind(_internalSignals.ShowPrivacyOptions)
                .ToSequence<ShowPrivacyOptionsCommand>()
                .ToSequence<PublishConsentCommand>();
        }

        public override void Launch()
        {
            base.Launch();

            // Every Setup in the scene has run, so a plug listed on this Root has plugged.
            _internalSignals.Launched.Dispatch();
        }
    }
}
