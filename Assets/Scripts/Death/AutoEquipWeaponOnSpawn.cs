using UnityEngine;

public class AutoEquipWeaponOnSpawn : MonoBehaviour
{
    public string weaponLayerName = "Weapon";          // Must match your layer name
    public GameObject[] weaponPrefabs;                 // Drag ALL weapon prefabs here in Inspector

    void Start()
    {
        // Remove any old weapon from previous lives (if respawning)
        int weaponLayer = LayerMask.NameToLayer(weaponLayerName);
        foreach (Transform child in transform)
        {
            if (child.gameObject.layer == weaponLayer)
                Destroy(child.gameObject);
        }

        // Read last equipped weapon from PlayerPrefs, default to "Bow"
        string weaponID = PlayerPrefs.GetString("EquippedWeapon", "Bow");

        // Find prefab matching weaponID
        foreach (var prefab in weaponPrefabs)
        {
            if (prefab.name == weaponID)
            {
                GameObject newWeapon = Instantiate(prefab, transform.position, Quaternion.identity, transform);
                newWeapon.name = weaponID;
                newWeapon.layer = weaponLayer; // Set to Weapon layer

                // SHIFT the weapon slightly to the right (adjust X until it looks good)
                newWeapon.transform.localPosition = new Vector3(0.3f, -0.2f, 0f);


                // OPTIONAL: Also set layer for all child objects (if needed)
                foreach (Transform grandchild in newWeapon.transform)
                    grandchild.gameObject.layer = weaponLayer;

                break;
            }
        }
    }
}
