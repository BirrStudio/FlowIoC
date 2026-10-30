using System;

namespace Modules.LocalizationModule.Data.ValueObjects
{
    /// <summary>
    /// One text as last synced. Unchecked while the sheet holds a GOOGLETRANSLATE formula for it:
    /// a machine wrote it and nobody has read it yet.
    /// </summary>
    [Serializable]
    public class SheetCellEVO
    {
        public string Language = string.Empty;
        public string Value = string.Empty;
        public bool Unchecked;
    }
}
