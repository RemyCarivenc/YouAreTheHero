using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Edit = Localization.Core.EditorImprovement;
using System.Linq;
using System.Globalization;
using CsvHelper;
using System.IO;
using System.Dynamic;
using System;

namespace Localization.Core
{


    /// <summary>
    /// The setting page of the localization tool
    /// </summary>
    public class LanguageEditorSettings : EditorWindowPage<LanguageWindow>
    {


        List<string> languageNames;
        Vector2 scrollView;
        string ioMessage = "";
        public string[] separatorNames = { "Comma", "Tabulation" };
        public char[] separators = { ',', '\t' };
        public int currentSeparatorIndex = 1;
        public bool ignoreQuote;
        string lgName = "";
        List<LanguageData> newLanguages;
        //int selectedLanguage = 0;1000
        string mergeLanguage = "";
        bool allowOverride;
        CsvHelper.Configuration.Configuration configurationCSV = new CsvHelper.Configuration.Configuration
        {
            HasHeaderRecord = true,
            IgnoreQuotes = true
        };

        public LanguageEditorSettings(LanguageWindow pWindow) : base(pWindow)
        {
        }

        public override void Enable()
        {
            newLanguages = new List<LanguageData>();
            Load();
        }

        public override void Disable() { }

        public override void DrawScene() { }

        public override void DrawEditor()
        {
            SettingPageHeader();
            Edit.View(ref scrollView, () =>
            {
                Edit.Column(() =>
                {
                    SystemInfo();
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    LanguageGroup();
                    //EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    //DisplayConvertSettings();
                });
            });
        }

        public override void Save()
        {
            lgName = "";
            foreach (var newLanguageData in newLanguages)
            {
                // Create file for all new languages
                LanguageManager.SaveData(newLanguageData);
                window.lgSentencePage.languagesData.Add(newLanguageData);
                string str = "Add " + newLanguageData.name;
                foreach (var row in newLanguageData.localizedList)
                {
                    str += "\nid:" + row.id + " | code:" + row.code + " | text:" + row.text;
                }
                Debug.Log(str);
                string lg = newLanguageData.name;


                foreach (var pair in window.lgSentencePage.GetData)
                {
                    LanguageEditor.SentenceBox box = pair.Value;
                    bool allowText = box.sentences.ElementAt(0).Value.Exist(LocalizedDataType.Text);
                    bool allowAudio = box.sentences.ElementAt(0).Value.Exist(LocalizedDataType.Audio);
                    bool allowImage = box.sentences.ElementAt(0).Value.Exist(LocalizedDataType.Texture);


                    Localized loca = null;
                    box.sentences.TryGetValue(lg, out loca);
                    if (loca == null)
                    {
                        loca = new Localized(box.code.ToLower(), pair.Key);
                        box.sentences.Add(lg, loca);
                        LanguageEditor.ChangeSentenceDataType(ref box, lg, LocalizedDataType.Text, allowText);
                        LanguageEditor.ChangeSentenceDataType(ref box, lg, LocalizedDataType.Audio, allowAudio);
                        LanguageEditor.ChangeSentenceDataType(ref box, lg, LocalizedDataType.Texture, allowImage);
                    }
                    else
                    {
                        bool currentText = loca.Exist(LocalizedDataType.Text);
                        bool currentAudio = loca.Exist(LocalizedDataType.Audio);
                        bool currentImage = loca.Exist(LocalizedDataType.Texture);
                        if (currentText != allowText) LanguageEditor.ChangeSentenceDataType(ref box, lg, LocalizedDataType.Text, allowText);
                        if (currentAudio != allowAudio) LanguageEditor.ChangeSentenceDataType(ref box, lg, LocalizedDataType.Audio, allowAudio);
                        if (currentImage != allowImage) LanguageEditor.ChangeSentenceDataType(ref box, lg, LocalizedDataType.Texture, allowImage);
                    }
                    //window.lgSentencePage.GetData[pair.Key] = box;
                }
            }
            // delete new languages
            newLanguages.Clear();
            EditorPrefs.SetString("AUDIO_ROOT_PATH", audioRootPath);
            //EnumGenerator.GenerateEnumWithValues(LanguageWindow.GENERATED_PATH, languageName, languageNames.ToArray(), languageValues.ToArray());
        }

