using System;
using Modules.ConsentModule.Shared.Data.ValueObjects;
using UnityEngine;

namespace Modules.ConsentModule.Shared.Data.UnityObjects
{
    /// <summary>
    /// The resolved consent, for a module that reads it when it is ready - a settings screen
    /// deciding whether to show its privacy button. Filed in ConsentServiceRoot's Shared
    /// Scriptables; written every Play and never serialized, so the asset on disk stays empty.
    /// </summary>
    [CreateAssetMenu(fileName = "RD_Consent", menuName = "FlowIoC/ConsentModule/Data/RD_Consent")]
    public class RD_Consent : ScriptableObject
    {
        [NonSerialized] private bool _isResolved;
        [NonSerialized] private ConsentVO _current;

        /// <summary>False until the first answer; a module that needs it earlier listens to Outgoing.Resolved.</summary>
        public bool IsResolved => _isResolved;

        public ConsentVO Current => _current;

        /// <summary>Written by the consent module only.</summary>
        public void Write(ConsentVO consent)
        {
            _current = consent;
            _isResolved = true;
        }
    }
}
