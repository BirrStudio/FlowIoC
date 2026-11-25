namespace Modules.ConsentModule.Shared.Enums
{
    /// <summary>What the tracking prompt (ATT on iOS) answered. Each value keeps its number.</summary>
    public enum TrackingStatus
    {
        /// <summary>The platform has no tracking prompt; tracking follows consent.</summary>
        NotApplicable = 0,

        /// <summary>The prompt has not been answered.</summary>
        NotDetermined = 1,

        Authorized = 2,

        Denied = 3,

        /// <summary>The device does not allow the prompt (parental controls, a managed device).</summary>
        Restricted = 4
    }
}
