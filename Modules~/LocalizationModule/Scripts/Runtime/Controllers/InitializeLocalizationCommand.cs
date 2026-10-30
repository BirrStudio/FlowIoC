using System;
using FlowIoC.BaseModule.Attributes;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.BaseModule.Provider.Update;
using FlowIoC.ConsoleModule;
using Modules.LocalizationModule.Models;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Modules.LocalizationModule.Controllers
{
    /// <summary>
    /// Waits for Unity Localization's own start: the startup selectors pick the language (the
    /// saved choice, then the device's, then English) and the preloaded Game table is read in.
    ///
    /// The wait reads the start operation afresh every frame rather than hanging on its Completed:
    /// a language picked while the start is still running releases that operation and begins
    /// another, and a callback on the released one never comes. The three ways out: ready; the
    /// start failed, logged and released so the game still boots with keys in place of texts; the
    /// start threw, the same.
    /// </summary>
    [LongRetain(30)]
    internal class InitializeLocalizationCommand : Command
    {
        [Inject] private ILocalizationModel _model { get; set; }
        [Inject] private IUpdateProvider _updateProvider { get; set; }

        [SignalParam] private Action _done { get; set; }

        private IUpdateProvider _frames;

        public override void Execute()
        {
            Retain();
            _frames = _updateProvider;
            _frames.AddUpdate(Wait);
            Wait();
        }

        private void Wait()
        {
            AsyncOperationHandle<LocalizationSettings> start;

            try
            {
                start = LocalizationSettings.InitializationOperation;
            }
            catch (Exception e)
            {
                Finish("Initialize - Unity Localization could not start: " + e.Message);
                return;
            }

            if (!start.IsDone)
                return;

            if (start.Status != AsyncOperationStatus.Succeeded)
            {
                Finish("Initialize - Unity Localization failed to start: " + start.OperationException?.Message);
                return;
            }

            _model.SetReady();
            FlowLogger.Log("Initialize - " + LocalizationSettings.SelectedLocale?.Identifier.Code);
            Finish(null);
        }

        private void Finish(string error)
        {
            _frames.RemoveUpdate(Wait);

            if (error != null)
                FlowLogger.LogError(error);

            Action done = _done;
            Release();
            done?.Invoke();
        }
    }
}
