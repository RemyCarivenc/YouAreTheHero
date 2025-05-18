using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Localization.Core.LanguageEditorSettings;
using Edit = Localization.Core.EditorImprovement;

namespace Localization.Core
{
    /// <summary>
    /// A window page that provide access to all languages data files
    /// </summary>
    public class LanguageEditor : EditorWindowPage<LanguageWindow>
    {
        #region Property

        public const string enumCodeName = "LanguageCode"; // DO NOT CHANGE THE NAME
        public const string enumNameSpaceName = "Localization";
        private Vector2 scrollView;
        private bool advancedOptions = true;
        private string newCodeValue;
        private GUIStyle wideBox;
        private GUIStyle rightTextField = null;
        private bool addTextLocalization = true;
        private bool addAudioLocalization;
        private bool addImageLocalization;
        private Dictionary<int, SentenceBox> boxData;
        private const string moreOptionParameter = "MORE_OPTION_BUTTON";
        private bool moreOptionsButton;
        public Dictionary<int, SentenceBox> GetData { get { return boxData; } }


        enum Order
        {
            AscCode,
            DescCode,
            AscId,
            DescId,
        }

        public enum Display
        {
            All,
            Uncompleted,
            Text,
            Audio,
            Image
        }

        public enum SearchMode
        {
            Contain,
            StartWith,
            EndWith,
        }

        public enum SearchIn
        {
            Code,
            Value,
            Id,
        }

        public SearchMode searchMode;
        public SearchIn searchIn;
        public Display displayMode = Display.All;
        Order orderMode = Order.AscCode;
        public string search = "";
        public List<LanguageData> languagesData;

        public class SentenceBox
        {
            public bool display;
            public string code;

            /// <summary>
            /// key : language index
            /// value : localization
            /// /// </summary>
            public Dictionary<string, Localized> sentences;

            public SentenceBox(bool pDisp, string pCode)
            {
                display = pDisp;
                code = pCode;
                sentences = new Dictionary<string, Localized>();
            }

            public void AddDataToSentence(string language, LocalizedData data)
            {
                if (sentences.ContainsKey(language))
                {
                    //foreach (var item in sentences)
                    //{
                    //	// Fast and bad debug
                    //	item.Value.TryAddData(new LocalizedData("", data.Type));
                    //	break;
                    //}
                    sentences[language].TryAddData(data);
                }
            }

            public void AddSentence(string lg, Localized st)
            {
                if (!sentences.ContainsKey(lg))
                {
                    sentences.Add(lg, st);
                }
            }

            public Localized GetSentence(string lg)
            {
                if (sentences.ContainsKey(lg))
                    return sentences[lg];
                else
                    return null;
            }

            public bool HasType(LocalizedDataType type)
            {
                return sentences.First().Value.HasType(type);
            }
        }

        #endregion Property

        #region Override

        public LanguageEditor(LanguageWindow pWindow) : base(pWindow)
        {
        }

        public override void Enable()
        {
            search = "";
            Load();
            moreOptionsButton = EditorPrefs.GetBool(moreOptionParameter, false);
        }

        private void Initialise()
        {
            wideBox = new GUIStyle(GUI.skin.box);
            wideBox.margin = new RectOffset(0, 0, 0, 0);
            wideBox.padding = new RectOffset(0, 0, 0, 0);
            rightTextField = new GUIStyle(GUI.skin.textArea);
            rightTextField.alignment = TextAnchor.MiddleRight;
        }

        public override void DrawEditor()
        {
            Initialise();
            LanguagePageHeader();
            EditorGUILayout.Space();
            LanguagePageContent();
        }

        public override void OnCompiled()
        {
        }

        public override void Disable()
        {
        }

        public override void DrawScene()
        {
        }

        public override void Save()
        {
            if (languagesData.Count == 0) return;
            EditorPrefs.SetBool(moreOptionParameter, moreOptionsButton);
            SortedList<int, string> identifiers = new SortedList<int, string>();
            foreach (var item in boxData)
            {
                identifiers.Add(item.Key, item.Value.code);
            }
            LanguageCodeUtility.Generate(identifiers);

            // Save all languages in theirs own files
            for (int i = 0; i < languagesData.Count; i++)
            {
                languagesData[i].localizedList.Clear();

                foreach (var b in boxData)
                {
                    languagesData[i].localizedList.Add(b.Value.GetSentence(languagesData[i].name));
                }

                LanguageManager.SaveData(languagesData[i]);
            }
            LanguageManager.CreateInfoFile();
            EditorPrefs.SetInt("searchMode", (int)searchMode);
            EditorPrefs.SetInt("searchIn", (int)searchIn);
            EditorPrefs.SetInt("displayMode", (int)displayMode);
            EditorPrefs.SetInt("orderMode", (int)orderMode);
        }

