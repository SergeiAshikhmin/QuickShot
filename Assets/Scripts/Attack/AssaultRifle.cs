using UnityEngine;

public class AssaultRifle : MonoBehaviour, IAmmoWeapon
{
    [Header("References")]
    public GameObject bulletPrefab;
    public GameObject muzzleFlashPrefab;
    public Transform firePoint;

    [Header("Settings")]
    public float shootForce = 25f;
    public float fireRate = 0.1f;
    public float pushbackForce = 1.5f;

    [Header("Ammo")]
    public int maxAmmo = 30;
    public float reloadTime = 2f;
    private int _ammo;
    private bool _isReloading = false;
    private bool _isOutOfAmmo = false;

    // For HUD/other scripts
    public bool isReloading => _isReloading;
    public bool isOutOfAmmo => _isOutOfAmmo;

    // IAmmoWeapon Interface implementation
    public int CurrentAmmo => _ammo;
    public int MaxAmmo => maxAmmo;
    public bool ShowAmmo => true;

    [Header("Audio")]
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public AudioSource audioSource; // Assign in Inspector or via child object

    private Rigidbody2D playerRB;
    private float lastShotTime;
    private Coroutine reloadRoutine;

    void Awake()
    {
        FindPlayerRB();
        _ammo = maxAmmo;
    }

    void Update()
    {
        // Block input if paused (ensures no shooting when paused!)
        if (Time.timeScale == 0f)
            return;

        // Always re-find player if lost (respawn case)
        if (playerRB == null)
            FindPlayerRB();

        // Rotate to face mouse (top-down or platformer style)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Flip Y to avoid upside-down gun
        Vector3 localScale = Vector3.one;
        localScale.y = (direction.x < 0) ? -1 : 1;
        transform.localScale = localScale;

        _isOutOfAmmo = (_ammo == 0);

        // Prevent shooting while reloading
        if (_isReloading)
            return;

        // Automatic reload if out of ammo and not already reloading
        if (_isOutOfAmmo && !_isReloading)
        {
            reloadRoutine = StartCoroutine(Reload());
            return;
        }

        // Auto-fire (hold mouse)
        if (Input.GetMouseButton(0) && Time.time >= lastShotTime + fireRate)
        {
            if (_ammo > 0)
            {
                Shoot();
                lastShotTime = Time.time;

                // Check after shooting if just used last bullet
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

        var rifleBullet = bullet.GetComponent<RifleBullet>();
        if (rifleBullet != null)
            rifleBullet.SetDirection(firePoint.right);

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
