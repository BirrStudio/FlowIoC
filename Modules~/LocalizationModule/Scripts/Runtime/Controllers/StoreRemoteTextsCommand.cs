using System;
using System.Collections.Generic;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.LocalizationModule.Data.ValueObjects;
using Modules.LocalizationModule.Models;
using Modules.LocalizationModule.Remote;

namespace Modules.LocalizationModule.Controllers
{
    /// <summary>
    /// Keeps what the sheet answered when it passes the download checks: no Status column, or no
    /// Key header, and nothing is kept. A failure is a warning, not an error - a phone offline is
    /// not a fault - and the step still ends, so a boot that waits learns at once that nothing came.
    /// </summary>
    internal class StoreRemoteTextsCommand : Command
    {
        [Inject] private ILocalizationModel _model { get; set; }

        [SignalParam] private RemoteDownloadVO _download { get; set; }

        private readonly RemoteTextsValidator _validator = new();

        public override void Execute()
        {
            _model.SetNewRemoteTexts(false);

            if (!_download.Succeeded)
            {
                FlowLogger.LogWarning("StoreRemoteTextsCommand - " + _download.Error);
                return;
            }

            if (!_download.Changed)
                return;

            // The two tabs are published apart and may lag each other: keep only a table that says the
            // same revision the Version tab did, or a stale table would be cached under the new number.
            string tableRevision = _validator.TableRevision(_download.Rows);

            if (tableRevision != _download.Revision)
            {
                FlowLogger.LogWarning("StoreRemoteTextsCommand - the published sheet is still catching up (Version says "
                    + _download.Revision + ", the Game tab " + (tableRevision ?? "nothing") + "); the next launch looks again");

                // Not counted as a look, so a Daily or Weekly phone does not wait a day for a lag of minutes.
                try
                {
                    _model.RemoteCache.Touch(0);
                }
                catch (Exception)
                {
                    // The cache keeps its time; the phone looks again when that runs out.
                }

                return;
            }

            if (!_validator.TryReadDownload(_download.Rows, out List<RemoteTextVO> texts, out string reason))
            {
                FlowLogger.LogWarning("StoreRemoteTextsCommand - the sheet was not applied: " + reason);
                return;
            }

            try
            {
                _model.RemoteCache.Write(new RemoteTextsCacheVO
                {
                    Revision = _download.Revision,
                    CheckedAtUtcTicks = DateTime.UtcNow.Ticks,
                    Texts = texts
                });
            }
            catch (Exception e)
            {
                FlowLogger.LogWarning("StoreRemoteTextsCommand - the cache could not be written: " + e.Message);
                return;
            }

            _model.SetNewRemoteTexts(true);
            FlowLogger.Log("StoreRemoteTextsCommand - revision " + _download.Revision + ", " + texts.Count + " texts");
        }
    }
}
