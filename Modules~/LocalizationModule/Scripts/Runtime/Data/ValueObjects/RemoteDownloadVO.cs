using System.Collections.Generic;

namespace Modules.LocalizationModule.Data.ValueObjects
{
    /// <summary>
    /// What one look at the published sheet came back with. <see cref="Changed"/> is false when the
    /// revision matched the cache's, and then no table was downloaded.
    /// </summary>
    public class RemoteDownloadVO
    {
        public bool Succeeded;
        public bool Changed;
        public string Revision = string.Empty;
        public List<List<string>> Rows = new();
        public string Error = string.Empty;
    }
}
