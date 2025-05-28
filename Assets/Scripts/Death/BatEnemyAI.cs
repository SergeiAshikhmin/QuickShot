using UnityEngine;
using System.Collections;

public class BatEnemyAI : MonoBehaviour
{
    [Header("Detection")]
    public string playerTag = "Player";
    public float detectionDistance = 5f;

    [Header("Patrol")]
    public float patrolSpeed = 2f;
    public float patrolDistance = 4f;
    public LayerMask groundLayer;
    public float edgeDetectDistance = 2.0f;
    public float raycastHorizontalOffset = 0.7f;

    [Header("Corner Handling")]
    public float turnCooldown = 0.3f;

    [Header("Attack")]
    public float attackRange = 0.9f;
    public float attackCooldown = 1.2f;
    public int attackDamage = 1;
    public float attackColliderExtension = 1.2f;
    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    [Header("Effects")]
    public Sprite hitEffectSprite;
    public float hitEffectDuration = 0.08f;
    public GameObject impactPrefab;

    [Header("Health")]
    public int maxHealth = 5;

    private int currentHealth;
    private Animator animator;
    private Vector2 startPoint;
    private int direction = 1; // 1 for right, -1 for left
    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer sr;
    private float lastTurnTime = -999f;
    private BoxCollider2D boxCollider;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        startPoint = transform.position;
        currentHealth = maxHealth;

        if (rb.bodyType == RigidbodyType2D.Dynamic)
            rb.gravityScale = 0f;
        if (rb != null)
            rb.freezeRotation = true;

        Vector3 pos = transform.position;
        transform.position = new Vector3(pos.x, pos.y, 0f);

        if (boxCollider != null)
        {
            originalColliderSize = boxCollider.size;
            originalColliderOffset = boxCollider.offset;
        }
        else
        {
            Debug.LogWarning($"{name} is missing a BoxCollider2D!");
        }

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
            }
        }

        if (!detected)
        {
            Patrol();
        }
        else
        {
            StopAndAttackPlayer();
        }
    }

    void Patrol()
    {
        animator.SetBool("isWalking", true);
        animator.SetBool("isAttacking", false);

        Vector2 origin = (Vector2)transform.position + Vector2.right * direction * raycastHorizontalOffset;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, edgeDetectDistance, groundLayer);

        Debug.DrawRay(origin, Vector2.down * edgeDetectDistance, hit.collider ? Color.green : Color.red);

        if (hit.collider)
        {
            rb.velocity = new Vector2(direction * patrolSpeed, 0);

            // Flip object, NOT just sprite
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, 1);

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

    void StopAndAttackPlayer()
    {
        if (player == null) return;

        float dx = player.position.x - transform.position.x;
        float distance = Mathf.Abs(dx);

        // Flip to face the player, hitbox always extends in positive X
        int facingDir = (dx > 0) ? 1 : -1;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * facingDir, transform.localScale.y, 1);

        // If within attack range, stop and attack repeatedly
        if (distance <= attackRange)
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("isWalking", false);
            animator.SetBool("isAttacking", true);

            // Only trigger a new attack when not already attacking and cooldown has passed
            if (!isAttacking && (Time.time - lastAttackTime >= attackCooldown))
            {
                lastAttackTime = Time.time;
                isAttacking = true;

                // Animation events should call EnableAttackHitbox/DisableAttackHitbox and AttemptPlayerAttack
                StartCoroutine(ResetAttackStateAndCollider());
            }
        }
        else
        {
            // Move toward player, but never overlap
            animator.SetBool("isWalking", true);
            animator.SetBool("isAttacking", false);

            float moveSpeed = Mathf.Sign(dx) * patrolSpeed * 1.25f;
            rb.velocity = new Vector2(moveSpeed, 0);
        }
    }

    // Animation Event - called exactly on the frame you want the hitbox active
    public void EnableAttackHitbox()
    {
        ExtendAttackCollider(true);
    }

    // Animation Event - called to end the hitbox extension
    public void DisableAttackHitbox()
    {
        ExtendAttackCollider(false);
    }

    // Animation Event - call this at the moment of "impact" in the attack animation
    public void AttemptPlayerAttack()
    {
        if (player && Mathf.Abs(player.position.x - transform.position.x) <= attackRange + 0.2f)
        {
            Debug.Log($"{name} attacks player!");
            player.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
        }
    }

    void ExtendAttackCollider(bool extend)
    {
        if (boxCollider == null) return;

        if (extend)
        {
            // Always extend to the right (positive X); flipping is handled by scale
            boxCollider.size = new Vector2(originalColliderSize.x + attackColliderExtension, originalColliderSize.y);
            boxCollider.offset = originalColliderOffset + new Vector2((attackColliderExtension / 2f), 0);
        }
        else
        {
            boxCollider.size = originalColliderSize;
            boxCollider.offset = originalColliderOffset;
        }
    }

    IEnumerator ResetAttackStateAndCollider()
    {
        // Wait the length of the attack animation; adjust as needed
        yield return new WaitForSeconds(0.4f);
        isAttacking = false;
        animator.SetBool("isAttacking", false);
        animator.SetBool("isWalking", true);

        // ExtendAttackCollider(false); // Animation event disables
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            HandleHit(other.ClosestPoint(transform.position));
            Destroy(other.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            Vector2 impactPos = collision.contacts.Length > 0 ? collision.contacts[0].point : (Vector2)transform.position;
            HandleHit(impactPos);
            Destroy(collision.gameObject);
        }
    }

    void HandleHit(Vector2 impactPoint)
    {
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

        currentHealth--;
        Debug.Log($"{name} took damage! Health is now {currentHealth}");

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
