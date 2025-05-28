using UnityEngine;

public class UnlockPoint : MonoBehaviour
{
    [Header("Unlock Settings")]
    public string weaponID = "Pistol";
    public GameObject popupPrefab; // Assign the UnlockPopup Canvas prefab in Inspector

    private bool triggered = false;

    void Awake() // or use Start()
    {
        // If weapon is already unlocked, disable (or destroy) this UnlockPoint
        if (PlayerPrefs.GetInt("WeaponUnlocked_" + weaponID, 0) == 1)
        {
            gameObject.SetActive(false); // Or Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        // Unlock the weapon in PlayerPrefs
        PlayerPrefs.SetInt("WeaponUnlocked_" + weaponID, 1);
        PlayerPrefs.Save();

        // Destroy the unlock point in the world
        Destroy(gameObject);

        // Freeze the game
        Time.timeScale = 0f;

        // Disable player movement (if present)
        var movement = other.GetComponent<AdvancedPlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        // Disable pause menu logic (if present)
        var pauseMenu = FindObjectOfType<PauseMenuManager>();
        if (pauseMenu != null)
            pauseMenu.enabled = false;

        // Show the popup UI
        if (popupPrefab)
        {
            GameObject popup = Instantiate(popupPrefab);

            // Parent the popup to the UI Canvas if one exists in scene
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
                popup.transform.SetParent(canvas.transform, false);

            var popupScript = popup.GetComponent<UnlockPopup>();
            if (popupScript != null)
            {
                popupScript.Setup(
                    weaponID,
                    () => OnPopupClosed(other.gameObject, pauseMenu)
                );
            }
        }
    }

    // Callback from UnlockPopup after closing
    void OnPopupClosed(GameObject playerObj, PauseMenuManager pauseMenu)
    {
        Time.timeScale = 1f; // Resume game

        // Re-enable player movement
        if (playerObj != null)
        {
            var movement = playerObj.GetComponent<AdvancedPlayerMovement>();
            if (movement != null)
                movement.enabled = true;
        }

        // Re-enable pause menu logic
        if (pauseMenu != null)
            pauseMenu.enabled = true;
    }
}
