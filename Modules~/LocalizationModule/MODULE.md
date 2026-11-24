# Localization

## Purpose
Translates a game's texts through Unity Localization: the device language by default, a language the player picks in settings, every text refreshed at once, and a panel that edits the table and syncs it with Google Sheets both ways - and, once the sheet is published, corrected texts that reach a shipped game without a build.

## Concepts
localization, localisation, translation, translate, language, locale, i18n, string table, LocalizedText, Google Sheets, GOOGLETRANSLATE, ILocalizationService, LanguageChanged, remote, runtime update, Publish to web, Status, revision, Version tab, RemoteTextsPostprocessor, CD_LocalizationRemote, without a build

## Decisions
- **Unity Localization, wrapped.** Chosen over I2 Localization and a system of our own (owner's
  brainstorm, 2026-10-18): it is a free UPM package a Library module may depend on - I2 is a paid
  Asset Store package a consumer may not own - it swaps fonts per locale for CJK, its Smart
  Strings cover Arabic's six plural forms, and it sits on Addressables, which FlowIoC already uses.
  No game type references it: a game knows `ILocalizationService`, a key and `LocalizedText`.
- **The tables are the game's, outside the module.** The panel's Set up writes
  `Assets/Localization` - settings, locales, the `Game` table, the sheet's two files - so updating
  or deleting the module never touches a translation. One table: one tab on the sheet, one list to
  search.
- **A label is a component, a value is a Command.** A text whose words never change is a
  `LocalizedText` that refreshes itself, because nothing is decided; a text with a value is read
  by the Command that opens its screen, and `LanguageChanged` - for a Connector - fills an open one
  again (owner, 2026-10-18).
- **The language choice is Unity's startup selectors**: the saved choice
  (`PlayerPrefLocaleSelector`, key `flowioc.localization.language`, which also writes on every
  change), the device's language, English. Nothing is decided at boot in code, so nothing is
  announced; the module stays a Service that depends on nothing, LocalSave included.
- **The Library's own modules stay English** (owner, 2026-10-18). Loading, BotBar and the rest know
  nothing of localization; a game translates them on its side.
- **The sheet is the source, the merge is three-way.** Pull and Push compare the table and the
  sheet with what they last agreed on (`ED_LocalizationSheet`), so a text changed on one side
  follows that side and only a text changed on both is asked about - and the answer is written to
  both. Machine translation is the sheet's own `GOOGLETRANSLATE`, so no API key; a cell still
  holding the formula is what "unchecked" means, and Approve writes the words over it.
- **Sheet access is the package's `SheetsServiceProvider`**, and the sync talks to the Sheets API
  through it directly rather than through the package's own Push and Pull, which overwrite a side
  and cannot write a formula.
- **The sample's texts are in the Game table only while its scene is open** (owner, 2026-10-30).
  Opening `LocalizationTestScene` adds `sample.title` and `sample.levels`; closing it, quitting
  Unity and starting a build take them out, so the game's table and build carry no row the game
  did not write. A table of the sample's own was weighed and dropped: Unity Localization groups
  every table's Addressables entries by language, so it would ship in every build.
- **An empty text means "not translated"** (owner, 2026-10-30): the panel marks it missing and
  Push hands it to `GOOGLETRANSLATE`, so a text cannot be kept empty in one language through the
  sheet. No game has needed that yet.
- **`LocalizationKeys.cs` is generated into the module**, because game modules reference
  `Modules.Localization` and nothing in `Assembly-CSharp`. It is the game's content, so it carries
  an `Extend:` line.
- **Phones read the published sheet** (owner, 2026-11-24). A Publish-to-web CSV needs no key and no
  sign-in, and the texts ship in the build anyway. A phone reads the revision in the Version tab's
  A1 first and downloads the Game tab only when it differs from its cache's; the number is the
  owner's choice over a date. Push and Approve raise it, and keep a `Status` column listing each
  row's unread machine translations, which a phone never takes (owner's pick over one column per
  language or a status tab).
- **A download shows on the next launch by default; Wait at boot is the option** (owner). Boot
  never waits unless told to, and nothing on screen changes. Waiting reloads the tables and
  refreshes every `LocalizedText`.
- **The patch is a table postprocessor saved on the settings asset**, not assigned in code: Unity
  Localization starts the first time anything touches it - a `LocalizedText` at scene load - and
  the preloaded table can load before the module's code runs. Set up writes it; a project set up
  before presses Set up once more. Its config, `CD_LocalizationRemote`, hangs off it, so it
  travels with the settings into the build and the Root needs no adapter.
- **Nothing remote is applied in the Editor.** There a table is the asset; a patched entry could
  be saved and pushed back to the sheet. Check now reports instead, and a device is the test.
- **The revision is in two places, the Version tab's A1 and the Game tab's `Revision N` header**
  (final review, 2026-11-24). Published tabs refresh apart, minutes apart; a phone keeps a
  download only when both say the same number, and a lag is not counted as the day's look. Status
  also lists every cell that still holds a formula, whatever the agreement says, and a published
  `Loading...` or `#N/A` is never taken.
- **The cache is a file, dropped when `Application.version` changes**, so a store update's own
  texts are never covered by an older download. Placeholders are checked as a table loads,
  against the text being patched, rather than loading every language at download time.

## Known gaps
- Arabic shaping and right-to-left layout are not handled; TMP needs something like RTLTMPro for
  that, added when Arabic comes.
- The Google Sheets side has been compiled and its merge unit-tested, but not yet run against a
  real sheet: that needs the owner's OAuth client.
- Pull and Push call the Sheets API synchronously and hold the Editor while they run, up to the
  provider's 60-second sign-in timeout.
- A Push, Pull or Add key made while `LocalizationTestScene` is open sends the sample's two texts
  to the sheet and writes their constants into `LocalizationKeys.cs`; they stay there until the
  next Push or Pull with the scene closed.
- Rows a translator sorts or inserts while a conflict question is open stop the pass with nothing
  written; it has to be run again.
- A fix made in the sheet by hand reaches phones only after a Pull and a Push, or when the number is
  raised by hand in both the Version tab and the Game tab's `Revision N` header (and Status cleared
  for a replaced machine translation).
- Wait at boot's reload (`StringDatabase.ResetState`, then preloading Game again) has not been
  run on a device yet; Next launch does not depend on it.

Extend: Scripts/Runtime/Constants/LocalizationKeys.cs
Version: 1.1.1

<!-- FLOWIOC:BEGIN version=1 hash=df74b59d | generated by Tools/FlowIoC/Module Scanner - do not edit inside this block -->
**Kind** Main · **Assemblies** Modules.Localization, Modules.Localization.Signals
**Root** LocalizationServiceRoot → LocalizationServiceContext
**Outgoing** LanguageChanged(string)
**Services** ILocalizationService
**Sub modules** LocalizationTestModule (Test)
<!-- FLOWIOC:END -->
