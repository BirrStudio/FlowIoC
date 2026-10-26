using FlowIoC.BaseModule.Attributes;
using FlowIoC.BaseModule.Root;

namespace FlowIoC.AssetModule.RootsContexts
{
    /// <summary>
    /// The asset service holds the handles of every loaded Addressable group, so it has to
    /// outlive the scene that first asked for one.
    /// </summary>
    [PersistentRoot]
    internal class AssetServiceRoot : Root<AssetServiceContext>
    {
    }
}
