using FlowIoC.BaseModule.Attributes;
using FlowIoC.BaseModule.Root;

namespace Modules.DeviceDebuggerModule.RootsContexts
{
    /// <summary>
    /// The ring outlives a scene load, so the error that happened during one can still be read.
    /// </summary>
    [PersistentRoot]
    public class DeviceDebuggerServiceRoot : Root<DeviceDebuggerServiceContext>
    {
    }
}
