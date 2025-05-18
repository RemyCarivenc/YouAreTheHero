using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Localization.Core
{
	/// <summary>
	/// Used to convert from int code to string for dev user in editor
	/// </summary>
	public static class LanguageCodeUtility
	{
		static SortedList<int, string> codes;

		public static bool IsGenerated()
		{
			return codes != null && codes.Count > 0;
		}

		public static SortedList<int, string> GetList()
		{
			return new SortedList<int, string>(codes);
		}

		public static void Generate(SortedList<int, string> list)
		{
			codes = new SortedList<int, string>(list);
		}

		public static void Generate(int[] keys, string[] values)
		{
			if(keys.Length == values.Length)
			{
				codes = new SortedList<int, string>();

				for (int i = 0; i < keys.Length; i++)
				{
					int key = keys[i];
					
					if (!codes.ContainsKey(key))
					{
						codes.Add(key, values[i]);
					}
					else
					{
						Debug.LogError("Same key found twice");
					}
				}
			}
			else
			{
				Debug.LogError("Key and Value are not the same size");
			}
		}


		public static string GetName(int code)
		{
			if (codes != null && codes.TryGetValue(code, out string value))
			{
				return value;
			}
			return "";
		}

		public static string[] GetNames()
		{
			if (codes != null)
			{
				return codes.Values.ToArray();
			}
			return null;
		}

		public static int[] GetKeys()
		{
			if (codes != null)
			{
				return codes.Keys.ToArray();
			}
			return null;
		}

		public static int GetCode(string name)
		{
			if(codes != null)
			{
				foreach (var item in codes)
				{
					if (item.Value == name)
					{
						return item.Key;
					}
				}
			}
			
			return -1;
		}
	}
}
