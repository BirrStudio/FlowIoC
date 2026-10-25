using Newtonsoft.Json;

namespace Modules.LocalSaveModule.Services
{
    /// <summary>
    /// The one way this module turns an asset into JSON and back. The save file is written with it,
    /// and so is the copy of each asset's starting values a reset puts back - so a reset restores
    /// exactly what a load would have overwritten, no field more and no field less.
    /// </summary>
    internal class LocalSaveSerializer
    {
        public JsonSerializer Serializer { get; } = JsonSerializer.Create(new JsonSerializerSettings
        {
            ContractResolver = new UnityFieldContractResolver(),

            // A field declared as an interface or a base class writes what it actually holds,
            // which is what makes a [SerializeReference] field survive the round trip. The
            // binder decides what such a name is allowed to build.
            TypeNameHandling = TypeNameHandling.Auto,
            SerializationBinder = new LocalSaveTypeBinder(),

            // Loading overwrites, so a collection is replaced rather than added to. Without
            // this an entry the player deleted would come back on every load, for ever.
            ObjectCreationHandling = ObjectCreationHandling.Replace,

            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
    }
}