        public override void Load()
        {
            string[] languagesNames = LanguageManager.GetLanguagesName();
            languagesData = new List<LanguageData>();
            boxData = new Dictionary<int, SentenceBox>();
            for (int i = 0; i < languagesNames.Length; i++)
            {
                LanguageData data = new LanguageData
                {
                    name = languagesNames[i]
                };
                LanguageManager.LoadData(ref data);
                languagesData.Add(data);
                if (data.localizedList == null) data.localizedList = new List<Localized>();
                for (int j = 0; j < data.localizedList.Count; j++)
                {
                    int identifier = data.localizedList[j].id;
                    if (boxData.ContainsKey(identifier))
                    {
                        boxData[identifier].AddSentence(data.name, data.localizedList[j]);
                    }
                    else
                    {
                        boxData.Add(identifier, new SentenceBox(false, data.localizedList[j].code));
                        boxData[identifier].AddSentence(data.name, data.localizedList[j]);
                    }
                }
                data.localizedList.Clear();

                OrderBox();
            }

            SortedList<int, string> identifiers = new SortedList<int, string>();
            foreach (var item in boxData)
            {
                identifiers.Add(item.Key, item.Value.code);
            }
            LanguageCodeUtility.Generate(identifiers);

            searchMode = (SearchMode)EditorPrefs.GetInt("searchMode", 0);
            searchIn = (SearchIn)EditorPrefs.GetInt("searchIn", 0);
            displayMode = (Display)EditorPrefs.GetInt("displayMode", 0);
            orderMode = (Order)EditorPrefs.GetInt("orderMode", 0);
        }

        #endregion Override

        #region Display

        private int GenerateUniqueIdentifier()
        {
            int identifier = 1;
            while (boxData.ContainsKey(identifier))
            {
                identifier++;
            }
            return identifier;
        }

