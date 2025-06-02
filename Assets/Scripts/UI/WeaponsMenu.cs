using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
        EquipIfNeeded();
    }

    void Update()
    {
        EquipIfNeeded();
    }

    void UpdateWeaponButtons()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        bool isLevel1 = currentScene == "Level1";

        foreach (var wb in weaponButtons)
        {
            if (isLevel1)
            {
                // Only show Bow button in Level1
                wb.button.gameObject.SetActive(wb.weaponID == "Bow");
            }
            else
            {
                // Show unlocked weapons
                bool unlocked = wb.weaponID == "Bow" || PlayerPrefs.GetInt("WeaponUnlocked_" + wb.weaponID, 0) == 1;
                wb.button.gameObject.SetActive(unlocked);
            }
        }
    }

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

        if (weaponPresent)
            return;

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Level1")
        {
            // Always equip Bow in Level1 regardless of PlayerPrefs
            EquipWeapon("Bow", force: true);
        }
        else
        {
            string equippedWeapon = PlayerPrefs.GetString("EquippedWeapon", "Bow");
            EquipWeapon(equippedWeapon);
        }
    }

    public void EquipWeapon(string weaponID)
    {
        EquipWeapon(weaponID, force: false);
    }

    // Internal overload that supports forced override
    private void EquipWeapon(string weaponID, bool force)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Level1" && !force)
        {
            Debug.Log("Weapon change blocked: Only Bow allowed in Level1.");
            return;
        }

        if (currentScene == "Level1")
        {
            weaponID = "Bow"; // Force override even if passed something else
        }

        if (string.IsNullOrEmpty(weaponID)) return;

        PlayerPrefs.SetString("EquippedWeapon", weaponID);
        PlayerPrefs.Save();
        ReplaceWeaponInScene(weaponID);
        lastEquippedWeapon = weaponID;
        Debug.Log("Equipped weapon: " + weaponID);
    }

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

        foreach (Transform child in player.transform)
        {
            if (child.gameObject.layer == weaponLayer)
            {
                foundWeapon = child;
                spawnPos = child.position;
                spawnRot = child.rotation;
                Destroy(child.gameObject);
            }
        }

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

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform t in obj.transform)
            SetLayerRecursively(t.gameObject, layer);
    }

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
