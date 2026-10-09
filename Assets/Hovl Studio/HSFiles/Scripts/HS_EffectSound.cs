using System.Collections;
using UnityEngine;

namespace Hovl
{
    public class HS_EffectSound : MonoBehaviour
    {
        public bool Repeating = true;
        public float RepeatTime = 2.0f;
        public float StartTime = 0.0f;

        public bool RandomVolume;
        public float minVolume = .4f;
        public float maxVolume = 1f;

        [Header("Fade Out")]
        public bool FadeOut = false;
        public float FadeOutStartTime = 1.0f;
        public float FadeOutDuration = 1.0f;

        public AudioClip clip;
        public AudioSource soundComponent;

        private Coroutine fadeCoroutine;

        void Start()
        {
            if (soundComponent == null)
                soundComponent = GetComponent<AudioSource>();

            if (clip == null)
                clip = soundComponent.clip;

            if (Repeating)
                InvokeRepeating(nameof(RepeatSound), StartTime, RepeatTime);
            else
                RepeatSound();
        }

        void RepeatSound()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            float volume = RandomVolume
                ? Random.Range(minVolume, maxVolume)
                : 1f;

            soundComponent.clip = clip;
            soundComponent.volume = volume;

            // Loop the clip if it is shorter than the full fade lifetime.
            if (FadeOut && clip != null)
            {
                float totalDuration = FadeOutStartTime + FadeOutDuration;
                soundComponent.loop = clip.length < totalDuration;
            }
            else
            {
                soundComponent.loop = false;
            }

            soundComponent.Play();

            if (FadeOut)
                fadeCoroutine = StartCoroutine(FadeSound(volume));
        }

        IEnumerator FadeSound(float startVolume)
        {
            yield return new WaitForSeconds(FadeOutStartTime);

            float time = 0f;

            while (time < FadeOutDuration)
            {
                time += Time.deltaTime;

                soundComponent.volume = Mathf.Lerp(
                    startVolume,
                    0f,
                    Mathf.Clamp01(time / FadeOutDuration)
                );

                yield return null;
            }

            soundComponent.volume = 0f;
            soundComponent.Stop();
            soundComponent.loop = false;

            fadeCoroutine = null;
        }
    }
}