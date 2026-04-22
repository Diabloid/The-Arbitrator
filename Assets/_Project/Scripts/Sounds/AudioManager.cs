using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Музика")]
    [SerializeField] private AudioSource musicSource;

    [Header("SFX")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    private AudioClip _previousMusic;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // 1. Метод для музики
    public void PlayMusic(AudioClip musicClip, bool saveCurrentAsPrevious = false)
    {
        if (musicClip == null) return;
        if (musicSource.clip == musicClip) return;

        if (saveCurrentAsPrevious)
        {
            _previousMusic = musicSource.clip;
        }

        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    // 2. Метод для звуків без зміни пітчу
    public void PlaySFX(AudioClip sfxClip, float volume = 1f)
    {
        if (sfxClip == null) return;
        SpawnAudioObject(sfxClip, volume, 1f);
    }

    // 3. Метод для звуків з випадковим пітчем
    public void PlaySFXRandomPitch(AudioClip sfxClip, float volume = 1f, float minPitch = 0.85f, float maxPitch = 1.15f)
    {
        if (sfxClip == null) return;
        float randomPitch = Random.Range(minPitch, maxPitch);
        SpawnAudioObject(sfxClip, volume, randomPitch);
    }

    // Повертає попередню музику
    public void PlayPreviousMusic()
    {
        if (_previousMusic != null)
        {
            PlayMusic(_previousMusic, false);
        }
    }

    // Метод який створює тимчасовий об'єкт для звуку і сам його видаляє
    private void SpawnAudioObject(AudioClip clip, float volume, float pitch)
    {
        GameObject audioObj = new GameObject("TempAudio_" + clip.name);
        audioObj.transform.SetParent(transform);

        AudioSource source = audioObj.AddComponent<AudioSource>();

        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;

        if (sfxMixerGroup != null)
        {
            source.outputAudioMixerGroup = sfxMixerGroup;
        }

        source.Play();

        Destroy(audioObj, clip.length / pitch);
    }
}