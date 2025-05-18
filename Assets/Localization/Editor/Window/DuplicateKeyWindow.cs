using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Localization.Core
{
    public class DuplicateKeyWindow : EditorWindow
    {
        List<DuplicateValue> data;
        LanguageEditor holderWindow;
        public static DuplicateKeyWindow ShowWindow(List<DuplicateValue> data, LanguageEditor holder)
        {
            DuplicateKeyWindow w = EditorWindow.GetWindow<DuplicateKeyWindow>();
            w.SetData(data, holder);
            return w;
        }

        public static DuplicateKeyWindow GetWindow()
        {
            return EditorWindow.GetWindow<DuplicateKeyWindow>();
        }

        public void SetData(List<DuplicateValue> d, LanguageEditor holder)
        {
            data = d;
            holderWindow = holder;
        }

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Refresh"))
            {
                if (holderWindow != null)
                {
                    data.Clear();
                    holderWindow.FindDuplicateValues();
                }
            }
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Language", GUILayout.Width(100));
            EditorGUILayout.LabelField("Value", GUILayout.Width(150));
            EditorGUILayout.LabelField("Keys");
            GUILayout.EndHorizontal();
            if (data == null || holderWindow == null || !EditorWindow.HasOpenInstances<LanguageWindow>())
            {
                this.Close();
                return;
            }
            foreach (var item in data)
            {
                if (item == null) continue;

                GUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(item.language, GUILayout.Width(100));
                EditorGUILayout.LabelField(item.value, GUILayout.Width(150));
                string s = "";
                foreach (var key in item.keys)
                {
                    if (s.Length == 0)
                        s += key;
                    else
                        s += ", " + key;
                }
                EditorGUILayout.LabelField(s);
                if (GUILayout.Button("Show", GUILayout.Width(60)))
                {
                    holderWindow.search = item.value;
                    holderWindow.searchMode = LanguageEditor.SearchMode.Contain;
                    holderWindow.searchIn = LanguageEditor.SearchIn.Value;
                    holderWindow.displayMode = LanguageEditor.Display.All;
                    EditorWindow.FocusWindowIfItsOpen<LanguageWindow>();
                }
                GUILayout.EndHorizontal();
            }
        }
    }

    public class DuplicateValue
    {
        public string value;
        public string language;
        public List<string> keys;

        public DuplicateValue()
        {
            keys = new List<string>();
        }
    }
}