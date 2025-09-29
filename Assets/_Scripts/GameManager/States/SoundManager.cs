using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("UI (optional)")]
    [SerializeField] private Slider volumeSlider;      // Music slider (0..1)
    [SerializeField] private Slider sfxVolumeSlider;   // SFX slider (0..1)

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mixer;         // Assign your mixer here
    [SerializeField] private string musicParam = "MusicVol"; // Exposed param name on Music group
    [SerializeField] private string sfxParam = "SFXVol";   // Exposed param name on SFX group

    [Header("Clips (optional)")]
    public AudioClip mainMenuMusic;
    public AudioClip levelMusic;

    private const string MUSIC_VOL_KEY = "musicVolume";
    private const string SFX_VOL_KEY = "sfxVolume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Load saved linear volumes (0..1), default to 1
        float savedMusic = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 1f);
        float savedSfx = PlayerPrefs.GetFloat(SFX_VOL_KEY, 1f);

        // Apply to mixer if available; fall back to sources if not
        ApplyMusicVolume(savedMusic);
        ApplySfxVolume(savedSfx);

        // Init sliders without triggering callbacks, then subscribe
        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(savedMusic);
            volumeSlider.onValueChanged.AddListener(SetMusicVolumeLinear);
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(savedSfx);
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolumeLinear);
        }
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(SetMusicVolumeLinear);
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(SetSFXVolumeLinear);
    }

    // ===== Public control (callable from UI) =====
    public void SetMusicVolumeLinear(float v) // v in [0..1]
    {
        v = Mathf.Clamp01(v);
        ApplyMusicVolume(v);
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, v);
        PlayerPrefs.Save();
    }

    public void SetSFXVolumeLinear(float v) // v in [0..1]
    {
        v = Mathf.Clamp01(v);
        ApplySfxVolume(v);
        PlayerPrefs.SetFloat(SFX_VOL_KEY, v);
        PlayerPrefs.Save();
    }

    // Keeps your previous APIs available (optional)
    public void ChangeVolume()
    {
        if (volumeSlider == null) return;
        SetMusicVolumeLinear(volumeSlider.value);
    }
    public void ChangeSFXVolume()
    {
        if (sfxVolumeSlider == null) return;
        SetSFXVolumeLinear(sfxVolumeSlider.value);
    }

    // ===== Internal apply helpers =====
    private void ApplyMusicVolume(float linear)
    {
        if (mixer != null && !string.IsNullOrEmpty(musicParam))
        {
            mixer.SetFloat(musicParam, LinearToDb(linear));
        }
        else
        {
            // Fallback if no mixer assigned
            if (musicSource != null) musicSource.volume = linear;
        }
    }

    private void ApplySfxVolume(float linear)
    {
        if (mixer != null && !string.IsNullOrEmpty(sfxParam))
        {
            mixer.SetFloat(sfxParam, LinearToDb(linear));
        }
        else
        {
            // Fallback if no mixer assigned
            if (sfxSource != null) sfxSource.volume = linear;
        }
    }

    private float LinearToDb(float v)
    {
        // Unity convention: around -80 dB is effectively mute
        return (v <= 0.0001f) ? -80f : 20f * Mathf.Log10(v);
    }

    // ===== Music / SFX playback =====
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null || clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public bool IsPlaying(AudioClip clip)
    {
        return musicSource != null && musicSource.clip == clip && musicSource.isPlaying;
    }
}
