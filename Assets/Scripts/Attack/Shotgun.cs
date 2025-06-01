using UnityEngine;

public class Shotgun : MonoBehaviour, IAmmoWeapon
{
    [Header("References")]
    public GameObject bulletPrefab;
    public GameObject muzzleFlashPrefab;
    public Transform firePoint;

    [Header("Settings")]
    public float shootForce = 16f;
    public float shootCooldown = 0.6f;
    public float pushbackForce = 5f;
    public int pelletCount = 4;
    public float spreadAngle = 15f;

    [Header("Ammo")]
    public int maxAmmo = 6;
    public float reloadTimePerPellet = 0.7f;
    private int _ammo;
    private bool _isReloading = false;
    private bool _isOutOfAmmo = false;

    // HUD Interface
    public bool isReloading => _isReloading;
    public bool isOutOfAmmo => _isOutOfAmmo;
    public int CurrentAmmo => _ammo;
    public int MaxAmmo => maxAmmo;
    public bool ShowAmmo => true;

    [Header("Audio")]
    public AudioClip fireSound;
    public AudioClip reloadShellSound;
    public AudioClip reloadReadySound;
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
        if (Time.timeScale == 0f) return;

        if (playerRB == null) FindPlayerRB();

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Vector3 localScale = Vector3.one;
        localScale.y = (direction.x < 0) ? -1 : 1;
        transform.localScale = localScale;

        _isOutOfAmmo = (_ammo == 0);

        if (_isReloading)
        {
            if (Input.GetMouseButtonDown(0) && _ammo >= pelletCount && Time.time >= lastShotTime + shootCooldown)
            {
                StopCoroutine(reloadRoutine);
                _isReloading = false;
                Shoot();
                lastShotTime = Time.time;
            }
            return;
        }

        if (Input.GetMouseButtonDown(0) && Time.time >= lastShotTime + shootCooldown && _ammo >= pelletCount)
        {
            Shoot();
            lastShotTime = Time.time;
        }
    }

    void Shoot()
    {
        int pelletsToFire = Mathf.Min(pelletCount, _ammo);
        float startAngle = -spreadAngle / 2f;
        float angleStep = (pelletCount > 1) ? (spreadAngle / (pelletCount - 1)) : 0f;

        for (int i = 0; i < pelletsToFire; i++)
        {
            float currentAngle = startAngle + angleStep * i;
            Quaternion pelletRotation = firePoint.rotation * Quaternion.Euler(0, 0, currentAngle);

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, pelletRotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.AddForce(pelletRotation * Vector2.right * shootForce, ForceMode2D.Impulse);
        }

        _ammo -= pelletsToFire;

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

        // Always reload immediately after shooting
        if (!_isReloading && _ammo < maxAmmo)
        {
            reloadRoutine = StartCoroutine(ReloadOneByOne());
        }
    }

    private System.Collections.IEnumerator ReloadOneByOne()
    {
        _isReloading = true;
        int lastReadyThreshold = Mathf.FloorToInt(_ammo / (float)pelletCount); // Track last multiple

        while (_ammo < maxAmmo)
        {
            yield return new WaitForSeconds(reloadTimePerPellet);
            _ammo++;

            if (audioSource != null && reloadShellSound != null)
                audioSource.PlayOneShot(reloadShellSound);

            int currentReadyThreshold = Mathf.FloorToInt(_ammo / (float)pelletCount);
            if (currentReadyThreshold > lastReadyThreshold)
            {
                if (audioSource != null && reloadReadySound != null)
                    audioSource.PlayOneShot(reloadReadySound);
                lastReadyThreshold = currentReadyThreshold;
            }

            // Allow player to shoot once ready
            if (Input.GetMouseButtonDown(0) && _ammo >= pelletCount)
                break;
        }

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
