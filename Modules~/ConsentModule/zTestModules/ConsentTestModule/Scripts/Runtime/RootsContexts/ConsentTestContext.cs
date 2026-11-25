#if UNITY_EDITOR
using FlowIoC.BaseModule.Connectors;
using FlowIoC.BaseModule.Contexts;
using Modules.ConsentModule.ConsentTestModule.Controllers;
using Modules.ConsentModule.ConsentTestModule.Signals;
using Modules.ConsentModule.Signals;

namespace Modules.ConsentModule.ConsentTestModule.RootsContexts
{
    /// <summary>
    /// Stands in for the game's Connector: the answer on Outgoing.Resolved is reported where the
    /// game would hand it to its ad and analytics SDKs. Ask again and open the privacy options
    /// from the Device Debugger's Consent rows; change CD_Consent's development answers to see
    /// the other outcomes.
    /// </summary>
    public class ConsentTestContext : Context
    {
        private const string GROUP = nameof(ConsentTestContext);

        private ConsentTestInternalSignals _signals;

        public override void SignalBindings()
        {
            base.SignalBindings();
            _signals = InjectionBinder.Bind<ConsentTestInternalSignals>();
        }

        public override void CommandBindings()
        {
            base.CommandBindings();
            CommandBinder.Bind(_signals.Resolved).ToSequence<ReportConsentCommand>();
        }

        public override void Setup()
        {
            base.Setup();
            InjectionBinderCrossContext.GetInstance<ConsentSignals>().Outgoing.Resolved.Connect(_signals.Resolved, GROUP);
        }

        public override void DestroyContext()
        {
            SignalConnector.DisconnectGroup(GROUP);
            base.DestroyContext();
        }
    }
}
#endif
