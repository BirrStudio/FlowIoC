#if UNITY_EDITOR
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.BaseModule.SharedData;
using FlowIoC.ConsoleModule;
using Modules.ConsentModule.Shared.Data.UnityObjects;
using Modules.ConsentModule.Shared.Data.ValueObjects;

namespace Modules.ConsentModule.ConsentTestModule.Controllers
{
    /// <summary>
    /// What a game's Connector would hand on, written as a line instead: the answer, the two
    /// readings an ad and an analytics SDK are given, and what RD_Consent now holds for a reader.
    /// </summary>
    internal class ReportConsentCommand : Command
    {
        [Inject] private ISharedDataModel _shared { get; set; }

        [SignalParam] private ConsentVO _consent { get; set; }

        public override void Execute()
        {
            RD_Consent published = _shared.GetScriptable<RD_Consent>();

            FlowLogger.Log(
                $"Resolved - {_consent} | ads personalised={_consent.CanPersonalise} tracking={_consent.CanTrack}"
                + $" | RD_Consent resolved={published != null && published.IsResolved}");
        }
    }
}
#endif
