using System;
using System.Collections.Generic;
using FlowIoC.AssetModule.Service;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.LoadingModule.LoadingScreenModule.Constants;
using Modules.LoadingModule.LoadingScreenModule.Models;
using Modules.LoadingModule.LoadingScreenModule.Signals;
using UnityEngine;

namespace Modules.LoadingModule.LoadingScreenModule.Controllers
{
    /// <summary>
    /// Brings in the addressable art behind the bar. The prefab comes from Resources with the
    /// same art bundled, so the screen never waits for this: it is on stage whatever Addressables
    /// is doing, and the art swaps in once the load lands - seconds later on a remote catalogue,
    /// never on this run if the boot ends first, from the cache the next time. Three ways out,
    /// each one resolving the retain. A load that came back with nothing was reported by the
    /// asset service and leaves the bundled art where it is.
    /// </summary>
    internal class LoadBackgroundCommand : Command
    {
        [Inject] private IAssetService _assetService { get; set; }
        [Inject] private ILoadingScreenModel _model { get; set; }
        [InjectSignal] private LoadingScreenInternalSignals _internalSignals { get; set; }

        public override async void Execute()
        {
            Retain();

            try
            {
                // A sprite is a sub-asset of its texture: content directories load it only through the sub-asset
                // syntax, where AssetBundles also accepted the texture's own address as a Sprite.
                IList<Sprite> sprites = await _assetService.LoadAssetAsync<IList<Sprite>>(LoadingScreenConstants.BACKGROUND_KEY);
                Sprite background = sprites != null && sprites.Count > 0 ? sprites[0] : null;

                if (background == null)
                {
                    Stop();
                    return;
                }

                _model.RememberBackground(background);
                _internalSignals.BackgroundLoaded.Dispatch(background);
                Release();
            }
            catch (Exception exception)
            {
                FlowLogger.LogError($"LoadBackgroundCommand threw while loading the loading screen's background: {exception}");
                Stop();
            }
        }
    }
}
