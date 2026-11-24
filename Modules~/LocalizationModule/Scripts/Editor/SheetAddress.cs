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
        /// <summary>
        /// What separates a formula's arguments on a sheet of this locale - en_US, tr_TR, as the
        /// Sheets API names it. A locale whose decimals take a comma takes a semicolon between
        /// arguments, and a comma-separated formula written to it is a parse error (#ERROR!). Reading
        /// the formula back says nothing: the API always answers in the comma form.
        /// </summary>
        public string ArgumentSeparator(string locale)
        {
            if (string.IsNullOrEmpty(locale))
                return ",";

            try
            {
                var culture = new System.Globalization.CultureInfo(locale.Replace('_', '-'));

                return culture.NumberFormat.NumberDecimalSeparator == "," ? ";" : ",";
            }
            catch (System.Globalization.CultureNotFoundException)
            {
                return ",";
            }
        }

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
