using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Localization.Core
{
	/// <summary>
	/// Class that provide with a different visualisation when creating editor tools
	/// </summary>
    public static class EditorImprovement
    {
        public static void Row(GUIStyle s, System.Action action)
        {
            EditorGUILayout.BeginHorizontal(s);
            action();
            EditorGUILayout.EndHorizontal();
        }

        public static void Row(System.Action action)
        {
            EditorGUILayout.BeginHorizontal();
            action();
            EditorGUILayout.EndHorizontal();
        }

        public static void Column(GUIStyle s, System.Action action)
        {
            EditorGUILayout.BeginVertical(s);
            action();
            EditorGUILayout.EndVertical();
        }

        public static void Column(System.Action action)
        {
            EditorGUILayout.BeginVertical();
            action();
            EditorGUILayout.EndVertical();
        }

        public static void View(ref Vector2 value, System.Action action, params GUILayoutOption[] opt)
        {
            value = GUILayout.BeginScrollView(value, opt);
            action();
            EditorGUILayout.EndScrollView();
        }
		
		public static string Write
        {
            set { EditorGUILayout.LabelField(value); }
        }

        public static void Indent( int count, System.Action action)
        {
            EditorGUI.indentLevel += count;
            action();
            EditorGUI.indentLevel -= count;
        }

        public static bool ToggleGroup(bool value, string name, int indent, GUIStyle opt, System.Action action)
        {
            Column(opt, () =>
            {
                if (value = EditorGUILayout.Foldout(value, name, true))
                {
                    Indent(indent, () =>
                    {
                        action();
                    });
                }
            });
            return value;
        }

        public static bool ToggleGroup(bool value, string name, int indent, System.Action action)
        {
            Column(() =>
            {
                if (value = EditorGUILayout.Foldout(value, name, true))
                {
                    Indent(indent, () =>
                    {
                        action();
                    });
                }
            });
            return value;
        }

		internal static void Row()
		{
			throw new NotImplementedException();
		}
	}
}


