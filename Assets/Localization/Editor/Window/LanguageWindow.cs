using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Edit = Localization.Core.EditorImprovement;

namespace Localization.Core
{
    /// <summary>
    /// Main window of the localisation tool
    /// </summary>
    public class LanguageWindow : EditorWindow
    {
        public LanguageEditor lgSentencePage;
        public LanguageEditorSettings lgSettingsPage;
        //static string folderGeneratedPath = "";
        //
        //public static string GENERATED_PATH
        //{
        //	get
        //	{
        //		if(folderGeneratedPath == "")GetGeneratedFolder();
        //		return folderGeneratedPath;
        //	}
        //}
        bool initialized;
        public bool isCompiling;
        int windowsTab;

        // Create the localization editor window
        [MenuItem("Tools/Localization")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(LanguageWindow));
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        public static void Preload()
        {
            GetWindow().Close();
        }

        public static EditorWindow GetWindow()
        {
            return EditorWindow.GetWindow(typeof(LanguageWindow));
        }

        public void SetWindowTab(int tab)
        {
            windowsTab = tab;
        }

        /// <summary>
        /// Find the path to the folder with generated content
        /// </summary>
        //static void GetGeneratedFolder()
        //{
        //	string[] res = System.IO.Directory.GetDirectories(Application.dataPath, "Generated", System.IO.SearchOption.AllDirectories);
        //	if (res.Length != 0)
        //	{
        //		string path = res[0].Replace("\\", "/");
        //		folderGeneratedPath = path;
        //	}
        //}


        void CheckLanguageFolder()
        {
            string streamingFolder = Application.streamingAssetsPath;
            if (!FileManager.IsValidDirector(streamingFolder))
            {
                FileManager.CreateFolder(streamingFolder);
            }

            string languageFolder = streamingFolder + "/Languages";
            if (!FileManager.IsValidDirector(languageFolder))
            {
                FileManager.CreateFolder(languageFolder);
            }
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Localization", "Windows for the localization");
            //GetGeneratedFolder();
            CheckLanguageFolder();
            LanguageManager.LoadLanguages();
            lgSentencePage = new LanguageEditor(this);
            lgSettingsPage = new LanguageEditorSettings(this);
            lgSentencePage.Enable();
            lgSettingsPage.Enable();
            initialized = true;
        }

        private void OnDisable()
        {
            initialized = false;
            lgSentencePage.Disable();
            lgSettingsPage.Disable();
        }

        private void OnGUI()
        {
            if (!initialized) return;
            if (EditorApplication.isCompiling)
            {
                isCompiling = true;
                EditorGUI.LabelField(new Rect(position.width / 3, this.position.height / 3, this.position.width, this.position.height / 2), "APPLICATION IS COMPILING...", EditorStyles.boldLabel);
                EditorGUI.BeginDisabledGroup(true);
            }
            else if (isCompiling)
            {
                isCompiling = false;
                lgSentencePage.OnCompiled();
                lgSettingsPage.OnCompiled();
            }

            Edit.Row(() =>
            {
                windowsTab = GUILayout.SelectionGrid(windowsTab, new string[] { "Languages", "Settings" }, 2, EditorStyles.toolbarButton);
                if (GUILayout.Button("Exit", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    this.Close();
                    return;
                }
            });

            if (windowsTab == 0)
            {
                lgSentencePage.DrawEditor();
            }
            else if (windowsTab == 1)
            {
                lgSettingsPage.DrawEditor();
            }

            if (EditorApplication.isCompiling) EditorGUI.EndDisabledGroup();
        }

        public void SaveAll()
        {

            lgSettingsPage.Save();
            lgSentencePage.Save();

            LanguageManager.LoadLanguages();
            LoadAll();
        }

        public void LoadAll()
        {
            lgSentencePage.Load();
            lgSettingsPage.Load();
        }

        // Make sure the window is visible to the user (WIP)
        //void CheckWindowVisibility()
        //{
        //	Rect pos = new Rect(position);
        //	if (pos.width < 200) pos.width = 500;
        //	if (pos.height < 200) pos.height = 500;
        //	if (pos.x < 0) pos.x = 500;
        //	if (pos.y < 0 || pos.y > (Screen.height - 50)) pos.y = 500;
        //	position = pos;
        //}

        // Change sentence using tab
        // Not working correctly (WIP)
        //public bool TabPressed()
        //{
        //	Event e = Event.current;
        //	if(e.type == EventType.KeyDown && Event.current.keyCode == (KeyCode.Tab) && !tabDown)
        //	{
        //		tabDown = true;
        //		Debug.Log("EVT");
        //		e.Use();
        //		return true;
        //	}
        //	else if (e.type == EventType.KeyUp && Event.current.keyCode == (KeyCode.Tab) && tabDown)
        //	{
        //		e.Use();
        //		tabDown = false;
        //	}
        //	
        //	return false;
        //}

        public static AudioClip PathToAsset(string path)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        public static string AssetToPath(Object clip)
        {
            if (clip != null)
            {
                return AssetDatabase.GetAssetPath(clip);
            }
            return "";
        }
    }
}