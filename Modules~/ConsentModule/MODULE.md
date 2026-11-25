# Consent

## Purpose
Asks the player for consent and, on iOS, for tracking, through one plugged provider, and
announces the answer once it is known, so a game hands it to its ad and analytics SDKs before
they start.

## Concepts
consent, GDPR, UMP, ATT, App Tracking Transparency, tracking, privacy options, privacy choices,
personalised ads, consent form, IConsentService, IConsentProvider, ConsentVO, ConsentStatus,
TrackingStatus, RD_Consent

## Using it
1. `ConsentServiceRoot` into the boot scene, Initialize Order -86, ahead of Analytics and Ads.
   `CD_Consent` ships filed on it: `RequestOnLaunch`, `DevelopmentConsent`,
   `DevelopmentTracking`; `RD_Consent` ships in its Shared Scriptables.
2. Two plugs on the Root's Sub Context Types, AutoSetup on: `UmpConsentServiceContext` for the
   consent form and `AttTrackingServiceContext` for the iOS tracking prompt - see their cards - or
   your own contexts plugging an `IConsentProvider` and an `ITrackingProvider`. Until a socket
   has a plug, the Editor and a Development Build answer from `CD_Consent`.
3. Ads and Analytics wait: a copy of `CD_Ads` with `InitializeOnLaunch` off, filed on
   `AdsServiceRoot` in place of the shipped one.
4. The hand-over is the game's Connector, because it knows both SDK modules - a sub-context in
   its ConnectorModule that hangs one sequence on `Outgoing.Resolved`. Which sequence depends on
   the form's plug, a choice the game makes once:
   - **With the UMP plug** - the answer is `Recorded`, and the player's choices are in the IAB TCF
     string MAX and AdMob read themselves. Hand the ad SDK no consent flag: `SetConsent` would
     override the string.
     ```csharp
     CommandBinder.Bind(_consent.Outgoing.Resolved)
         .ToSequence<ToAnalyticsConsentCommand>()     // Release(new AnalyticsConsentVO(c.AnalyticsStorage, c.AdStorage, c.AdUserData, c.AdPersonalization))
         .ToSequence<IAnalyticsService.Commands.SetConsent>()
         .ToSequence<IAdsService.Commands.Initialize>();
     ```
   - **With a plug that answers Granted or Denied** - your own form, or the stand-ins:
     ```csharp
     CommandBinder.Bind(_consent.Outgoing.Resolved)
         .ToSequence<ToAdsConsentCommand>()           // Release(new AdsConsentVO(consent.CanPersonalise))
         .ToSequence<IAdsService.Commands.SetConsent>()
         .ToSequence<ToAnalyticsConsentCommand>()     // Release(new AnalyticsConsentVO(c.AnalyticsStorage, c.AdStorage, c.AdUserData, c.AdPersonalization))
         .ToSequence<IAnalyticsService.Commands.SetConsent>()
         .ToSequence<IAdsService.Commands.Initialize>();
     ```
   The `To...` steps only translate the payload, and the analytics one is the same line in both:
   `ConsentVO`'s four readings are Google consent mode's four flags, read from the TCF purposes
   for a UMP answer and from Granted or Denied otherwise. `Initialize` a second time - after the
   privacy options answer - is logged and ignored.
5. A settings screen's privacy button binds `IConsentService.Commands.ShowPrivacyOptions`, and
   shows itself only when `RD_Consent.Current.PrivacyOptionsRequired` is set.

## Decisions
- **Two sockets, one plug each.** The consent form and the tracking prompt come from different
  SDKs, so each has its own interface; a second plug on either is refused with an error. The
  module owns the order - form, tracking prompt, publish - and the plugs own the SDK calls.
- **A TCF answer is `Recorded`, with its purposes.** UMP tells only that the form was answered;
  the choices are in the TCF string. Reporting `Granted` would hand MAX a consent the player may
  have refused, so the ad SDKs read the string themselves. The plug reads `IABTCF_PurposeConsents`
  back (`ITcfConsentProvider`) and `ConsentVO` maps it the way Google consent mode does:
  `AdStorage` purpose 1, `AdUserData` purposes 1 and 7, `AdPersonalization` purposes 3 and 4.
  TCF has no analytics purpose, so `AnalyticsStorage` follows purpose 1.
- **No plug in a release build answers Denied, with an error.** The Editor and a Development
  Build plug a stand-in that answers from `CD_Consent`; a store build never uses personal data
  because a plug was forgotten, and ads still run, non-personalised. A missing tracking plug is
  an error on iOS only, the one platform with a prompt.
- **Knows no other module.** The answer goes out as `ConsentVO` with Google consent mode's four
  flags plus `CanPersonalise` and `CanTrack`; building each SDK's own value object is the
  Connector's translation, so Consent installs without Ads or Analytics.
- **Announced and filed.** `Outgoing.Resolved` is for the Connector, wired before Launch;
  `RD_Consent` is for a module that reads later. Resolved goes out again after the privacy
  options answer.
- **A provider that throws counts as Denied; a second answer is ignored.** The form itself is
  waited on without a bound (`[LongRetain]`): a player may leave it up.
- **Nothing persisted.** The SDKs remember the answer; the module asks them every boot.

## Known gaps
- No CCPA do-not-sell or COPPA age flag: `AdsConsentVO`'s other two flags are left to the game.

Version: 1.0.1

<!-- FLOWIOC:BEGIN version=1 hash=40e350e1 | generated by Tools/FlowIoC/Module Scanner - do not edit inside this block -->
**Kind** Main · **Assemblies** Modules.Consent, Modules.Consent.Shared, Modules.Consent.Signals
**Root** ConsentServiceRoot → ConsentServiceContext
**Outgoing** Resolved(ConsentVO)
**Publishes** ConsentStatus, ConsentVO, RD_Consent, TrackingStatus
**Services** IConsentProvider, IConsentService, ITcfConsentProvider, ITrackingProvider
**Sub modules** AttTrackingModule (Sub) · ConsentTestModule (Test) · UmpConsentModule (Sub)
<!-- FLOWIOC:END -->
