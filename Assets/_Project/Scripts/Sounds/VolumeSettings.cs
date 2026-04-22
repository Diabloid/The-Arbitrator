using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [Header("Налаштування")]
    [SerializeField] private AudioMixer _myMixer;
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    private void Start()
    {
        _masterSlider.value = PlayerPrefs.GetFloat("MasterVol", 1f);
        _musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 1f);
        _sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 1f);

        SetMasterVolume();
        SetMusicVolume();
        SetSFXVolume();
    }
    public void SetMasterVolume()
    {
        float volume = _masterSlider.value;
        _myMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVol", volume);
    }

    public void SetMusicVolume()
    {
        float volume = _musicSlider.value;
        _myMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVol", volume);
    }

    public void SetSFXVolume()
    {
        float volume = _sfxSlider.value;
        _myMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVol", volume);
    }
}