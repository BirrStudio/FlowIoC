using System;
using FlowIoC.BaseModule.Attributes;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.ConsentModule.Models;
using Modules.ConsentModule.Services;

namespace Modules.ConsentModule.Controllers
{
    /// <summary>
    /// The privacy options form, from a settings button. Refused before consent is first resolved
    /// and while another form is waiting; past that the provider shows it and the new answer is
    /// published. A provider that throws keeps the answer it had.
    /// </summary>
    [LongRetain]
    internal class ShowPrivacyOptionsCommand : Command
    {
        [Inject] private IConsentModel _model { get; set; }

        public override void Execute()
        {
            Retain();

            if (!_model.IsResolved || _model.IsAsking || _model.Provider == null)
            {
                FlowLogger.LogWarning("PrivacyOptions - refused: consent has not been resolved yet, or a form is already up.");
                Stop();
                return;
            }

            IConsentModel model = _model;
            IConsentProvider provider = model.Provider;
            bool answered = false;
            model.SetAsking(true);

            try
            {
                provider.ShowPrivacyOptions(status =>
                {
                    if (answered) return;
                    answered = true;

                    FlowLogger.Log($"PrivacyOptions - consent {status} from '{provider.Name}'");
                    model.SetConsent(status);
                    Release();
                });
            }
            catch (Exception exception)
            {
                if (answered) return;
                answered = true;

                FlowLogger.LogError($"PrivacyOptions - '{provider.Name}' threw on ShowPrivacyOptions; the answer is kept: {exception.Message}");
                Release();
            }
        }
    }
}
