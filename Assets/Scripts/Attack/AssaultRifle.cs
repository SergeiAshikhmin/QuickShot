using UnityEngine;

public class AssaultRifle : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;
    public GameObject muzzleFlashPrefab;
    public Transform firePoint;

    [Header("Settings")]
    public float shootForce = 25f;      // Stronger than pistol
    public float fireRate = 0.1f;       // Time between shots (auto fire)
    public float pushbackForce = 1.5f;  // Weaker recoil than pistol

    private Rigidbody2D playerRB;
    private float lastShotTime;

    void Awake()
    {
        FindPlayerRB();
    }

    void Update()
    {
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

        // Auto-fire (hold down mouse button)
        if (Input.GetMouseButton(0) && Time.time >= lastShotTime + fireRate)
        {
            Shoot();
            lastShotTime = Time.time;
        }
    }

    void Shoot()
    {
        // Instantiate bullet and set its velocity
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.AddForce(firePoint.right * shootForce, ForceMode2D.Impulse);

        // Optional: Set direction if using a custom script instead of Rigidbody2D
        var rifleBullet = bullet.GetComponent<RifleBullet>();
        if (rifleBullet != null)
            rifleBullet.SetDirection(firePoint.right);

        // Muzzle flash
        if (muzzleFlashPrefab)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(flash, 0.1f);
        }

        // Recoil / Pushback
        if (playerRB != null)
        {
            Vector2 pushDirection = -firePoint.right.normalized;
            playerRB.AddForce(pushDirection * pushbackForce, ForceMode2D.Impulse);
        }
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
