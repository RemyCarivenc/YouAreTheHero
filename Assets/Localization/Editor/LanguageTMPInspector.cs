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
	[CustomEditor(typeof(LocalizationTMP))]
	public class LanguageTMPInspector : Editor
	{
		LocalizationTMP property;
#if TMPPRO_PRESENT
		TMPro.TMP_Text textUI;
#endif
		int code;

		private void OnEnable()
		{
			property = target as LocalizationTMP;
#if TMPPRO_PRESENT
			textUI = property.GetComponent<TMPro.TMP_Text>();
#endif
		}

		public override void OnInspectorGUI()
		{
			code = property.Localized.Code;

			base.OnInspectorGUI();

#if TMPPRO_PRESENT
			if(code != property.Localized.Code && textUI)
			{
				textUI.text = LanguageCodeUtility.GetName(property.Localized.Code);
			}
#endif
		}
	}
}
