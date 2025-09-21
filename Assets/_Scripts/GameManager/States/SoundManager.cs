using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    public AudioClip mainMenuMusic;
    public AudioClip levelMusic;
    private float pausedTime = 0f;

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


    // ----- M�sica -----
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }
    public void PauseMusic()
    {
        if (musicSource.isPlaying)
        {
            pausedTime = musicSource.time; // guarda d�nde iba
            musicSource.Stop();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource.clip != null && pausedTime > 0f)
        {
            musicSource.time = pausedTime; // vuelve al punto guardado
            musicSource.Play();
            pausedTime = 0f; // reset para evitar reanudar dos veces
        }
    }
    // ----- SFX -----
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource.clip == clip && sfxSource.isPlaying)
            return;

        sfxSource.PlayOneShot(clip);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }

    public bool IsPlaying(AudioClip clip)
    {
        return musicSource.clip == clip && musicSource.isPlaying;
    }
}