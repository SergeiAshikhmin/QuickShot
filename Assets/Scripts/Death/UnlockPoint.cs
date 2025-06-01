using UnityEngine;
using UnityEngine.UI;

public class UnlockPoint : MonoBehaviour
{
    [Header("Unlock Settings")]
    public string weaponID = "Pistol";
    public GameObject weaponPrefab;
    public GameObject popupPrefab;

    [Header("Bobbing Settings")]
    public float bobAmplitude = 0.25f;
    public float bobFrequency = 1f;

    [Header("Audio")]
    public AudioClip unlockSound;         // ✅ Assign in Inspector
    public AudioSource audioSource;       // ✅ Optional (assign in Inspector)

    private bool triggered = false;
    private Vector3 startPosition;
    private Sprite cachedWeaponSprite = null;

    void Awake()
    {
        startPosition = transform.position;

        if (PlayerPrefs.GetInt("WeaponUnlocked_" + weaponID, 0) == 1)
        {
            gameObject.SetActive(false);
            return;
        }

        if (weaponPrefab != null)
        {
            SpriteRenderer weaponSpriteRenderer = weaponPrefab.GetComponentInChildren<SpriteRenderer>();
            SpriteRenderer currentSpriteRenderer = GetComponent<SpriteRenderer>();

            if (weaponSpriteRenderer != null)
            {
                cachedWeaponSprite = weaponSpriteRenderer.sprite;
                if (currentSpriteRenderer != null)
                    currentSpriteRenderer.sprite = cachedWeaponSprite;
            }
        }
    }

    void Update()
    {
        float offsetY = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = startPosition + new Vector3(0f, offsetY, 0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player"))
            return;

        triggered = true;

        // ✅ Play unlock sound
        if (unlockSound != null)
        {
            if (audioSource != null)
                audioSource.PlayOneShot(unlockSound);
            else
                AudioSource.PlayClipAtPoint(unlockSound, transform.position);
        }

        PlayerPrefs.SetInt("WeaponUnlocked_" + weaponID, 1);
        PlayerPrefs.Save();

        Time.timeScale = 0f;

        var movement = other.GetComponent<AdvancedPlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        var pauseMenu = FindObjectOfType<PauseMenuManager>();
        if (pauseMenu != null)
            pauseMenu.enabled = false;

        if (popupPrefab)
        {
            GameObject popup = Instantiate(popupPrefab);
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
                popup.transform.SetParent(canvas.transform, false);

            var popupScript = popup.GetComponent<UnlockPopup>();
            if (popupScript != null)
            {
                popupScript.Setup(
                    weaponID,
                    () => OnPopupClosed(other.gameObject, pauseMenu),
                    cachedWeaponSprite
                );
            }
        }
        else
        {
            // If no popup, fall back to normal
            OnPopupClosed(other.gameObject, pauseMenu);
        }
    }

    void OnPopupClosed(GameObject playerObj, PauseMenuManager pauseMenu)
    {
        Time.timeScale = 1f;

        if (playerObj != null)
        {
            var movement = playerObj.GetComponent<AdvancedPlayerMovement>();
            if (movement != null)
                movement.enabled = true;
        }

        if (pauseMenu != null)
            pauseMenu.enabled = true;

        if (this != null)
            Destroy(gameObject);
    }
}
