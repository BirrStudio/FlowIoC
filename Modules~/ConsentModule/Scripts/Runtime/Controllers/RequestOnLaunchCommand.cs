using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.ConsentModule.Models;

namespace Modules.ConsentModule.Controllers
{
    /// <summary>
    /// Launch's one decision: ask now, or wait for the game's IConsentService.Request.
    /// Off in CD_Consent means the game's flow asks later, and the sequence stops here.
    /// </summary>
    internal class RequestOnLaunchCommand : Command
    {
        [Inject] private IConsentModel _model { get; set; }

        public override void Execute()
        {
            Retain();

            if (!_model.Settings.RequestOnLaunch)
            {
                FlowLogger.Log("Request - waiting for IConsentService.Request (CD_Consent.RequestOnLaunch is off).");
                Stop();
                return;
            }

            Release();
        }
    }
}
