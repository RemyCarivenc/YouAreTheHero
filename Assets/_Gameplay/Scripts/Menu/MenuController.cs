using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public Button loadSavedGame;
    private void Start()
    {
        GameManager.Instance.Init();

        if (System.IO.File.Exists(Application.dataPath + "/AdventureSheet.json"))
            loadSavedGame.interactable = false;
    }

    public void NewGame()
    {
        ScenesManager.Instance.LoadNewGame();
    }

    public void Load()
    {
        ScenesManager.Instance.LoadSaveGame();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
