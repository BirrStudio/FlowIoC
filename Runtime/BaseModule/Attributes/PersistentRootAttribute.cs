using System;

namespace FlowIoC.BaseModule.Attributes
{
    /// <summary>
    /// A Root that lives for the whole run rather than for its scene. The framework detaches it and
    /// marks it do not destroy before its context is built, and a second Root of the same type -
    /// the one a reloaded scene brings back - stands down instead of building a context of its own:
    /// it binds nothing, files nothing, and is destroyed with whatever hangs under it. The Root that
    /// is alive keeps its context, its bindings and its state.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PersistentRootAttribute : Attribute
    {
    }
}
