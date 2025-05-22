using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TypeCharacter : MonoBehaviour
{
    [SerializeField]
    private float typingSpeed = 0.05f;

    private LocalizedString localizedString;
    private TMP_Text textTMP;
    private string currentText = "";

    private Coroutine typingCoroutine;

    void OnEnable()
    {
        Actions.StartType += StartTypeWriter;
        Actions.SkipType += SkipTyping;

        textTMP = GetComponent<TMP_Text>();
    }

    void OnDisable()
    {
        Actions.StartType -= StartTypeWriter;
        Actions.SkipType -= SkipTyping;
        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }

    void Start()
    {
        StartCoroutine(SubscribeToLanguageChanges());
    }

    #region Event Locale Changed
    private IEnumerator SubscribeToLanguageChanges()
    {
        // Attendre que le système de localisation soit initialisé
        yield return LocalizationSettings.InitializationOperation;

        // S'abonner aux changements de langue
        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
    }

    private void OnLocaleChanged(Locale newLocale)
    {
        StartCoroutine(UpdateTextForNewLanguageCoroutine());
    }

    private IEnumerator UpdateTextForNewLanguageCoroutine()
    {
        if (localizedString != null || !localizedString.IsEmpty)
        {
            var operation = localizedString.GetLocalizedStringAsync();
            yield return operation;

            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                currentText = operation.Result;

                // Afficher immédiatement le nouveau texte complet
                textTMP.text = currentText;
            }
        }
    }
    #endregion



    private void StartTypeWriter(LocalizedString _newSentence)
    {
        StopTypewriter();

        localizedString = _newSentence;

        typingCoroutine = StartCoroutine(StartTypewriterCoroutine());
    }

    private void SkipTyping()
    {
        StopTypewriter();
        Actions.EndType?.Invoke();
        textTMP.text = currentText;
    }

    private void StopTypewriter()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    private IEnumerator StartTypewriterCoroutine()
    {
        // Attendre que le système de localisation soit initialisé
        yield return LocalizationSettings.InitializationOperation;

        if (localizedString != null || !localizedString.IsEmpty)
        {
            // Obtenir le texte localisé
            var operation = localizedString.GetLocalizedStringAsync();
            yield return operation;

            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                currentText = operation.Result;

                if (!string.IsNullOrEmpty(currentText))
                {
                    textTMP.text = "";

                    foreach (char letter in currentText.ToCharArray())
                    {
                        textTMP.text += letter;
                        yield return new WaitForSeconds(typingSpeed);
                    }
                    Actions.EndType?.Invoke();
                }
                else
                    Debug.LogWarning("Le texte localisé est vide !");
            }
            else
            {
                Debug.LogError("Échec de récupération du texte localisé !");
            }
        }
    }
}
