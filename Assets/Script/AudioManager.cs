using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public Slider musicSlider, sfxSlider;

    [Header("Volume Mixers")]
    public AudioMixer mixer;

    [Header("Audio Sources")]
    public AudioSource musicSource;

    [Header("Audio Settings")]
    [Range(0.001f, 1f)] public float musicVolume = 1f; // This remains unchanged
    [Range(0.001f, 1f)] public float masterVolume = 1f;
    [Range(0.001f, 1f)] public float sfxVolume = 1f;
    public float fadeDuration = 1.2f; // Duration of fade in/out
    private float startingMusicVolume;

    [Header("SFX Pool")]
    public GameObject audioSourcePrefab;
    public int poolSize = 30;
    private Queue<AudioSource> sfxPool;

    private Coroutine fadeCoroutine = null;

    [Header("Music Dictionary")]
    [SerializeField] private List<AudioClip> sfxClipList;
    private Dictionary<string,  AudioClip> sfxDictionary = new();

    private void Awake()
    {
        Instance = this;

        InitializeSFXPool();
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        foreach (AudioClip clip in sfxClipList)
        {
            sfxDictionary[clip.name] = clip;
        }
    }

    private void Start()
    {
        UpdateVolumes();
        StartCoroutine(LoopMusicWithFade());
        startingMusicVolume = musicSource.volume;
    }

    private void InitializeSFXPool()
    {
        sfxPool = new Queue<AudioSource>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject newAudioSource = Instantiate(audioSourcePrefab, transform);
            AudioSource source = newAudioSource.GetComponent<AudioSource>();
            source.outputAudioMixerGroup = mixer.FindMatchingGroups("SFX")[0];
            newAudioSource.SetActive(false);
            sfxPool.Enqueue(source);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip) return; // Avoid restarting same track

        if (musicSource.isPlaying)
        {
            // Fade out current track, then set new track and fade in
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeMusic(false, clip));
        }
        else
        {
            // Start playing immediately with fade-in
            musicSource.clip = clip;
            musicSource.Play();
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeMusic(true));
        }
    }

    private IEnumerator FadeMusic(bool fadeIn, AudioClip newClip = null)
    {
        float startVolume = fadeIn ? 0 : startingMusicVolume;
        float targetVolume = fadeIn ? startingMusicVolume : 0;
        float time = 0;

        // Fade in/out effect (only affects AudioSource volume)
        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, time / fadeDuration);
            yield return null;
        }

        if (!fadeIn && newClip != null)
        {
            // If fading out, swap the track and fade back in
            musicSource.Stop();
            musicSource.clip = newClip;
            musicSource.Play();
            fadeCoroutine = StartCoroutine(FadeMusic(true));
        }
    }

    private IEnumerator LoopMusicWithFade()
    {
        while (true)
        {
            if (musicSource.isPlaying && musicSource.clip != null)
            {
                yield return new WaitForSecondsRealtime(musicSource.clip.length - fadeDuration);
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeMusic(false, musicSource.clip)); // Fade out and restart
                yield return new WaitForSecondsRealtime(fadeDuration);
            }
            yield return null;
        }
    }

    public void PlaySFX(AudioClip clip, float pitch = -1, float volume = 1f)
    {
        if (pitch == -1)
        {
            pitch = Random.Range(0.8f, 1.2f);
        }
        AudioSource source = GetAvailableAudioSource();
        if (source != null)
        {
            source.pitch = pitch;
            source.volume = volume;
            source.clip = clip;
            source.Play();

            StartCoroutine(ReturnSourceToPoolAfterDuration(source, clip.length / Mathf.Abs(pitch)));
        }
        else
        {
            Debug.LogWarning("No available AudioSource in pool!");
        }
    }
    public void PlaySFX(string clipName, float pitch = -1, float volume = -1)
    {
        var canGet = sfxDictionary.TryGetValue(clipName, out var audio);
        if (!canGet) return;

        PlaySFX(audio, pitch, volume);
    }

    private AudioSource GetAvailableAudioSource()
    {
        if (sfxPool.Count > 0)
        {
            AudioSource source = sfxPool.Dequeue();
            source.gameObject.SetActive(true);
            return source;
        }
        return null;
    }

    private void ReturnAudioSourceToPool(AudioSource source)
    {
        source.Stop();
        source.clip = null;
        source.gameObject.SetActive(false);
        sfxPool.Enqueue(source);
    }

    private IEnumerator ReturnSourceToPoolAfterDuration(AudioSource source, float duration)
    {
        yield return new WaitForSeconds(duration);
        ReturnAudioSourceToPool(source);
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = volume;
        UpdateVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        UpdateVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        UpdateVolumes();
    }

    private void UpdateVolumes()
    {
        mixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20);
        mixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20);
        mixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20);
    }
}
