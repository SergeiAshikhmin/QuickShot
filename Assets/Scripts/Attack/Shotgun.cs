using UnityEngine;

public class Shotgun : MonoBehaviour
{
    [Header("References")]
    public GameObject bulletPrefab;
    public GameObject muzzleFlashPrefab;
    public Transform firePoint;

    [Header("Settings")]
    public float shootForce = 16f;         // Slightly lower force than pistol, adjust as desired
    public float shootCooldown = 0.6f;     // Slower fire rate
    public float pushbackForce = 5f;       // Stronger pushback for shotgun
    public int pelletCount = 4;            // Number of pellets
    public float spreadAngle = 15f;        // Total spread angle in degrees

    private Rigidbody2D playerRB;
    private float lastShotTime;

    private void Awake()
    {
        FindPlayerRB();
    }

    void Update()
    {
        if (playerRB == null)
            FindPlayerRB();

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Flip Y to avoid upside down weapon
        Vector3 localScale = Vector3.one;
        localScale.y = (direction.x < 0) ? -1 : 1;
        transform.localScale = localScale;

        // Single fire
        if (Input.GetMouseButtonDown(0) && Time.time >= lastShotTime + shootCooldown)
        {
            Shoot();
            lastShotTime = Time.time;
        }
    }

    void Shoot()
    {
        float startAngle = -spreadAngle / 2f;
        float angleStep = (spreadAngle / (pelletCount - 1));

        for (int i = 0; i < pelletCount; i++)
        {
            float currentAngle = startAngle + angleStep * i;
            Quaternion pelletRotation = firePoint.rotation * Quaternion.Euler(0, 0, currentAngle);

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, pelletRotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.AddForce(pelletRotation * Vector2.right * shootForce, ForceMode2D.Impulse);
        }

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
