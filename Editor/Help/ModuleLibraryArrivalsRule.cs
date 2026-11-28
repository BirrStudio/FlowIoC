#if UNITY_EDITOR

using System;
using System.Collections.Generic;

namespace FlowIoC.Editor.Help
{
    /// <summary>
    /// Which modules are new. A first record flags nothing - on a first open every module is
    /// uninstalled, and flagging all of them says nothing. A version change flags the modules
    /// the previous version did not ship, on top of the flags the reader has not seen yet: a
    /// module that arrived in a minor stays new through the patches after it, until its page is
    /// opened.
    /// </summary>
    internal class ModuleLibraryArrivalsRule
    {
        internal ModuleLibraryArrivalsEVO Next(ModuleLibraryArrivalsEVO previous, string version,
            IReadOnlyList<string> shipped)
        {
            var current = new List<string>(shipped);
            current.Sort(StringComparer.Ordinal);

            if (previous == null)
                return new ModuleLibraryArrivalsEVO {Version = version, Shipped = current.ToArray()};

            if (previous.Version == version)
                return previous;

            var arrived = new List<string>();

            foreach (string folder in current)
            {
                bool unseen = Array.IndexOf(previous.New, folder) >= 0;

                if (unseen || Array.IndexOf(previous.Shipped, folder) < 0)
                    arrived.Add(folder);
            }

            return new ModuleLibraryArrivalsEVO
            {
                Version = version, Shipped = current.ToArray(), New = arrived.ToArray()
            };
        }

        /// <summary>The record with one module's flag taken down, or the same record when it carried none.</summary>
        internal ModuleLibraryArrivalsEVO Seen(ModuleLibraryArrivalsEVO record, string folder)
        {
            if (record == null || Array.IndexOf(record.New, folder) < 0)
                return record;

            var left = new List<string>(record.New);
            left.Remove(folder);

            return new ModuleLibraryArrivalsEVO
            {
                Version = record.Version, Shipped = record.Shipped, New = left.ToArray()
            };
        }
    }
}

#endif
