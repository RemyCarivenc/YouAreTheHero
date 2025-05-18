using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LanguageButton : MonoBehaviour
{
    [HideInInspector]
    public int languageID = 0;
    [HideInInspector]
    public LanguageChoices manager;

    public Button button;
    public Image image;

    public void ChooseThis()
    {
        manager.ValueChanged(languageID);
    }
}
