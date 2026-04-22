using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VideoSettingsController : MonoBehaviour
{
    [Header("UI Елементи")]
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private Toggle _fullscreenToggle;
    [SerializeField] private Toggle _vSyncToggle;
    [SerializeField] private TMP_Dropdown _fpsDropdown;

    private List<Resolution> _filteredResolutions;
    private int[] _fpsLimits = { 30, 60, 120, 144, -1 };

    private void Start()
    {
        SetupResolutions();
        LoadSettings();
    }


    private void SetupResolutions()
    {
        Resolution[] allResolutions = Screen.resolutions;
        _filteredResolutions = new List<Resolution>();
        List<string> options = new List<string>();

        Dictionary<string, Resolution> uniqueResolutions = new Dictionary<string, Resolution>();

        foreach (Resolution res in allResolutions)
        {
            string key = $"{res.width} x {res.height}";
            uniqueResolutions[key] = res;
        }

        int currentResIndex = 0;
        int index = 0;

        foreach (var kvp in uniqueResolutions)
        {
            _filteredResolutions.Add(kvp.Value);
            options.Add(kvp.Key);

            if (kvp.Value.width == Screen.currentResolution.width &&
                kvp.Value.height == Screen.currentResolution.height)
            {
                currentResIndex = index;
            }
            index++;
        }

        _resolutionDropdown.ClearOptions();
        _resolutionDropdown.AddOptions(options);
        _resolutionDropdown.value = currentResIndex;
        _resolutionDropdown.RefreshShownValue();
    }

    private void LoadSettings()
    {
        // 1. Повноекранний режим (1 - увімкнено, 0 - вимкнено)
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        _fullscreenToggle.isOn = isFullscreen;
        Screen.fullScreen = isFullscreen;

        // 2. V-Sync (1 - увімкнено, 0 - вимкнено)
        bool isVSync = PlayerPrefs.GetInt("VSync", 1) == 1;
        _vSyncToggle.isOn = isVSync;
        QualitySettings.vSyncCount = isVSync ? 1 : 0;

        // 3. Ліміт кадрів (FPS)
        int savedFpsIndex = PlayerPrefs.GetInt("FpsLimitIndex", 1);
        _fpsDropdown.value = savedFpsIndex;
        ApplyFPSLimit(savedFpsIndex);
    }

    // Методи для кнопок та перемикачів
    public void SetResolution(int resolutionIndex)
    {
        Resolution res = _filteredResolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        Debug.Log($"Роздільна здатність змінена на: {res.width}x{res.height}");
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetVSync(bool isVSync)
    {
        QualitySettings.vSyncCount = isVSync ? 1 : 0;
        PlayerPrefs.SetInt("VSync", isVSync ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetFPSLimit(int fpsIndex)
    {
        ApplyFPSLimit(fpsIndex);
        PlayerPrefs.SetInt("FpsLimitIndex", fpsIndex);
        PlayerPrefs.Save();
    }

    private void ApplyFPSLimit(int index)
    {
        if (index < 0 || index >= _fpsLimits.Length) index = 1;
        Application.targetFrameRate = _fpsLimits[index];
    }
}