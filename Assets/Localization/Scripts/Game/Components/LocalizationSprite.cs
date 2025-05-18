using UnityEngine;
using UnityEngine.UI;

namespace Localization
{
    /// <summary>
    /// Changing sprite of an image automatically
    /// </summary>
    [AddComponentMenu("Localization/L.Sprite")]
    [RequireComponent(typeof(Image))]
    public class LocalizationSprite : LocalizationScript
    {
        private Image source;

        protected override void Initialise()
        {
            source = GetComponent<Image>();
        }

        protected override void LocalizationUpdate()
        {
            if (source)
            {
                Texture tex = localized;
				if(tex != null)
                	source.sprite = Sprite.Create(tex as Texture2D, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
				else
				{
                    Debug.LogWarning("Localized sprite [Code:"+ Localized.Code.ToString() +"] could not be loaded");
                }
			}
        }
    }
}
