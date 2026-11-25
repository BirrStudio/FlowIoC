namespace Modules.AdsModule.MockAdsModule.Enums
{
    /// <summary>How the mock answers a rewarded show. Unity serializes the number, so each value keeps its own.</summary>
    public enum MockAdsOutcome
    {
        /// <summary>The ad is watched to the end and the reward is earned.</summary>
        Rewarded = 0,

        /// <summary>The player closes the ad before the reward.</summary>
        Dismissed = 1,

        /// <summary>The SDK cannot show the loaded ad.</summary>
        DisplayFailed = 2
    }
}
