using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace Localization.Core
{
	/// <summary>
	/// Update the text ui in editor mode to display the selected code
	/// </summary>
	[CustomEditor(typeof(LocalizationText))]
	public class LanguageTextInspector : Editor
	{
		LocalizationText property;
		Text textUI;
		int code;

		private void OnEnable()
		{
			property = target as LocalizationText;
			textUI = property.GetComponent<Text>();
		}

		public override void OnInspectorGUI()
		{
			code = property.Localized.Code;

			base.OnInspectorGUI();
			
			if(code != property.Localized.Code && textUI)
			{
				textUI.text = LanguageCodeUtility.GetName(property.Localized.Code);
			}
		}
	}
}
