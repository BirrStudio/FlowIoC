#if UNITY_EDITOR

using System.Collections.Generic;
using FlowIoC.Editor.Icons;

namespace FlowIoC.Editor.Help.Pages.Modules
{
    /// <summary>
    /// The consent module: the form and the tracking prompt behind one plug, the answer it
    /// announces, the stand-in with no plug, and the Connector sequence that hands the answer to
    /// the ad and analytics SDKs.
    /// </summary>
    internal class ConsentModulePage : ModulePage
    {
        private readonly HelpImages _images = new HelpImages();

        public override string ModuleFolderName => "ConsentModule";

        public override string Title => "Consent Module";

        public override string Subtitle => "The consent form and the tracking prompt before the ad and analytics SDKs start";

        public override FlowIcon Icon => FlowIcon.Lock;

        public override IReadOnlyList<HelpTab> MoreTabs => new[]
        {
            new HelpTab("Setup", DrawSetup,
                "The Root ahead of Ads and Analytics, the plug, and the SDKs told to wait.",
                "A store build with no plug answers Denied and reports it as an error; an Ads Root left "
                + "on InitializeOnLaunch starts the SDK before anyone has asked."),
            new HelpTab("Usage", DrawUsage,
                "The Connector sequence that hands the answer on, and the privacy button.",
                "The module knows no other module: the game's Connector is where the answer becomes each "
                + "SDK's flags."),
            new HelpTab("Providers", DrawProviders,
                "The socket, the stand-ins, and your own plug over UMP or ATT.",
                "A provider shows the form it is told to and answers once; the order and the publishing "
                + "are the module's.")
        };

        public override string InstalledHint =>
            "Drop ConsentServiceRoot into your boot scene ahead of Analytics and Ads, plug your consent "
            + "SDK, and hang the hand-over on Outgoing.Resolved in your Connector; the Setup tab has the steps.";

        public override string BodyHeadline =>
            "Ask once, before anything personal is used, and hand the answer to every SDK.";

        public override string BodyTagline =>
            "The consent form, then the tracking prompt on iOS, then one announcement the Connector hangs "
            + "the ad and analytics SDKs on - and a privacy button that asks again.";

        public override void DrawBody(HelpPainter painter)
        {
            painter.SubHeading("What it gives you");
            painter.Bullet(
                "IConsentService: IsResolved, Current, Request, ShowPrivacyOptions. A Service, so any "
                + "module references Modules.Consent and injects it.");
            painter.Bullet(
                "A socket: IConsentProvider, one consent SDK - Google UMP for the form, Apple's ATT for "
                + "the tracking prompt, or one SDK doing both.");
            painter.Bullet(
                "Two steps a game binds: Commands.Request, when CD_Consent.RequestOnLaunch is off, and "
                + "Commands.ShowPrivacyOptions, for a settings button.");
            painter.Bullet(
                "ConsentVO with two readings, CanPersonalise and CanTrack, announced on Outgoing.Resolved "
                + "and filed in RD_Consent for a module that reads it later.");

            painter.Separator();
            painter.SubHeading("When it decides");
            painter.Paragraph(
                "At Launch, unless CD_Consent says the game asks: the form, then the tracking prompt, "
                + "then the answer published. Each is waited on for as long as the player leaves it up. "
                + "The privacy options form publishes again; it is refused before the first answer.");
            painter.Note(
                "With no plug, the Editor and a Development Build plug a stand-in that answers "
                + "DevelopmentConsent and DevelopmentTracking from CD_Consent, with a warning. A release "
                + "build reports the missing plug as an error and answers Denied: ads still run, "
                + "non-personalised, and nothing personal is used because a plug was forgotten.");

            painter.Separator();
            painter.SubHeading("Trying it out");
            painter.Paragraph(
                "Open ConsentTestScene and press Play: the Flow Console's Consent channel shows the "
                + "stand-in's answer and what the game would hand on. The Device Debugger's Consent rows "
                + "ask again and open the privacy options; change CD_Consent's development answers to see "
                + "the other outcomes.");
        }

