namespace Modules.ConsentModule.Shared.Enums
{
    /// <summary>What the consent form answered. Unity serializes the number, so each value keeps its own.</summary>
    public enum ConsentStatus
    {
        /// <summary>Not asked yet, or the provider could not tell.</summary>
        Unknown = 0,

        /// <summary>The player agreed to personalised ads and data use.</summary>
        Granted = 1,

        /// <summary>The player refused, or nothing could ask: ads stay non-personalised.</summary>
        Denied = 2,

        /// <summary>No consent is required where the player is; personalised ads may run.</summary>
        NotRequired = 3,

        /// <summary>
        /// The player answered an IAB TCF form (Google UMP): what they chose, purpose by purpose, is
        /// in the TCF string the ad SDKs read themselves, so no flag is handed to them.
        /// </summary>
        Recorded = 4
    }
}
