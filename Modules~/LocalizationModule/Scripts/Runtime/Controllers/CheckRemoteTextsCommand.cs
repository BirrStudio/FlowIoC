using System;
using FlowIoC.BaseModule.Attributes;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Function.Provider;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.BaseModule.Provider.Update;
using FlowIoC.ConsoleModule;
using Modules.LocalizationModule.Data.UnityObjects;
using Modules.LocalizationModule.Data.ValueObjects;
using Modules.LocalizationModule.Models;
using Modules.LocalizationModule.Remote;
using Modules.LocalizationModule.Signals;
using UnityEngine;

namespace Modules.LocalizationModule.Controllers
{
    /// <summary>
    /// Whether this launch looks at the published sheet, and whether boot waits for the answer.
    /// The look counts before it is made, so a phone offline is not retried on every launch of the
    /// day. Without the wait the step ends at once and the download reaches the cache for the next
    /// launch; with it the step holds until the arrival steps close the wait or the time runs out.
    /// </summary>
    [LongRetain(30)]
    internal class CheckRemoteTextsCommand : Command
    {
        [Inject] private ILocalizationModel _model { get; set; }
        [Inject] private IFunctionProvider _functionProvider { get; set; }
        [Inject] private IUpdateProvider _updateProvider { get; set; }
        [InjectSignal] private LocalizationInternalSignals _internalSignals { get; set; }

        private readonly RemoteCheckSchedule _schedule = new();

        private IUpdateProvider _frames;
        private ILocalizationModel _waitModel;
        private float _deadline;

        public override void Execute()
        {
            CD_LocalizationRemote config = _model.RemoteConfig;

            if (_model.Remote == null || config == null || string.IsNullOrEmpty(config.TableUrl) || string.IsNullOrEmpty(config.RevisionUrl))
                return;

            RemoteTextsCacheVO cache = _model.RemoteCache.Read();
            long now = DateTime.UtcNow.Ticks;

            if (!_schedule.IsDue(config.Frequency, cache?.CheckedAtUtcTicks, now))
                return;

            try
            {
                _model.RemoteCache.Touch(now);
            }
            catch (Exception e)
            {
                FlowLogger.LogWarning("CheckRemoteTextsCommand - the cache could not be written, so the sheet is not read: " + e.Message);
                return;
            }

            LocalizationInternalSignals signals = _internalSignals;

            if (config.WaitAtBoot)
            {
                Retain();
                _model.OpenBootWait();
            }

            _functionProvider
                .CallAsync<DownloadRemoteTextsFunction, RemoteDownloadVO>()
                .AddFunctionCompletedCallback(result => signals.RemoteTextsArrived.Dispatch(result))
                .ExecuteAsync();

            if (!config.WaitAtBoot)
                return;

            _frames = _updateProvider;
            _waitModel = _model;
            _deadline = Time.realtimeSinceStartup + Mathf.Clamp(config.WaitSeconds, 0f, 20f);
            _frames.AddUpdate(Wait);
        }

        private void Wait()
        {
            if (_waitModel.IsBootWaitOpen && Time.realtimeSinceStartup < _deadline)
                return;

            _frames.RemoveUpdate(Wait);

            if (_waitModel.IsBootWaitOpen)
            {
                // Out of time: what arrives later is cached for the next launch.
                _waitModel.CloseBootWait();
                FlowLogger.Log("CheckRemoteTextsCommand - the sheet did not answer in time; the next launch applies it");
            }

            Release();
        }
    }
}
