using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.ConsentModule.Models;
using Modules.ConsentModule.Services;

namespace Modules.ConsentModule.Controllers
{
    /// <summary>
    /// Nothing plugged by the time consent is asked: the Editor and a Development Build plug the
    /// stand-in that answers from CD_Consent, with a warning; a release build reports the missing
    /// plug as an error and falls back to denied, so no store build uses personal data unasked.
    /// </summary>
    internal class EnsureProviderCommand : Command
    {
        [Inject] private IConsentModel _model { get; set; }

        public override void Execute()
        {
            if (_model.Provider != null)
                return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            FlowLogger.LogWarning(
                $"Request - no consent provider plugged; the development stand-in answers {_model.Settings.DevelopmentConsent} from CD_Consent. A release build answers Denied instead.");
            _model.Plug(new DevelopmentConsentProvider(_model.Settings));
#else
            FlowLogger.LogError(
                "Request - no consent provider plugged on ConsentServiceRoot; consent is resolved as Denied and ads run non-personalised.");
            _model.Plug(new DeniedConsentProvider());
#endif
        }
    }
}
