#if UNITY_EDITOR

using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.LocalizationModule.LocalizationTestModule.Models;
using Modules.LocalizationModule.LocalizationTestModule.Signals;
using Modules.LocalizationModule.Services;

namespace Modules.LocalizationModule.LocalizationTestModule.Controllers
{
    /// <summary>
    /// What a game's screen-opening Command does for a text that carries a value: reads it in the
    /// selected language with the value filled, and hands it to the view.
    /// </summary>
    internal class FillSampleCommand : Command
    {
        public const string LEVELS_KEY = "sample.levels";

        [Inject] private ILocalizationService _localization { get; set; }
        [Inject] private LocalizationTestModel _model { get; set; }
        [InjectSignal] private LocalizationTestInternalSignals _signals { get; set; }

        public override void Execute()
        {
            string line = _localization.Get(LEVELS_KEY, _model.Levels);
            _signals.Filled.Dispatch(line, _localization.Language);
        }
    }
}

#endif
