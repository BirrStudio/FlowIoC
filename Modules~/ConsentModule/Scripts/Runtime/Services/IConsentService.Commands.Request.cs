using FlowIoC.BaseModule.Attributes;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;

namespace Modules.ConsentModule.Services
{
    public partial interface IConsentService
    {
        public static partial class Commands
        {
            /// <summary>
            /// Asks for consent where the game's flow wants it. Only needed when
            /// CD_Consent.RequestOnLaunch is off: <c>.ToSequence&lt;IConsentService.Commands.Request&gt;()</c>.
            /// </summary>
            [DebugOption("Consent", "Request")]
            public class Request : Command
            {
                [Inject] private IConsentService _consent { get; set; }

                public override void Execute() => _consent.Request();
            }
        }
    }
}
