using UnityEngine;
using System.Collections;

public class AutoEquipWeaponOnSpawn : MonoBehaviour
{
    public string weaponLayerName = "Weapon";          // Must match your layer name
    public GameObject[] weaponPrefabs;                 // Drag ALL weapon prefabs here in Inspector

    void Start()
    {
        StartCoroutine(EquipDelayed());
    }

    IEnumerator EquipDelayed()
    {
        // Wait one frame to allow PlayerPrefs to be set by other scripts like WeaponsMenu
        yield return null;

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

                // SHIFT the weapon slightly to the right (adjust X/Y as needed)
                newWeapon.transform.localPosition = new Vector3(0.3f, -0.2f, 0f);

                // Set layer for all children recursively
                foreach (Transform grandchild in newWeapon.transform)
                    grandchild.gameObject.layer = weaponLayer;

                break;
            }
        }
    }
}
