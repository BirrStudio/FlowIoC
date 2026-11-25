using FlowIoC.BaseModule.Attributes;
using FlowIoC.BaseModule.Contexts;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using FlowIoC.BaseModule.SharedData;
using FlowIoC.ConsoleModule;
using Modules.AdsModule.MockAdsModule.Data.UnityObjects;
using Modules.AdsModule.MockAdsModule.Services;
using Modules.AdsModule.Services;
#endif

namespace Modules.AdsModule.MockAdsModule.RootsContexts
{
    /// <summary>
    /// The mock plug, listed on AdsServiceRoot's Sub Context Types with AutoSetup on - the seat a
    /// real mediation's plug takes. Its Setup reads CD_MockAds off the shared data and plugs the
    /// provider, in the Editor and in a Development Build only: a release build compiles the
    /// context empty, plugs nothing, and every video button stays disabled. Moving to the real SDK
    /// is replacing this entry on the Root with that SDK's plug: two plugs listed together are
    /// refused with an error, whichever sets up second.
    /// </summary>
    [AllowAsSubContext]
    public class MockAdsServiceContext : Context
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private IAdsService _ads;
        private MockAdsProvider _provider;

        public override void InjectionBindings()
        {
            base.InjectionBindings();

            _provider = InjectionBinder.Bind<MockAdsProvider>();
        }

        public override void Setup()
        {
            base.Setup();

            _ads = InjectionBinderCrossContext.GetInstance<IAdsService>();

            if (_ads == null)
            {
                FlowLogger.LogError("Setup - IAdsService is not bound: list this context on AdsServiceRoot, or put that Root in the scene.");
                return;
            }

            // A missing filing is reported by the shared data model itself; the mock then runs on its class defaults.
            _provider.Configure(InjectionBinderCrossContext.GetInstance<ISharedDataModel>().GetScriptable<CD_MockAds>());
            _ads.Plug(_provider);
        }

        public override void DestroyContext()
        {
            // Kept from Setup: the host's own context is destroyed before its sub-contexts.
            if (_provider != null)
                _ads?.Unplug(_provider);

            base.DestroyContext();
        }
#endif
    }
}
