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
            /// Bound to the signal a settings list dispatches with a language code - a
            /// Signal&lt;string&gt; - and holds the sequence until the new texts are in:
            /// <c>CommandBinder.Bind(_signals.LanguagePicked).ToSequence&lt;ILocalizationService.Commands.SetLanguage&gt;()</c>.
            /// </summary>
            [LongRetain(30)]
            public class SetLanguage : Command
            {
                [SignalParam] private string _code { get; set; }

                [Inject] private ILocalizationService _localization { get; set; }

                public override void Execute()
                {
                    Retain();
                    _localization.SetLanguage(_code, () => Release());
                }
            }
        }
    }
}
