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
                "Two sockets, the shipped UMP and ATT plugs, the stand-ins, and your own.",
                "A provider shows the form it is told to and answers once; the order and the publishing "
                + "are the module's.")
        };

        public override string InstalledHint =>
            "Drop ConsentServiceRoot into your boot scene ahead of Analytics and Ads, list the UMP and ATT "
            + "plugs on it, and hang the hand-over on Outgoing.Resolved in your Connector; the Setup tab has the steps.";

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
                "Two sockets: IConsentProvider for the consent form and ITrackingProvider for the "
                + "tracking prompt. The Google UMP and Apple ATT plugs ship, each compiled only when its "
                + "SDK is in the project.");
            painter.Bullet(
                "Two steps a game binds: Commands.Request, when CD_Consent.RequestOnLaunch is off, and "
                + "Commands.ShowPrivacyOptions, for a settings button.");
            painter.Bullet(
                "ConsentVO with Google consent mode's four flags - AdStorage, AdUserData, AdPersonalization, "
                + "AnalyticsStorage - plus CanPersonalise and CanTrack, announced on Outgoing.Resolved and "
                + "filed in RD_Consent for a module that reads it later.");

            painter.Separator();
            painter.SubHeading("When it decides");
            painter.Paragraph(
                "At Launch, unless CD_Consent says the game asks: the form, then the tracking prompt, "
                + "then the answer published. Each is waited on for as long as the player leaves it up. "
                + "The privacy options form publishes again; it is refused before the first answer.");
            painter.Note(
                "With no plug, the Editor and a Development Build plug a stand-in that answers "
                + "DevelopmentConsent and DevelopmentTracking from CD_Consent, with a warning. A release "
                + "build reports the missing plug as an error and answers ReleaseConsentWithoutPlug - "
                + "Denied as shipped, so nothing personal is used because a plug was forgotten; a tester "
                + "build made before the consent SDK is in sets NotRequired and stays measured.");

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
                + "ReleaseConsentWithoutPlug (Denied), DevelopmentConsent (Granted), DevelopmentTracking "
                + "(Authorized); RD_Consent ships in its Shared Scriptables.");
            painter.Image(_images.Get("ConsentRootHierarchy.png"),
                "ConsentServiceRoot in MainScene, in the Services band.");
            painter.Image(_images.Get("ConsentRootAdapter.png"),
                "Its adapter as it ships: CD_Consent in the Scriptable Map, RD_Consent in the Shared one.");

            painter.Separator();
            painter.SubHeading("The plug");
            painter.Note(
                "Important: a plug is an entry on ConsentServiceRoot's Sub Context Types with AutoSetup "
                + "on - UmpConsentServiceContext for the form, AttTrackingServiceContext for the iOS "
                + "prompt. With none, a release build answers ReleaseConsentWithoutPlug - Denied as shipped "
                + "- for every player: the error is the only report, and neither personalised ads nor "
                + "analytics run. A tester build without the SDK sets NotRequired there.");
            painter.Note(
                "The UMP plug compiles only when com.google.ads.mobile is in the manifest (OpenUPM); from "
                + "the .unitypackage, add FLOWIOC_GOOGLE_UMP to Scripting Define Symbols. The ATT plug "
                + "needs com.unity.ads.ios-support and an NSUserTrackingUsageDescription in Info.plist; "
                + "iOS refuses the prompt without it.");

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
                "A sub-context in the game's ConnectorModule, because it knows both SDK modules. Which "
                + "sequence it hangs on Resolved depends on the form's plug - a choice the game makes once. "
                + "The To steps only translate the payload and hand it on with Release.");
            painter.Note(
                "Important: with the UMP plug, hand the ad SDK no consent flag. The answer is Recorded and "
                + "the player's choices are in the IAB TCF string MAX and AdMob read themselves; a "
                + "SetConsent would override the string, and nothing reports it.");
            painter.Paragraph(
                "The analytics step is the same in both: ConsentVO's readings are Google consent mode's "
                + "flags - from the TCF purposes for a UMP answer (AdStorage purpose 1, AdUserData 1 and "
                + "7, AdPersonalization 3 and 4, AnalyticsStorage 1), and from Granted or Denied otherwise. "
                + "ad_personalization takes CanTrack, which adds the ATT answer, so an iOS player who "
                + "refused tracking is not profiled.");
            painter.Code(
                "CommandBinder.Bind(_consent.Outgoing.Resolved)\n"
                + "    .ToSequence<ToAnalyticsConsentCommand>()       // Release(new AnalyticsConsentVO(c.AnalyticsStorage, c.AdStorage, c.AdUserData, c.CanTrack))\n"
                + "    .ToSequence<IAnalyticsService.Commands.SetConsent>()\n"
                + "    .ToSequence<IAdsService.Commands.Initialize>();",
                "With the UMP plug.");
            painter.Code(
                "CommandBinder.Bind(_consent.Outgoing.Resolved)\n"
                + "    .ToSequence<ToAdsConsentCommand>()             // Release(new AdsConsentVO(consent.CanPersonalise))\n"
                + "    .ToSequence<IAdsService.Commands.SetConsent>()\n"
                + "    .ToSequence<ToAnalyticsConsentCommand>()       // Release(new AnalyticsConsentVO(c.AnalyticsStorage, c.AdStorage, c.AdUserData, c.CanTrack))\n"
                + "    .ToSequence<IAnalyticsService.Commands.SetConsent>()\n"
                + "    .ToSequence<IAdsService.Commands.Initialize>();",
                "With a plug that answers Granted or Denied - your own form, or the stand-ins.");
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
            painter.SubHeading("The sockets");
            painter.Paragraph(
                "IConsentProvider has two calls, each answered exactly once on the main thread: "
                + "RequestConsent (the status, and whether privacy options must be offered) and "
                + "ShowPrivacyOptions (the new status). ITrackingProvider has one, RequestTracking "
                + "(NotApplicable where the platform has no prompt). A provider that throws counts as "
                + "Denied.");

            painter.Separator();
            painter.SubHeading("The shipped plugs");
            painter.Bullet(
                "UmpConsentServiceContext - Google UMP. A form the player answered is Recorded, with "
                + "the TCF purposes read back; NotRequired passes as it is, a form that could not be "
                + "shown is Denied.");
            painter.Bullet(
                "AttTrackingServiceContext - Apple ATT. A status already decided is answered without "
                + "the prompt; off an iPhone the answer is NotApplicable.");

            painter.Separator();
            painter.SubHeading("Your own plug");
            painter.Code(
                "[AllowAsSubContext]\n"
                + "public class MyConsentContext : Context\n"
                + "{\n"
                + "    private IConsentService _consent;\n"
                + "    private MyConsentProvider _provider;\n"
                + "\n"
                + "    public override void Setup()\n"
                + "    {\n"
                + "        base.Setup();\n"
                + "        _consent = InjectionBinderCrossContext.GetInstance<IConsentService>();\n"
                + "        _provider = new MyConsentProvider();\n"
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
