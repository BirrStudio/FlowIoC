using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Modules.LocalizationModule.Entities
{
    /// <summary>
    /// Put beside a TMP text whose words never change - a title, a button label - and pick its key.
    /// It writes the selected language's text, and writes it again whenever the language changes,
    /// with no Command and no Mediator: nothing is decided. A text that carries a value is filled
    /// by the Command that opens its screen, through <c>ILocalizationService.Get</c>.
    ///
    /// The font is optional. Left empty, the TMP keeps its own; set, a language whose script the
    /// game's font lacks - Chinese, Arabic - gets the font its table names.
    ///
    /// It subscribes in OnEnable and lets go in OnDisable, so a pooled screen that opens many
    /// times holds one subscription while it is up and none while it waits in the pool.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    [DisallowMultipleComponent]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private LocalizedString _text = new();
        [SerializeField] private LocalizedTmpFont _font = new();

        private TMP_Text _label;
        private TMP_FontAsset _ownFont;

        /// <summary>The key's entry, for the editor that picks or creates it.</summary>
        public LocalizedString Text => _text;

        private void Awake()
        {
            _label = GetComponent<TMP_Text>();
            _ownFont = _label.font;
        }

        private void OnEnable()
        {
            if (!_text.IsEmpty)
                _text.StringChanged += Write;

            if (!_font.IsEmpty)
                _font.AssetChanged += Write;
        }

        private void OnDisable()
        {
            _text.StringChanged -= Write;
            _font.AssetChanged -= Write;
        }

        private void Write(string text) => _label.text = text;

        /// <summary>A language the font table names nothing for goes back to the label's own font.</summary>
        private void Write(TMP_FontAsset font) => _label.font = font != null ? font : _ownFont;
    }
}
