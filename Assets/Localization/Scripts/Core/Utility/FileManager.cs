using UnityEngine;
using System.IO;
using UnityEngine.Networking;

namespace Localization.Core
{
    /// <summary>
    /// File management wrapper
    /// </summary>
    public static class FileManager
    {
        public static void Save(string path, string content, bool overwrite = false)
        {
            if (overwrite && File.Exists(path)) File.Delete(path);

            File.WriteAllText(path, content);
        }

        public static string Load(string path)
        {
#if (UNITY_ANDROID || UNITY_WEBGL) && !UNITY_EDITOR
			path = Path.ChangeExtension(path, null);
			Debug.Log("PATH TO LOAD : "+path);
            TextAsset textAsset = Resources.Load<TextAsset>(path);
            return textAsset.text;
#else
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            return "";
#endif
        }

        /// <summary>
        /// Create a text file from an object
        /// </summary>
        public static void Save<T>(string path, T content)
        {
            Save(path, JsonUtility.ToJson(content, true), true);
        }

        /// <summary>
        /// Read a text file and convert it to an object
        /// </summary>
        public static void Load<T>(string path, ref T objectToLoad)
        {
            JsonUtility.FromJsonOverwrite(Load(path), objectToLoad);
        }

        /// <summary>
        /// Create the folder if it does not exist
        /// </summary>
        public static void CreateFolder(string pathAndName)
        {
            if (!Directory.Exists(pathAndName))
            {
                Directory.CreateDirectory(pathAndName);
            }
        }

        /// <summary>
        /// Delete the folder if it exist
        /// </summary>
        public static void DeleteFolder(string pathAndName)
        {
            if (Directory.Exists(pathAndName))
            {
                Directory.Delete(pathAndName);
            }
        }

        /// <summary>
        /// Delete the folder if it exist
        /// </summary>
        public static void DeleteFile(string pathAndName)
        {
            if (File.Exists(pathAndName))
            {
                File.Delete(pathAndName);
            }
        }

        public static bool IsValidDirector(string path)
        {
            return Directory.Exists(path);
        }

        public static bool IsValidFile(string path)
        {
            //#if UNITY_ANDROID
            //		UnityWebRequest www = UnityWebRequest.Get(path);
            //		www.SendWebRequest();
            //		if (www.isNetworkError || www.isHttpError)
            //		{
            //			Debug.Log(www.error);
            //			return false;
            //		}
            //		else if(www.downloadHandler.text.Length > 1)
            //		{
            //			return true;
            //		}
            //		return false;
            //#else
            return File.Exists(path);
            //#endif
        }

        public static string[] GetAllFiles(string path, string extention)
        {
            return System.IO.Directory.GetFiles(path, extention, System.IO.SearchOption.TopDirectoryOnly);
        }
    }
}