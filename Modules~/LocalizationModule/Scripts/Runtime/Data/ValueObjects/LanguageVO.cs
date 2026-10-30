namespace Modules.LocalizationModule.Data.ValueObjects
{
    /// <summary>A language the game offers: the code a Command passes, the name a settings list shows.</summary>
    public class LanguageVO
    {
        /// <summary>The locale code - <c>en</c>, <c>tr</c>, <c>zh-Hans</c>.</summary>
        public string Code { get; }

        /// <summary>The language's name in itself - English, Türkçe - so a player finds their own.</summary>
        public string Name { get; }

        public LanguageVO(string code, string name)
        {
            Code = code;
            Name = name;
        }
    }
}
