using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Localization.Core;

namespace Localization
{
    /// <summary>
    /// Load the language file and provide access to the sentences
    /// </summary>
    public static class LanguageManager
    {
        public struct LanguageFile
        {
            public string name;
            public string path;
        }

        /// <summary>
        /// Key : language name
        /// Value : language path
        /// </summary>
        public static List<LanguageFile> languageFiles { get; private set; }

        /// <summary>
        /// File containing all text and audio for the current selected language
        /// </summary>
        static LanguageData languageData = null;

        /// <summary>
        /// Use sorted list to optimize data access
        /// </summary>
        public static SortedList<int, Localized> orderedSentences;

        public static List<Sprite> flags;

        /// <summary>
        /// The key used to save the last used language
        /// </summary>
        public const string PLAYER_PREF_KEY = "LANGUAGE_KEY";

        /// <summary>
        /// Path where the languages files are stored
        /// </summary>
        // If you change one of this values you have to move and rename languages files manually
        public static string LG_FILE_PATH
        {
#if (UNITY_ANDROID || UNITY_WEBGL) && !UNITY_EDITOR
            get { return "Languages"; }
#else
            get { return (Path.Combine(Application.streamingAssetsPath, "Languages")); }
#endif
        }

        static string GENERATED_FILE_PATH { get { return Path.Combine(Application.dataPath, "Localization/Generated/Resources/Languages"); } }

        public const string FILE_PREFIX = "";
        public const string FILE_EXTENSION = ".loca";
        public const string FLAG_PREFIX = "SP_Flag_";
        public const string INFO_FILE_NAME = "info";

        /// <summary>
        /// Delegate called everytime the language is changed
        /// </summary>
        /// <param name="lg">New selected language</param>
        public delegate void OnLanguageChanged();
        public static OnLanguageChanged LanguageChanged;
        private static Localized cachedSentence;
        private static bool needArabicFix;

        public class SwitcherData
        {
            public string SearchPattern { get; private set; }
            public string ReplacePattern { get; private set; }

            public SwitcherData(string search, string replace)
            {
                SearchPattern = search.ToLower();
                ReplacePattern = replace.ToLower();
            }
        }

        static List<SwitcherData> switchers;

        /// <summary>
        /// Search for a defined pattern in the sentence code and if found replace the pattern with the second string in order to replace the sentence.
        /// </summary>
        /// <param name="searchPattern">String pattern to find in the code</param>
        /// <param name="replacePattern">String pattern used to replace the searchPattern</param>
        public static void AddSwitcher(string searchPattern, string replacePattern)
        {
            AddSwitcher(new SwitcherData(searchPattern, replacePattern));
        }

        public static void AddSwitcher(SwitcherData data)
        {
            if (switchers == null)
            {
                switchers = new List<SwitcherData>();
            }
            if (!switchers.Any(x => x.SearchPattern == data.SearchPattern))
                switchers.Add(data);
        }

        /// <summary>
        /// Remove all switcher search keys that correspond
        /// </summary>
        /// <param name="searchKey"></param>
        public static void RemoveSwitcherBySearchKey(string searchKey)
        {
            if (switchers == null)
            {
                return;
            }
            searchKey = searchKey.ToLower();
            switchers.RemoveAll(x => x.SearchPattern == searchKey);
        }
        public static void RemoveSwitcher(string searchKey, string replaceKey)
        {
            if (switchers == null)
            {
                return;
            }
            searchKey = searchKey.ToLower();
            replaceKey = replaceKey.ToLower();
            switchers.RemoveAll(x => x.SearchPattern == searchKey && x.ReplacePattern == replaceKey);
        }

        public static int SwitcherCount
        {
            get
            {
                if (switchers == null) return 0;
                return switchers.Count;
            }
        }
        public static void RemoveAllSwitcher()
        {
            switchers = new List<SwitcherData>();
        }


        /// <summary>
        /// RTL language is read from right to left
        /// </summary>
        public static bool IsCurrentRTL()
        {
            if (languageData != null)
                return languageData.isRightToLeft;
            else
                return false;
        }

        /// <summary>
        /// Not optimized should not be used often
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int GetCode(string name)
        {
            if (languageData == null)
            {
                InitialiseToDefault();
            }
            name = name.ToLower();
            if (languageData != null)
            {
                foreach (var item in languageData.localizedList)
                {
                    if (item.code.ToLower().Equals(name))
                    {
                        return item.id;
                    }
                }
            }
            else
            {
                Debug.LogError("LanguageManager is not initialized or no language is found");
            }
            return -1;
        }

        public static string CurrentLanguage
        {
            get
            {
                if (languageData == null)
                {
                    if (!InitialiseToDefault()) return "";
                }
                return languageData.name;
            }
            set
            {
                if (languageData == null || languageData.name != value)
                {
                    InitialiseLanguage(value);
                }
            }
        }

