#if UNITY_EDITOR

using System.Collections.Generic;
using FlowIoC.Editor.Icons;

namespace FlowIoC.Editor.Help.Pages.Modules
{
    /// <summary>
    /// The localization module: Unity Localization behind one Service, a component for labels, the
    /// language choice, and the panel that edits the Game table and syncs it with a Google Sheet.
    /// </summary>
    internal class LocalizationModulePage : ModulePage
    {
        private readonly HelpImages _images = new HelpImages();

        public override string ModuleFolderName => "LocalizationModule";

        public override string Title => "Localization Module";

        public override string Subtitle => "Every text in the player's language, and a Google Sheet to translate them in";

        public override FlowIcon Icon => FlowIcon.SwitchArrows;

        public override IReadOnlyList<string> RequiredPackages => new[] {"com.unity.localization"};

        public override IReadOnlyList<HelpTab> MoreTabs => new[]
        {
            new HelpTab("Setup", DrawSetup,
                "Set up in the panel, the Root in the boot scene, Initialize bound before the first screen.",
                "Set up makes Assets/Localization - the settings, English and Turkish, the Game table. "
                + "The Google Sheet needs a Google Cloud OAuth client once, and nothing reports it missing until Pull."),
            new HelpTab("Usage", DrawUsage,
                "A LocalizedText on a label; ILocalizationService.Get in the Command that fills a screen.",
                "A label whose words never change needs no code. A text that carries a value is read by the "
                + "Command that opens its screen, and LanguageChanged fills an open screen again."),
            new HelpTab("Sheet", DrawSheet,
                "Pull, Push, machine translation in the sheet, and what happens when both sides changed a text.",
                "The sheet is where texts are written and translated; the table is the game's copy. Each "
                + "button compares both with what they last agreed on, so only a text changed on both sides is asked about.")
        };

        public override string InstalledHint =>
            "Open Tools/FlowIoC-Modules/Localization/Panel and press Set up, drop LocalizationServiceRoot into your boot "
            + "scene and bind ILocalizationService.Commands.Initialize before the first screen; the Setup tab has the steps.";

        public override string BodyHeadline =>
            "Every text the player reads, in the player's language.";

        public override string BodyTagline =>
            "The phone's language on first launch, the one the player picks in settings after that, and every "
            + "label on screen changing the moment they pick it.";

        public override void DrawBody(HelpPainter painter)
        {
            painter.Image(_images.Get("LocalizationTestScene.png"),
                "LocalizationTestScene in play: the title is a LocalizedText, the line under it a plural filled by a Command.");

            painter.Separator();
            painter.SubHeading("What it gives you");
            painter.Bullet(
                "Unity Localization behind one Service, ILocalizationService. No game type references the "
                + "package; a game knows a key, a language code and the service.");
            painter.Bullet(
                "LocalizedText, a component beside a TMP text: pick the key, and the label shows it in the "
                + "selected language and changes with it. Its Create key turns what the label already says "
                + "into a new English entry.");
            painter.Bullet(
                "Get(key, arguments) for a text that carries a value, through Smart Strings - "
                + "\"{0:plural:One level|{} levels}\" picks the form each language needs.");
            painter.Bullet(
                "The language choice: the player's saved choice, else the phone's language, else English. "
                + "SetLanguage changes it, keeps it, loads the new table and announces LanguageChanged.");
            painter.Bullet(
                "A panel that shows every key in every language, what is missing, and what a machine "
                + "translated and nobody has read - and syncs the table with a Google Sheet both ways.");
            painter.Bullet(
                "LocalizationKeys.cs, one constant per key, so code reads a text by a name the compiler checks.");

            painter.Space();
            painter.Note(
                "It is a Service, so a screen or a System references Modules.Localization and injects "
                + "ILocalizationService directly. Its one signal, LanguageChanged, is for a Connector: an "
                + "open screen with variable text is filled again when the language changes.");

            painter.Separator();
            painter.SubHeading("The ready-made modules stay English");
            painter.Paragraph(
                "Loading, BotBar and the other modules FlowIoC installs ship their texts in English and know "
                + "nothing of localization. A game that translates them does it on its own side - a "
                + "LocalizedText on its copy of the prefab, a Get in its own Command.");

            painter.Separator();
            painter.SubHeading("Trying it out");
            painter.Paragraph(
                "Open LocalizationTestScene under the test module's Scenes folder and press Play. The title is "
                + "a LocalizedText; the line under it is filled by a Command with a number, so \"+1 level\" "
                + "shows the plural form change; English and Türkçe switch the language, and both texts follow. "
                + "Opening the scene adds its two texts, sample.title and sample.levels, to the Game table when "
                + "they are missing; delete them with the test module.");
        }

