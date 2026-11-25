namespace Modules.ConsentModule.Services
{
    /// <summary>
    /// A consent provider whose form is an IAB TCF form (Google UMP). After each answer the module
    /// reads the purposes the player consented to, so the published ConsentVO can map them to each
    /// SDK's flags.
    /// </summary>
    public interface ITcfConsentProvider : IConsentProvider
    {
        /// <summary>IABTCF_PurposeConsents as the form wrote it, or empty when it wrote none.</summary>
        string ReadPurposeConsents();
    }
}
