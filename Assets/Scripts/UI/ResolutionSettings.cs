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

        resolutionDropdown.value = savedIndex;
        fullscreenToggle.isOn = isFullscreen;

        ApplyResolution(savedIndex, isFullscreen);
    }

    public void OnResolutionChange(int index)
    {
        ApplyResolution(index, fullscreenToggle.isOn);
        PlayerPrefs.SetInt("ResolutionIndex", index);
    }

    public void OnFullscreenToggle(bool isFullscreen)
    {
        ApplyResolution(resolutionDropdown.value, isFullscreen);
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    void ApplyResolution(int index, bool fullscreen)
    {
        Resolution res = filteredResolutions[index];
        Screen.SetResolution(res.width, res.height, fullscreen);
    }
}
