namespace Modules.LocalizationModule.Models
{
    /// <summary>
    /// Whether the module is ready. Everything else - the languages, the selected one, the texts -
    /// is Unity Localization's state, read where it is kept rather than copied here.
    /// </summary>
    public interface ILocalizationModel
    {
        bool IsReady { get; }

        void SetReady();
    }
}
