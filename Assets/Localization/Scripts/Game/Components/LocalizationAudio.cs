using UnityEngine;

namespace Localization
{
    /// <summary>
    /// Changing clip of audio source automatically
    /// </summary>
    [AddComponentMenu("Localization/L.Audio")]
    [RequireComponent(typeof(AudioSource))]
    public class LocalizationAudio : LocalizationScript
    {
        [SerializeField]
        private bool autoPlayOnUpdate = false;
        private AudioSource source;

        protected override void Initialise()
        {
            source = GetComponent<AudioSource>();
        }

        protected override void LocalizationUpdate()
        {
            if (source == null) return;
		  
            source.clip = localized;
            if (autoPlayOnUpdate)
            {
                source.Play();
            }
        }
    }
}
