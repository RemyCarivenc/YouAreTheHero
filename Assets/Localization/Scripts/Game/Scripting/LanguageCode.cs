using UnityEngine;
using UnityEngine.UIElements;

namespace Localization
{
	/// <summary>
	/// An easy way of implementing localisation inside a script
	/// See "ExampleScript.cs"
	/// </summary>
	[System.Serializable]
	public class LanguageCode
	{
		[SerializeField]
		protected int code = 1;
		
		public int Code { 
			get 
			{
				return code; 
			}
		}

		public LanguageCode()
		{
			
		}

		public LanguageCode(int c)
		{
			code = c;
		}

		/// <summary>
		/// In normal condition it should not be used.false Make sure the code is valid
		/// </summary>
		/// <param name="c"></param>
		// I didn't consider that the code would change but the tool "Immersive UI" need it.
		// nothing break if it change but you have to make sure it is valid.
		public void SetCode(int c)
		{
			code = c;
		}

		public static implicit operator string(LanguageCode d)
		{
			return LanguageManager.GetSentence(d.code);
		}

		public static implicit operator AudioClip(LanguageCode d)
		{
			return LanguageManager.GetAudio(d.code);
		}

		public static implicit operator Texture(LanguageCode d)
		{
			return LanguageManager.GetTexture(d.code);
		}
	}
}
