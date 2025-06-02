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
    private const string EquippedKey = "EquippedWeapon";
    private const string TempKey = "PreviousEquippedWeapon";

    void Awake()
    {
        HandleSceneSpecificWeaponOverride(); // Moved to Awake to execute before any Start()
    }

    void OnEnable()
    {
        UpdateWeaponButtons();
        EquipIfNeeded();
    }

    void Update()
    {
        EquipIfNeeded();
    }

    // Handles scene-specific logic (e.g. weapon override in Level-1)
    void HandleSceneSpecificWeaponOverride()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        string equipped = PlayerPrefs.GetString(EquippedKey, "Bow");

        if (currentScene == "Level-1")
        {
            // Save current weapon if not already bow
            if (equipped != "Bow")
            {
                PlayerPrefs.SetString(TempKey, equipped);
                PlayerPrefs.SetString(EquippedKey, "Bow");
                PlayerPrefs.Save();
            }
        }
        else
        {
            // Restore previous weapon if stored
            if (equipped == "Bow" && PlayerPrefs.HasKey(TempKey))
            {
                string previous = PlayerPrefs.GetString(TempKey);
                PlayerPrefs.SetString(EquippedKey, previous);
                PlayerPrefs.DeleteKey(TempKey);
                PlayerPrefs.Save();
            }
        }
    }

    void UpdateWeaponButtons()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        bool isLevel1 = currentScene == "Level-1";

        foreach (var wb in weaponButtons)
        {
            if (isLevel1)
            {
                wb.button.gameObject.SetActive(wb.weaponID == "Bow");
            }
            else
            {
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

        string equippedWeapon = PlayerPrefs.GetString(EquippedKey, "Bow");
        EquipWeapon(equippedWeapon);
    }

    public void EquipWeapon(string weaponID)
    {
        if (string.IsNullOrEmpty(weaponID)) return;

        PlayerPrefs.SetString(EquippedKey, weaponID);
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
