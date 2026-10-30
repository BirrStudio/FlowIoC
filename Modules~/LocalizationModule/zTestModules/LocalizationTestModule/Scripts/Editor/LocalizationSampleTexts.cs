#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine.Localization.Tables;
using UnityEngine.SceneManagement;

namespace Modules.LocalizationModule.LocalizationTestModule.Editor
{
    /// <summary>
    /// The sample's two texts, in the Game table only while its scene is open. A project that
    /// installs the module has none of them, so opening the scene adds them; closing it, quitting
    /// Unity or starting a build takes them out again, so the game's table, its sheet and its build
    /// never carry a row the game did not write.
    /// </summary>
    [InitializeOnLoad]
    internal class LocalizationSampleTexts
    {
        private const string SCENE = "LocalizationTestScene";
        private const string TABLE = "Game";

        private readonly (string key, string language, string text)[] _texts =
        {
            ("sample.title", "en", "Localization sample"),
            ("sample.title", "tr", "Yerelleştirme örneği"),
            ("sample.levels", "en", "{0:plural:One level unlocked|{} levels unlocked}"),
            ("sample.levels", "tr", "{0} seviye açıldı")
        };

        static LocalizationSampleTexts()
        {
            var texts = new LocalizationSampleTexts();
            EditorSceneManager.sceneOpened += (scene, _) => texts.Opened(scene);
            EditorSceneManager.sceneClosing += (scene, _) => texts.Closing(scene);
            EditorApplication.quitting += texts.Remove;

            // The scene Unity reopens at startup raises no sceneOpened.
            EditorApplication.delayCall += texts.EnsureIfOpen;
        }

        private void Opened(Scene scene)
        {
            if (scene.name == SCENE)
                Ensure();
        }

        private void Closing(Scene scene)
        {
            if (scene.name == SCENE)
                Remove();
        }

        public void EnsureIfOpen()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i).name == SCENE)
                {
                    Ensure();
                    return;
                }
            }
        }

        public void Ensure()
        {
            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(TABLE);

            if (collection == null)
                return;

            bool added = false;

            foreach ((string key, string language, string text) in _texts)
                added |= Add(collection, key, language, text);

            if (added)
                AssetDatabase.SaveAssets();
        }

        public void Remove()
        {
            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(TABLE);

            if (collection == null)
                return;

            bool removed = false;

            foreach ((string key, _, _) in _texts)
            {
                if (!collection.SharedData.Contains(key))
                    continue;

                collection.RemoveEntry(key);
                removed = true;
            }

            if (!removed)
                return;

            EditorUtility.SetDirty(collection.SharedData);

            foreach (StringTable table in collection.StringTables)
                EditorUtility.SetDirty(table);

            AssetDatabase.SaveAssets();
        }

        private bool Add(StringTableCollection collection, string key, string language, string text)
        {
            bool added = false;

            foreach (StringTable table in collection.StringTables)
            {
                if (table.LocaleIdentifier.Code != language || table.GetEntry(key) != null)
                    continue;

                StringTableEntry entry = table.AddEntry(key, text);
                entry.IsSmart = true;
                EditorUtility.SetDirty(table);
                EditorUtility.SetDirty(table.SharedData);
                added = true;
            }

            return added;
        }
    }

    /// <summary>A build never carries the sample's texts: they leave the table before it and come back after it when the scene is still open.</summary>
    internal class LocalizationSampleTextsBuild : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private readonly LocalizationSampleTexts _texts = new();

        /// <summary>Ahead of Unity Localization (0) and the Addressables content build (1), which pack the tables.</summary>
        public int callbackOrder => -100;

        public void OnPreprocessBuild(BuildReport report) => _texts.Remove();

        public void OnPostprocessBuild(BuildReport report) => _texts.EnsureIfOpen();
    }
}

#endif