        private void DrawSetup(HelpPainter painter)
        {
            painter.SubHeading("1. Set up");
            painter.Paragraph(
                "Open Tools/FlowIoC-Modules/Localization/Panel and press Set up. It makes Assets/Localization: "
                + "the Localization Settings, made active; English and Turkish; the Game string table, "
                + "preloaded; the startup order (saved choice, phone language, English); and the two files "
                + "the sheet sync reads. It makes only what is missing, so a project that already localizes "
                + "keeps its settings. The tables are the game's texts and live outside the module's folder, "
                + "so updating or deleting the module never touches a translation.");
            painter.Image(_images.Get("LocalizationPanel.png"),
                "The Localization panel: the languages, the sheet, and every key in every language.");

            painter.Separator();
            painter.SubHeading("2. The Root");
            painter.Paragraph(
                "Drop LocalizationServiceRoot from the module's Prefabs folder into the boot scene. It ships at "
                + "Initialize Order -65, in the Service band.");

            painter.Separator();
            painter.SubHeading("3. Initialize before the first screen");
            painter.Paragraph(
                "Bind the module's step in the boot sequence before the step that opens the first screen. It "
                + "holds the sequence until the selected language's table is loaded, so every text after it is "
                + "read at once.");
            painter.Code(
                "CommandBinder.Bind(_signals.Boot)\n"
                + "    .ToSequence<ILocalizationService.Commands.Initialize>()\n"
                + "    .ToSequence<OpenMainScreenCommand>();");
            painter.Note(
                "Important: a text read before Initialize has finished waits for its table on the spot, and a "
                + "LocalizedText on a screen that is already up shows its key's text a frame late. Neither "
                + "reports anything; bind the step first.");

            painter.Separator();
            painter.SubHeading("4. The Google Sheet");
            painter.Paragraph(
                "Select SheetsServiceProvider in Assets/Localization (the panel's Settings button). Create an "
                + "OAuth client in a Google Cloud project with the Sheets API enabled - type Desktop app - paste "
                + "its client id and secret, and press Authorize; a browser asks once, and the token stays in "
                + "Library/Google on this machine. Then paste the sheet's address in the panel and press Link, "
                + "or press Create sheet.");
            painter.Note(
                "Important: the client secret is saved in the provider asset. Keep the asset out of a public "
                + "repository, or give each developer a client of their own. Without credentials the panel "
                + "works on the table alone and Pull and Push report the refusal.");
        }

        private void DrawUsage(HelpPainter painter)
        {
            painter.SubHeading("A label");
            painter.Paragraph(
                "Add LocalizedText beside the TMP text and pick the key, or type a new key and press Create "
                + "key: the label's current text becomes the key's English. Nothing else - no Command, no "
                + "Mediator - because nothing is decided. A pooled screen is fine: it subscribes while the "
                + "label is enabled.");
            painter.Paragraph(
                "Its Font slot is for later: left empty the label keeps its font; set to a localized font, a "
                + "language whose script the game's font lacks - Chinese, Arabic - gets its own.");

            painter.Separator();
            painter.SubHeading("A text with a value");
            painter.Paragraph(
                "The Command that opens the screen reads it before the screen shows and sets it on the view "
                + "Show returns, like any other data the screen shows - inside the retained try the Screen "
                + "Module page describes.");
            painter.Code(
                "[Inject] private ILocalizationService _localization { get; set; }\n"
                + "\n"
                + "string unlock = _localization.Get(LocalizationKeys.TICKET_UNLOCKS_AT, _level.Next);\n"
                + "TicketScreenView screen = await _screenService.Open<TicketScreenView>().Show<TicketScreenView>();\n"
                + "screen.SetUnlock(unlock);");

            painter.Separator();
            painter.SubHeading("Changing the language");
            painter.Paragraph(
                "A settings list reads Languages - each with its code and its name in itself, English, "
                + "Türkçe - and dispatches the picked code on a Signal<string>. The module's step does the rest.");
            painter.Code(
                "CommandBinder.Bind(_signals.LanguagePicked).ToSequence<ILocalizationService.Commands.SetLanguage>();");

            painter.Separator();
            painter.SubHeading("An open screen with a value");
            painter.Paragraph(
                "LocalizedText labels change by themselves. A text a Command wrote does not, so wire "
                + "LanguageChanged to the screen's own refresh in the game's Connector, and fill it again.");
            painter.Code(
                "_localizationSignals.Outgoing.LanguageChanged.Connect(_ticketSignals.Incoming.Refresh);");
        }

        private void DrawSheet(HelpPainter painter)
        {
            painter.SubHeading("The layout");
            painter.Paragraph(
                "One tab: Key in column A, a language code over every column after it - en, tr, de. Every row "
                + "under it is a key. Push writes the header and adds a language's column the first time it is "
                + "needed.");

            painter.Separator();
            painter.SubHeading("Pull and Push");
            painter.Paragraph(
                "Both read the whole sheet and compare it with the table and with what the two last agreed on. "
                + "A text changed only in the sheet is Pull's to bring in; a text changed only in the table is "
                + "Push's to send out; a key deleted on one side and untouched on the other is deleted on the "
                + "other. A text changed on both sides is asked about - keep the project's, keep the sheet's, "
                + "or skip - and the answer is written to both, so it is not asked again.");

            painter.Separator();
            painter.SubHeading("Machine translation");
            painter.Paragraph(
                "Push writes =GOOGLETRANSLATE(<English cell>, \"en\", \"<code>\") into every language cell "
                + "that is empty on both sides. The sheet translates it; the next Pull brings the words in, "
                + "marked unchecked in the panel because the cell still holds the formula. Editing the text, "
                + "in the panel or the sheet, makes it a person's; Approve all writes the machine's words over "
                + "their formulas when they are good as they are.");
            painter.Note(
                "A GOOGLETRANSLATE cell is recomputed by the sheet now and then, and may come back slightly "
                + "different. Approve what is good so it stops moving.");
        }
    }
}

#endif
