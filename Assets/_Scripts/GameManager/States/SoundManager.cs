using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    // This component is not used in the project. The implementation has been
    // commented out to avoid accidental usage. Keep the file for compatibility.

    /*
    // Minimal compatibility layer used by UI sliders in the Main Menu.
    // Forwards volume changes to SoundManagerOcta and persists them with PlayerPrefs.

    public static SoundManager Instance { get; private set; }

    [Header("Optional UI Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private const string MusicPrefKey = "musicVolume";
    private const string SfxPrefKey = "sfxVolume";

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

    private void OnEnable()
    {
        // Load saved values and hook sliders if assigned
        float m = PlayerPrefs.GetFloat(MusicPrefKey, 1f);
        float s = PlayerPrefs.GetFloat(SfxPrefKey, 1f);

        // Apply to runtime SoundManagerOcta if available
        if (SoundManagerOcta.Instance != null)
        {
            SoundManagerOcta.Instance.SetMusicVolume(m);
            SoundManagerOcta.Instance.SetSFXVolume(s);
        }

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(m);
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(s);
            sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }
    }

    private void OnDisable()
    {
        if (musicSlider != null)
            musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);

        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
    }

    // Public API expected by UI: call these from Slider.OnValueChanged
    public void OnMusicSliderChanged(float value)
    {
        SetMusicVolume(value);
    }

    public void OnSFXSliderChanged(float value)
    {
        SetSFXVolume(value);
    }

    // Keep method names that other scripts or inspector may reference
    public void SetMusicVolume(float volume)
    {
        float v = Mathf.Clamp01(volume);
        if (SoundManagerOcta.Instance != null)
            SoundManagerOcta.Instance.SetMusicVolume(v);

        PlayerPrefs.SetFloat(MusicPrefKey, v);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        float v = Mathf.Clamp01(volume);
        if (SoundManagerOcta.Instance != null)
            SoundManagerOcta.Instance.SetSFXVolume(v);

        PlayerPrefs.SetFloat(SfxPrefKey, v);
        PlayerPrefs.Save();
    }
    */

    // Intentionally left empty.
}
