using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance;

    // REMOVE 'static' from these so they belong to the Instance
    private AudioSource audioSource;
    private AudioSource randomPitchAudioSource;
    private AudioSource voiceAudioSource;
    private SoundEffectLibrary soundEffectLibrary;

    [SerializeField] private Slider sfxSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
           

            AudioSource[] audioSources = GetComponents<AudioSource>();

            if (audioSources.Length >= 3)
            {
                audioSource = audioSources[0];
                randomPitchAudioSource = audioSources[1];
                voiceAudioSource = audioSources[2];
            }
            soundEffectLibrary = GetComponent<SoundEffectLibrary>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Change methods to check Instance
    public static void Play(string soundName, bool randomPitch = false)
    {
        if (Instance == null || Instance.soundEffectLibrary == null) return;

        AudioClip audioClip = Instance.soundEffectLibrary.GetRandomClip(soundName);
        if (audioClip != null)
        {
            if (randomPitch)
            {
                Instance.randomPitchAudioSource.pitch = Random.Range(1f, 1.2f); // Lowered max pitch for safety
                Instance.randomPitchAudioSource.PlayOneShot(audioClip);
            }
            else
            {
                Instance.audioSource.PlayOneShot(audioClip);
            }
        }
    }

    public static void PlayVoice(AudioClip audioClip, float pitch = 1f)
    {
        if (Instance == null || Instance.voiceAudioSource == null || audioClip == null) return;

        Instance.voiceAudioSource.pitch = pitch;
        Instance.voiceAudioSource.PlayOneShot(audioClip);
    }

void Start()
    {
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(delegate { OnValueChanged(); });
        }
    }

    public static void SetVolume(float volume)
    {
        if (Instance == null) return;

        // Use Instance. to access the non-static AudioSources
        if (Instance.audioSource != null) Instance.audioSource.volume = volume;
        if (Instance.randomPitchAudioSource != null) Instance.randomPitchAudioSource.volume = volume;
        if (Instance.voiceAudioSource != null) Instance.voiceAudioSource.volume = volume;
    }

    public void OnValueChanged()
    {
        // Now this works because SetVolume is a static helper
        SetVolume(sfxSlider.value);
    }
}