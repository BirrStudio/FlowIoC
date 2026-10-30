#if UNITY_EDITOR

using System;
using FlowIoC.BaseModule.Injectable.Components;
using FlowIoC.BaseModule.ViewsMediators.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.LocalizationModule.LocalizationTestModule.ViewsMediators
{
    /// <summary>Scene references and raw input: a button per language, a level button, the variable line.</summary>
    [RequireComponent(typeof(ViewInjector))]
    public class LocalizationTestView : MonoBehaviour, IView
    {
        public bool IsRegistered { get; set; }

        [SerializeField] private Button _english;
        [SerializeField] private Button _turkish;
        [SerializeField] private Button _levelUp;
        [SerializeField] private TMP_Text _line;
        [SerializeField] private TMP_Text _language;

        public Action<string> OnLanguagePressed;
        public Action OnLevelUpPressed;

        public void SetLine(string line, string language)
        {
            _line.text = line;
            _language.text = language;
        }

        private void OnEnable()
        {
            _english.onClick.AddListener(English);
            _turkish.onClick.AddListener(Turkish);
            _levelUp.onClick.AddListener(LevelUp);
        }

        private void OnDisable()
        {
            _english.onClick.RemoveListener(English);
            _turkish.onClick.RemoveListener(Turkish);
            _levelUp.onClick.RemoveListener(LevelUp);
        }

        private void English() => OnLanguagePressed?.Invoke("en");

        private void Turkish() => OnLanguagePressed?.Invoke("tr");

        private void LevelUp() => OnLevelUpPressed?.Invoke();
    }
}

#endif
