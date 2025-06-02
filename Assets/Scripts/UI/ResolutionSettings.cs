using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ResolutionSettings : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    private List<Resolution> filteredResolutions = new();
    private int currentResolutionIndex = 0;

    void Start()
    {
        PopulateResolutions();
        LoadPreferences();

        // Listen for toggle changes in real-time
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggle);
    }

    void PopulateResolutions()
    {
        Resolution[] allResolutions = Screen.resolutions;
        HashSet<string> seen = new();
        List<string> options = new();

        for (int i = 0; i < allResolutions.Length; i++)
        {
            string resStr = $"{allResolutions[i].width} x {allResolutions[i].height}";
            if (!seen.Contains(resStr))
            {
                seen.Add(resStr);
                filteredResolutions.Add(allResolutions[i]);
                options.Add(resStr);

                if (allResolutions[i].width == Screen.currentResolution.width &&
                    allResolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = filteredResolutions.Count - 1;
                }
            }
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    void LoadPreferences()
    {
        int savedIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        if (savedIndex < 0 || savedIndex >= filteredResolutions.Count)
            savedIndex = currentResolutionIndex;

        resolutionDropdown.value = savedIndex;
        fullscreenToggle.isOn = isFullscreen;

        ApplyResolution(savedIndex, isFullscreen);
    }

    public void OnResolutionChange(int index)
    {
        if (index < 0 || index >= filteredResolutions.Count) return;

        ApplyResolution(index, fullscreenToggle.isOn);
        PlayerPrefs.SetInt("ResolutionIndex", index);
    }

    public void OnFullscreenToggle(bool isFullscreen)
    {
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        Screen.fullScreen = isFullscreen;

        int index = resolutionDropdown.value;
        if (index >= 0 && index < filteredResolutions.Count)
        {
            Resolution res = filteredResolutions[index];
            Screen.SetResolution(res.width, res.height, isFullscreen);
        }
    }

    void ApplyResolution(int index, bool fullscreen)
    {
        if (index >= 0 && index < filteredResolutions.Count)
        {
            Resolution res = filteredResolutions[index];
            Screen.SetResolution(res.width, res.height, fullscreen);
            Screen.fullScreen = fullscreen; // force toggle enforcement
        }
    }
}
