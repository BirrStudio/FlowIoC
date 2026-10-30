namespace Modules.LocalizationModule.Constants
{
    public static class LocalizationConstants
    {
        /// <summary>The one string table a game's texts live in, in the code and on the sheet.</summary>
        public const string TABLE = "Game";

        /// <summary>The language a key is first written in, and the source machine translation reads.</summary>
        public const string SOURCE_LANGUAGE = "en";

        /// <summary>Where the player's choice is kept; Unity's PlayerPrefLocaleSelector reads and writes it.</summary>
        public const string PREFS_KEY = "flowioc.localization.language";
    }
}
