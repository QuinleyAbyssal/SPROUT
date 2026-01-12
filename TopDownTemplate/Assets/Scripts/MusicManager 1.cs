using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // REQUIRED for scene detection

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    private AudioSource audioSource;

    [Header("Track Library")]
    public List<AudioClip> musicTracks;

    [Header("Settings")]
    [SerializeField] private Slider musicSlider;
    public float fadeTime = 1.5f;
    private float targetVolume = 1.0f; // Track the slider value separately

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            audioSource.loop = true; // Ensure music loops!
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // --- AUTOMATIC SCENE MUSIC ---
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Check if we are loading a save (handled by SaveController)
        // If the SaveController is about to ApplySaveData, we might want to skip this
        if (StartMenuController.ShouldLoadGame) return;

        // 2. Assign tracks to specific scenes
        switch (scene.name)
        {
            case "Menu":
                PlayTrackByName("MenuTheme");
                break;
            case "SampleScene":
                PlayTrackByName("ForestTheme");
                break;
            case "Cutscene":
                PlayTrackByName("MenuTheme");
                break;
            default:
                Debug.Log("No specific music assigned to: " + scene.name);
                break;
        }
    }

    void Start()
    {
        if (musicSlider != null)
        {
            targetVolume = musicSlider.value;
            audioSource.volume = targetVolume;
            musicSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void SetVolume(float volume)
    {
        targetVolume = volume;
        // If we aren't currently fading, apply immediately
        audioSource.volume = volume;
    }

    public string GetCurrentTrackName()
    {
        return audioSource.clip != null ? audioSource.clip.name : "";
    }

    public void PlayTrackByName(string trackName)
    {
        if (string.IsNullOrEmpty(trackName)) return;
        if (audioSource.clip != null && audioSource.clip.name == trackName && audioSource.isPlaying) return;

        AudioClip newClip = musicTracks.Find(t => t.name == trackName);

        if (newClip != null)
        {
            StopAllCoroutines(); // Stop any existing fades before starting a new one
            StartCoroutine(FadeToTrack(newClip));
        }
    }

    private IEnumerator FadeToTrack(AudioClip newClip)
    {
        // Fade Out
        while (audioSource.volume > 0)
        {
            audioSource.volume -= Time.deltaTime / fadeTime;
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();

        // Fade In to the CURRENT targetVolume (respects the slider)
        while (audioSource.volume < targetVolume)
        {
            audioSource.volume += Time.deltaTime / fadeTime;
            yield return null;
        }

        audioSource.volume = targetVolume;
    }
}