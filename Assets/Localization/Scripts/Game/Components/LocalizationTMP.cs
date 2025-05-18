using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if TMPPRO_PRESENT
using TMPro;
#endif

namespace Localization
{
    /// <summary>
    /// Update the value of a Text component with the current language
    /// </summary>
#if TMPPRO_PRESENT
    [RequireComponent(typeof(TMP_Text))]
    [AddComponentMenu("Localization/L.TextMeshPro")]
#endif
    public class LocalizationTMP : LocalizationScript
    {
        [Tooltip("Should only be used to add punctuation.")]
        public string AddBefore = "";
        [Tooltip("Should only be used to add punctuation.")]
        public string AddAfter = "";
#if TMPPRO_PRESENT
        private TMP_Text textComponent;
#endif

        protected override void Initialise()
        {
#if TMPPRO_PRESENT
            if (textComponent == null) textComponent = GetComponent<TMP_Text>();
#else
            Debug.LogError("TextMesh Pro is not present yet a LocalizationTMP is used on object : " + name);
#endif
        }

        protected override void LocalizationUpdate()
        {
#if TMPPRO_PRESENT
            textComponent.text = AddBefore + localized + AddAfter;
#endif
        }
    }
}

