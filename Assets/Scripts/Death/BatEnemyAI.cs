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

    [Header("Jumping")]
    public float jumpForce = 6f;
    public float jumpDistance = 2.0f;
    public float jumpRayDownLength = 4f;
    public float jumpYOffset = 0.2f;
    // Removed jumpCooldown and lastJumpTime

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
    private int direction = 1;
    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer sr;
    private float lastTurnTime = -999f;
    private BoxCollider2D boxCollider;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;

    private bool isJumping = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        startPoint = transform.position;
        currentHealth = maxHealth;

        if (rb.bodyType == RigidbodyType2D.Dynamic)
            rb.gravityScale = 5f;
        rb.freezeRotation = true;

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

        if (!isJumping)
        {
            bool detected = false;
            if (player != null)
            {
                float distance = Vector2.Distance(transform.position, player.position);
                if (distance <= detectionDistance)
                    detected = true;
            }

            if (!detected)
                Patrol();
            else
                StopAndAttackPlayer();
        }
        else
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isAttacking", false);
        }
    }

    void Patrol()
    {
        animator.SetBool("isWalking", true);
        animator.SetBool("isAttacking", false);

        // 1. Check patrol distance first: flip if out of bounds
        if (Mathf.Abs(transform.position.x - startPoint.x) > patrolDistance)
        {
            if (Time.time - lastTurnTime > turnCooldown)
            {
                direction *= -1;
                lastTurnTime = Time.time;
                Debug.Log($"{name}: Out of patrol bounds. Flipping direction to {direction}.");
            }
        }

        // 2. Ground check ahead (bat's "feet" + direction)
        Vector2 edgeOrigin = (Vector2)transform.position + Vector2.right * direction * raycastHorizontalOffset;
        RaycastHit2D edgeHit = Physics2D.Raycast(edgeOrigin, Vector2.down, edgeDetectDistance, groundLayer);

        bool edgeFound = (edgeHit.collider && edgeHit.collider.gameObject != this.gameObject);

        if (!edgeFound)
            Debug.Log($"{name}: Edge ray found NO ground directly ahead.");

        if (!edgeFound)
        {
            // 3. Check for jumpable platform ahead
            float forwardCheckY = transform.position.y - (boxCollider != null ? boxCollider.size.y / 2f : 0.5f) - jumpYOffset;
            Vector2 forwardCheckOrigin = new Vector2(
                transform.position.x + direction * (raycastHorizontalOffset + jumpDistance),
                forwardCheckY
            );
            RaycastHit2D jumpHit = Physics2D.Raycast(forwardCheckOrigin, Vector2.down, jumpRayDownLength, groundLayer);

            bool jumpFound = (jumpHit.collider != null);

            if (jumpFound)
            {
                Debug.Log($"{name}: JUMPING! Forward platform found.");
                Vector2 jumpVector = new Vector2(direction * patrolSpeed * 3f, jumpForce);
                rb.velocity = Vector2.zero;
                rb.AddForce(jumpVector, ForceMode2D.Impulse);
                isJumping = true;
                animator.SetBool("isWalking", false);
                return;
            }
            else
            {
                // Can't jump; flip direction and do NOT move this frame
                if (Time.time - lastTurnTime > turnCooldown)
                {
                    direction *= -1;
                    lastTurnTime = Time.time;
                    Debug.Log($"{name}: Could not jump, flipping direction to {direction}.");
                }
                rb.velocity = Vector2.zero;
                return;
            }
        }
        else
        {
            rb.velocity = new Vector2(direction * patrolSpeed, rb.velocity.y);
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, 1);
        }
    }

    void StopAndAttackPlayer()
    {
        if (player == null) return;

        float dx = player.position.x - transform.position.x;
        float distance = Mathf.Abs(dx);

        int facingDir = (dx > 0) ? 1 : -1;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * facingDir, transform.localScale.y, 1);

        if (distance <= attackRange)
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("isWalking", false);
            animator.SetBool("isAttacking", true);

            if (!isAttacking && (Time.time - lastAttackTime >= attackCooldown))
            {
                lastAttackTime = Time.time;
                isAttacking = true;
                StartCoroutine(ResetAttackStateAndCollider());
            }
        }
        else
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isAttacking", false);

            float moveSpeed = Mathf.Sign(dx) * patrolSpeed * 1.25f;
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
        }
    }

    public void EnableAttackHitbox() => ExtendAttackCollider(true);
    public void DisableAttackHitbox() => ExtendAttackCollider(false);

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
        yield return new WaitForSeconds(0.4f);
        isAttacking = false;
        animator.SetBool("isAttacking", false);
        animator.SetBool("isWalking", true);
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
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isJumping = false;
            animator.SetBool("isWalking", true);
        }

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

        Vector2 edgeOrigin = (Vector2)transform.position + Vector2.right * direction * raycastHorizontalOffset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(edgeOrigin, edgeOrigin + Vector2.down * edgeDetectDistance);

        Vector2 forwardCheckOrigin = (Vector2)transform.position + Vector2.right * direction * (raycastHorizontalOffset + jumpDistance);
        forwardCheckOrigin.y -= jumpYOffset;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(forwardCheckOrigin, forwardCheckOrigin + Vector2.down * jumpRayDownLength);
    }
}
