using System;
using System.Collections.Generic;

namespace Modules.LocalizationModule.Data.ValueObjects
{
    /// <summary>
    /// What the phone last read from the published sheet, kept between launches: the revision it
    /// was read at, when the sheet was last looked at, the app version it was read under - a cache
    /// from another version is dropped - and the texts that passed the download checks.
    /// </summary>
    [Serializable]
    public class RemoteTextsCacheVO
    {
        public string Revision = string.Empty;
        public long CheckedAtUtcTicks;
        public string AppVersion = string.Empty;
        public List<RemoteTextVO> Texts = new();
    }

    /// <summary>One corrected text: a key, a language code and its words.</summary>
    [Serializable]
    public class RemoteTextVO
    {
        public string Key = string.Empty;
        public string Language = string.Empty;
        public string Text = string.Empty;
    }
}