        private void DrawSetup(HelpPainter painter)
        {
            painter.SubHeading("The Root");
            painter.Bullet(
                "ConsentServiceRoot into the scene the game boots from, Initialize Order -86, ahead of "
                + "Analytics and Ads. CD_Consent ships filed on its adapter: RequestOnLaunch (on), "
                + "DevelopmentConsent (Granted), DevelopmentTracking (Authorized); RD_Consent ships in "
                + "its Shared Scriptables.");
            painter.Image(_images.Get("ConsentRootHierarchy.png"),
                "ConsentServiceRoot in MainScene, in the Services band.");
            painter.Image(_images.Get("ConsentRootAdapter.png"),
                "Its adapter as it ships: CD_Consent in the Scriptable Map, RD_Consent in the Shared one.");

            painter.Separator();
            painter.SubHeading("The plug");
            painter.Note(
                "Important: a plug is an entry on ConsentServiceRoot's Sub Context Types with AutoSetup "
                + "on. With none, a release build answers Denied for every player - the error is the only "
                + "report, and personalised ads never run.");

            painter.Separator();
            painter.SubHeading("The SDKs wait");
            painter.Note(
                "Important: copy CD_Ads, turn InitializeOnLaunch off, and file the copy on AdsServiceRoot "
                + "in place of the shipped one. Left on, the ad SDK starts before the form is answered and "
                + "nothing reports it; the Connector sequence on the Usage tab initializes it instead.");
        }

        private void DrawUsage(HelpPainter painter)
        {
            painter.SubHeading("Handing the answer on");
            painter.Paragraph(
                "A sub-context in the game's ConnectorModule, because it knows both SDK modules. The two "
                + "To steps only translate the payload and hand it on with Release:");
            painter.Code(
                "CommandBinder.Bind(_consent.Outgoing.Resolved)\n"
                + "    .ToSequence<ToAdsConsentCommand>()             // Release(new AdsConsentVO(consent.CanPersonalise))\n"
                + "    .ToSequence<IAdsService.Commands.SetConsent>()\n"
                + "    .ToSequence<ToAnalyticsConsentCommand>()       // Release(new AnalyticsConsentVO(p, p, p, consent.CanTrack))\n"
                + "    .ToSequence<IAnalyticsService.Commands.SetConsent>()\n"
                + "    .ToSequence<IAdsService.Commands.Initialize>();");
            painter.Paragraph(
                "Resolved goes out again after the privacy options answer; the second Initialize is "
                + "logged and ignored.");

            painter.Separator();
            painter.SubHeading("The privacy button");
            painter.Paragraph(
                "A settings screen shows it when RD_Consent.Current.PrivacyOptionsRequired is set, and "
                + "binds the click to the shipped step:");
            painter.Code(
                "CommandBinder.Bind(_signals.Incoming.PrivacyClicked).ToSequence<IConsentService.Commands.ShowPrivacyOptions>();");
        }

        private void DrawProviders(HelpPainter painter)
        {
            painter.SubHeading("The socket");
            painter.Paragraph(
                "IConsentProvider has three calls, each answered exactly once on the main thread: "
                + "RequestConsent (the status, and whether privacy options must be offered), "
                + "RequestTracking (NotApplicable where the platform has no prompt), and "
                + "ShowPrivacyOptions (the new status). A provider that throws counts as Denied.");

            painter.Separator();
            painter.SubHeading("Your own plug");
            painter.Code(
                "[AllowAsSubContext]\n"
                + "public class UmpConsentContext : Context\n"
                + "{\n"
                + "    private IConsentService _consent;\n"
                + "    private UmpConsentProvider _provider;\n"
                + "\n"
                + "    public override void Setup()\n"
                + "    {\n"
                + "        base.Setup();\n"
                + "        _consent = InjectionBinderCrossContext.GetInstance<IConsentService>();\n"
                + "        _provider = new UmpConsentProvider();\n"
                + "        _consent.Plug(_provider);\n"
                + "    }\n"
                + "\n"
                + "    public override void DestroyContext()\n"
                + "    {\n"
                + "        _consent?.Unplug(_provider);\n"
                + "        base.DestroyContext();\n"
                + "    }\n"
                + "}");
        }
    }
}

#endif
