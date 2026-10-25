using System;
using System.Collections.Generic;
using Modules.LocalSaveModule.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Modules.LocalSaveModule.Models
{
    /// <summary>
    /// What each persisted asset held before the save was read over it - the values it shipped
    /// with - so a reset can put them back in a build, where nothing else remembers them once the
    /// load has run.
    ///
    /// Not EditorAssetSnapshots, though in the Editor both are taken from the same state. That
    /// one is the Editor's, and its job is a clean asset after play: it reads the asset's file
    /// and carries Unity's whole serialization. This one runs everywhere and goes through the
    /// save's own serializer, so a reset puts back exactly the fields a load overwrites, in a
    /// build and in the Editor alike.
    /// </summary>
    public class AssetStartingValues
    {
        private readonly JsonSerializer _serializer = new LocalSaveSerializer().Serializer;

        private readonly Dictionary<ScriptableObject, JToken> _values = new();

        /// <summary>Records what each asset holds now, replacing anything captured before.</summary>
        public void Capture(IEnumerable<ScriptableObject> assets)
        {
            _values.Clear();

            foreach (ScriptableObject asset in assets)
            {
                if (asset == null)
                    continue;

                try
                {
                    _values[asset] = JToken.FromObject(asset, _serializer);
                }
                catch (Exception exception)
                {
                    Debug.LogError("[LocalSave] Capture failed for '" + asset.name
                                   + "', a reset will leave it as it is. " + exception);
                }
            }
        }

        /// <summary>Writes every captured state back over the asset it came from.</summary>
        public void Restore()
        {
            foreach (KeyValuePair<ScriptableObject, JToken> value in _values)
            {
                if (value.Key == null)
                    continue;

                try
                {
                    using (JsonReader reader = value.Value.CreateReader())
                        _serializer.Populate(reader, value.Key);
                }
                catch (Exception exception)
                {
                    Debug.LogError("[LocalSave] Reset failed for '" + value.Key.name + "'. " + exception);
                }
            }
        }
    }
}
