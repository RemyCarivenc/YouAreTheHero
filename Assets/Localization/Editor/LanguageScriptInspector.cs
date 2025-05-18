using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Localization.Core
{
    /// <summary>
    /// Override the display in inspector of a Language code enumerator to provide a search box.
    /// </summary>
    [CustomPropertyDrawer(typeof(LanguageCode), true)]
    public class LanguageScriptInspector : PropertyDrawer
    {

        private const float fixedElementHeight = 25;        // constant height of an element
        private const float fixedAdditionnalHeight = 30;
        private float incrementHeight = 0;
        private int selected = -5;
        public static string search = "";
        private LanguageWindow editor = null;

        // Everything in the region below is unreadable, you should not try to read it.
        // In a CustomPropertyDrawer, I can't use GUI LAYOUT so I have to control manually position and size of all elements
        // Resulting to a lot of magic numbers to make it look good
        #region BadLands
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            incrementHeight = 0;
            label = EditorGUI.BeginProperty(position, label, property);
            int indent = EditorGUI.indentLevel;
            //EditorGUI.indentLevel = 0;
            Rect content = position;
            content.y += 10;
            Rect boxContent = content;
            boxContent.y -= 5;
            boxContent.x -= 5;
            boxContent.height = property.isExpanded ? 105 : 30; // Change the height of the box when open/closed
            boxContent.width += 7;
            GUI.Box(boxContent, new GUIContent());
            float baseWidth = content.width;
            if (!LanguageCodeUtility.IsGenerated())
            {
                content.height = 18;
                EditorGUI.LabelField(content, "LOCALIZATION TOOL NOT LOADED");
                content.x = position.x + baseWidth - 45;
                content.width = 42;
                if (GUI.Button(content, "Load"))
                {
                    LanguageWindow.GetWindow().Close();
                }
                content.x = position.x + baseWidth - 90;
                if (GUI.Button(content, "Open"))
                {
                    LanguageWindow.ShowWindow();
                }
                EditorGUI.EndProperty();
                EditorGUI.indentLevel = indent;
                return;
            }

            content.height = 20;
            SerializedProperty codeField = property.FindPropertyRelative("code");
            EditorGUI.BeginDisabledGroup(true);
            content.width -= 50;
            //EditorGUI.PropertyField(content, codeField, label);
            //content.y += 20;
            EditorGUI.LabelField(content, property.displayName, LanguageCodeUtility.GetName(codeField.intValue));
            //content.y += 20;
            //if(LanguageCodes.IsGenerated())
            //	
            EditorGUI.EndDisabledGroup();
            content.width = 40;
            content.height = 15;
            content.x = position.x + baseWidth - 40;
            property.isExpanded = GUI.Toggle(content, property.isExpanded, property.isExpanded ? "-" : "+", "Button");
            content.width = position.width;
            content.x = position.x;
            content.height = 20;
            if (property.isExpanded && codeField != null)
            {
                Modifier(content, property);
            }

            EditorGUI.EndProperty();
            EditorGUI.indentLevel = indent;
        }

        void Modifier(Rect content, SerializedProperty property)
        {
            SerializedProperty codeField = property.FindPropertyRelative("code");
            if (selected < 0) selected = codeField.intValue;

            incrementHeight += 80;
            content.y += fixedElementHeight;
            search = EditorGUI.TextField(content, new GUIContent("Search", ""), search);
            //if (results == null) results = new List<string>(codeField.enumNames);

            //List<string> searchResult = new List<string>(results);
            SortedList<int, string> searchList = LanguageCodeUtility.GetList();

            if (search.Length > 0)
            {
                //searchResult.RemoveAll(x => !x.Contains(search));
                searchList = new SortedList<int, string>(searchList.Where(x => x.Value.Contains(search)).ToDictionary(q => q.Key, q => q.Value));
                if (!searchList.ContainsKey(selected))
                {
                    selected = searchList.FirstOrDefault().Key;
                }
            }
            content.y += fixedElementHeight;
            if (searchList == null || searchList.Count == 0) return;
            selected = EditorGUI.IntPopup(content, selected, searchList.Values.ToArray(), searchList.Keys.ToArray());
            //selected = EditorGUI.Popup(content, selected, searchResult.ToArray());
            content.y += fixedElementHeight;
            Rect editorButton = new Rect(content);
            content.width = content.width * 0.66f;
            editorButton.width = (editorButton.width - content.width) - 5;
            content.x += editorButton.width + 5;
            if (GUI.Button(content, "Validate"))
            {
                if (searchList[selected].Length > 0)
                {
                    Debug.Log("Code selected: " + searchList[selected] + " / id: " + selected);

                    codeField.intValue = selected;
                    search = "";
                    property.isExpanded = false;
                }
            }
            incrementHeight += 20;
            content.y += fixedElementHeight;

            // Link the current search to the editor window
            if (editor == null)
            {
                if (GUI.Button(editorButton, "Editor"))
                {
                    editor = EditorWindow.GetWindow(typeof(LanguageWindow)) as LanguageWindow;
                }
            }
            else
            {
                if (GUI.Button(editorButton, "Editor"))
                {
                    editor.Focus();
                }
                editor.lgSentencePage.search = search;
            }
        }
        #endregion

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + (property.isExpanded ? 90 : 15);
        }

    }
}
