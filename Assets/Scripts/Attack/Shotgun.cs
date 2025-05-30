using UnityEngine;

public class Shotgun : MonoBehaviour, IAmmoWeapon
{
    [Header("References")]
    public GameObject bulletPrefab;
    public GameObject muzzleFlashPrefab;
    public Transform firePoint;

    [Header("Settings")]
    public float shootForce = 16f;         // Slightly lower force than pistol
    public float shootCooldown = 0.6f;     // Slower fire rate
    public float pushbackForce = 5f;       // Stronger pushback for shotgun
    public int pelletCount = 4;            // Number of pellets per shot
    public float spreadAngle = 15f;        // Total spread angle in degrees

    [Header("Ammo")]
    public int maxAmmo = 6;                // Total shells in the magazine
    public float reloadTimePerPellet = 0.7f; // Time to load one pellet
    private int _ammo;
    private bool _isReloading = false;
    private bool _isOutOfAmmo = false;

    // For HUD/other scripts
    public bool isReloading => _isReloading;
    public bool isOutOfAmmo => _isOutOfAmmo;

    // IAmmoWeapon interface
    public int CurrentAmmo => _ammo;
    public int MaxAmmo => maxAmmo;
    public bool ShowAmmo => true;

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
        // Block input if paused (stops firing or reloading on pause)
        if (Time.timeScale == 0f)
            return;

        if (playerRB == null)
            FindPlayerRB();

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Vector3 localScale = Vector3.one;
        localScale.y = (direction.x < 0) ? -1 : 1;
        transform.localScale = localScale;

        _isOutOfAmmo = (_ammo == 0);

        // Don't allow shooting while reloading
        if (_isReloading)
        {
            // Allow interrupting reload with fire button (classic pump shotgun style)
            if (Input.GetMouseButtonDown(0) && _ammo > 0 && Time.time >= lastShotTime + shootCooldown)
            {
                StopCoroutine(reloadRoutine);
                _isReloading = false;
                Shoot();
                lastShotTime = Time.time;
            }
            return;
        }

        // Automatic reload if empty or not full and reload requested (could be triggered with 'R')
        if ((_isOutOfAmmo || (Input.GetKeyDown(KeyCode.R) && _ammo < maxAmmo)) && !_isReloading)
        {
            reloadRoutine = StartCoroutine(ReloadOneByOne());
            return;
        }

        // Fire if enough ammo for a shot
        if (Input.GetMouseButtonDown(0) && Time.time >= lastShotTime + shootCooldown && _ammo > 0)
        {
            Shoot();
            lastShotTime = Time.time;
            if (_ammo == 0 && !_isReloading)
                reloadRoutine = StartCoroutine(ReloadOneByOne());
        }
    }

    void Shoot()
    {
        int pelletsToFire = Mathf.Min(pelletCount, _ammo); // Don't fire more than you have
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

        if (playerRB != null)
        {
            Vector2 pushDirection = -firePoint.right.normalized;
            playerRB.AddForce(pushDirection * pushbackForce, ForceMode2D.Impulse);
        }
    }

    private System.Collections.IEnumerator ReloadOneByOne()
    {
        _isReloading = true;
        while (_ammo < maxAmmo)
        {
            yield return new WaitForSeconds(reloadTimePerPellet);
            _ammo++;
            // Early exit if player tries to shoot mid-reload
            if (Input.GetMouseButtonDown(0))
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
