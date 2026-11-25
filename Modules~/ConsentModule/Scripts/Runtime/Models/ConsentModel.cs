using FlowIoC.BaseModule.Adapters;
using FlowIoC.BaseModule.Constructables;
using FlowIoC.BaseModule.Injectable.Attributes;
using FlowIoC.ConsoleModule;
using Modules.ConsentModule.Data.UnityObjects;
using Modules.ConsentModule.Data.ValueObjects;
using Modules.ConsentModule.RootsContexts;
using Modules.ConsentModule.Services;
using Modules.ConsentModule.Shared.Data.UnityObjects;
using Modules.ConsentModule.Shared.Data.ValueObjects;
using Modules.ConsentModule.Shared.Enums;
using UnityEngine;

namespace Modules.ConsentModule.Models
{
    /// <summary>
    /// PostConstruct takes CD_Consent and RD_Consent off the Root's adapter; the adapter reports a
    /// missing filing itself, the settings keep their defaults, and the result is still kept here.
    /// </summary>
    internal class ConsentModel : IConsentModel, IConstructable
    {
        [Inject(nameof(ConsentServiceContext))] private GameObject _root { get; set; }

        private RD_Consent _published;
        private ConsentStatus _consent;
        private TrackingStatus _tracking;
        private bool _privacyOptionsRequired;
        private string _purposes = string.Empty;

        public bool IsPostConstructed { get; set; }

        public bool IsDeconstructed { get; set; }

        public ConsentSettingsCVO Settings { get; private set; } = new();

        public IConsentProvider Provider { get; private set; }

        public ITrackingProvider TrackingProvider { get; private set; }

        public bool IsAsking { get; private set; }

        public bool IsResolved { get; private set; }

        public ConsentVO Current { get; private set; }

        public void PostConstruct()
        {
            RootAdapter adapter = _root != null ? _root.GetComponent<RootAdapter>() : null;

            if (adapter == null)
            {
                FlowLogger.LogError("ConsentServiceRoot has no RootAdapter, so CD_Consent and RD_Consent cannot be read; the defaults hold.", _root);
                return;
            }

            CD_Consent asset = adapter.GetScriptable<CD_Consent>();
            ApplySettings(asset != null ? asset.Settings : null);
            ApplyPublished(adapter.GetScriptable<RD_Consent>());
        }

        /// <summary>The settings to hold; null keeps the defaults. Internal so a test sets them without an adapter.</summary>
        internal void ApplySettings(ConsentSettingsCVO settings)
        {
            if (settings != null)
                Settings = settings;
        }

        /// <summary>The asset the result is written into. Internal so a test hands in the one the adapter would.</summary>
        internal void ApplyPublished(RD_Consent published) => _published = published;

        public void Plug(IConsentProvider provider) => Provider = provider;

        public void Unplug()
        {
            Provider = null;
            IsAsking = false;
        }

        public void PlugTracking(ITrackingProvider provider) => TrackingProvider = provider;

        public void UnplugTracking() => TrackingProvider = null;

        public void SetAsking(bool asking) => IsAsking = asking;

        public void SetConsent(ConsentStatus consent, bool privacyOptionsRequired)
        {
            _consent = consent;
            _privacyOptionsRequired = privacyOptionsRequired;
        }

        public void SetConsent(ConsentStatus consent) => _consent = consent;

        public void SetTracking(TrackingStatus tracking) => _tracking = tracking;

        public void SetPurposes(string purposeConsents) => _purposes = purposeConsents ?? string.Empty;

        public ConsentVO Publish()
        {
            Current = new ConsentVO(_consent, _tracking, _privacyOptionsRequired, _purposes);
            IsResolved = true;

            if (_published != null)
                _published.Write(Current);

            return Current;
        }
    }
}
