using FlowIoC.BaseModule.Contexts;
using Modules.LocalizationModule.Controllers;
using Modules.LocalizationModule.Models;
using Modules.LocalizationModule.Services;
using Modules.LocalizationModule.Signals;

namespace Modules.LocalizationModule.RootsContexts
{
    public class LocalizationServiceContext : Context
    {
        private LocalizationInternalSignals _internalSignals;

        public override void SignalBindings()
        {
            base.SignalBindings();

            _internalSignals = InjectionBinder.Bind<LocalizationInternalSignals>();
            InjectionBinderCrossContext.Bind<LocalizationSignals>();
        }

        public override void InjectionBindings()
        {
            base.InjectionBindings();

            InjectionBinder.Bind<ILocalizationModel, LocalizationModel>();

            // The one type other modules reference directly, which is what makes this a Service.
            InjectionBinderCrossContext.Bind<ILocalizationService, LocalizationService>();
        }

        public override void CommandBindings()
        {
            base.CommandBindings();

            CommandBinder.Bind(_internalSignals.Initialize).ToSequence<InitializeLocalizationCommand>();

            CommandBinder.Bind(_internalSignals.SetLanguage).ToSequence<ApplyLanguageCommand>();
        }
    }
}
