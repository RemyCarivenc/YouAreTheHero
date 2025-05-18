using System;

namespace Localization
{
	/*	How to use :
	 
		languageAuto.RegisterCallback(() => {
			textComponent.text = languageAuto;
		});
		

		languageAuto.RegisterCallback(FunctionName);
	*/

	/// <summary>
	/// Same as LanguageCode but it provide an action called everytime the language update.
	/// See "ExampleScriptAuto.cs"
	/// </summary>
	[Serializable]
	public class LanguageAuto : LanguageCode
	{
		Action refreshFuncRef;
		bool alreadyRegistered;

		public LanguageAuto()
		{
			alreadyRegistered = false;
		}

		public LanguageAuto(int code):base(code)
		{
			alreadyRegistered = false;
		}

		public void RegisterCallback(Action refreshFunc)
		{
			refreshFuncRef = refreshFunc;
			refreshFuncRef();
			if (!alreadyRegistered)
			{
				alreadyRegistered = true;
				LanguageManager.LanguageChanged += AutoUpdate;
			}
		}

		void AutoUpdate()
		{
			if (refreshFuncRef != null)
			{
				refreshFuncRef();
			}
		}

		public void UnregisterCallback()
		{
			LanguageManager.LanguageChanged -= AutoUpdate;
			refreshFuncRef = null;
			alreadyRegistered = false;
		}

		~LanguageAuto()
		{
			refreshFuncRef = null;
			LanguageManager.LanguageChanged -= AutoUpdate;
		}
	}
}