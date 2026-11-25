#if UNITY_EDITOR
using FlowIoC.BaseModule.Signals;
using Modules.ConsentModule.Shared.Data.ValueObjects;

namespace Modules.ConsentModule.ConsentTestModule.Signals
{
    internal class ConsentTestInternalSignals : ISignalHolder
    {
        public Signal<ConsentVO> Resolved = new();
    }
}
#endif
