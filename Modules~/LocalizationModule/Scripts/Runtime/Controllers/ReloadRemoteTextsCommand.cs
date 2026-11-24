using System;
using FlowIoC.BaseModule.Attributes;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.BaseModule.Provider.Update;
using FlowIoC.ConsoleModule;
using Modules.LocalizationModule.Constants;
using Modules.LocalizationModule.Entities;
using Modules.LocalizationModule.Models;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Modules.LocalizationModule.Controllers
{
    /// <summary>
    /// While boot waits, puts a download that just arrived on screen: the loaded tables are
    /// released and loaded again, so the postprocessor reads the new cache, and every LocalizedText
    /// asks for its text afresh. Outside the wait nothing is done - the next launch applies it.
    ///
    /// Unity Localization's own start is waited out first: a table released while it still runs
    /// would be loaded again by it without the new texts. Then the reload is waited the same way,
    /// read afresh every frame. Every way out closes the wait.
    /// </summary>
    [LongRetain(30)]
    internal class ReloadRemoteTextsCommand : Command
    {
        [Inject] private ILocalizationModel _model { get; set; }
        [Inject] private IUpdateProvider _updateProvider { get; set; }

        private IUpdateProvider _frames;
        private ILocalizationModel _waitModel;
        private AsyncOperationHandle _reload;
        private bool _isReloading;

        public override void Execute()
        {
            if (!_model.IsBootWaitOpen)
                return;

            if (!_model.HasNewRemoteTexts || _model.Remote == null)
            {
                _model.CloseBootWait();
                return;
            }

            Retain();
            _frames = _updateProvider;
            _waitModel = _model;
            _isReloading = false;
            _reload = default;
            _frames.AddUpdate(Wait);
            Wait();
        }

        private void Wait()
        {
            try
            {
                if (!_isReloading)
                {
                    if (!LocalizationSettings.InitializationOperation.IsDone)
                        return;

                    // Boot stopped waiting meanwhile: the screens are up, so the next launch applies it.
                    if (!_waitModel.IsBootWaitOpen)
                    {
                        Finish();
                        return;
                    }

                    _waitModel.Remote.Forget();
                    LocalizationSettings.StringDatabase.ResetState();
                    _reload = LocalizationSettings.StringDatabase.PreloadTables(LocalizationConstants.TABLE);
                    _isReloading = true;
                }

                if (!_reload.IsDone)
                    return;

                if (_reload.Status == AsyncOperationStatus.Succeeded)
                {
                    foreach (LocalizedText text in UnityEngine.Object.FindObjectsByType<LocalizedText>(FindObjectsSortMode.None))
                        text.Text.RefreshString();

                    FlowLogger.Log("ReloadRemoteTextsCommand - the sheet's texts are on screen");
                }
                else
                {
                    FlowLogger.LogWarning("ReloadRemoteTextsCommand - the table did not load again; the next launch applies the sheet");
                }
            }
            catch (Exception e)
            {
                FlowLogger.LogWarning("ReloadRemoteTextsCommand - the sheet's texts could not be put on screen: " + e.Message);
            }

            Finish();
        }

        private void Finish()
        {
            _frames.RemoveUpdate(Wait);
            _waitModel.CloseBootWait();
            Release();
        }
    }
}
