using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region singleton
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("GameManager").AddComponent<GameManager>();
                DontDestroyOnLoad(instance);
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }
    #endregion

    public AdventureSheet adventureSheet { get; set; }

    public void Init()
    {
        adventureSheet = new AdventureSheet();
        if(File.Exists(Application.dataPath + "/AdventureSheet.json"))
            LoadFromJson();
    }

    public void SaveToJson()
    {
        string json = JsonUtility.ToJson(adventureSheet, true);
        File.WriteAllText(Application.dataPath + "/AdventureSheet.json", json);
    }

    public void LoadFromJson()
    {
        string json = File.ReadAllText(Application.dataPath + "/AdventureSheet.json");
        adventureSheet = JsonUtility.FromJson<AdventureSheet>(json);

    }
}
