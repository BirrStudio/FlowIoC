#if UNITY_EDITOR

using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.BaseModule.ViewsMediators.Mediator;
using Modules.LocalizationModule.LocalizationTestModule.Signals;

namespace Modules.LocalizationModule.LocalizationTestModule.ViewsMediators
{
    public class LocalizationTestMediator : IMediator
    {
        [Inject] private LocalizationTestView _view { get; set; }

        [InjectSignal] private LocalizationTestInternalSignals _signals { get; set; }

        public void OnRegister()
        {
            _view.OnLanguagePressed += PickLanguage;
            _view.OnLevelUpPressed += LevelUp;
            _signals.Filled.AddListener(_view.SetLine);
        }

        public void OnRemove()
        {
            _view.OnLanguagePressed -= PickLanguage;
            _view.OnLevelUpPressed -= LevelUp;
            _signals.Filled.RemoveListener(_view.SetLine);
        }

        private void PickLanguage(string code) => _signals.LanguagePicked.Dispatch(code);

        private void LevelUp() => _signals.LevelUp.Dispatch();
    }
}

#endif
