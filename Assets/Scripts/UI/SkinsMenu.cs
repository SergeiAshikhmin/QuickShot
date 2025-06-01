using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinsMenu : MonoBehaviour
{
    [System.Serializable]
    public class SkinButton
    {
        public string skinID;   // e.g. "Pink", "Blue", "White"
        public Button button;   // Assign in Inspector
    }

    [Header("UI Elements")]
    public List<SkinButton> skinButtons;                             // e.g. { ("Pink", pinkButton), ("Blue", blueButton), ("White", whiteButton) }
    public List<AnimatorOverrideController> skinOverrideControllers; // Drag your OverrideControllers here (named "Player_Override_Pink", "Player_Override_Blue", "Player_Override_White")
    public string playerTag = "Player";                              // Make sure your Player GameObject is tagged "Player"

    private string lastEquippedSkin = "";

    void Start()
    {
        // 1) Show all three buttons and wire up their onClick listeners:
        foreach (var sb in skinButtons)
        {
            sb.button.gameObject.SetActive(true);
            sb.button.onClick.RemoveAllListeners();
            string id = sb.skinID; 
            sb.button.onClick.AddListener(() => SelectSkin(id));
        }

        // 2) On start, read saved skin (default to "Pink" if none) and apply:
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return;

        string savedSkin = PlayerPrefs.GetString("SelectedSkin", "Pink");
        ApplySkinToPlayer(savedSkin, player);
    }

    /// <summary>
    /// Called by UI buttons. Saves the skin choice and immediately applies it.
    /// </summary>
    public void SelectSkin(string skinID)
    {
        if (string.IsNullOrEmpty(skinID)) return;

        PlayerPrefs.SetString("SelectedSkin", skinID);
        PlayerPrefs.Save();

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
            ApplySkinToPlayer(skinID, player);
    }

    /// <summary>
    /// Builds the expected override-controller name ("Player_Override_<skinID>"),
    /// finds it in the list, and assigns it to the player's Animator.
    /// </summary>
    private void ApplySkinToPlayer(string skinID, GameObject player)
    {
        // Construct the full asset name:
        string overrideName = $"Player_Override_{skinID}";

        // Find the matching AnimatorOverrideController by exact name:
        var overrideController = skinOverrideControllers
            .Find(x => x != null && x.name == overrideName);

        if (overrideController == null)
        {
            Debug.LogWarning($"[SkinsMenu] No AnimatorOverrideController found named '{overrideName}'.");
            return;
        }

        var anim = player.GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogWarning("[SkinsMenu] Player has no Animator component.");
            return;
        }

        anim.runtimeAnimatorController = overrideController;
        lastEquippedSkin = skinID;
        Debug.Log($"[SkinsMenu] Applied skin: {skinID}");
    }
}