        // Get last used language or use the language at index 0
        private static bool InitialiseToDefault()
        {
            LoadLanguages();

            if (languageFiles != null && languageFiles.Count > 0)
            {
                string lg = PlayerPrefs.GetString(PLAYER_PREF_KEY, "");
                if (DoesLanguageExist(lg))
                {
                    return InitialiseLanguage(lg);
                }
                else if (DoesLanguageExist(languageFiles[0].name))
                {
                    return InitialiseLanguage(languageFiles[0].name);
                }
            }
            Debug.LogError("No Languages founds");
            return false;
        }

        private static bool InitialiseLanguage(string lg)
        {
            if (string.IsNullOrEmpty(lg))
            {
                Debug.LogError("Wrong string passed as language");
                return false;
            }


            lg = lg.ToLower();
            LoadLanguages();
            if (!DoesLanguageExist(lg))
            {
                Debug.LogError("No language found with the name " + lg);
                return false;
            }

            languageData = new LanguageData
            {
                name = lg
            };
            LoadData(ref languageData);
            PlayerPrefs.SetString(PLAYER_PREF_KEY, lg);

            orderedSentences = new SortedList<int, Localized>();

            if (languageData != null)
            {
                if (languageData.localizedList != null)
                {
                    for (int i = 0; i < languageData.localizedList.Count; i++)
                    {
                        orderedSentences.Add(languageData.localizedList[i].id, languageData.localizedList[i]);
                    }
                }
                else
                {
                    Debug.LogError("LocalizedList could not be loaded");
                }
            }
            else
            {
                Debug.LogError("LanguageData could not be loaded");
            }

            LoadFlags();

            needArabicFix = languageData.name.ToLower() == "arabic";

            if (LanguageChanged != null) LanguageChanged();
            return true;
        }

        public static string GetCompleteFilePath(string lgName)
        {
            if (languageFiles == null || languageFiles.Count == 0)
            {
                languageFiles = new List<LanguageFile>();
            }

            foreach (var file in languageFiles)
            {
                if (file.name == lgName)
                {
                    return file.path;
                }
            }

            // Path not found, creating new path
            string path = "";
            lgName = lgName.ToLower();
            path = Path.Combine(LG_FILE_PATH, lgName + FILE_EXTENSION);

            LanguageFile fileInfo = new LanguageFile();
            fileInfo.name = lgName;
            fileInfo.path = path;
            languageFiles.Add(fileInfo);

            return path;
        }



#if UNITY_EDITOR
        /// <summary>
        /// Save the current language file (Should only be used in editor mode)
        /// </summary>
        public static void SaveData(LanguageData data)
        {
            FileManager.CreateFolder(LG_FILE_PATH);
            FileManager.Save<LanguageData>(GetCompleteFilePath(data.name), data);

            //if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android
            //|| UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
            {
                string completePathWithName = Path.Combine(GENERATED_FILE_PATH, data.name);
                completePathWithName = Path.ChangeExtension(completePathWithName, "json");
                FileManager.CreateFolder(GENERATED_FILE_PATH);
                FileManager.Save<LanguageData>(completePathWithName, data);
            }

        }

        public static void CreateInfoFile()
        {
            string lginfo = "";
            foreach (var item in languageFiles)
            {
                lginfo += item.name + Environment.NewLine;
            }
            FileManager.Save(Path.Combine(LG_FILE_PATH, INFO_FILE_NAME), lginfo, true);

            //if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android
            //|| UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
            {
                string completePathWithName = Path.Combine(GENERATED_FILE_PATH, INFO_FILE_NAME);
                completePathWithName = Path.ChangeExtension(completePathWithName, "txt");
                FileManager.Save(completePathWithName, lginfo, true);
            }

            UnityEditor.AssetDatabase.Refresh();
        }
#endif

        public static bool InfoFileExist()
        {
            return FileManager.IsValidFile(Path.Combine(LG_FILE_PATH, INFO_FILE_NAME));
        }

        /// <summary>
        /// Load the language file
        /// </summary>
        public static void LoadData(ref LanguageData data)
        {
            string path = GetCompleteFilePath(data.name);
            FileManager.Load<LanguageData>(path, ref data);
        }

        /// <summary>
        /// Return a sentence in the current language
        /// </summary>
        /// <param name="code">hash of the identifier of the sentence</param>
        public static string GetSentence(int code)
        {
            if (languageData == null)
            {
                InitialiseToDefault();
            }
            if (orderedSentences.TryGetValue(code, out cachedSentence))
            {
                SwitcherData switcher = SwitcherPatternMatch(cachedSentence.code);
                if (switcher != null)
                {
                    FindReplaceSwitch(ref cachedSentence, switcher);
                }
                if (needArabicFix && cachedSentence.text.Length > 0)
                {
                    return ArabicFixerTool.FixLine(cachedSentence.text);
                }
                return cachedSentence.text;
            }
            return "";
        }

        private static void FindReplaceSwitch(ref Localized localized, SwitcherData switcher)
        {
            string code = localized.code;
            code = code.Replace(switcher.SearchPattern, switcher.ReplacePattern);
            int index = GetCode(code);

            if (index >= 0)
            {
                if (orderedSentences.TryGetValue(index, out Localized newCached))
                {
                    localized = newCached;
                }
            }
        }

