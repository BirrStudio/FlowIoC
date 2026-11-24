using Modules.LocalizationModule.Enums;
using UnityEngine;

namespace Modules.LocalizationModule.Data.UnityObjects
{
    /// <summary>
    /// Whether and how phones read corrected texts from the published sheet. Set up makes it in
    /// Assets/Localization, beside the tables it corrects, and the settings asset's postprocessor
    /// holds it, so it ships with the build. The two addresses are written by the panel's Link.
    /// </summary>
    public class CD_LocalizationRemote : ScriptableObject
    {
        /// <summary>The Game tab as published CSV. Empty turns the feature off.</summary>
        public string TableUrl = string.Empty;

        /// <summary>The revision number as published CSV, read before the table. Empty turns the feature off.</summary>
        public string RevisionUrl = string.Empty;

        public RemoteFrequency Frequency = RemoteFrequency.Never;

        /// <summary>
        /// Off: a download is applied on the next launch and boot never waits. On: boot waits up
        /// to <see cref="WaitSeconds"/> and a download that arrives in time is applied at once.
        /// </summary>
        public bool WaitAtBoot;

        [Range(0f, 20f)] public float WaitSeconds = 3f;
    }
}
