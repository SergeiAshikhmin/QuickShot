using UnityEngine;

public class Pistol : MonoBehaviour, IAmmoWeapon
{
    [Header("References")]
    public GameObject bulletPrefab;
    public GameObject muzzleFlashPrefab;
    public Transform firePoint;

    [Header("Settings")]
    public float shootForce = 20f;
    public float shootCooldown = 0.25f;
    public float pushbackForce = 2f;

    [Header("Ammo")]
    public int maxAmmo = 7;
    public float reloadTime = 1.5f;
    private int _ammo;
    private bool _isReloading = false;
    private bool _isOutOfAmmo = false;

    // For your HUD
    public bool isReloading => _isReloading;
    public bool isOutOfAmmo => _isOutOfAmmo;

    // IAmmoWeapon Interface
    public int CurrentAmmo => _ammo;
    public int MaxAmmo => maxAmmo;
    public bool ShowAmmo => true;

    [Header("Audio")]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioSource audioSource; // Assign in Inspector

    private Rigidbody2D playerRB;
    private float lastShotTime;
    private Coroutine reloadRoutine;

    private void Awake()
    {
        FindPlayerRB();
        _ammo = maxAmmo;
    }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        if (playerRB == null)
            FindPlayerRB();

        // Get mouse position
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // CHANGED: Rotate towards mouse without flipping
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // CHANGED: Flip gun vertically if facing left
        Vector3 localScale = transform.localScale;
        localScale.y = (angle > 90 || angle < -90) ? -1 : 1;
        transform.localScale = localScale;

        _isOutOfAmmo = (_ammo == 0);

        if (_isReloading)
            return;

        if (_isOutOfAmmo && !_isReloading)
        {
            reloadRoutine = StartCoroutine(Reload());
            return;
        }

        if (Input.GetMouseButtonDown(0) && Time.time >= lastShotTime + shootCooldown)
        {
            if (_ammo > 0)
            {
                Shoot();
                lastShotTime = Time.time;

                if (_ammo == 0 && !_isReloading)
                {
                    reloadRoutine = StartCoroutine(Reload());
                }
            }
        }
    }

    void Shoot()
    {
        _ammo--;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.AddForce(firePoint.right * shootForce, ForceMode2D.Impulse);

        if (muzzleFlashPrefab)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(flash, 0.1f);
        }

        if (audioSource != null && fireSound != null)
            audioSource.PlayOneShot(fireSound);

        if (playerRB != null)
        {
            Vector2 pushDirection = -firePoint.right.normalized;
            playerRB.AddForce(pushDirection * pushbackForce, ForceMode2D.Impulse);
        }
    }

    private System.Collections.IEnumerator Reload()
    {
        _isReloading = true;

        if (audioSource != null && reloadSound != null)
            audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(reloadTime);

        _ammo = maxAmmo;
        _isReloading = false;
    }

    private void FindPlayerRB()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
        {
            if (go.layer == playerLayer && go.GetComponent<Rigidbody2D>() != null)
            {
                playerRB = go.GetComponent<Rigidbody2D>();
                return;
            }
        }
        if (playerRB == null)
        {
            Debug.LogWarning("No Rigidbody2D found on any GameObject on the Player layer!");
        }
    }
}