        public override void Load()
        {
            languageNames = new List<string>(LanguageManager.GetLanguagesName());
            audioRootPath = EditorPrefs.GetString("AUDIO_ROOT_PATH", "");
        }

        void SettingPageHeader()
        {
            Edit.Column((() =>
            {
                Edit.Row((() =>
                {
                    float width = (window.position.width - 49) / 2;
                    GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.Width(width));
                    if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(width)))
                    {
                        window.SaveAll();
                        return;
                    }
                    GUILayout.Label("", EditorStyles.toolbarButton, GUILayout.Width(50));
                }));
            }));
        }

        void SystemInfo()
        {
            List<string> languages = new List<string>(LanguageManager.GetLanguagesName());
            if (languages.Count == 0)
            {
                EditorGUILayout.LabelField("Create a language to display settings");
                if (languages != null && languages.Count > 0)
                {
                    PlayerPrefs.SetString(LanguageManager.PLAYER_PREF_KEY, languages[0]);
                }
                return;
            }

            CultureInfo ci = CultureInfo.CurrentUICulture;
            EditorGUILayout.LabelField("System Informations");
            EditorGUILayout.LabelField("Unity Version", Application.unityVersion);
            EditorGUILayout.LabelField("Language", Application.systemLanguage.ToString());
            EditorGUILayout.LabelField("Application path", Application.dataPath);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Tool Informations");
            EditorGUILayout.LabelField("All Languages", "[" + string.Join(", ", languages) + "]");

            //if (languages.IndexOf(LanguageManager.CurrentLanguage) < 0)
            //{
            //    LanguageManager.CurrentLanguage = languages[0];
            //    mergeLanguage = LanguageManager.CurrentLanguage;
            //}
            int newSelection = EditorGUILayout.Popup("Default Language", 0, languages.ToArray(), GUILayout.Width(400));

            if (newSelection != 0)
            {
                LanguageManager.SetDefaultLanguage(newSelection);
            }


            EditorGUILayout.LabelField("Files data path", "Assets" + LanguageManager.LG_FILE_PATH.Substring(Application.dataPath.Length));
            //EditorGUILayout.LabelField("Generated path", "Assets" + LanguageWindow.GENERATED_PATH.Substring(Application.dataPath.Length));
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Import / export excel file");
            currentSeparatorIndex = EditorGUILayout.Popup("Data Separator", currentSeparatorIndex, separatorNames, GUILayout.Width(350));
            bool isCSV = currentSeparatorIndex == 0;
            ignoreQuote = EditorGUILayout.Toggle("Ignore Quote", ignoreQuote);
            Edit.Row(() =>
            {
                string fileExtension = isCSV ? "CSV" : "TSV";
                if (GUILayout.Button("Export " + fileExtension, GUILayout.Width(100)))
                {
                    //string str = window.lgSentencePage.ExportToTextFile(separators[currentSeparatorIndex]);
                    string path = LanguageManager.LG_FILE_PATH + "/" + Application.productName + "_Localisation." + fileExtension.ToLower();
                    //FileManager.Save(path, str, true);
                    WriteCSVFile(path);
                    ioMessage = "Exported to " + path.Substring(Application.dataPath.Length);
                }
                if (GUILayout.Button("Import " + fileExtension, GUILayout.Width(100)))
                {
                    string file = EditorUtility.OpenFilePanel("Import Text file", LanguageManager.LG_FILE_PATH, fileExtension.ToLower());
                    if (FileManager.IsValidFile(file))
                    {
                        ReadCSVFile(file);
                    }
                    else
                    {
                        Debug.LogError("File is not valid");
                    }
                }
                GUILayout.Label(ioMessage);
            });

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Merge loca file");

            if (mergeLanguage == "") mergeLanguage = languages[0];
            int indexMerge = languages.IndexOf(mergeLanguage);
            if (indexMerge < 0)
            {
                indexMerge = 0;
                mergeLanguage = languages[0];
            }

            mergeLanguage = languages[EditorGUILayout.Popup("Merge into", languages.IndexOf(mergeLanguage), languages.ToArray(), GUILayout.Width(400))];
            allowOverride = EditorGUILayout.Toggle("Allow Override", allowOverride);
            if (GUILayout.Button("Select file", GUILayout.Width(200)))
            {
                string pathToFolder = EditorPrefs.GetString("MergeFilePath", LanguageManager.LG_FILE_PATH);
                if (!FileManager.IsValidDirector(pathToFolder)) pathToFolder = LanguageManager.LG_FILE_PATH;

                string file = EditorUtility.OpenFilePanel("Import Loca file", pathToFolder, "loca");
                if (FileManager.IsValidFile(file))
                {
                    EditorPrefs.SetString("MergeFilePath", file);
                    LanguageData externalFile = new LanguageData();
                    FileManager.Load<LanguageData>(file, ref externalFile);
                    if (externalFile != null && externalFile.localizedList != null && externalFile.localizedList.Count > 0)
                    {
                        externalFile.name = mergeLanguage;
                        MergeExternalLanguageFile(externalFile);
                    }
                    else
                    {
                        Debug.LogError("File not found or could not be converted from json at " + file);
                    }
                }
            }

            AutofillVoices();

            AutofillImages();
        }

        #region Audio Auto Fill

        string prefix;
        string audioRootPath = "";
        public static int editorPrefixIgnore = 1;
        public static int filePrefixIgnore = 2;

        void AutofillVoices()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            Edit.Row(() =>
            {
                EditorGUILayout.LabelField(new GUIContent("Auto fill AudioClips", "This will search audio clip in project and match them with audio already linked to localization"));
                EditorGUILayout.Space();
                if (GUILayout.Button("NamingConvention", GUILayout.Width(130)))
                {
                    string str = "";
                    str += ("At least one audio clip have to be filled for each code");
                    str += ("\n\nFiles have to be placed in folder as Resources/LanguageName/File");
                    str += ("\n\nFiles have to be named as Ignored_Ignored_Same_Same...");
                    str += ("\n\nFirst two prefix and case is ignored");
                    str += ("\nCharacters - and _ are considered the same");
                    str += ("\n\nExamples of corresponding names :");
                    str += ("\nResources/French/VO_FR_MAINCHARACTER_VOICE_1_NEW");
                    str += ("\nResources/english/sfx_en_maincharacter_voice_1_new");
                    str += ("\nResources/Italian/vo_it_maincharacter-voice-1-new");
                    EditorUtility.DisplayDialog("Voice naming convention", str, "OK");
                }
            });
            Edit.Row(() =>
            {
                EditorGUILayout.LabelField("Audio root path", audioRootPath);
                EditorGUILayout.Space();
                if (GUILayout.Button("Select path", GUILayout.Width(80)))
                {
                    string path = EditorUtility.OpenFolderPanel("Select folder", Application.dataPath, "");
                    if (path != "") audioRootPath = path;
                }
            });
            filePrefixIgnore = EditorGUILayout.IntField(new GUIContent("File prefix ignore", "Define how many prefix should be ignored on an audio file"), filePrefixIgnore);
            editorPrefixIgnore = EditorGUILayout.IntField(new GUIContent("Code prefix ignore", "Define how many prefix should be ignored on an localisation code"), editorPrefixIgnore);

            filePrefixIgnore = Mathf.Clamp(filePrefixIgnore, 0, 10);
            editorPrefixIgnore = Mathf.Clamp(editorPrefixIgnore, 0, 10);
            Edit.Row(() =>
            {
                if (audioRootPath != "" && GUILayout.Button(new GUIContent("Fill with audio", "Try to fill audio clips using already linked audio clips"), GUILayout.Width(130)))
                {
                    window.SetWindowTab(0);
                    window.lgSentencePage.AutoFillVoices(GenerateAudioData(audioRootPath));
                }
                if (audioRootPath != "" && GUILayout.Button(new GUIContent("Fill with code", "Try to fill audio clips using the localisation codes"), GUILayout.Width(130)))
                {
                    window.SetWindowTab(0);
                    window.lgSentencePage.AutoFillVoicesWithCode(GenerateAudioData(audioRootPath));
                }
            });
        }

        Dictionary<string, AudioData> GenerateAudioData(string path)
        {
            Dictionary<string, AudioData> audios = new Dictionary<string, AudioData>();
            foreach (var lg in languageNames)
            {
                string str = path + "/" + lg;
                if (Directory.Exists(str))
                {
                    audios.Add(lg, GetAudio(str));
                }
            }

            return audios;
        }

        AudioData GetAudio(string path)
        {
            AudioData data = new AudioData();
            data.path = path;

            string[] fileEntries = Directory.GetFiles(path);
            foreach (string fileName in fileEntries)
            {
                if (!fileName.EndsWith("ogg") && !fileName.EndsWith("mp3") && !fileName.EndsWith("wav")) continue;

                //int index = fileName.LastIndexOf("/");
                string localPath = fileName.Split(new string[] { "Resources" }, StringSplitOptions.RemoveEmptyEntries).Last();
                localPath = localPath.Replace('\\', '/');
                localPath = localPath.Split('.').First();
                localPath = localPath.Remove(0, 1);
                //if (index > 0)
                //    localPath += fileName.Substring(index);

                //Debug.Log("Search asset at " + localPath);
                AudioClip c = Resources.Load<AudioClip>(localPath);

                if (c != null)
                {

                    ClipData clip = new ClipData();
                    clip.name = Path.GetFileNameWithoutExtension(fileName);
                    clip.localPath = localPath;
                    clip.obj = c;
                    data.clips.Add(clip.name, clip);
                    //Debug.Log("Add asset " + clip.name);
                }
            }
            return data;
        }

        public class AudioData
        {

            public string path;
            public Dictionary<string, ClipData> clips = new Dictionary<string, ClipData>();

        }

        public class ClipData
        {
            public string name;
            public string localPath;
            public AudioClip obj;
        }

        #endregion


        #region Image Auto Fill

        public static int imageEditorPrefixIgnore = 0;
        public static int imageFilePrefixIgnore = 1;

        public const string PathToLocalizedImages = "Localization/Images/";
        public const string AllLanguagesFolderName = "AllLanguages";

        void AutofillImages()
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            Edit.Row(() =>
            {
                EditorGUILayout.LabelField(new GUIContent("Auto fill Images", "This will search images in project and match them with images already linked to localization"));
                EditorGUILayout.Space();
                if (GUILayout.Button("NamingConvention", GUILayout.Width(130)))
                {
                    string str = "";
                    str += ("At least one image have to be filled for each code");
                    str += ("\n\nFiles have to be placed in folder as Resources/LanguageName/File");
                    str += ("\n\nFiles have to be named as Ignored_Ignored_Same_Same...");
                    str += ("\n\nFirst two prefix and case is ignored");
                    str += ("\nCharacters - and _ are considered the same");
                    str += ("\n\nExamples of corresponding names :");
                    str += ("\nResources/French/IM_FR_MAINCHARACTER_IMAGE_1_NEW");
                    str += ("\nResources/english/sfx_en_maincharacter_image_1_new");
                    str += ("\nResources/Italian/im_it_maincharacter-image-1-new");
                    EditorUtility.DisplayDialog("Image naming convention", str, "OK");
                }
            });
            imageFilePrefixIgnore = EditorGUILayout.IntField(new GUIContent("File prefix ignore", "Define how many prefix should be ignored on an audio file"), imageFilePrefixIgnore);
            imageEditorPrefixIgnore = EditorGUILayout.IntField(new GUIContent("Code prefix ignore", "Define how many prefix should be ignored on an localisation code"), imageEditorPrefixIgnore);

            imageFilePrefixIgnore = Mathf.Clamp(imageFilePrefixIgnore, 0, 10);
            imageEditorPrefixIgnore = Mathf.Clamp(imageEditorPrefixIgnore, 0, 10);
            Edit.Row(() =>
            {
                if (GUILayout.Button(new GUIContent("Fill with image", "Try to fill images using already linked images"), GUILayout.Width(130)))
                {
                    window.SetWindowTab(0);
                    window.lgSentencePage.AutoFillImages(GenerateImageData());
                }
                if (GUILayout.Button(new GUIContent("Fill with code", "Try to fill images using the localisation codes"), GUILayout.Width(130)))
                {
                    window.SetWindowTab(0);
                    window.lgSentencePage.AutoFillImagesWithCode(GenerateImageData());
                }
            });
        }

        Dictionary<string, ImageData> GenerateImageData()
        {
            Dictionary<string, ImageData> images = new Dictionary<string, ImageData>();
            {
                Texture[] textures = Resources.LoadAll<Texture>(PathToLocalizedImages + AllLanguagesFolderName);
                if (textures.Length > 0)
                {
                    images.Add(AllLanguagesFolderName, GetImage(PathToLocalizedImages + AllLanguagesFolderName, textures));
                }
            }
            foreach (var lg in languageNames)
            {
                Texture[] textures = Resources.LoadAll<Texture>(PathToLocalizedImages + lg);
                if (textures.Length > 0)
                {
                    images.Add(lg, GetImage(PathToLocalizedImages + lg, textures));
                }
            }

            return images;
        }

        ImageData GetImage(string path, Texture[] textures)
        {
            ImageData data = new ImageData();
            data.path = path;

            foreach (Texture texture in textures)
            {
                string localPath = path + "/" + texture.name;
                localPath = localPath.Replace('\\', '/');

                TextureData textureData = new TextureData();
                textureData.name = texture.name;
                textureData.localPath = localPath;
                textureData.code = RemoveImagePrefix(texture.name);
                textureData.obj = texture;
                data.textures.Add(textureData.code, textureData);
            }
            return data;
        }

        string RemoveImagePrefix(string name)
        {
            string fileName = "";
            string[] splited = name.Split('_');
            if (splited.Length == 1)
            {
                return name;
            }
            else if (splited.Length == 2)
            {
                return splited[1];
            }
            else if (splited.Length >= 3)
            {
                for (int i = 0; i < splited.Length; i++)
                {
                    if (i == 0) continue;
                    if (fileName == "") fileName += splited[i];
                    else fileName += "_" + splited[i];
                }
            }
            return fileName;
        }

        public class ImageData
        {
            public string path;
            public Dictionary<string, TextureData> textures = new Dictionary<string, TextureData>();
        }

        public class TextureData
        {
            public string name;
            public string localPath;
            public string code;
            public Texture obj;
        }
        #endregion


        void LanguageGroup()
        {
            for (int i = 0; i < languageNames.Count; i++)
            {
                LanguageDisplay(i);
            }

            AddLanguage();
        }

        void LanguageDisplay(int index)
        {
            Edit.Row("Box", () =>
            {
                bool isSaved = index < window.lgSentencePage.languagesData.Count;
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(isSaved ? ("Language " + index) : ("Language (Not saved)"), languageNames[index]);
                EditorGUI.EndDisabledGroup();
                if (isSaved)
                {
                    window.lgSentencePage.languagesData[index].isRightToLeft = EditorGUILayout.Toggle(new GUIContent("RTL", "Right to left text"), window.lgSentencePage.languagesData[index].isRightToLeft);
                }
                else
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.Toggle("RTL", false);
                    EditorGUI.EndDisabledGroup();
                }
                if (GUILayout.Button("Remove", GUILayout.Width(100)))
                {
                    if (EditorUtility.DisplayDialog("Removing " + languageNames[index], "Removing a language will remove the data file of the language (YOU WILL LOSE ALL DATA FOR THE SELECTED LANGUAGE)", "continue", "cancel"))
                    {
                        DeleteLanguage(index);
                    }
                }
            });
        }

        void DeleteLanguage(int delIndex)
        {
            string delName = languageNames[delIndex];
            languageNames.RemoveAt(delIndex);
            string path = LanguageManager.GetCompleteFilePath(delName);

            int editorIndex = -1;
            for (int i = 0; i < window.lgSentencePage.languagesData.Count; i++)
            {
                if (window.lgSentencePage.languagesData[i].name == delName)
                {
                    editorIndex = i;
                }
            }
            if (editorIndex >= 0)
            {
                window.lgSentencePage.languagesData.RemoveAt(editorIndex);
                LanguageManager.languageFiles.RemoveAt(editorIndex);
                LanguageManager.CreateInfoFile();
            }
            FileManager.DeleteFile(path);

            if (languageNames.Count > 0)
            {
                if (PlayerPrefs.GetString(LanguageManager.PLAYER_PREF_KEY) == delName)
                {
                    PlayerPrefs.SetString(LanguageManager.PLAYER_PREF_KEY, languageNames[0]);
                }
            }
            else
            {
                PlayerPrefs.DeleteKey(LanguageManager.PLAYER_PREF_KEY);
            }
        }

        void AddLanguage()
        {
            Edit.Row("Box", () =>
            {
                lgName = EditorGUILayout.TextField("New Language", lgName).ToLower();

                if (GUILayout.Button("Add", GUILayout.Width(100)))
                {
                    ProcessAddLanguage(lgName);
                }
            });
        }

        void ProcessAddLanguage(string name)
        {
            name = CleanString(name);
            int cache;
            bool isNumeric = int.TryParse(name, out cache);
            if (name.Length < 2)
            {
                Debug.Log("Name (" + name + ") is too short");
            }
            else if (name.Length > 50)
            {
                Debug.Log("Name (" + name + ") is too long");
            }
            else if (languageNames.Contains(name) || isNumeric)
            {
                Debug.LogError("Name (" + name + ") already exist or is invalid");
            }
            else
            {
                languageNames.Add(name);
                LanguageData language = new LanguageData();
                language.name = name;
                language.localizedList = new List<Localized>();
                Localized[] loca = window.lgSentencePage.GetLocalizedSample();
                foreach (var baseSample in loca)
                {
                    Localized newSample = new Localized();
                    newSample.id = baseSample.id;
                    newSample.code = baseSample.code;
                    if (baseSample.Exist(LocalizedDataType.Text)) newSample.TryAddData(new LocalizedData("", LocalizedDataType.Text));
                    if (baseSample.Exist(LocalizedDataType.Audio)) newSample.TryAddData(new LocalizedData("", LocalizedDataType.Audio));
                    if (baseSample.Exist(LocalizedDataType.Texture)) newSample.TryAddData(new LocalizedData("", LocalizedDataType.Texture));
                    language.localizedList.Add(newSample);
                }
                newLanguages.Add(language);
                name = "";
            }
        }

        string CleanString(string str)
        {
            string removeChars = "\\ ?&^$#@!()+-,:;<>’\"'-_*/";
            string result = str;

            foreach (char c in removeChars)
            {
                result = result.Replace(c.ToString(), string.Empty);
            }
            return result;
        }

        public override void OnCompiled()
        {
            Load();
        }

        /*	Used to convert files from old version to 1.0
		 * 
		 * 
		void DisplayConvertSettings()
		{
			string[] paths = Old.LanguageManager.GetAllLanguagesFilesPath();

			if(paths.Length > 0)
			{
				EditorGUILayout.LabelField("Deprecated data files");

				foreach (var path in paths)
				{
					Edit.Row(() => {
						EditorGUILayout.LabelField(System.IO.Path.GetFileName(path), path);

						string name = GetNameFromOldPath(path);
						if (!LanguageManager.DoesLanguageExist(name))
						{
							if (GUILayout.Button("Add language", GUILayout.Width(90)))
							{
								ProcessAddLanguage(name);
							}
						}
						else
						{
							if (GUILayout.Button("Add data", GUILayout.Width(80)))
							{
								ConvertOldFormat(path);
							}
						}
						
						if (GUILayout.Button("Delete", GUILayout.Width(80)))
						{
							if (EditorUtility.DisplayDialog("Removing file "+ System.IO.Path.GetFileNameWithoutExtension(path), "Removing a deprecated language file will remove the data contained inside the file (YOU WILL LOSE ALL DATA INSIDE THE SELECTED FILE)", "continue", "cancel"))
							{
								FileManager.DeleteFile(path);
							}
						}
					});
				}
			}
		}

		void ConvertOldFormat(string filePath)
		{
			this.window.isCompiling = true;
			Old.LanguageData oldData = new Old.LanguageData();
			if(FileManager.IsValidFile(filePath))
			{
				FileManager.Load<Old.LanguageData>(filePath, ref oldData);
				if (oldData != null)
				{
					ConvertData(oldData, filePath);
				}
				else
				{
					Debug.Log("Old data could not be loaded");
				}
			}
			else
			{
				Debug.Log("File path is not valid");
			}
			this.window.isCompiling = false;
		}

		string GetNameFromOldPath(string path)
		{
			string languageName = System.IO.Path.GetFileNameWithoutExtension(path);
			languageName = languageName.Remove(0, Old.LanguageManager.FILE_PREFIX.Length).ToLower();
			return languageName;
		}

		void ConvertData(Old.LanguageData oldData, string path)
		{
			string languageName = GetNameFromOldPath(path).ToLower();
			
			foreach (Old.Sentence item in oldData.sentencesData)
			{
				item.code = item.code.ToLower();
				this.window.lgSentencePage.AddSentence(item.code, item.code.GetHashCode());
				this.window.lgSentencePage.AddDataToSentence(item.code, languageName, LocalizedDataType.Text, item.text);
				if(!string.IsNullOrEmpty(item.clip))
				{
					this.window.lgSentencePage.AddDataToSentence(item.code, languageName, LocalizedDataType.Audio, item.clip);
				}
			}
			
		}
		*/

        void MergeExternalLanguageFile(LanguageData data)
        {
            foreach (var sentence in data.localizedList)
            {
                window.lgSentencePage.AddDataToSentence(mergeLanguage, sentence, allowOverride);
                window.SetWindowTab(0);
            }
        }

        public void ReadCSVFile(string path)
        {
            configurationCSV.Delimiter = separators[currentSeparatorIndex].ToString();
            configurationCSV.Encoding = System.Text.Encoding.UTF8;
            configurationCSV.IgnoreQuotes = ignoreQuote;
            configurationCSV.BadDataFound = BadDataFoundCallback;
            configurationCSV.IgnoreBlankLines = true;

            Dictionary<string, ImageData> iData = GenerateImageData();
            ImageData allLanguages = null;
            if (iData.ContainsKey(AllLanguagesFolderName))
            {
                allLanguages = iData[AllLanguagesFolderName];
            }

            TextReader reader = File.OpenText(path);

            if (reader != null)
            {
                var csv = new CsvReader(reader, configurationCSV);
                if (csv != null)
                {
                    //var record = new ExpandoObject() as IDictionary<string, System.Object>;
                    var records = csv.GetRecords<dynamic>();

                    foreach (IDictionary<string, System.Object> item in records)
                    {
                        string code = "";
                        bool isFirst = true;
                        string allLanguagesImagePath = null;
                        foreach (var v in item)
                        {
                            if (isFirst)
                            {
                                isFirst = false;
                                code = (v.Value as string).ToLower();
                                if (allLanguages != null && allLanguages.textures.ContainsKey(code))
                                {
                                    TextureData texture = allLanguages.textures[code];
                                    allLanguagesImagePath = PathToLocalizedImages + AllLanguagesFolderName + "/" + texture.name;
                                }
                            }
                            else
                            {
                                Debug.Log("Add [code:" + code + ", value:" + v.Value + ", lg:" + v.Key + "]");
                                string language = v.Key.ToLower();
                                window.lgSentencePage.AddDataToSentence(code, language, LocalizedDataType.Text, v.Value as string);

                                if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(language))
                                {
                                    bool added = false;

                                    Dictionary<string, TextureData> textures = null;
                                    if (iData.ContainsKey(language))
                                    {
                                        textures = iData[language].textures;
                                        Debug.Log(code);
                                        if (textures.ContainsKey(code))
                                        {
                                            TextureData texture = textures[code];
                                            window.lgSentencePage.AddDataToSentence(code, v.Key.ToLower(), LocalizedDataType.Texture, PathToLocalizedImages + language + "/" + texture.name);
                                            added = true;
                                        }
                                    }

                                    if (!added && !string.IsNullOrEmpty(allLanguagesImagePath))
                                    {
                                        window.lgSentencePage.AddDataToSentence(code, v.Key.ToLower(), LocalizedDataType.Texture, allLanguagesImagePath);
                                    }
                                }


                            }
                        }
                    }
                    window.SetWindowTab(0);
                }
            }
            reader.Close();
        }

        private void BadDataFoundCallback(ReadingContext obj)
        {
            Debug.LogError("Bad data found in imported file at line " + obj.RawRow
            + "\n RawRecored : " + obj.RawRecord
            + "\n\n Field : " + obj.Field
            + "\n\n IsBadfield : " + obj.IsFieldBad);
        }

        void WriteCSVFile(string path)
        {
            TextWriter writer = File.CreateText(path);
            configurationCSV.Delimiter = separators[currentSeparatorIndex].ToString();
            if (writer != null)
            {
                var csv = new CsvWriter(writer, configurationCSV);
                if (csv != null)
                {
                    try
                    {
                        List<dynamic> records = new List<dynamic>();
                        foreach (var sentenceBox in window.lgSentencePage.GetData)
                        {
                            // Create a dynamic record
                            var record = new ExpandoObject() as IDictionary<string, System.Object>;

                            // a record contain data from 1 row exmample : record.Add(column name, value)

                            record.Add("Code", sentenceBox.Value.code);  // Add first row (code)
                            foreach (var localized in sentenceBox.Value.sentences)
                            {
                                // add sentences under their languages
                                record.Add(localized.Key, localized.Value.text);
                            }

                            records.Add(record);
                        }
                        csv.WriteRecords(records);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.Message);
                    }
                }
                writer.Close();
            }
        }
    }
}