using Modules.LocalizationModule.Data.UnityObjects;
using Modules.LocalizationModule.Remote;

namespace Modules.LocalizationModule.Models
{
    /// <summary>
    /// Whether the module is ready, and where phones read corrected texts from. Everything else -
    /// the languages, the selected one, the texts - is Unity Localization's state, read where it
    /// is kept rather than copied here.
    /// </summary>
    public interface ILocalizationModel
    {
        bool IsReady { get; }

        void SetReady();

        /// <summary>The settings asset's postprocessor; null when the project was set up before phones could read the sheet.</summary>
        internal RemoteTextsPostprocessor Remote { get; }

        /// <summary>The postprocessor's settings; null means phones do not read the sheet.</summary>
        CD_LocalizationRemote RemoteConfig { get; }

        internal RemoteTextsCache RemoteCache { get; }

        /// <summary>True while boot waits for a download; the step that applies one closes it.</summary>
        bool IsBootWaitOpen { get; }

        /// <summary>True when the last download wrote texts the loaded tables do not have yet.</summary>
        bool HasNewRemoteTexts { get; }

        void OpenBootWait();

        void CloseBootWait();

        void SetNewRemoteTexts(bool value);
    }
}
