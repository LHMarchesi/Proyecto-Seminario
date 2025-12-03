using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManagerOcta : MonoBehaviour
{
    public static SoundManagerOcta Instance;

    [Header("Audio Mixers (Opcional)")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Sound Library")]
    [SerializeField] private List<SoundData> sounds = new List<SoundData>();

    [Header("Optional UI Sliders")]
    [SerializeField] private UnityEngine.UI.Slider musicSlider;
    [SerializeField] private UnityEngine.UI.Slider sfxSlider;

    private const string MusicPrefKey = "musicVolume";
    private const string SfxPrefKey = "sfxVolume";

    // current user-controlled multipliers (0..1)
    private float currentMusicVolume = 1f;
    private float currentSfxVolume = 1f;

    // last base volume used for the currently playing music track (from SoundData.volume)
    private float lastMusicBaseVolume = 1f;

    private Dictionary<string, SoundData> soundDictionary = new Dictionary<string, SoundData>();

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Load saved volumes and apply to internal multipliers (do not overwrite per-sound base volumes)
        float m = PlayerPrefs.GetFloat(MusicPrefKey, 1f);
        float s = PlayerPrefs.GetFloat(SfxPrefKey, 1f);

        currentMusicVolume = Mathf.Clamp01(m);
        currentSfxVolume = Mathf.Clamp01(s);

        // Apply to mixer or sources so UI reflects initial state
        if (mainMixer != null)
        {
            mainMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(currentMusicVolume, 0.0001f, 1f)) * 20);
            mainMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(currentSfxVolume, 0.0001f, 1f)) * 20);
        }
        else
        {
            // If there's already a clip assigned, capture its base volume and apply multiplier
            lastMusicBaseVolume = musicSource.clip ? musicSource.volume : 1f;
            musicSource.volume = lastMusicBaseVolume * currentMusicVolume;
            // Keep SFX source volume at 1 and control SFX loudness via PlayOneShot scale parameter
            sfxSource.volume = 1f;
        }

        // Initialize sliders if assigned
        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(m);
            musicSlider.onValueChanged.AddListener((v) => {
                // update multiplier and persist
                SetMusicVolume(v);
                PlayerPrefs.SetFloat(MusicPrefKey, Mathf.Clamp01(v));
                PlayerPrefs.Save();
            });
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(s);
            sfxSlider.onValueChanged.AddListener((v) => {
                SetSFXVolume(v);
                PlayerPrefs.SetFloat(SfxPrefKey, Mathf.Clamp01(v));
                PlayerPrefs.Save();
            });
        }

        // Inicializa el diccionario de sonidos
        foreach (var sound in sounds)
        {
            if (!soundDictionary.ContainsKey(sound.name))
                soundDictionary.Add(sound.name, sound);
        }
    }

    /// <summary>
    /// Reproduce un efecto de sonido por nombre.
    /// </summary>
    public void PlaySound(string name)
    {
        if (!soundDictionary.ContainsKey(name))
        {
            Debug.LogWarning($"⚠️ SoundManager: No se encontró el sonido '{name}'");
            return;
        }

        SoundData data = soundDictionary[name];
        // Multiply per-sound base volume by user-controlled SFX multiplier so slider affects playback
        float vol = data.volume * currentSfxVolume;
        sfxSource.PlayOneShot(data.clip, vol);
    }

    /// <summary>
    /// Reproduce música de fondo.
    /// Si ya está sonando la misma, no la reinicia.
    /// </summary>
    public void PlayMusic(string name)
    {
        if (!soundDictionary.ContainsKey(name))
        {
            Debug.LogWarning($"⚠️ SoundManager: No se encontró la música '{name}'");
            return;
        }

        SoundData data = soundDictionary[name];
        if (musicSource.clip == data.clip && musicSource.isPlaying)
            return; // already playing

        musicSource.clip = data.clip;
        // remember base volume for this track and apply user multiplier
        lastMusicBaseVolume = data.volume;
        musicSource.volume = lastMusicBaseVolume * currentMusicVolume;
        musicSource.loop = true;
        musicSource.Play();
    }

    /// <summary>
    /// Detiene la música actual.
    /// </summary>
    public void StopMusic()
    {
        musicSource.Stop();
    }

    /// <summary>
    /// Cambia el volumen general (0.0 - 1.0)
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        if (mainMixer != null)
            mainMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Clamp(volume, 0.001f, 1f)) * 20);
        else
        {
            musicSource.volume = volume;
            sfxSource.volume = volume;
        }
    }

    /// <summary>
    /// Cambia el volumen de los efectos de sonido (solo SFX)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        currentSfxVolume = Mathf.Clamp01(volume);

        if (mainMixer != null)
        {
            mainMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(currentSfxVolume, 0.0001f, 1f)) * 20);
        }
        else
        {
            // update source volume baseline; future PlayOneShot calls are multiplied by this value
            sfxSource.volume = currentSfxVolume;
        }
    }

    /// <summary>
    /// Cambia el volumen de la música (solo música)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        currentMusicVolume = Mathf.Clamp01(volume);

        if (mainMixer != null)
        {
            mainMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Clamp(currentMusicVolume, 0.0001f, 1f)) * 20);
        }
        else
        {
            // apply multiplier to currently playing track base volume
            musicSource.volume = lastMusicBaseVolume * currentMusicVolume;
        }
    }
}

[Serializable]
public class SoundData
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
}
