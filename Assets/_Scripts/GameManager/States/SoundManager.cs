using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    [Header("Audio Sources")] 
    [SerializeField] private AudioSource  musicSource;
    [SerializeField] private AudioSource sfxSource;
    [Header("UI (optional)")] 
    [SerializeField] private Slider volumeSlider; 
 
    public AudioClip mainMenuMusic; 
    public AudioClip levelMusic;
    private void Awake() 
    { 
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject); return; 
        } 
        Instance = this;
        DontDestroyOnLoad(gameObject);
    } 
    private void Start() 
    { 
        float saved = PlayerPrefs.GetFloat("musicVolume", 1f); 
        musicSource.volume = saved; 
        
        if (volumeSlider != null) 
        { 
            volumeSlider.SetValueWithoutNotify(saved); 
            volumeSlider.onValueChanged.AddListener(OnSliderChanged);
        } 

    } 
    private void OnDestroy()
    { 
        if (volumeSlider != null) 
            volumeSlider.onValueChanged.RemoveListener(OnSliderChanged); 

    } 
    public void PlayMusic(AudioClip clip, bool loop = true) 
    { 
        if (musicSource.clip == clip && musicSource.isPlaying)
            return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }
    public void StopMusic()
    { 
        musicSource.Stop(); 
    } 
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume); 
    } 
    public void PlaySFX(AudioClip clip) 
    {
        if (sfxSource.clip == clip && sfxSource.isPlaying) 
            return;
        sfxSource.PlayOneShot(clip);
    } 
    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume); 
    } 
    public bool IsPlaying(AudioClip clip)
    { 
        return 
            
            musicSource.clip == clip && musicSource.isPlaying;
    } 
    public void OnSliderChanged(float value)
    { 
        SetMusicVolume(value); 
        PlayerPrefs.SetFloat("musicVolume", value);
        PlayerPrefs.Save(); 
    } 
    public void ChangeVolume() 
    { 
        if (volumeSlider == null) 
            return;
        SetMusicVolume(volumeSlider.value); 
        Save();
    } 
    private void Load() 
    { float v = PlayerPrefs.GetFloat("musicVolume", 1f);
        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(v); 
        musicSource.volume = v;
    } 
    private void Save()
    { 
        if (volumeSlider == null)
            return;
        PlayerPrefs.SetFloat("musicVolume", volumeSlider.value); 
        PlayerPrefs.Save();
    }
}
