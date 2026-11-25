using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.ConsentModule.Models;
using Modules.ConsentModule.Services;
#if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
using Modules.ConsentModule.Shared.Enums;
#endif

namespace Modules.ConsentModule.Controllers
{
    /// <summary>
    /// A socket still empty when consent is asked: the Editor and a Development Build plug the
    /// stand-in that answers from CD_Consent, with a warning; a release build reports the missing
    /// plug as an error and falls back to CD_Consent.ReleaseConsentWithoutPlug - Denied unless the
    /// developer chose otherwise, so no store build uses personal data unasked by default.
    /// A missing tracking plug is reported only on iOS, the one platform with a prompt.
    /// </summary>
    internal class EnsureProviderCommand : Command
    {
        [Inject] private IConsentModel _model { get; set; }

        public override void Execute()
        {
            if (_model.Provider == null)
                EnsureConsent();

            if (_model.TrackingProvider == null)
                EnsureTracking();
        }

        private void EnsureConsent()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            FlowLogger.LogWarning(
                $"Request - no consent provider plugged; the development stand-in answers {_model.Settings.DevelopmentConsent} from CD_Consent. A release build answers Denied instead.");
            _model.Plug(new DevelopmentConsentProvider(_model.Settings));
#else
            ConsentStatus answer = _model.Settings.ReleaseConsentWithoutPlug;
            FlowLogger.LogError(
                $"Request - no consent provider plugged on ConsentServiceRoot; consent is resolved as {answer} from CD_Consent.ReleaseConsentWithoutPlug.");
            _model.Plug(new FallbackConsentProvider(answer));
#endif
        }

        private void EnsureTracking()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _model.PlugTracking(new DevelopmentConsentProvider(_model.Settings));
#else
#if UNITY_IOS
            FlowLogger.LogError(
                "Request - no tracking provider plugged on ConsentServiceRoot; on iOS tracking is resolved as Denied and the ATT prompt is never shown.");
#endif
            _model.PlugTracking(new FallbackConsentProvider(_model.Settings.ReleaseConsentWithoutPlug));
#endif
        }
    }
}
