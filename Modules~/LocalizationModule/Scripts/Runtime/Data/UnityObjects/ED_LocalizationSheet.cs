using System.Collections.Generic;
using Modules.LocalizationModule.Data.ValueObjects;
using UnityEngine;

namespace Modules.LocalizationModule.Data.UnityObjects
{
    /// <summary>
    /// Which Google Sheet the Game table syncs with, and what both sides held the last time they
    /// agreed. The panel's Pull and Push compare the table and the sheet against these rows, so a
    /// text changed on one side follows that side and a text changed on both is asked about.
    /// The panel's Set up makes it in Assets/Localization; nothing reads it at runtime.
    /// </summary>
    public class ED_LocalizationSheet : ScriptableObject
    {
        /// <summary>The id in the sheet's address, between /d/ and /edit.</summary>
        public string SpreadsheetId = string.Empty;

        /// <summary>The tab's gid, the number after #gid= in the address.</summary>
        public int SheetId;

        /// <summary>The last synced state, one row per key.</summary>
        public List<SheetRowEVO> Rows = new();
    }
}
