using System;
using System.Collections;
using System.Collections.Generic;
using FlowIoC.BaseModule.Function.AsyncFunctions;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.LocalizationModule.Data.UnityObjects;
using Modules.LocalizationModule.Data.ValueObjects;
using Modules.LocalizationModule.Models;
using Modules.LocalizationModule.Remote;
using UnityEngine.Networking;

namespace Modules.LocalizationModule.Controllers
{
    /// <summary>
    /// One look at the published sheet: the revision first, a few bytes, and the Game tab only
    /// when the revision differs from the cache's. It decides nothing about the texts; the step it
    /// reports to does.
    /// </summary>
    internal class DownloadRemoteTextsFunction : AsyncFunction<RemoteDownloadVO>
    {
        private const int TIMEOUT_SECONDS = 10;

        [Inject] private ILocalizationModel _model { get; set; }

        public override IEnumerator Execute()
        {
            CD_LocalizationRemote config = _model.RemoteConfig;
            RemoteTextsCacheVO cache = _model.RemoteCache.Read();
            var result = new RemoteDownloadVO();
            var csv = new RemoteCsv();

            UnityWebRequest revisionRequest = Request(config.RevisionUrl, result);

            if (revisionRequest == null)
                yield break;

            using (UnityWebRequest revision = revisionRequest)
            {
                revision.timeout = TIMEOUT_SECONDS;
                yield return revision.SendWebRequest();

                if (revision.result != UnityWebRequest.Result.Success)
                {
                    Finish(result, "the revision did not download: " + revision.error);
                    yield break;
                }

                List<List<string>> cells = csv.Parse(revision.downloadHandler.text);
                string number = cells.Count > 0 && cells[0].Count > 0 ? cells[0][0].Trim() : string.Empty;

                if (!int.TryParse(number, out _))
                {
                    Finish(result, "the revision is not a number: " + number);
                    yield break;
                }

                result.Revision = number;
            }

            if (cache != null && cache.Revision == result.Revision)
            {
                result.Succeeded = true;
                Finish(result, null);
                yield break;
            }

            UnityWebRequest tableRequest = Request(config.TableUrl, result);

            if (tableRequest == null)
                yield break;

            using (UnityWebRequest table = tableRequest)
            {
                table.timeout = TIMEOUT_SECONDS;
                yield return table.SendWebRequest();

                if (table.result != UnityWebRequest.Result.Success)
                {
                    Finish(result, "the sheet did not download: " + table.error);
                    yield break;
                }

                result.Rows = csv.Parse(table.downloadHandler.text);
            }

            result.Succeeded = true;
            result.Changed = true;
            Finish(result, null);
        }

        /// <summary>A request for the address, or null - reported - when the address does not parse.</summary>
        private UnityWebRequest Request(string url, RemoteDownloadVO result)
        {
            try
            {
                return UnityWebRequest.Get(url);
            }
            catch (Exception e)
            {
                Finish(result, "the address " + url + " is not one a phone can read: " + e.Message);
                return null;
            }
        }

        private void Finish(RemoteDownloadVO result, string error)
        {
            if (error != null)
                result.Error = error;

            FunctionCompletedCallback?.Invoke(result);
        }
    }
}
