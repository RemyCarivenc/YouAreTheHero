using System.Collections;
using System.Collections.Generic;
using Localization;
using UnityEngine;

public abstract class LocalizationScript : MonoBehaviour
{
    [SerializeField] protected LanguageCode localized;

    public LanguageCode Localized { get { return localized; } }

    protected void OnEnable()
    {
        Initialise();
        LocalizationUpdate();
        LanguageManager.LanguageChanged += LocalizationUpdate;
    }

    protected void OnDestroy()
    {
        LanguageManager.LanguageChanged -= LocalizationUpdate;
    }

    /// <summary>
    /// Called each time the localization have to update
    /// </summary>
    protected abstract void Initialise();

    /// <summary>
    /// Called each time the localization have to update
    /// </summary>
    protected abstract void LocalizationUpdate();

    public void SetCode(int code, bool forceRefresh = false)
    {
        localized.SetCode(code);
        if(forceRefresh)
        {
            LocalizationUpdate();
        }
    }
}
