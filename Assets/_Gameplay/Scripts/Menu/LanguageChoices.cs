using System;
using System.Collections;
using System.Collections.Generic;
using Localization;
using UnityEngine;
using UnityEngine.UI;

public class LanguageChoices : MonoBehaviour
{
    public GameObject itemPrefab;
    public GameObject panel;
    public Image currentImage;

    private List<string> languages = new List<string>();
    private Sprite currentFLag;

    private List<GameObject> buttonLanguages = new List<GameObject>();

    void Start()
    {

        // Get languages names and values
        string[] languagesNames = LanguageManager.GetLanguagesName();
        for (int i = 0; i < languagesNames.Length; i++)
            languages.Add(languagesNames[i]);

        currentFLag = LanguageManager.GetFlag(LanguageManager.CurrentLanguage);
        currentImage.sprite = currentFLag;
    }

    public void GenerateButtonLanguage()
    {
        LanguageButton languageButton;
        int idCurrent = -1;
        for (int i = 0; i < languages.Count; i++)
        {
            if (languages[i] != LanguageManager.CurrentLanguage)
            {
                GameObject go = Instantiate(itemPrefab, panel.transform);
                languageButton = go.GetComponent<LanguageButton>();
                languageButton.image.sprite = LanguageManager.GetFlag(languages[i]);
                languageButton.manager = this;
                languageButton.languageID = i;
            }
            else
                idCurrent = i;
        }

        GameObject goFirstLanguage = Instantiate(itemPrefab, panel.transform);
        languageButton = goFirstLanguage.GetComponent<LanguageButton>();
        languageButton.image.sprite = LanguageManager.GetFlag(LanguageManager.CurrentLanguage);
        languageButton.manager = this;
        languageButton.languageID = idCurrent;
    }

    public void ValueChanged(int index)
    {
        LanguageManager.CurrentLanguage = languages[index];
        currentImage.sprite = LanguageManager.GetFlag(LanguageManager.CurrentLanguage);
        for (int i = panel.transform.childCount; i > 0; i--)
        {
            Destroy(panel.transform.GetChild(i-1).gameObject);
        }
    }
}
