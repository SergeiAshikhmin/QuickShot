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

    [Header("Alert Settings")]
    public GameObject warningSignPrefab;
    public float alertYOffset = 1.5f;

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
    private bool isPlayerDetected = false;
    private bool hasAlerted = false;
    private GameObject currentAlertInstance = null;

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

        StartCoroutine(UpdateHurtFlash());

        Debug.Log($"{name}: groundLayer mask value is {groundLayer.value}. If this is 0, it is 'Nothing' and will never detect ground!");
    }

    void Update()
    {
        transform.eulerAngles = Vector3.zero;

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        player = playerObj ? playerObj.transform : null;

        bool isNowDetected = false;
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= detectionDistance)
                isNowDetected = true;
        }

        if (isNowDetected && !hasAlerted)
        {
            hasAlerted = true;
            ShowWarningAlert();
        }

        if (!isNowDetected && hasAlerted)
        {
            hasAlerted = false;
        }

        isPlayerDetected = isNowDetected;

        if (isJumping)
        {
            animator.SetBool("isWalking", false);
            if (isPlayerDetected)
                StopAndAttackPlayer();
            return;
        }

        if (isPlayerDetected)
            StopAndAttackPlayer();
        else
            Patrol();
    }

    void ShowWarningAlert()
    {
        if (warningSignPrefab != null && currentAlertInstance == null)
        {
            currentAlertInstance = Instantiate(warningSignPrefab, transform);
            currentAlertInstance.transform.localPosition = new Vector3(0f, alertYOffset, 0f);

            float animLength = 1.5f;
            Animator anim = currentAlertInstance.GetComponent<Animator>();
            if (anim != null && anim.runtimeAnimatorController != null)
            {
                AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
                if (clips.Length > 0)
                    animLength = clips[0].length;
            }

            Destroy(currentAlertInstance, animLength);
            StartCoroutine(ClearAlertReferenceAfter(animLength));
        }
    }

    IEnumerator ClearAlertReferenceAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentAlertInstance = null;
    }

    void Patrol()
    {
        animator.SetBool("isWalking", true);
        animator.SetBool("isAttacking", false);

        if (Mathf.Abs(transform.position.x - startPoint.x) > patrolDistance)
        {
            if (Time.time - lastTurnTime > turnCooldown)
            {
                direction *= -1;
                lastTurnTime = Time.time;
            }
        }

        Vector2 edgeOrigin = (Vector2)transform.position + Vector2.right * direction * raycastHorizontalOffset;
        RaycastHit2D edgeHit = Physics2D.Raycast(edgeOrigin, Vector2.down, edgeDetectDistance, groundLayer);
        bool edgeFound = edgeHit.collider && edgeHit.collider.gameObject != this.gameObject;

        if (!edgeFound)
        {
            float forwardCheckY = transform.position.y - (boxCollider != null ? boxCollider.size.y / 2f : 0.5f) - jumpYOffset;
            Vector2 forwardCheckOrigin = new Vector2(
                transform.position.x + direction * (raycastHorizontalOffset + jumpDistance),
                forwardCheckY
            );
            RaycastHit2D jumpHit = Physics2D.Raycast(forwardCheckOrigin, Vector2.down, jumpRayDownLength, groundLayer);

            if (jumpHit.collider != null)
            {
                Vector2 jumpVector = new Vector2(direction * patrolSpeed * 3f, jumpForce);
                rb.velocity = Vector2.zero;
                rb.AddForce(jumpVector, ForceMode2D.Impulse);
                isJumping = true;
                animator.SetBool("isWalking", false);
                return;
            }
            else
            {
                if (Time.time - lastTurnTime > turnCooldown)
                {
                    direction *= -1;
                    lastTurnTime = Time.time;
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
        float distance2D = Vector2.Distance(transform.position, player.position);
        int facingDir = (dx > 0) ? 1 : -1;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * facingDir, transform.localScale.y, 1);

        if (distance2D <= attackRange)
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
            rb.velocity = new Vector2(Mathf.Sign(dx) * patrolSpeed * 1.25f, rb.velocity.y);
        }
    }

    public void EnableAttackHitbox() => ExtendAttackCollider(true);
    public void DisableAttackHitbox() => ExtendAttackCollider(false);

    public void AttemptPlayerAttack()
    {
        if (player && Vector2.Distance(transform.position, player.position) <= attackRange + 0.2f)
        {
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

        if (other.gameObject.layer == LayerMask.NameToLayer("KillEnemy"))
        {
            Die(transform.position);
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

        if (collision.gameObject.layer == LayerMask.NameToLayer("KillEnemy"))
        {
            Die(transform.position);
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

        currentHealth--;
        if (currentHealth <= 0)
        {
            Die(impactPoint);
        }
    }

    void Die(Vector2 impactPoint)
    {
        if (impactPrefab)
        {
            Instantiate(impactPrefab, impactPoint, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    IEnumerator UpdateHurtFlash()
    {
        Color baseColor = Color.white;

        while (true)
        {
            if (sr != null && currentHealth < maxHealth)
            {
                float t = Mathf.PingPong(Time.time * 4f, 1f);
                float damageRatio = 1f - ((float)currentHealth / maxHealth);
                Color flashColor = Color.Lerp(baseColor, Color.red, t * damageRatio);
                sr.color = flashColor;
            }
            else if (sr != null)
            {
                sr.color = baseColor;
            }

            yield return null;
        }
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
