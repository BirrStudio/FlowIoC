using FlowIoC.BaseModule.Signals;

namespace Modules.LocalizationModule.Signals
{
    public class LocalizationSignals : ISignalHolder
    {
        public LocalizationSignalsIncoming Incoming = new();
        public LocalizationSignalsOutgoing Outgoing = new();
    }

    public class LocalizationSignalsIncoming
    {
    }

    public class LocalizationSignalsOutgoing
    {
        /// <summary>
        /// The new language's code, once its table is loaded. A game's Connector wires it to an
        /// open screen that shows variable text, so the Command that filled the screen fills it
        /// again. A LocalizedText refreshes itself and needs nothing.
        /// </summary>
        public Signal<string> LanguageChanged = new();
    }
}
