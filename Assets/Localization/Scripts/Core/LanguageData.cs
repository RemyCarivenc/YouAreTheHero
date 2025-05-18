using System.Collections.Generic;
using UnityEngine;

namespace Localization.Core
{
    /// <summary>
    /// All data of one language
    /// </summary>
    public class LanguageData
    {
        public string name { get; set; }
        public bool isRightToLeft = false;
        public List<Localized> localizedList;
    }

    public enum LocalizedDataType
    {
        Text,
        Audio,
        Texture,
    }

    [System.Serializable]
    public class Localized
    {
        [SerializeField]
        public int id;
        [SerializeField]
        public string code;
        [SerializeField]
        List<LocalizedData> localizedDataHolder;

        #region interface
        public string text
        {
            get
            {
                return GetData(LocalizedDataType.Text);
            }
            set
            {
                SetValue(value, LocalizedDataType.Text);
            }
        }

        public string clip
        {
            get
            {
                return GetData(LocalizedDataType.Audio);
            }
            set
            {
                SetValue(value, LocalizedDataType.Audio);
            }
        }

        public string texture
        {
            get
            {
                return GetData(LocalizedDataType.Texture);
            }
            set
            {
                SetValue(value, LocalizedDataType.Texture);
            }
        }
        #endregion

        public Localized()
        {
            id = 0;
            code = "";
            localizedDataHolder = new List<LocalizedData>();
        }

        public Localized(string cd, int id)
        {
            code = cd;
            this.id = id;
            localizedDataHolder = new List<LocalizedData>();
        }

        public bool Exist(LocalizedDataType type)
        {
            for (int i = 0; i < localizedDataHolder.Count; i++)
            {
                if (localizedDataHolder[i].Type == type) return true;
            }
            return false;
        }

        public void SetValue(string value, LocalizedDataType type)
        {
            foreach (var item in localizedDataHolder)
            {
                if (type == item.Type)
                {
                    item.value = value;
                }
            }
        }

        public bool TryAddData(LocalizedData data)
        {
            foreach (var item in localizedDataHolder)
            {
                if (data.Type == item.Type)
                {
                    SetValue(data.value, data.Type);
                    return false;
                }
            }
            localizedDataHolder.Add(data);
            return true;
        }

        public void RemoveData(LocalizedDataType type)
        {
            LocalizedData data = null;
            foreach (var item in localizedDataHolder)
            {
                if (type == item.Type)
                {
                    data = item;
                    break;
                }
            }
            if (data != null)
            {
                localizedDataHolder.Remove(data);
            }
        }

        public bool HasType(LocalizedDataType type)
        {
            for (int i = 0; i < localizedDataHolder.Count; i++)
            {
                if (localizedDataHolder[i].Type == type)
                {
                    return true;
                }
            }
            return false;
        }

        public string GetData(LocalizedDataType type)
        {
            for (int i = 0; i < localizedDataHolder.Count; i++)
            {
                if (localizedDataHolder[i].Type == type)
                {
                    return localizedDataHolder[i].value;
                }
            }
            return "";
        }
    }


    [System.Serializable]
    public class LocalizedData
    {
        [SerializeField]
        protected LocalizedDataType type;

        public LocalizedDataType Type
        {
            get
            {
                return type;
            }
        }

        [SerializeField]
        public string value = "";

        public LocalizedData(string txt, LocalizedDataType dataType)
        {
            value = txt;
            type = dataType;
        }
    }

}
