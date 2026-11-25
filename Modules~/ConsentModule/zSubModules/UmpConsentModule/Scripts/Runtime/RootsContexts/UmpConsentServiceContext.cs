using FlowIoC.BaseModule.Attributes;
using FlowIoC.BaseModule.Contexts;
using FlowIoC.ConsoleModule;
using Modules.ConsentModule.Services;
using Modules.ConsentModule.UmpConsentModule.Services;

namespace Modules.ConsentModule.UmpConsentModule.RootsContexts
{
    /// <summary>
    /// The UMP plug. Listed on ConsentServiceRoot's Sub Context Types; its Setup - the one phase that
    /// may reach across modules - gets the service and plugs the consent form provider. The Root
    /// beside it is for a game that prefers the plug on a Root of its own.
    /// </summary>
    [AllowAsSubContext]
    public class UmpConsentServiceContext : Context
    {
        private IConsentService _consent;
        private UmpConsentProvider _provider;

        public override void Setup()
        {
            base.Setup();

            _consent = InjectionBinderCrossContext.GetInstance<IConsentService>();

            if (_consent == null)
            {
                FlowLogger.LogError("Setup - IConsentService is not bound: list this context on ConsentServiceRoot, or put that Root in the scene.");
                return;
            }

            _provider = new UmpConsentProvider();
            _consent.Plug(_provider);
        }

        public override void DestroyContext()
        {
            // Kept from Setup: the host's own context is destroyed before its sub-contexts.
            if (_provider != null)
                _consent?.Unplug(_provider);

            base.DestroyContext();
        }
    }
}
