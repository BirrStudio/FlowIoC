using System;
using FlowIoC.BaseModule.Attributes;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.BaseModule.Provider.Update;
using FlowIoC.ConsoleModule;
using Modules.LocalizationModule.Signals;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Modules.LocalizationModule.Controllers
{
    /// <summary>
    /// Selects the language and waits for its table, then announces it. Unity's
    /// PlayerPrefLocaleSelector writes the choice as the locale changes, so the next start opens in
    /// it. An unknown code and the language already selected change nothing and announce nothing.
    ///
    /// The wait reads the load operation afresh every frame: a second pick before the first has
    /// loaded releases the first operation, and a Completed callback on it would never come. What
    /// is announced is the locale Unity selected - asked for pt-BR, a project with only pt answers pt.
    /// </summary>
    [LongRetain(30)]
    internal class ApplyLanguageCommand : Command
    {
        [InjectSignal] private LocalizationSignals _signals { get; set; }
        [Inject] private IUpdateProvider _updateProvider { get; set; }

        [SignalParam] private string _code { get; set; }
        [SignalParam] private Action _done { get; set; }

        private IUpdateProvider _frames;
        private string _selected;

        public override void Execute()
        {
            Retain();
            _frames = _updateProvider;

            try
            {
                Locale locale = LocalizationSettings.AvailableLocales.GetLocale(_code);

                if (locale == null)
                {
                    FlowLogger.LogWarning("SetLanguage - the project has no language " + _code);
                    Finish(null);
                    return;
                }

                if (locale == LocalizationSettings.SelectedLocale)
                {
                    Finish(null);
                    return;
                }

                _selected = locale.Identifier.Code;
                LocalizationSettings.SelectedLocale = locale;
            }
            catch (Exception e)
            {
                FlowLogger.LogError("SetLanguage - " + _code + " could not be selected: " + e.Message);
                Finish(null);
                return;
            }

            _frames.AddUpdate(Wait);
            Wait();
        }

        private void Wait()
        {
            AsyncOperationHandle<LocalizationSettings> load = LocalizationSettings.InitializationOperation;

            if (!load.IsDone)
                return;

            _frames.RemoveUpdate(Wait);

            if (load.Status != AsyncOperationStatus.Succeeded)
            {
                FlowLogger.LogError("SetLanguage - the table for " + _selected + " failed to load: " + load.OperationException?.Message);
                Finish(null);
                return;
            }

            FlowLogger.Log("SetLanguage - " + _selected);
            Finish(_selected);
        }

        private void Finish(string changedTo)
        {
            Action done = _done;
            LocalizationSignals signals = _signals;
            Release();

            if (changedTo != null)
                signals.Outgoing.LanguageChanged.Dispatch(changedTo);

            done?.Invoke();
        }
    }
}
