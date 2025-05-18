using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Localization
{
    /// <summary>
    /// Update the value of a Text component with the current language
    /// </summary>
    [RequireComponent(typeof(Text))]
    [AddComponentMenu("Localization/L.Text")]
    public class LocalizationText : LocalizationScript
    {
        [Tooltip("Should only be used to add punctuation.")]
        public string AddBefore = "";
        [Tooltip("Should only be used to add punctuation.")]
        public string AddAfter = "";
        private Text textComponent;

        protected override void Initialise()
        {
            if (textComponent == null) textComponent = GetComponent<Text>();
        }

        protected override void LocalizationUpdate()
        {
            if (textComponent != null) textComponent.text = AddBefore + localized + AddAfter;
        }

    }
}

