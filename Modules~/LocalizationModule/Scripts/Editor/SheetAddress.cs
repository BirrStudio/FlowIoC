#if UNITY_EDITOR

namespace Modules.LocalizationModule.Editor
{
    /// <summary>How the sheet names a place and a language: A1 columns, Google Translate's codes.</summary>
    internal class SheetAddress
    {
        /// <summary>A1 letters for a zero-based column: 0 is A, 26 is AA.</summary>
        public string Column(int index)
        {
            string letters = string.Empty;

            for (int n = index + 1; n > 0; n = (n - 1) / 26)
                letters = (char)('A' + (n - 1) % 26) + letters;

            return letters;
        }

        /// <summary>The code Google Translate knows a locale by: zh-Hans is zh-CN, pt-BR stays itself, most are their language alone.</summary>
        public string TranslateCode(string code)
        {
            switch (code)
            {
                case "zh-Hans":
                case "zh-CN":
                    return "zh-CN";
                case "zh-Hant":
                case "zh-TW":
                    return "zh-TW";
                case "pt-BR":
                    return "pt-BR";
            }

            int dash = code.IndexOf('-');

            return dash < 0 ? code : code.Substring(0, dash);
        }
    }
}

#endif
