using Modules.ConsentModule.Data.ValueObjects;
using UnityEngine;

namespace Modules.ConsentModule.Data.UnityObjects
{
    /// <summary>
    /// The module's settings, filed on ConsentServiceRoot's adapter. The shipped asset carries the
    /// defaults; a game edits it in place.
    /// </summary>
    [CreateAssetMenu(fileName = "CD_Consent", menuName = "FlowIoC/ConsentModule/Data/CD_Consent")]
    public class CD_Consent : ScriptableObject
    {
        public ConsentSettingsCVO Settings = new();
    }
}
