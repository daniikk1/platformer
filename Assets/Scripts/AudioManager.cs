using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    private bool isAudioPaused = false;

    [Header("Mixer & Snapshots")]
    public AudioMixer audioMixer;
    public AudioMixerSnapshot normalSnapshot;
    public AudioMixerSnapshot pausedSnapshot;
    public float snapshotTransitionTime = 0.25f;

    [Header("Music Sources & Clips")]
    public AudioSource menuMusicSource;
    public AudioSource levelMusicSource;
    public AudioClip menuMusicClip;
    public AudioClip levelMusicClip;
    public float crossfadeDuration = 1.0f;

    [Header("SFX Source & Clips")]
    public AudioSource sfxSource;
    public AudioClip jumpClip;
    public AudioClip shootClip;
    public AudioClip enemyWindUpClip;
    public AudioClip enemyShootClip;
    public AudioClip enemyHitClip;
    public AudioClip enemyDeathClip;
    public AudioClip playerHurtClip;
    public AudioClip playerDeathClip;
    public AudioClip levelWinClip;
    public AudioClip uiPlayClip;
    public AudioClip uiClickClip;
    public AudioClip uiBackClip;

    [Header("Pitch Variation")]
    public float minPitch = 0.92f;
    public float maxPitch = 1.08f;

    private Coroutine crossfadeCoroutine;
    private int lastSceneIndex = -1;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (menuMusicSource != null && menuMusicClip != null)
        {
            menuMusicSource.clip = menuMusicClip;
            menuMusicSource.loop = true;
            menuMusicSource.volume = 1f;
        }

        if (levelMusicSource != null && levelMusicClip != null)
        {
            levelMusicSource.clip = levelMusicClip;
            levelMusicSource.loop = true;
            levelMusicSource.volume = 0f;
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        ApplySavedVolumes();
        HandleMusicForScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleMusicForScene(scene.buildIndex);
    }

    private void HandleMusicForScene(int sceneIndex)
    {
        // Если мы уже на уровне и переходим с уровня 1 на уровень 2 — не перезапускаем музыку уровня
        bool wasOnLevel = lastSceneIndex > 0;
        bool isOnLevel = sceneIndex > 0;
        lastSceneIndex = sceneIndex;

        if (!isOnLevel)
        {
            CrossfadeTo(menuMusicSource, levelMusicSource);
        }
        else if (!wasOnLevel)
        {
            CrossfadeTo(levelMusicSource, menuMusicSource);
        }
    }

    public void CrossfadeTo(AudioSource fadeInSource, AudioSource fadeOutSource)
    {
        if (fadeInSource == null || fadeOutSource == null) return;
        if (crossfadeCoroutine != null) StopCoroutine(crossfadeCoroutine);
        crossfadeCoroutine = StartCoroutine(CrossfadeRoutine(fadeInSource, fadeOutSource));
    }

    private IEnumerator CrossfadeRoutine(AudioSource fadeInSource, AudioSource fadeOutSource)
    {
        if (!fadeInSource.isPlaying && fadeInSource.clip != null)
        {
            fadeInSource.Play();
        }

        float startFadeIn = fadeInSource.volume;
        float startFadeOut = fadeOutSource.volume;
        float elapsed = 0f;

        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / crossfadeDuration);
            fadeInSource.volume = Mathf.Lerp(startFadeIn, 1f, t);
            fadeOutSource.volume = Mathf.Lerp(startFadeOut, 0f, t);
            yield return null;
        }

        fadeInSource.volume = 1f;
        fadeOutSource.volume = 0f;

        if (fadeOutSource.volume <= 0.001f)
        {
            fadeOutSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip, bool randomPitch = true)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.pitch = randomPitch ? Random.Range(minPitch, maxPitch) : 1f;
        sfxSource.PlayOneShot(clip);
    }

    public void SetPausedAudio(bool isPaused)
    {
        if (isPaused == isAudioPaused) return;
        isAudioPaused = isPaused;

        if (isPaused)
        {
            if (audioMixer != null)
            {
                audioMixer.ClearFloat("MusicVolume");
            }
            if (pausedSnapshot != null)
            {
                pausedSnapshot.TransitionTo(snapshotTransitionTime);
            }
        }
        else
        {
            if (normalSnapshot != null)
            {
                normalSnapshot.TransitionTo(snapshotTransitionTime);
            }
            ApplySavedVolumes();
        }
    }

    // Слайдеры передают значение от 0.0001 до 1 (или от 0 до 1)
    public void SetMusicVolume(float sliderValue)
    {
        float db = ConvertLinearToDb(sliderValue);
        if (audioMixer != null && !isAudioPaused)
        {
            audioMixer.SetFloat("MusicVolume", db);
        }

        if (SaveManager.Instance != null && SaveManager.Instance.saveData != null)
        {
            SaveManager.Instance.saveData.musicVolume = sliderValue;
            SaveManager.Instance.SaveGame();
        }
    }

    public void SetSFXVolume(float sliderValue)
    {
        float db = ConvertLinearToDb(sliderValue);
        if (audioMixer != null)
        {
            audioMixer.SetFloat("SFXVolume", db);
        }

        if (SaveManager.Instance != null && SaveManager.Instance.saveData != null)
        {
            SaveManager.Instance.saveData.sfxVolume = sliderValue;
            SaveManager.Instance.SaveGame();
        }
    }

    public void ApplySavedVolumes()
    {
        float musicVol = 0.8f;
        float sfxVol = 0.8f;

        if (SaveManager.Instance != null && SaveManager.Instance.saveData != null)
        {
            musicVol = SaveManager.Instance.saveData.musicVolume;
            sfxVol = SaveManager.Instance.saveData.sfxVolume;
        }

        if (audioMixer != null)
        {
            if (!isAudioPaused)
            {
                audioMixer.SetFloat("MusicVolume", ConvertLinearToDb(musicVol));
            }
            audioMixer.SetFloat("SFXVolume", ConvertLinearToDb(sfxVol));
        }
    }

    private float ConvertLinearToDb(float value)
    {
        float clamped = Mathf.Clamp(value, 0.0001f, 1f);
        return Mathf.Log10(clamped) * 20f;
    }
}