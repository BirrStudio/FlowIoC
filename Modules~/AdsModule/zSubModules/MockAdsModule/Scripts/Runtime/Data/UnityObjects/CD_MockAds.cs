using Modules.AdsModule.MockAdsModule.Enums;
using UnityEngine;

namespace Modules.AdsModule.MockAdsModule.Data.UnityObjects
{
    /// <summary>
    /// How the mock SDK behaves. Filed in AdsServiceRoot's Shared Scriptables beside the plug's
    /// entry, the way a real plug files its ad units. Change it during Play to try a dismissed or a
    /// failed rewarded ad, or loads that never land (the video buttons then stay disabled).
    /// </summary>
    [CreateAssetMenu(fileName = "CD_MockAds", menuName = "FlowIoC/AdsModule/MockAds/Data/CD_MockAds")]
    public class CD_MockAds : ScriptableObject
    {
        [Tooltip("How a rewarded show ends. Interstitials always show and close.")]
        public MockAdsOutcome RewardedOutcome = MockAdsOutcome.Rewarded;

        [Tooltip("Every load fails, so no ad is ever ready - what a player with no fill sees. The Ads module retries with its own backoff.")]
        public bool LoadsFail;

        [Tooltip("Seconds a load takes before the ad is ready.")]
        [Min(0f)] public float LoadSeconds = 0.5f;

        [Tooltip("Seconds an ad stays 'on screen' before it closes. The mock draws nothing.")]
        [Min(0f)] public float ShowSeconds = 1f;
    }
}
