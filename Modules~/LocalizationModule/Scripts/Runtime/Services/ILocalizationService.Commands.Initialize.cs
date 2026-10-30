using FlowIoC.BaseModule.Attributes;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;

namespace Modules.LocalizationModule.Services
{
    public partial interface ILocalizationService
    {
        public static partial class Commands
        {
            /// <summary>
            /// Holds the sequence until the texts can be read, so the screen the next step opens
            /// fills in the player's language: bind it in the boot before the first screen,
            /// <c>.ToSequence&lt;ILocalizationService.Commands.Initialize&gt;()</c>.
            /// </summary>
            [LongRetain(30)]
            public class Initialize : Command
            {
                [Inject] private ILocalizationService _localization { get; set; }

                public override void Execute()
                {
                    Retain();
                    _localization.Initialize(() => Release());
                }
            }
        }
    }
}
