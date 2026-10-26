using FlowIoC.BaseModule.Attributes;
using FlowIoC.BaseModule.Root;
using Modules.AbTestFlowModule.Services;

namespace Modules.AbTestFlowModule.RootsContexts
{
    /// <summary>
    /// Persistent, because surviving is what keeps a scene change from re-rolling a group or
    /// re-applying an override.
    /// </summary>
    [PersistentRoot]
    public class AbTestFlowServiceRoot : Root<AbTestFlowServiceContext>
    {
#if UNITY_EDITOR
        /// <summary>
        /// The Editor keeps whatever a play session wrote into a ScriptableObject, so the originals
        /// are put back before the process ends. The service is the module's one counterpart, so it
        /// is what the Root asks - directly rather than through a dispatched signal, which may not
        /// finish before the context is torn down.
        /// </summary>
        private void OnApplicationQuit()
        {
            if (Context == null || _rootsManager == null)
                return;

            var service = (IAbTestFlowService) _rootsManager.InjectionBinderCrossContext.GetInstance(typeof(IAbTestFlowService));

            service?.RestoreEditorAssets();
        }
#endif
    }
}