using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Localization.Core
{
	public abstract class EditorWindowPage<T>
	{
		protected T window;

		public EditorWindowPage(T pWindow)
		{
			window = pWindow;

		}
		public abstract void Enable();
		public abstract void Disable();
		/// <summary>
		/// Update when the world scene is changed
		/// </summary>
		public abstract void DrawScene();
		/// <summary>
		/// Update when the editor window is changed
		/// </summary>
		public abstract void DrawEditor();
		public abstract void Save();
		public abstract void Load();
		public abstract void OnCompiled();
	}
}