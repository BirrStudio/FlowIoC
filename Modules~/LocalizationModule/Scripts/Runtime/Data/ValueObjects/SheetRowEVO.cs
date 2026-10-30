using System;
using System.Collections.Generic;

namespace Modules.LocalizationModule.Data.ValueObjects
{
    /// <summary>One key as the table and the sheet last agreed on it: a cell per language.</summary>
    [Serializable]
    public class SheetRowEVO
    {
        public string Key = string.Empty;
        public List<SheetCellEVO> Cells = new();

        public SheetCellEVO Cell(string language)
        {
            foreach (SheetCellEVO cell in Cells)
            {
                if (cell.Language == language)
                    return cell;
            }

            return null;
        }
    }
}
