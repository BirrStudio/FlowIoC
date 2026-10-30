#if UNITY_EDITOR

using Modules.LocalizationModule.Constants;
using Modules.LocalizationModule.Entities;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Modules.LocalizationModule.Editor
{
    /// <summary>
    /// Unity's own pickers for the key and the font, and under them Create key: a new key in the
    /// Game table whose English is what the label already says, assigned to this component. A
    /// label is localized where it is designed, without opening the panel.
    /// </summary>
    [CustomEditor(typeof(LocalizedText))]
    [CanEditMultipleObjects]
    internal class LocalizedTextEditor : UnityEditor.Editor
    {
        private readonly LocalizationProject _project = new();
        private readonly LocalizationKeysWriter _keys = new();
        private LocalizationTableTools _tables;
        private string _newKey = string.Empty;

        private void OnEnable() => _tables = new LocalizationTableTools(_project);

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (targets.Length != 1)
                return;

            EditorGUILayout.Space();

            if (!_project.IsSetUp)
            {
                EditorGUILayout.HelpBox("No localization in the project yet. Tools/FlowIoC-Modules/Localization/Panel sets it up.", MessageType.Info);
                return;
            }

            var component = (LocalizedText)target;
            TMP_Text label = component.GetComponent<TMP_Text>();

            EditorGUILayout.BeginHorizontal();
            _newKey = EditorGUILayout.TextField("New key", _newKey);

            string key = _newKey.Trim();
            bool taken = key.Length > 0 && _tables.Has(key);

            using (new EditorGUI.DisabledScope(key.Length == 0 || taken))
            {
                if (GUILayout.Button("Create key", GUILayout.Width(90f)))
                    Create(component, key, label != null ? label.text : string.Empty);
            }

            EditorGUILayout.EndHorizontal();

            if (taken)
                EditorGUILayout.HelpBox($"{key} is already in the Game table - pick it above.", MessageType.None);
        }

        private void Create(LocalizedText component, string key, string english)
        {
            _tables.Write(key, LocalizationConstants.SOURCE_LANGUAGE, english);
            _tables.Save();

            Undo.RecordObject(component, "Localization key");
            // By id, the way Unity's own picker stores it: a key renamed later still resolves, and
            // the panel's Find unused finds the reference.
            component.Text.SetReference(LocalizationConstants.TABLE, _project.Collection.SharedData.GetId(key));
            EditorUtility.SetDirty(component);

            _keys.Write(_tables.Keys());
            _newKey = string.Empty;
        }
    }
}

#endif
