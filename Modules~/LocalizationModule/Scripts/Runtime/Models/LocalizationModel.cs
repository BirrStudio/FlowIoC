using Modules.LocalizationModule.Data.UnityObjects;
using Modules.LocalizationModule.Remote;
using UnityEngine;

namespace Modules.LocalizationModule.Models
{
    public class LocalizationModel : ILocalizationModel
    {
        private RemoteTextsCache _remoteCache;

        public bool IsReady { get; private set; }

        public void SetReady() => IsReady = true;

        RemoteTextsPostprocessor ILocalizationModel.Remote => RemoteTextsPostprocessor.Current();

        public CD_LocalizationRemote RemoteConfig => RemoteTextsPostprocessor.Current()?.Config;

        RemoteTextsCache ILocalizationModel.RemoteCache =>
            _remoteCache ??= new RemoteTextsCache(RemoteTextsCache.DefaultPath(), Application.version);

        public bool IsBootWaitOpen { get; private set; }

        public bool HasNewRemoteTexts { get; private set; }

        public void OpenBootWait() => IsBootWaitOpen = true;

        public void CloseBootWait() => IsBootWaitOpen = false;

        public void SetNewRemoteTexts(bool value) => HasNewRemoteTexts = value;
    }
}