        /// <summary>
        /// Display small option panel on top of the page
        /// </summary>
        private void LanguagePageHeader()
        {
            Edit.Column(wideBox, (() =>
            {
                Edit.Row((() =>
                {
                    float width = (window.position.width - 49) / 4;
                    advancedOptions = GUILayout.Toggle(advancedOptions, "Advanced", EditorStyles.toolbarButton, GUILayout.Width(width));
                    if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(width)))
                    {
                        Save();
                        return;
                    }
                    GUILayout.Label("", EditorStyles.toolbarButton);
                }));
                if (advancedOptions)
                {
                    Edit.Column((System.Action)(() =>
                     {
                         Edit.Row((System.Action)(() =>
                         {
                             EditorGUILayout.LabelField("New Localization");
                             addTextLocalization = GUILayout.Toggle(addTextLocalization, "Text", EditorStyles.miniButtonLeft, GUILayout.Width(80));
                             addAudioLocalization = GUILayout.Toggle(addAudioLocalization, "Audio", EditorStyles.miniButtonMid, GUILayout.Width(80));
                             addImageLocalization = GUILayout.Toggle(addImageLocalization, "Image", EditorStyles.miniButtonRight, GUILayout.Width(80));
                         }));
                         Edit.Row((System.Action)(() =>
                         {
                             EditorGUILayout.LabelField("Code", GUILayout.Width(80));
                             newCodeValue = EditorGUILayout.TextField(newCodeValue);
                             if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.Width(50)))
                             {
                                 if (newCodeValue != null && newCodeValue.Length > 0 && (addTextLocalization || addAudioLocalization || addImageLocalization))
                                 {
                                     newCodeValue = newCodeValue.ToLower();
                                     AddSentence(newCodeValue);
                                     newCodeValue = "";
                                 }
                                 else
                                 {
                                     Debug.Log("Abort : Code not valid");
                                 }
                                 return;
                             }
                         }));
                     }));
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    Edit.Row((System.Action)(() =>
                    {
                        EditorGUILayout.LabelField("Search", GUILayout.Width(80));
                        search = EditorGUILayout.TextField(search);
                        searchMode = (SearchMode)EditorGUILayout.EnumPopup(searchMode, GUILayout.Width(80));
                        searchIn = (SearchIn)EditorGUILayout.EnumPopup(searchIn, GUILayout.Width(80));

                        if (GUILayout.Button("Clear", EditorStyles.miniButtonRight, GUILayout.Width(50)))
                        {
                            search = "";
                        }
                    }));
                    Edit.Row(() =>
                    {
                        moreOptionsButton = GUILayout.Toggle(moreOptionsButton, "Edit Type", EditorStyles.miniButton, GUILayout.Width(100));

                        orderMode = (Order)EditorGUILayout.EnumPopup(orderMode);
                        displayMode = (Display)EditorGUILayout.EnumPopup(displayMode);

                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Display all", EditorStyles.miniButtonLeft, GUILayout.Width(100)))
                        {
                            foreach (var box in boxData)
                            {
                                box.Value.display = true;
                            }
                        }
                        if (GUILayout.Button("Collapse all", EditorStyles.miniButtonRight, GUILayout.Width(100)))
                        {
                            foreach (var box in boxData)
                            {
                                box.Value.display = false;
                            }
                        }
                        if (GUILayout.Button("Find Duplicate", EditorStyles.miniButtonRight, GUILayout.Width(100)))
                        {
                            FindDuplicateValues();
                        }
                    });
                }
            }));
        }

        #region Auto Fille Voices

        internal void AutoFillVoices(Dictionary<string, AudioData> aData)
        {
            List<int> keys = GetOrderedKeys();

            foreach (var key in keys)
            {
                SentenceBox box = boxData[key];
                box.display = false;
                if (box.HasType(LocalizedDataType.Audio))
                {

                    Dictionary<string, string> newPair = new Dictionary<string, string>();
                    string baseFileName = "";
                    foreach (var b in box.sentences)
                    {
                        if (!string.IsNullOrEmpty(b.Value.clip))
                        {
                            baseFileName = b.Value.clip;
                            baseFileName = baseFileName.Split(new char[] { '\\', '/' }).Last();
                            //Debug.Log(baseFileName);
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(baseFileName))
                    {
                        foreach (var b in box.sentences)
                        {
                            if (string.IsNullOrEmpty(b.Value.clip) && aData.ContainsKey(b.Key))
                            {
                                string path = GetPathOfAudioWithName(RemoveAudioPrefix(baseFileName), aData[b.Key]);

                                if (!string.IsNullOrEmpty(path))
                                {
                                    Debug.Log("Find clip at path : " + path + " for " + b.Key);
                                    // TODO
                                    b.Value.SetValue(path, LocalizedDataType.Audio);
                                    box.display = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void AutoFillVoicesWithCode(Dictionary<string, AudioData> aData)
        {
            List<int> keys = GetOrderedKeys();

            foreach (var key in keys)
            {
                SentenceBox box = boxData[key];
                box.display = false;
                if (box.HasType(LocalizedDataType.Audio))
                {
                    box.display = true;
                    Dictionary<string, string> newPair = new Dictionary<string, string>();

                    foreach (var b in box.sentences)
                    {
                        if (string.IsNullOrEmpty(b.Value.clip) && aData.ContainsKey(b.Key))
                        {
                            string path = GetPathOfAudioWithName(RemoveAudioPrefixCode(box.code), aData[b.Key]);

                            if (!string.IsNullOrEmpty(path))
                            {
                                Debug.Log("Find clip at path : " + path + " for " + b.Key);
                                // TODO
                                b.Value.SetValue(path, LocalizedDataType.Audio);
                                box.display = true;
                            }
                        }
                    }
                }
            }
        }

        string GetPathOfAudioWithName(string audioname, AudioData singleLgAudio)
        {
            //audioname = RemoveAudioPrefix(audioname);
            foreach (var clip in singleLgAudio.clips)
            {
                string fileName = clip.Key;

                fileName = RemoveAudioPrefix(fileName);

                if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(audioname)) return "";

                Debug.Log("Compare [" + fileName + "] and [" + audioname + "]");
                if (string.Equals(fileName, audioname, StringComparison.InvariantCultureIgnoreCase))
                {
                    Debug.Log("Found correspondance [" + fileName + "] and [" + audioname + "]");
                    return clip.Value.localPath;
                }
            }
            return "";
        }

        string RemoveAudioPrefix(string name)
        {
            if (LanguageEditorSettings.filePrefixIgnore <= 0)
            {
                return name;
            }

            string fileName = "";
            string[] splited = name.Split(new char[] { '-', '_' });
            if (splited.Length == 1)
            {
                return name;
            }
            else if (splited.Length == 2)
            {
                return name;
            }
            else if (splited.Length >= 3)
            {
                for (int i = 0; i < splited.Length; i++)
                {
                    if (i < LanguageEditorSettings.filePrefixIgnore) continue;
                    if (fileName == "") fileName += splited[i];
                    else fileName += "_" + splited[i];
                }
            }
            return fileName;
        }

        string RemoveAudioPrefixCode(string name)
        {
            if (LanguageEditorSettings.editorPrefixIgnore <= 0)
            {
                return name;
            }

            string fileName = "";
            string[] splited = name.Split(new char[] { '-', '_' });
            if (splited.Length == 1)
            {
                return name;
            }
            else if (splited.Length == 2)
            {
                return name;
            }
            else if (splited.Length >= 3)
            {
                for (int i = 0; i < splited.Length; i++)
                {
                    if (i < LanguageEditorSettings.editorPrefixIgnore) continue;
                    if (fileName == "") fileName += splited[i];
                    else fileName += "_" + splited[i];
                }
            }
            return fileName;
        }

        #endregion

        #region Auto Fill Images

        internal void AutoFillImages(Dictionary<string, ImageData> iData)
        {
            List<int> keys = GetOrderedKeys();
            ImageData allLanguages = null;
            if (iData.ContainsKey(AllLanguagesFolderName))
            {
                allLanguages = iData[AllLanguagesFolderName];
            }

            foreach (var key in keys)
            {
                SentenceBox box = boxData[key];
                box.display = false;
                if (box.HasType(LocalizedDataType.Texture))
                {
                    string baseFileName = "";
                    foreach (var b in box.sentences)
                    {
                        if (!string.IsNullOrEmpty(b.Value.texture))
                        {
                            baseFileName = b.Value.texture;
                            baseFileName = baseFileName.Split(new char[] { '\\', '/' }).Last();
                            //Debug.Log(baseFileName);
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(baseFileName))
                    {
                        string allLanguagesImagePath = null;
                        if (allLanguages != null)
                        {
                            allLanguagesImagePath = GetPathOfImageWithName(RemoveImagePrefix(baseFileName), allLanguages);
                        }
                        foreach (var b in box.sentences)
                        {
                            if (string.IsNullOrEmpty(b.Value.texture))
                            {
                                if (!string.IsNullOrEmpty(allLanguagesImagePath))
                                {
                                    Debug.Log("Find texture at path : " + allLanguagesImagePath + " for " + AllLanguagesFolderName);
                                    b.Value.SetValue(allLanguagesImagePath, LocalizedDataType.Texture);
                                    box.display = true;
                                }
                                if (iData.ContainsKey(b.Key))
                                {
                                    string path = GetPathOfImageWithName(RemoveImagePrefix(baseFileName), iData[b.Key]);

                                    if (!string.IsNullOrEmpty(path))
                                    {
                                        Debug.Log("Find texture at path : " + path + " for " + b.Key);
                                        b.Value.SetValue(path, LocalizedDataType.Texture);
                                        box.display = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void AutoFillImagesWithCode(Dictionary<string, ImageData> iData)
        {
            List<int> keys = GetOrderedKeys();
            ImageData allLanguages = null;
            if (iData.ContainsKey(AllLanguagesFolderName))
            {
                allLanguages = iData[AllLanguagesFolderName];
            }

            foreach (var key in keys)
            {
                SentenceBox box = boxData[key];
                box.display = false;
                if (box.HasType(LocalizedDataType.Texture))
                {
                    box.display = true;

                    string allLanguagesImagePath = null;
                    if (allLanguages != null)
                    {
                        allLanguagesImagePath = GetPathOfImageWithName(RemoveImagePrefixCode(box.code), allLanguages);
                    }
                    foreach (var b in box.sentences)
                    {
                        if (string.IsNullOrEmpty(b.Value.texture))
                        {
                            if (!string.IsNullOrEmpty(allLanguagesImagePath))
                            {
                                Debug.Log("Find texture at path : " + allLanguagesImagePath + " for " + AllLanguagesFolderName);
                                b.Value.SetValue(allLanguagesImagePath, LocalizedDataType.Texture);
                                box.display = true;
                            }
                            if (iData.ContainsKey(b.Key))
                            {
                                string path = GetPathOfImageWithName(RemoveImagePrefixCode(box.code), iData[b.Key]);

                                if (!string.IsNullOrEmpty(path))
                                {
                                    Debug.Log("Find texture at path : " + path + " for " + b.Key);
                                    b.Value.SetValue(path, LocalizedDataType.Texture);
                                    box.display = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        string GetPathOfImageWithName(string imageName, ImageData singleLgImage)
        {
            foreach (var texture in singleLgImage.textures)
            {
                string fileName = texture.Key;

                //fileName = RemoveImagePrefix(fileName);

                if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(imageName)) return "";

                Debug.Log("Compare [" + fileName + "] and [" + imageName + "]");
                if (string.Equals(fileName, imageName, StringComparison.InvariantCultureIgnoreCase))
                {
                    Debug.Log("Found correspondance [" + fileName + "] and [" + imageName + "]");
                    return texture.Value.localPath;
                }
            }
            return "";
        }

        string RemoveImagePrefix(string name)
        {
            if (LanguageEditorSettings.imageFilePrefixIgnore <= 0)
            {
                return name;
            }

            string fileName = "";
            string[] splited = name.Split(new char[] { '-', '_' });
            if (splited.Length == 1)
            {
                return name;
            }
            else if (splited.Length == 2)
            {
                return name;
            }
            else if (splited.Length >= 3)
            {
                for (int i = 0; i < splited.Length; i++)
                {
                    if (i < LanguageEditorSettings.imageFilePrefixIgnore) continue;
                    if (fileName == "") fileName += splited[i];
                    else fileName += "_" + splited[i];
                }
            }
            return fileName;
        }

        string RemoveImagePrefixCode(string name)
        {
            if (LanguageEditorSettings.imageEditorPrefixIgnore <= 0)
            {
                return name;
            }

            string fileName = "";
            string[] splited = name.Split(new char[] { '-', '_' });
            if (splited.Length == 1)
            {
                return name;
            }
            else if (splited.Length == 2)
            {
                return name;
            }
            else if (splited.Length >= 3)
            {
                for (int i = 0; i < splited.Length; i++)
                {
                    if (i < LanguageEditorSettings.imageEditorPrefixIgnore) continue;
                    if (fileName == "") fileName += splited[i];
                    else fileName += "_" + splited[i];
                }
            }
            return fileName;
        }

        #endregion



        /// <summary>
        /// Display all sentences in a scroll view
        /// </summary>
        private void LanguagePageContent()
        {
            Edit.View(ref scrollView, () =>
            {
                Edit.Column(() =>
                {
                    SentenceContainer();
                });
            });
        }

        List<int> GetOrderedKeys()
        {
            switch (orderMode)
            {
                case Order.AscCode:
                    return boxData.OrderBy(kp => kp.Value.code).Select(kp => kp.Key).ToList();
                case Order.DescCode:
                    return boxData.OrderByDescending(kp => kp.Value.code).Select(kp => kp.Key).ToList();
                case Order.AscId:
                    return boxData.OrderBy(kp => kp.Key).Select(kp => kp.Key).ToList();
                case Order.DescId:
                    return boxData.OrderByDescending(kp => kp.Key).Select(kp => kp.Key).ToList();
                default:
                    return boxData.Select(kp => kp.Key).ToList();
            }
        }

        bool ShouldHideCode(int key)
        {
            switch (displayMode)
            {
                case Display.All:
                    return false;
                case Display.Uncompleted:
                    return IsCodeFull(key);
                case Display.Text:
                    return !CodeContainDataType(key, LocalizedDataType.Text);
                case Display.Audio:
                    return !CodeContainDataType(key, LocalizedDataType.Audio);
                case Display.Image:
                    return !CodeContainDataType(key, LocalizedDataType.Texture);
                default:
                    break;
            }
            return false;
        }

        bool CodeContainDataType(int key, LocalizedDataType type)
        {
            return boxData[key].sentences.First().Value.Exist(type);
        }

        bool IsInSearch(SentenceBox box)
        {
            string strToSearch = "";
            switch (searchIn)
            {
                case SearchIn.Code:
                    strToSearch = box.code;
                    break;
                case SearchIn.Value:
                    foreach (var lg in box.sentences)
                    {
                        strToSearch += lg.Value.text;
                        strToSearch += lg.Value.clip;
                        strToSearch += lg.Value.texture;
                    }
                    break;
                case SearchIn.Id:
                    strToSearch = box.sentences.First().Value.id.ToString();
                    break;
                default:
                    break;
            }

            switch (searchMode)
            {
                case SearchMode.Contain:
                    return strToSearch.Contains(search);
                case SearchMode.StartWith:
                    return strToSearch.StartsWith(search, StringComparison.CurrentCulture);
                case SearchMode.EndWith:
                    return strToSearch.EndsWith(search, StringComparison.CurrentCulture);
                default:
                    break;
            }

            return true;
        }

        public void FindDuplicateValues()
        {
            List<DuplicateValue> results = new List<DuplicateValue>();

            for (int i = 0; i < languagesData.Count; i++)
            {
                languagesData[i].localizedList.Clear();

                foreach (var b in boxData)
                {
                    languagesData[i].localizedList.Add(b.Value.GetSentence(languagesData[i].name));
                }
            }

            if (languagesData == null || languagesData.Count == 0)
            {
                Debug.Log("LanguageData is null or empty");
            }

            foreach (var language in languagesData)
            {
                if (language == null)
                {
                    Debug.Log("Language is null");
                    continue;
                }
                if (language.localizedList.Count == 0)
                {
                    Debug.Log("LocalizedList is empty");
                    continue;
                }
                List<IGrouping<string, Localized>> val = language.localizedList.GroupBy(x => x.text).ToList();

                foreach (var groups in val)
                {
                    if (groups.Count() > 1 && groups.First().text.Length > 0)
                    {
                        DuplicateValue duplicate = new DuplicateValue();
                        duplicate.value = groups.Key;
                        duplicate.language = language.name;

                        foreach (var grp in groups)
                        {
                            duplicate.keys.Add(grp.code); ;
                        }
                        results.Add(duplicate);
                    }
                }
            }

            Debug.Log(results.Count + " Duplicate has been found(s)");
            if (results.Count > 0)
            {
                DuplicateKeyWindow.ShowWindow(results, this);
            }
        }
        /// <summary>
        ///	Display sentences list box and theirs contents
        /// </summary>
        private void SentenceContainer()
        {
            // Get key ordered by code names
            List<int> keys = GetOrderedKeys();
            foreach (int key in keys)
            {
                SentenceBox box = boxData[key];
                bool taggedForDelete = false;

                // Hide completed box
                if (ShouldHideCode(key) && GUI.GetNameOfFocusedControl() != box.code) continue;

                if (search.Length > 0 && !IsInSearch(box))
                {
                    continue;
                }
                Edit.Row("Box", () =>
                {
                    box.display = Edit.ToggleGroup(box.display, box.code, 0, () =>
                    {
                        Edit.Column(() =>
                        {
                            bool allowText = false;
                            bool allowAudio = false;
                            bool allowImage = false;
                            Edit.Row(() =>
                            {
                                if (moreOptionsButton)
                                {
                                    string codeName = EditorGUILayout.TextField(box.code).ToLower();
                                    if (codeName != box.code)
                                    {
                                        if (codeName.Length > 1 && !IsCodeAlreadyExisting(codeName))
                                        {
                                            box.code = codeName;
                                            foreach (var pair in box.sentences)
                                            {
                                                pair.Value.code = codeName;
                                            }
                                        }
                                        else
                                        {
                                            Debug.Log("Code already existing or invalid : " + codeName);
                                        }
                                    }

                                    bool currentText = box.sentences.ElementAt(0).Value.Exist(LocalizedDataType.Text);
                                    bool currentAudio = box.sentences.ElementAt(0).Value.Exist(LocalizedDataType.Audio);
                                    bool currentImage = box.sentences.ElementAt(0).Value.Exist(LocalizedDataType.Texture);
                                    allowText = GUILayout.Toggle(currentText, "Text", EditorStyles.miniButtonLeft, GUILayout.Width(80));
                                    allowAudio = GUILayout.Toggle(currentAudio, "Audio", EditorStyles.miniButtonMid, GUILayout.Width(80));
                                    allowImage = GUILayout.Toggle(currentImage, "Image", EditorStyles.miniButtonRight, GUILayout.Width(80));
                                    GUILayout.FlexibleSpace();
                                    if (GUILayout.Button("X", GUILayout.Width(40)))
                                    {
                                        RemoveSentence(key);
                                        taggedForDelete = true;
                                    }
                                }
                                else
                                {
                                    // set allow values when button are not displayed
                                    allowText = box.sentences.ElementAt(0).Value.Exist(LocalizedDataType.Text);
                                    allowAudio = box.sentences.ElementAt(0).Value.Exist(LocalizedDataType.Audio);
                                    allowImage = box.sentences.ElementAt(0).Value.Exist(LocalizedDataType.Texture);
                                }

                            });

                            if (!taggedForDelete)
                            {
                                GUILayout.Label("ID : " + key.ToString());
                                //EditorGUILayout.Space();

                                for (int i = 0; i < languagesData.Count; i++)
                                {
                                    string lg = languagesData[i].name;
                                    Localized loca = null;
                                    box.sentences.TryGetValue(lg, out loca);
                                    if (loca == null)
                                    {
                                        loca = new Localized(box.code.ToLower(), key);
                                        box.sentences.Add(lg, loca);
                                        ChangeSentenceDataType(ref box, lg, LocalizedDataType.Text, allowText);
                                        ChangeSentenceDataType(ref box, lg, LocalizedDataType.Audio, allowAudio);
                                        ChangeSentenceDataType(ref box, lg, LocalizedDataType.Texture, allowImage);
                                    }
                                    else
                                    {
                                        bool currentText = loca.Exist(LocalizedDataType.Text);
                                        bool currentAudio = loca.Exist(LocalizedDataType.Audio);
                                        bool currentImage = loca.Exist(LocalizedDataType.Texture);
                                        if (currentText != allowText) ChangeSentenceDataType(ref box, lg, LocalizedDataType.Text, allowText);
                                        if (currentAudio != allowAudio) ChangeSentenceDataType(ref box, lg, LocalizedDataType.Audio, allowAudio);
                                        if (currentImage != allowImage) ChangeSentenceDataType(ref box, lg, LocalizedDataType.Texture, allowImage);
                                    }
                                    box.sentences[lg] = DisplaySentences(languagesData[i], loca);
                                    boxData[key] = box;
                                }
                            }
                        });
                    });
                    if (!box.display)
                    {
                        if (GUILayout.Button("X", GUILayout.Width(40)))
                        {
                            RemoveSentence(key);
                            taggedForDelete = true;
                        }
                    }

                });

                if (taggedForDelete) break;
            }
        }

        public static void ChangeSentenceDataType(ref SentenceBox box, string lg, LocalizedDataType type, bool value)
        {
            if (value)
            {
                box.sentences[lg].TryAddData(new LocalizedData("", type));
            }
            else
            {
                box.sentences[lg].RemoveData(type);
            }
        }

        /// <summary>
        /// Display content of a sentence
        /// </summary>
        private Localized DisplaySentences(LanguageData language, Localized sentence)
        {
            Edit.Column("Box", () =>
            {
                EditorGUILayout.LabelField(language.name);
                //sentence.clip = LanguageWindow.AudioToPath(EditorGUILayout.ObjectField(lg.GetName(), LanguageWindow.PathToAudio(sentence.clip), typeof(AudioClip), true));
                if (sentence.Exist(LocalizedDataType.Audio))
                {
                    sentence.clip = LanguageWindow.AssetToPath(EditorGUILayout.ObjectField(Resources.Load(sentence.clip), typeof(AudioClip), false));
                    sentence.clip = CleanResourcePath(sentence.clip);
                    if (sentence.clip != "") EditorGUILayout.LabelField(sentence.clip);
                }
                if (sentence.Exist(LocalizedDataType.Texture))
                {
                    sentence.texture = LanguageWindow.AssetToPath(EditorGUILayout.ObjectField(Resources.Load(sentence.texture), typeof(Texture), false));
                    sentence.texture = CleanResourcePath(sentence.texture);
                    if (sentence.texture != "") EditorGUILayout.LabelField(sentence.texture);
                }
                if (sentence.Exist(LocalizedDataType.Text))
                {
                    Edit.Row(() =>
                    {
                        GUI.SetNextControlName(sentence.code);
                        if (language.isRightToLeft && rightTextField != null)
                        {
                            int count = sentence.text.Length;
                            sentence.text = GUILayout.TextArea(sentence.text, rightTextField);
                            if (count < sentence.text.Length)
                            {
                                TextEditor editor = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                                editor.MoveLeft();
                            }
                        }
                        else
                        {
                            sentence.text = EditorGUILayout.TextArea(sentence.text);
                        }
                        if (GUILayout.Button("", EditorStyles.miniButton, GUILayout.Width(14)))
                        {
                        }
                    });
                }
            });

            return sentence;
        }

        #endregion Display

        #region DataManagement

        private string CleanResourcePath(string dirtyPath)
        {
            if (!dirtyPath.Contains(".") || dirtyPath.Length < 2) return dirtyPath;

            string[] str = dirtyPath.Split(new char[] { '\\', '/' });
            string path = "";
            bool canAdd = false;
            for (int i = 0; i < str.Length; i++)
            {
                if (canAdd)
                {
                    path += str[i] + '/';
                }
                else if (str[i] == "Resources")
                {
                    canAdd = true;
                }
            }
            return path.Split('.')[0];
        }

        /// <summary>
        /// Check if all sentences of a code have been completed
        /// </summary>
        private bool IsCodeFull(int key)
        {
            foreach (var item in boxData[key].sentences)
            {
                if (item.Value.Exist(LocalizedDataType.Text) && item.Value.text.Length <= 0)
                {
                    return false;
                }
                else if (item.Value.Exist(LocalizedDataType.Audio) && item.Value.clip.Length <= 0)
                {
                    return false;
                }
                else if (item.Value.Exist(LocalizedDataType.Texture) && item.Value.texture.Length <= 0)
                {
                    return false;
                }
            }
            return true;
        }

        private void RemoveSentence(int hash)
        {
            boxData.Remove(hash);
        }

        private void OrderBox()
        {
            boxData.OrderBy(kp => kp.Value.code);
        }

        bool IsCodeAlreadyExisting(string code)
        {
            return boxData.Values.ToList().Any(x => x.code == code);
        }

        /// <summary>
        /// Add new localized value
        /// </summary>
        /// <param name="code"></param>
        /// <returns>the identifier of the new value</returns>
        public int AddSentence(string code, int id = 0)
        {
            int identifier = id == 0 ? GenerateUniqueIdentifier() : id;
            code = code.ToLower();

            if (!string.IsNullOrEmpty(code) && !IsCodeAlreadyExisting(code) && !boxData.ContainsKey(identifier))
            {
                //Debug.Log("Add sentence : " + code);
                boxData.Add(identifier, new SentenceBox(true, code));
                for (int i = 0; i < languagesData.Count; i++)
                {
                    Localized loca = new Localized(code, identifier);
                    if (addTextLocalization) loca.TryAddData(new LocalizedData("", LocalizedDataType.Text));
                    if (addAudioLocalization) loca.TryAddData(new LocalizedData("", LocalizedDataType.Audio));
                    if (addImageLocalization) loca.TryAddData(new LocalizedData("", LocalizedDataType.Texture));
                    boxData[identifier].AddSentence(languagesData[i].name, loca);
                }

                OrderBox();
            }
            else
            {
                identifier = -1;
                Debug.Log("Code Already exist");
                return boxData.Values.ToList().Find(x => x.code == code).sentences.First().Value.id;
            }
            return identifier;
        }

        public int AddDataToSentence(string language, Localized loca, bool allowOverride)
        {
            if (!string.IsNullOrEmpty(loca.code))
            {
                int languageIndex = -1;
                for (int i = 0; i < languagesData.Count; i++)
                {
                    if (languagesData[i].name.ToLower() == language.ToLower())
                    {
                        languageIndex = i;
                        break;
                    }
                }

                if (languageIndex < 0)
                {
                    Debug.LogError("Language not found");
                    return languageIndex;
                }

                int foundIndex = -1;
                foreach (var item in boxData)
                {
                    if (item.Value.code == loca.code)
                    {
                        foundIndex = item.Key;
                    }
                }

                if (foundIndex < 0)
                {
                    loca.id = AddSentence(loca.code);
                }
                else if (!allowOverride)
                {
                    Debug.Log("<color=red>Override not allowed.</color> Code [" + loca.code + "] already exist at index [" + foundIndex + "]");
                    return -1;
                }
                else
                {
                    loca.id = foundIndex;
                }

                Debug.Log("<color=green>New entry added.</color> Code [" + loca.code + "] added at index [" + loca.id + "]");

                if (loca.Exist(LocalizedDataType.Texture) && languageIndex != 0 && !boxData[loca.id].sentences.First().Value.Exist(LocalizedDataType.Audio))
                {
                    boxData[loca.id].sentences.First().Value.TryAddData(new LocalizedData("", LocalizedDataType.Audio));
                }

                if (loca.Exist(LocalizedDataType.Texture) && languageIndex != 0 && !boxData[loca.id].sentences.First().Value.Exist(LocalizedDataType.Texture))
                {
                    boxData[loca.id].sentences.First().Value.TryAddData(new LocalizedData("", LocalizedDataType.Texture));
                }

                boxData[loca.id].sentences[languagesData[languageIndex].name] = loca;

                return loca.id;
            }
            return -1;
        }

        public void AddDataToSentence(string code, string language, LocalizedDataType type, string value)
        {
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(language) || string.IsNullOrEmpty(value)) return;
            int boxIndex = -1;
            SentenceBox box = null;

            foreach (var item in boxData)
            {
                if (item.Value.code == code)
                {
                    boxIndex = item.Key;
                    box = item.Value;
                    break;
                }
            }

            if (boxIndex < 0)
            {
                boxIndex = AddSentence(code);
                box = boxData[boxIndex];
            }

            if (box.sentences != null && box.sentences.ContainsKey(language))
            {
                //Debug.Log("[c:" + code + " | v:" + value + " | t:" + type.ToString() + "] \nCode found and language found");
                box.AddDataToSentence(language, new LocalizedData(value, type));
            }
            else
            {
                //Debug.Log("[c:" + code + " | v:" + value + "] \nCode found and creating language ");
                Localized loca = new Localized(code, boxIndex);
                loca.TryAddData(new LocalizedData(value, type));
                box.AddSentence(language, loca);
            }
        }

        /// <summary>
        /// Export to CSV or TSV text file
        /// </summary>
        /// <param name="separator"></param>
        /// <returns></returns>
        public string ExportToTextFile(char separator)
        {
            string end = System.Environment.NewLine;
            string file = "code";

            foreach (var lg in languagesData)
            {
                file += separator + lg.name.ToString();
            }
            file += end;
            foreach (var pair in window.lgSentencePage.boxData)
            {
                file += pair.Value.code;
                foreach (var sentence in pair.Value.sentences)
                {
                    file += separator + sentence.Value.text;
                }
                file += end;
            }

            return file;
        }

        public void ImportFromTextFile(string csv, char separator)
        {
            List<string> lines = new List<string>(csv.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None));

            if (lines.Count <= 1)
            {
                Debug.Log("Not enough data");
                return;
            }
            List<string> header = new List<string>(lines[0].Split(separator));
            lines.RemoveAt(0);
            header.RemoveAt(0);

            ImportData(header, lines, separator);
        }

        public void ImportData(List<string> languages, List<string> lines, char separator)
        {
            int languageCount = languages.Count;
            int sentenceCount = lines.Count;
            List<string> codes = new List<string>();
            {
                List<string> currentCode = GetCurrentCodes();
                // Get all codes in file and create
                for (int i = 0; i < sentenceCount; i++)
                {
                    string code = lines[i].Split(separator)[0].ToLower();
                    if (!codes.Contains(code) && code.Length > 3)
                    {
                        if (!currentCode.Contains(code))
                        {
                            AddSentence(code);
                        }
                        codes.Add(code);
                    }
                }
            }
            string[] languagesNames = LanguageManager.GetLanguagesName();
            List<string> languagesValues = new List<string>();
            for (int i = 0; i < languageCount; i++)
            {
                bool done = false;
                for (int j = 0; j < languagesNames.Length; j++)
                {
                    //Debug.Log(languages[i].ToLower() + " vs " + languagesNames[j].ToLower() +" = "+ (languages[i].ToLower() == languagesNames[j].ToLower()));
                    if (languages[i].ToLower() == languagesNames[j].ToLower())
                    {
                        Debug.Log("Import language found : " + languages[i]);
                        done = true;
                        languagesValues.Add(languagesNames[j]);
                        break;
                    }
                }
                if (!done)
                {
                    // Add bad value to understand language is not found
                    languagesValues.Add("");
                }
            }

            Debug.Log("Import sentences : " + sentenceCount);
            for (int i = 0; i < sentenceCount; i++)
            {
                GenerateSentence(languagesValues, new List<string>(lines[i].Split(separator)));
            }
            OrderBox();
        }

        private void GenerateSentence(List<string> language, List<string> line)
        {
            string code = line[0].ToLower();
            if (code.Length < 3) return;

            line.RemoveAt(0);
            foreach (var item in boxData)
            {
                //Debug.Log(code + " vs " + item.Value.code + " = " + (item.Value.code == code).ToString());
                if (code == item.Value.code)
                {
                    item.Value.sentences.Clear();
                    for (int i = 0; i < line.Count; i++)
                    {
                        if (language[i].Length > 0)
                        {
                            //Debug.Log("Add :" + ((Language)language[i]).ToString() + " " + code);
                            Localized sentence = new Localized(code, GenerateUniqueIdentifier());
                            sentence.TryAddData(new LocalizedData(line[i], LocalizedDataType.Text));
                            item.Value.AddSentence(language[i], sentence);
                        }
                    }
                    break;
                }
            }
        }

        private List<string> GetCurrentCodes()
        {
            List<string> languageCodes = new List<string>();
            foreach (var item in boxData)
            {
                languageCodes.Add(item.Value.code);
            }
            return languageCodes;
        }

        public Localized[] GetLocalizedSample()
        {
            List<Localized> localized = new List<Localized>();
            foreach (var item in boxData)
            {
                localized.Add(item.Value.sentences.First().Value);
            }
            return localized.ToArray();
        }


        #endregion DataManagement
    }
}