using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace Localization.Core
{
	/// <summary>
	/// Class used to generate enum in editor mode ONLY
	/// </summary>
	public static class EnumGenerator
	{
		/// <summary>
		/// Generate an enum in a new file (delete the file if it already exist)
		/// Values are "auto" defined by the compiler
		/// </summary>
		public static void GenerateEnum(string destinationPath, string enumName, string[] enumEntries, string namespaceName = "")
		{
			string filePathAndName = GetFilePathAndName(enumName, destinationPath);
			if (File.Exists(filePathAndName)) File.Delete(filePathAndName);

			WriteEnum(filePathAndName, enumName, namespaceName, () =>
			{
				string text = "";
				for (int i = 0; i < enumEntries.Length; i++)
				{
					text += "\t" + enumEntries[i] + "," + System.Environment.NewLine;
				}
				return text;
			});

			AssetDatabase.Refresh();
		}

		/// <summary>
		/// Generate an enum where all names keep theirs values even when changing place (It use the name hash code)
		/// </summary>
		/// <param name="enumName">The name of the Enum</param>
		/// <param name="enumEntries">Values of the Enum</param>
		public static void GenerateStaticEnum(string destinationPath, string enumName, string[] enumEntries, string namespaceName = "")
		{
			string filePathAndName = GetFilePathAndName(enumName, destinationPath);

			if (File.Exists(filePathAndName)) File.Delete(filePathAndName);

			WriteEnum(filePathAndName, enumName, namespaceName, () =>
			{
				string text = "";
				for (int i = 0; i < enumEntries.Length; i++)
				{
					text += "\t" + enumEntries[i] + " = " + enumEntries[i].GetHashCode() + "," + System.Environment.NewLine;
				}
				return text;
			});

			AssetDatabase.Refresh();
		}

		/// <summary>
		/// Generate an enum with the values already defined
		/// </summary>
		/// <param name="enumName">The name of the Enum</param>
		/// <param name="enumEntries">Values of the Enum</param>
		public static void GenerateEnumWithValues(string destinationPath, string enumName, string[] enumEntries, int[] values, string namespaceName = "")
		{
			if (values.Length != enumEntries.Length)
			{
				Debug.LogError("Entries and Values does not match");
				return;
			}
			if (IsValuesDuplicated(values))
			{
				Debug.LogError("Values contains some duplicates");
				return;
			}
			string filePathAndName = GetFilePathAndName(enumName, destinationPath);

			if (File.Exists(filePathAndName)) File.Delete(filePathAndName);

			WriteEnum(filePathAndName, enumName, namespaceName, () =>
			{
				string text = "";
				for (int i = 0; i < enumEntries.Length; i++)
				{
					text += "\t" + enumEntries[i] + " = " + values[i] + "," + System.Environment.NewLine;
				}
				return text;
			});

			AssetDatabase.Refresh();
		}

		/// <summary>
		/// Generate an enum with the values already defined
		/// </summary>
		/// <param name="enumName">The name of the Enum</param>
		/// <param name="enumEntries">Values of the Enum</param>
		public static void GenerateEnumSortedList(string destinationPath, string enumName, SortedList<int, string> enumEntries, string namespaceName = "")
		{
			if (enumEntries == null)
			{
				Debug.LogError("List is null");
				return;
			}
			string filePathAndName = GetFilePathAndName(enumName, destinationPath);

			if (File.Exists(filePathAndName)) File.Delete(filePathAndName);

			WriteEnum(filePathAndName, enumName, namespaceName, () =>
			{
				string text = "";
				foreach (var item in enumEntries)
				{
					text += "\t" + item.Value + " = " + item.Key + "," + System.Environment.NewLine;
				}
				return text;
			});

			AssetDatabase.Refresh();
		}

		static void WriteEnum(string filePathAndName, string enumName, string namespaceName, System.Func<string> writeContent)
		{
			using (StreamWriter streamWriter = new StreamWriter(filePathAndName))
			{
				streamWriter.WriteLine("namespace " + namespaceName + "{");
				streamWriter.WriteLine("public enum " + enumName);
				streamWriter.WriteLine("{");

				streamWriter.Write(writeContent());

				streamWriter.WriteLine("}");
				streamWriter.WriteLine("}");
			}
		}

		static string GetFilePathAndName(string enumName, string folderPath)
		{
			FileManager.CreateFolder(folderPath);
			return Path.Combine(folderPath, enumName + ".cs");
		}

		static bool IsValuesDuplicated(int[] values)
		{
			for (int i = 0; i < values.Length; i++)
			{
				for (int j = i + 1; j < values.Length; j++)
				{
					if (values[i] == values[j]) return true;
				}
			}
			return false;
		}

		static string CleanCommentString(string com)
		{
			com = com.Replace("\n", ", ");
			return com;
		}

		static bool IsDirectoryExisting(string path)
		{
			if (!Directory.Exists(path))
			{
				DirectoryInfo info = Directory.CreateDirectory(path);
				if (info != null && info.Exists)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			return true;
		}
	}
}

