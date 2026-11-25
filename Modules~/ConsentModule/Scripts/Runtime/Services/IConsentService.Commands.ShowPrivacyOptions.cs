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
            /// Shows the privacy options form - the step a settings screen's privacy button binds:
            /// <c>.ToSequence&lt;IConsentService.Commands.ShowPrivacyOptions&gt;()</c>. The new answer
            /// goes out on Outgoing.Resolved again.
            /// </summary>
            [DebugOption("Consent", "Privacy options")]
            public class ShowPrivacyOptions : Command
            {
                [Inject] private IConsentService _consent { get; set; }

                public override void Execute() => _consent.ShowPrivacyOptions();
            }
        }
    }
}
