using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponsMenu : MonoBehaviour
{
    [System.Serializable]
    public class WeaponButton
    {
        public string weaponID;   // e.g., "Bow", "Pistol"
        public Button button;     // Drag in Inspector
    }

    public List<WeaponButton> weaponButtons;   // Fill in Inspector
    public List<GameObject> weaponPrefabs;     // Drag weapon prefabs here (names match weaponID)
    public string playerTag = "Player";        // Tag your player prefab with "Player"
    public string weaponLayerName = "Weapon";  // Your layer for weapon objects

    private string lastEquippedWeapon = "";

    void OnEnable()
    {
        UpdateWeaponButtons();
        // Only equip if a weapon is chosen and not present
        EquipIfNeeded();
    }

    void Update()
    {
        // Only equip if a weapon is chosen and not present
        EquipIfNeeded();
    }

    void UpdateWeaponButtons()
    {
        foreach (var wb in weaponButtons)
        {
            bool unlocked = wb.weaponID == "Bow" || PlayerPrefs.GetInt("WeaponUnlocked_" + wb.weaponID, 0) == 1;
            wb.button.gameObject.SetActive(unlocked);
        }
    }

    // Equip if a weapon is selected in PlayerPrefs, and not present in scene
    void EquipIfNeeded()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
            return;

        int weaponLayer = LayerMask.NameToLayer(weaponLayerName);

        bool weaponPresent = false;
        foreach (Transform child in player.transform)
        {
            if (child.gameObject.layer == weaponLayer)
            {
                weaponPresent = true;
                break;
            }
        }

        // If weapon already present, do nothing
        if (weaponPresent)
            return;

        // Get the weapon from PlayerPrefs
        string equippedWeapon = PlayerPrefs.GetString("EquippedWeapon", "");

        // Only equip if there's a valid weapon ID stored
        if (!string.IsNullOrEmpty(equippedWeapon))
        {
            EquipWeapon(equippedWeapon);
        }
    }

    // Called by UI buttons, or by script to equip default/PlayerPrefs weapon
    public void EquipWeapon(string weaponID)
    {
        if (string.IsNullOrEmpty(weaponID)) return;

        PlayerPrefs.SetString("EquippedWeapon", weaponID);
        PlayerPrefs.Save();
        ReplaceWeaponInScene(weaponID);
        lastEquippedWeapon = weaponID;
        Debug.Log("Equipped weapon: " + weaponID);
    }

    // Finds the player and replaces the current weapon child with the new one
    void ReplaceWeaponInScene(string weaponID)
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            Debug.LogWarning("No player found in scene with tag " + playerTag);
            return;
        }

        int weaponLayer = LayerMask.NameToLayer(weaponLayerName);
        Transform foundWeapon = null;
        Vector3 spawnPos = player.transform.position;
        Quaternion spawnRot = player.transform.rotation;

        // Search all direct children for the current weapon (on Weapon layer)
        foreach (Transform child in player.transform)
        {
            if (child.gameObject.layer == weaponLayer)
            {
                foundWeapon = child;
                // Remember position/rotation for respawn
                spawnPos = child.position;
                spawnRot = child.rotation;
                Destroy(child.gameObject);
            }
        }

        // Find the correct weapon prefab
        GameObject prefab = weaponPrefabs.Find(p => p.name == weaponID);
        if (prefab != null)
        {
            GameObject newWeapon = Instantiate(prefab, spawnPos, spawnRot, player.transform);
            newWeapon.name = weaponID;
            SetLayerRecursively(newWeapon, weaponLayer);
        }
        else
        {
            Debug.LogWarning("No prefab found for weaponID: " + weaponID);
        }
    }

    // Sets the layer for the GameObject and all children
    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform t in obj.transform)
            SetLayerRecursively(t.gameObject, layer);
    }

    // Checks if the correct weapon is present as a child of the player
    bool WeaponInScene(string weaponID)
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null) return false;

        int weaponLayer = LayerMask.NameToLayer(weaponLayerName);
        foreach (Transform child in player.transform)
        {
            if (child.gameObject.layer == weaponLayer && child.name == weaponID)
                return true;
        }
        return false;
    }
}
