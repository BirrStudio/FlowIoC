#if UNITY_EDITOR

using FlowIoC.BaseModule.Signals;

namespace Modules.LocalizationModule.LocalizationTestModule.Signals
{
    /// <summary>The sample's own traffic: the buttons, and the line its Command fills.</summary>
    internal class LocalizationTestInternalSignals : ISignalHolder
    {
        public Signal Start = new();
        public Signal<string> LanguagePicked = new();
        public Signal LevelUp = new();
        public Signal<string> Refill = new();
        public Signal<string, string> Filled = new();
    }
}

#endif
