#if UNITY_EDITOR

using FlowIoC.BaseModule.Connectors;
using FlowIoC.BaseModule.Contexts;
using Modules.LocalizationModule.LocalizationTestModule.Controllers;
using Modules.LocalizationModule.LocalizationTestModule.Models;
using Modules.LocalizationModule.LocalizationTestModule.Signals;
using Modules.LocalizationModule.LocalizationTestModule.ViewsMediators;
using Modules.LocalizationModule.Services;
using Modules.LocalizationModule.Signals;

namespace Modules.LocalizationModule.LocalizationTestModule.RootsContexts
{
    /// <summary>
    /// The sample a game copies from: the boot binds Initialize before the first fill, a language
    /// button binds SetLanguage, and LanguageChanged - wired here the way a game's Connector wires
    /// it - fills the variable line again. The title is a LocalizedText and needs none of this.
    /// </summary>
    public class LocalizationTestContext : Context
    {
        private LocalizationTestInternalSignals _signals;
        private LocalizationSignals _localizationSignals;

        public override void SignalBindings()
        {
            base.SignalBindings();

            _signals = InjectionBinder.Bind<LocalizationTestInternalSignals>();
        }

        public override void InjectionBindings()
        {
            base.InjectionBindings();

            InjectionBinder.Bind<LocalizationTestModel>();
        }

        public override void MediationBindings()
        {
            base.MediationBindings();

            MediationBinder.Bind<LocalizationTestView>().To<LocalizationTestMediator>();
        }

        public override void CommandBindings()
        {
            base.CommandBindings();

            CommandBinder.Bind(_signals.Start)
                .ToSequence<ILocalizationService.Commands.Initialize>()
                .ToSequence<FillSampleCommand>();

            CommandBinder.Bind(_signals.LanguagePicked).ToSequence<ILocalizationService.Commands.SetLanguage>();

            CommandBinder.Bind(_signals.LevelUp)
                .ToSequence<AddLevelCommand>()
                .ToSequence<FillSampleCommand>();

            CommandBinder.Bind(_signals.Refill).ToSequence<FillSampleCommand>();
        }

        public override void Setup()
        {
            base.Setup();

            // A test module stands in for the Connector.
            _localizationSignals = InjectionBinderCrossContext.GetInstance<LocalizationSignals>();
            _localizationSignals.Outgoing.LanguageChanged.Connect(_signals.Refill);
        }

        public override void Launch()
        {
            base.Launch();

            _signals.Start.Dispatch();
        }

        public override void DestroyContext()
        {
            _localizationSignals?.Outgoing.LanguageChanged.Disconnect();

            base.DestroyContext();
        }
    }
}

#endif
