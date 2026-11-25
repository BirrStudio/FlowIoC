using FlowIoC.BaseModule.Attributes;
using FlowIoC.BaseModule.Contexts;
using FlowIoC.ConsoleModule;
using Modules.ConsentModule.AttTrackingModule.Services;
using Modules.ConsentModule.Services;

namespace Modules.ConsentModule.AttTrackingModule.RootsContexts
{
    /// <summary>
    /// The ATT plug. Listed on ConsentServiceRoot's Sub Context Types beside the consent form's plug;
    /// its Setup gets the service and plugs the tracking provider. The Root beside it is for a game
    /// that prefers the plug on a Root of its own.
    /// </summary>
    [AllowAsSubContext]
    public class AttTrackingServiceContext : Context
    {
        private IConsentService _consent;
        private AttTrackingProvider _provider;

        public override void Setup()
        {
            base.Setup();

            _consent = InjectionBinderCrossContext.GetInstance<IConsentService>();

            if (_consent == null)
            {
                FlowLogger.LogError("Setup - IConsentService is not bound: list this context on ConsentServiceRoot, or put that Root in the scene.");
                return;
            }

            _provider = new AttTrackingProvider();
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
