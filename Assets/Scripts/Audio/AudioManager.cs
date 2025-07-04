using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource bgmSource;
    private AudioSource sfxSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.volume = 0.4f;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.volume = 0.6f;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Toca BGM normal (sem fade)
    public void PlayBGM(AudioClip music)
    {
        if (music == null) return;

        bgmSource.clip = music;
        bgmSource.Play();
    }

    // Toca BGM com Fade Out e Fade In
    public void FadeAndPlayBGM(AudioClip newMusic, float fadeDuration = 1f)
    {
        if (newMusic == null) return;
        StartCoroutine(FadeOutAndIn(newMusic, fadeDuration));
    }

    private IEnumerator FadeOutAndIn(AudioClip newMusic, float fadeDuration)
    {
        float startVolume = bgmSource.volume;

        // Fade Out
        while (bgmSource.volume > 0)
        {
            bgmSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.clip = newMusic;
        bgmSource.Play();

        // Fade In
        while (bgmSource.volume < 0.2f)
        {
            bgmSource.volume += startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        bgmSource.volume = 0.2f;
    }

    // Toca efeitos sonoros
    public void PlaySFX(AudioClip sound)
    {
        if (sound == null) return;
        sfxSource.PlayOneShot(sound, 0.2f);
    }
}
