using FlowIoC.BaseModule.Attributes;
using FlowIoC.BaseModule.Root;
using Modules.LocalSaveModule.Models;

namespace Modules.LocalSaveModule.RootsContexts
{
    /// <summary>
    /// Persistent, so the model and the starting values it captured survive a scene load - the
    /// reload after ResetAll included - and the scene that comes back brings no second save.
    /// </summary>
    [PersistentRoot]
    public class LocalSaveServiceRoot : Root<LocalSaveServiceContext>
    {
        /// <summary>
        /// The order matters: the session has to reach disk before the assets are rolled back, or
        /// the play session's data is thrown away with the dirt.
        ///
        /// SaveAll is called on the model rather than dispatched as a signal, because a command
        /// dispatched during shutdown may not complete before the context is torn down.
        /// </summary>
        private void OnApplicationQuit()
        {
            if (Context == null || _rootsManager == null)
                return;

            var model = (ILocalSaveModel) _rootsManager.InjectionBinderCrossContext
                .GetInstance(typeof(ILocalSaveModel));

            if (model == null)
                return;

            model.SaveAll();

#if UNITY_EDITOR
            model.RestoreEditorAssets();
#endif
        }
    }
}