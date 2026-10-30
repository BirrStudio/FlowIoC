using System;
using System.Collections.Generic;
using Modules.LocalizationModule.Data.ValueObjects;

namespace Modules.LocalizationModule.Services
{
    /// <summary>
    /// The module's one counterpart, and the only name a game uses: no game type references Unity
    /// Localization. A Command reads a text here when it fills a screen; a static label needs no
    /// code at all - a <c>LocalizedText</c> beside it does the work. The steps a game binds sit under
    /// <see cref="Commands"/>, one file each beside this one.
    /// </summary>
    public partial interface ILocalizationService
    {
        /// <summary>True once <see cref="Initialize"/> has finished; a text read before then waits for its table.</summary>
        bool IsReady { get; }

        /// <summary>The selected language's code - <c>en</c>, <c>tr</c>.</summary>
        string Language { get; }

        /// <summary>Every language the project has, in the order the settings list them.</summary>
        IReadOnlyList<LanguageVO> Languages { get; }

        /// <summary>The key's text in the selected language, from the Game table.</summary>
        string Get(string key);

        /// <summary>
        /// The key's text with its placeholders filled - <c>Unlocks at level {0}</c> - through
        /// Smart Strings, so a plural form is chosen by the number the way each language needs.
        /// </summary>
        string Get(string key, params object[] arguments);

        /// <summary>Brings Unity Localization up and loads the selected language's table, then calls <paramref name="done"/>.</summary>
        void Initialize(Action done);

        /// <summary>
        /// Selects a language, keeps the choice, loads its table and announces
        /// <c>LanguageChanged</c>, then calls <paramref name="done"/>. A code the project does not
        /// have, or the language already selected, changes nothing and announces nothing.
        /// </summary>
        void SetLanguage(string code, Action done);

        /// <summary>
        /// The steps a game binds in a sequence of its own. They sit inside the interface so that
        /// the one name a game knows - the Service it injects - is also where its steps are found.
        /// Each step is a file of its own, <c>ILocalizationService.Commands.&lt;Step&gt;.cs</c>.
        /// </summary>
        public static partial class Commands
        {
        }
    }
}
