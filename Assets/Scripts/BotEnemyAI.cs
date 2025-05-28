using UnityEngine;
using System.Collections;


public class BotEnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public string playerTag = "Player";
    public float detectionDistance = 5f;

    [Header("Patrol")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 4f;
    public LayerMask groundLayer;
    public float edgeDetectDistance = 2.0f;    // For flying bots!
    public float raycastHorizontalOffset = 0.7f;

    [Header("Corner Handling")]
    public float turnCooldown = 0.3f;

    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("VFX")]
    public GameObject impactPrefab;  // Drag Impact01 here!
    public Sprite hitEffectSprite;   // Optional: sprite for hit effect
    public float hitEffectDuration = 0.08f;

    [Header("Enemy Attack")]
    public GameObject bulletPrefab;   // Drag your bullet prefab here!
    public float bulletSpeed = 2f;    // Editable in Inspector
    public float fireRate = 2f;       // Time (sec) between shots

    private float lastFireTime = -999f;

    private Animator animator;
    private Vector2 startPoint;
    private int direction = 1;

    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer sr;
    private float lastTurnTime = -999f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        startPoint = transform.position;
        currentHealth = maxHealth;

        if (rb.bodyType == RigidbodyType2D.Dynamic)
            rb.gravityScale = 0f;

        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x, pos.y, 0f);

        if (rb != null)
            rb.freezeRotation = true;

        Debug.Log($"{name}: groundLayer mask value is {groundLayer.value}. If this is 0, it is 'Nothing' and will never detect ground!");
    }

    void Update()
    {
        transform.eulerAngles = Vector3.zero;

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        player = playerObj ? playerObj.transform : null;

        bool detected = false;
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= detectionDistance)
            {
                detected = true;
                // Fire only if enough time has passed since last shot
                if (Time.time - lastFireTime > fireRate)
                {
                    FireBullet();
                    lastFireTime = Time.time;
                }
            }
        }

        if (animator) animator.SetBool("Detected", detected);

        if (!detected)
        {
            Patrol();
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    void Patrol()
    {
        Vector2 origin = (Vector2)transform.position + Vector2.right * direction * raycastHorizontalOffset;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, edgeDetectDistance, groundLayer);

        Debug.DrawRay(origin, Vector2.down * edgeDetectDistance, hit.collider ? Color.green : Color.red);

        if (hit.collider)
        {
            rb.velocity = new Vector2(direction * patrolSpeed, 0);

            if (sr != null)
                sr.flipX = (direction < 0);

            if (Mathf.Abs(transform.position.x - startPoint.x) > patrolDistance)
            {
                if (Time.time - lastTurnTime > turnCooldown)
                {
                    direction *= -1;
                    lastTurnTime = Time.time;
                }
            }
        }
        else
        {
            if (Time.time - lastTurnTime > turnCooldown)
            {
                direction *= -1;
                lastTurnTime = Time.time;
            }
            rb.velocity = Vector2.zero;
        }
    }

    void FireBullet()
    {
        if (bulletPrefab != null && player != null)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            EnemyBullet eb = bullet.GetComponent<EnemyBullet>();
            if (eb != null)
            {
                eb.speed = bulletSpeed;
                eb.SetDirection(dir);
            }
            else
            {
                // Fallback if not using EnemyBullet script
                Rigidbody2D rb2d = bullet.GetComponent<Rigidbody2D>();
                if (rb2d != null)
                    rb2d.velocity = dir * bulletSpeed;
            }
        }
    }

    // Handle bullet triggers (works for "Is Trigger" bullets)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            TakeDamage(1, other.ClosestPoint(transform.position));
            Destroy(other.gameObject); // Destroy bullet
        }
    }

    // Handle bullet collisions (works for non-trigger bullets, or for flexibility)
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            Vector2 impactPos = collision.contacts.Length > 0 ? collision.contacts[0].point : (Vector2)transform.position;
            TakeDamage(1, impactPos);
            Destroy(collision.gameObject); // Destroy bullet
        }
    }

    void TakeDamage(int amount, Vector2 impactPoint)
    {
        currentHealth -= amount;
        Debug.Log($"{name} took damage! Health is now {currentHealth}");

        // Hit effect (sprite flash)
        if (hitEffectSprite != null)
        {
            GameObject hitObj = new GameObject("HitEffect");
            hitObj.transform.position = impactPoint;
            var spriteR = hitObj.AddComponent<SpriteRenderer>();
            spriteR.sprite = hitEffectSprite;
            spriteR.sortingOrder = 999;
            Destroy(hitObj, hitEffectDuration);
        }
        else if (sr != null)
        {
            StartCoroutine(HitFlash());
        }

        if (currentHealth <= 0)
        {
            Die(impactPoint);
        }
    }

    IEnumerator HitFlash()
    {
        Color original = sr.color;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.07f);
        sr.color = original;
    }

    void Die(Vector2 impactPoint)
    {
        Debug.Log($"{name} died! Spawning impact effect.");
        if (impactPrefab)
        {
            Instantiate(impactPrefab, impactPoint, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionDistance);

        Vector2 origin = (Vector2)transform.position + Vector2.right * direction * raycastHorizontalOffset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + Vector2.down * edgeDetectDistance);
    }
}
