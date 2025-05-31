using UnityEngine;
using System.Collections;

public class LaserPistol : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;
    public GameObject muzzleFlashPrefab;
    public Transform firePoint;

    [Header("Charge Settings")]
    public float maxCharge = 100f;
    public float chargePerShot = 20f;
    public float rechargeRate = 2f; // % per second
    public float overheatCooldown = 10f;

    [Header("Shoot Settings")]
    public float shootForce = 20f;
    public float shootCooldown = 0.25f;
    public float pushbackForce = 2f;

    private float _currentCharge;
    private bool _isOverheated = false;
    private float _lastShotTime;
    private float _overheatStartTime;

    private Rigidbody2D _playerRB;
    private Coroutine _overheatRoutine;
    private SpriteRenderer _sprite;

    void Awake()
    {
        _currentCharge = maxCharge;
        FindPlayerRB();
        _sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        AimAtMouse();
        Recharge();

        if (_isOverheated || Time.time < _lastShotTime + shootCooldown)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            TryShoot();
        }
    }

    void TryShoot()
    {
        _lastShotTime = Time.time;

        if (_currentCharge >= chargePerShot)
        {
            _currentCharge -= chargePerShot;
            Shoot();

            if (_currentCharge == 0f)
                StartCoroutine(CheckOverheat());
        }
        else
        {
            // Not enough charge = overheat!
            StartCoroutine(CheckOverheat());
        }
    }

    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.AddForce(firePoint.right * shootForce, ForceMode2D.Impulse);

        if (muzzleFlashPrefab)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(flash, 0.1f);
        }

        if (_playerRB != null)
        {
            Vector2 pushDirection = -firePoint.right.normalized;
            _playerRB.AddForce(pushDirection * pushbackForce, ForceMode2D.Impulse);
        }
    }

    IEnumerator CheckOverheat()
    {
        if (_isOverheated) yield break;

        _isOverheated = true;
        _overheatStartTime = Time.time;

        float timer = 0f;
        while (timer < overheatCooldown)
        {
            BlinkRed(timer);
            timer += 0.2f;
            yield return new WaitForSeconds(0.2f);
        }

        _isOverheated = false;

        // Ensure enough charge for one shot after cooldown
        _currentCharge = Mathf.Max(_currentCharge, chargePerShot);
    }

    void BlinkRed(float t)
    {
        if (_sprite)
            _sprite.color = (Mathf.FloorToInt(t * 5) % 2 == 0) ? Color.red : Color.white;
    }

    void Recharge()
    {
        if (!_isOverheated && _currentCharge < maxCharge)
        {
            _currentCharge += rechargeRate * Time.deltaTime;
            _currentCharge = Mathf.Min(_currentCharge, maxCharge);
        }
    }

    void AimAtMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Vector3 scale = transform.localScale;
        scale.y = (direction.x < 0) ? -1 : 1;
        transform.localScale = scale;
    }

    void FindPlayerRB()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
        {
            if (go.layer == playerLayer && go.GetComponent<Rigidbody2D>())
            {
                _playerRB = go.GetComponent<Rigidbody2D>();
                return;
            }
        }
    }

    // Public properties for UI access
    public float CurrentCharge => _currentCharge;
    public bool IsOverheated => _isOverheated;
    public float OverheatCooldownRemaining => Mathf.Max(0f, overheatCooldown - (Time.time - _overheatStartTime));
}
