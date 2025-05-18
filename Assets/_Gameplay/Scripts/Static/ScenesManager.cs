using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    #region singleton
    private static ScenesManager instance;
    public static ScenesManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new GameObject("ScenesManager").AddComponent<ScenesManager>();
                DontDestroyOnLoad(instance);
            }
            return instance;
        }
    }

    private void Awake() {
        if(instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }

    #endregion

    public void LoadNewGame()
    {
        SceneManager.LoadScene("NewGame");
    }

    public void LoadSaveGame()
    {
        SceneManager.LoadScene("Storie");
    }

    public void ReturnMenu()
    {

    }
}