        /// <summary>
        /// Return an audio clip in the current language
        /// </summary>
        /// <param name="code">hash of the identifier of the audio</param>
        public static AudioClip GetAudio(int code)
        {
            if (languageData == null)
            {
                InitialiseToDefault();
            }
            if (orderedSentences.TryGetValue(code, out cachedSentence))
            {
                SwitcherData switcher = SwitcherPatternMatch(cachedSentence.code);
                if (switcher != null)
                {
                    FindReplaceSwitch(ref cachedSentence, switcher);
                }
                return Resources.Load<AudioClip>(cachedSentence.clip);
            }
            return null;
        }

        /// <summary>
        /// Return a texture in the current language
        /// </summary>
        /// <param name="code">hash of the identifier of the audio</param>
        public static Texture GetTexture(int code)
        {
            if (languageData == null)
            {
                InitialiseToDefault();
            }
            if (orderedSentences.TryGetValue(code, out cachedSentence))
            {
                SwitcherData switcher = SwitcherPatternMatch(cachedSentence.code);
                if (switcher != null)
                {
                    FindReplaceSwitch(ref cachedSentence, switcher);
                }
                return Resources.Load<Texture>(cachedSentence.texture);
            }
            return null;
        }

        static SwitcherData SwitcherPatternMatch(string code)
        {
            if (switchers != null && switchers.Count > 0)
            {
                foreach (var item in switchers)
                {
                    if (code.Contains(item.SearchPattern))
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        public static bool TryGetCurrentFlag(out Sprite flag)
        {
            flag = null;
            if (languageData != null)
            {
                flag = GetFlag(languageData.name);
            }
            if (flag == null) return false;
            return true;
        }

        public static Sprite GetFlag(string language)
        {
            LoadFlags();
            if (flags != null)
            {
                foreach (Sprite f in flags)
                {
                    if (f.name.ToLower().Contains(language.ToLower()))
                    {
                        return f;
                    }
                }
            }
            return null;
        }

        static void LoadFlags()
        {
            if (flags == null)
            {
                flags = new List<Sprite>(Resources.LoadAll<Sprite>("Localized/"));
            }
        }

        public static string[] GetLanguagesName()
        {
            if (languageFiles != null)
            {
                string[] names = new string[languageFiles.Count];

                for (int i = 0; i < languageFiles.Count; i++)
                {
                    names[i] = languageFiles[i].name;
                }
                return names;
            }
            Debug.LogError("Languages is not initialised");
            return new string[0];
        }

        public static void SetDefaultLanguage(int index)
        {
            if (index > 0 && index < languageFiles.Count)
            {
                LanguageFile oldDefaultFile = languageFiles[0];
                languageFiles[0] = languageFiles[index];
                languageFiles[index] = oldDefaultFile;


            }
            else
            {
                Debug.LogError("Error while selecting new language");
            }
        }

        public static bool DoesLanguageExist(string lg)
        {
            if (string.IsNullOrEmpty(lg))
            {
                return false;
            }
            string[] languages = GetLanguagesName();
            for (int i = 0; i < languages.Length; i++)
            {
                if (languages[i].ToLower() == lg.ToLower())
                {
                    return true;
                }
            }
            return false;
        }

        public static void LoadLanguages()
        {
            //if (languageFiles != null && languageFiles.Count > 0) return;

            string[] paths = GetAllLanguagesFilesPath();
            if (paths != null && paths.Length > 0)
            {
                languageFiles = new List<LanguageFile>();
                foreach (var filePath in paths)
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath).ToLower();
                    if (!languageFiles.Any(prod => prod.name == fileName))
                    {
                        LanguageFile fileInfo = new LanguageFile();
                        fileInfo.name = fileName;
                        fileInfo.path = filePath;
                        languageFiles.Add(fileInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Return all present languages from the info file
        /// </summary>
        /// <returns></returns>
        static string[] GetAllLanguagesFilesPath()
        {
            //#if UNITY_EDITOR
            //			return FileManager.GetAllFiles(LG_FILE_PATH, "*" + FILE_EXTENSION);
            //#endif
            string infoFile = "";
            infoFile = FileManager.Load(Path.Combine(LG_FILE_PATH, INFO_FILE_NAME));

            List<string> languages = new List<string>();
            if (infoFile.Length > 1)
            {
                StringReader reader = new StringReader(infoFile);
                string lgName;
                while (true)
                {
                    lgName = reader.ReadLine();
                    if (lgName == null)
                    {
                        break;
                    }
                    else if (lgName.Length > 1)
                    {
                        languages.Add(Path.Combine(LG_FILE_PATH, lgName + FILE_EXTENSION));
                    }
                    //Debug.Log(lgName);
                }
                reader.Close();
                reader.Dispose();
            }
            return languages.ToArray();

        }
    }
}