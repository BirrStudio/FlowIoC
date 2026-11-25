#if UNITY_EDITOR || DEVELOPMENT_BUILD

using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.BaseModule.Provider.Coroutine;
using FlowIoC.ConsoleModule;
using Modules.AdsModule.MockAdsModule.Data.UnityObjects;
using Modules.AdsModule.MockAdsModule.Enums;
using Modules.AdsModule.Data.ValueObjects;
using Modules.AdsModule.Services;
using Modules.AdsModule.Shared.Enums;

namespace Modules.AdsModule.MockAdsModule.Services
{
    /// <summary>
    /// A stand-in mediation SDK. It draws nothing: it initializes at once, a load lands after
    /// CD_MockAds's LoadSeconds, and a show reports Displayed, waits ShowSeconds and ends the way
    /// the asset says. It only reports to the listener, like a real plug; every decision - pacing,
    /// retry, timeout, whether the reward is granted - stays in the Ads module. Compiled only in
    /// the Editor and in a Development Build, so a release build has no plug at all.
    /// </summary>
    public class MockAdsProvider : IAdsProvider
    {
        private const float INITIALIZE_SECONDS = 0.1f;

        [Inject] private ICoroutineProvider _coroutines { get; set; }

        private readonly bool[] _loaded = new bool[2];
        private CD_MockAds _settings;
        private IAdsProviderListener _listener;

        public string Name => "Mock";

        /// <summary>The asset the plug's context found filed; a missing one leaves the class defaults.</summary>
        public void Configure(CD_MockAds settings) => _settings = settings;

        public void Initialize(IAdsProviderListener listener)
        {
            _listener = listener;

            if (_settings == null)
                _settings = UnityEngine.ScriptableObject.CreateInstance<CD_MockAds>();

            _coroutines.WaitForSecondsRealTime(INITIALIZE_SECONDS, () => _listener.OnInitialized(true));
        }

        public void Load(AdFormat format)
        {
            _coroutines.WaitForSecondsRealTime(_settings.LoadSeconds, () =>
            {
                if (_settings.LoadsFail)
                {
                    _listener.OnLoadFailed(format, "mock: LoadsFail is on in CD_MockAds");
                    return;
                }

                _loaded[(int) format] = true;
                _listener.OnLoaded(format);
            });
        }

        public bool IsReady(AdFormat format) => _loaded[(int) format];

        public void Show(AdFormat format, string placement)
        {
            _loaded[(int) format] = false;

            if (format == AdFormat.Rewarded && _settings.RewardedOutcome == MockAdsOutcome.DisplayFailed)
            {
                _listener.OnDisplayFailed(format, "mock: RewardedOutcome is DisplayFailed in CD_MockAds");
                return;
            }

            FlowLogger.Log($"Show - mock {format} '{placement}' on screen for {_settings.ShowSeconds} s");
            _listener.OnDisplayed(format);

            _coroutines.WaitForSecondsRealTime(_settings.ShowSeconds, () =>
            {
                // The order the listener asks for: the reward before the close.
                if (format == AdFormat.Rewarded && _settings.RewardedOutcome == MockAdsOutcome.Rewarded)
                    _listener.OnRewardEarned(new AdRewardVO("mock", 1));

                _listener.OnClosed(format);
            });
        }

        public void SetConsent(AdsConsentVO consent) => FlowLogger.Log($"SetConsent - mock {consent}");

        public void SetMuted(bool muted) => FlowLogger.Log($"SetMuted - mock {muted}");

        public void ShowDebugger() => FlowLogger.Log("ShowDebugger - the mock has no debugger");
    }
}

#endif
