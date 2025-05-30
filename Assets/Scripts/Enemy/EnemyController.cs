using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    public Transform player;            // drag the Player object here (or find by tag)
    public LayerMask groundLayer;       // for simple edge detection (optional)
    [Tooltip("Assign several different hit sounds; a random one plays each hit")]
    [SerializeField] private AudioClip[] hitClips;

    [Header("Stats")] 
    public int health = 3;
    public float hitPauseDuration = 0.3f;
    
    [Header("Movement")]
    public float moveSpeed = 3f;        // chase speed
    public float aggroRadius = 8f;      // start chasing when player enters
    public float stopDistance = 1.2f;   // stand here to attack
    public float edgeCheckDistance = 0.5f; // prevents walking off cliffs
    
    [Header("Edge Check")]
    [Tooltip("Horizontal offset of the raycast origin in front of the enemy")]
    public float ledgeOffsetX    = 0.4f;
    [Tooltip("How far down the raycast looks for ground")]
    public float ledgeCheckDepth = 1f;
    
    [Header("Attack")]
    public float attackCooldown = 1.5f; // seconds between attacks
    public int damage = 1;              // call your own Health component
    public Transform hitPoint;          // empty child where the swing originates
    public float hitRadius = 0.8f;      // attack reach
    public LayerMask playerLayer;       // just the player
    
    private Rigidbody2D rb;
    private float lastAttackTime;
    private bool facingRight = true;
    
    AudioSource audioSource;                  // cache in Awake()
    int lastClipIndex = -1;
    
    private Animator animator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        
        // 0a.  Face the player *before* we run any edge logic
        float dirX = player.position.x - transform.position.x;
        if ((dirX > 0 && !facingRight) || (dirX < 0 && facingRight))
            Flip();                                    // <─ new early flip

        // 0b. Abort chase if a cliff is now ahead of that new facing
        if (!GroundAhead())
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            animator.SetBool("IsWalking", false);
            return;
        }

        // 1. Decide whether to chase
        if (distance < aggroRadius && distance > stopDistance)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            Vector2 targetVel = new Vector2(dir.x * moveSpeed, rb.velocity.y);
            rb.velocity = targetVel;

            // Flip sprite when necessary
            if ((dir.x > 0 && !facingRight) || (dir.x < 0 && facingRight))
                Flip();
            
            animator.SetBool("IsWalking", false);
        }
        else
        {
            // idle / braking
            rb.velocity = new Vector2(0, rb.velocity.y);
            animator.SetBool("IsWalking", false);
        }

        // 2. Attack if close enough and cooldown elapsed
        if (distance <= stopDistance && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
        
        bool isMoving = Mathf.Abs(rb.velocity.x) > 0.1f;
        animator.SetBool("IsWalking", isMoving);
    }
    
    void Attack()
    {
        // Very simple melee: damage anything in a small circle
        Collider2D hit = Physics2D.OverlapCircle(hitPoint.position, hitRadius, playerLayer);
        if (hit != null)
        {
            animator.SetTrigger("Attack");
            PlayAttackSound();
            
            // Replace with your own health system
            hit.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            Debug.Log("Hit " + hit.name);
        }
        // TODO: trigger animation & sound here
    }

    public void TakeDamage(int amount)
    {
        if (health <= 0) return; // already dead
        
        health -= amount;
        
        // Play the hit animation
        animator.SetTrigger("TakeHit");
        
        // Optional: temporarily stop movement
        StartCoroutine(HitPause(hitPauseDuration));

        if (health <= 0) Die();
    }
    
    private IEnumerator HitPause(float duration)
    {
        float originalSpeed = moveSpeed;
        moveSpeed = 0;
        rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(duration);

        moveSpeed = originalSpeed;
    }
    
    void Die()
    {
        animator.SetBool("IsWalking", false);
        animator.SetTrigger("Die");
        rb.velocity = Vector2.zero;
        moveSpeed = 0;

        // Optional: disable enemy logic
        this.enabled = false;

        // Optional: disable colliders to prevent future hits
        // foreach (Collider2D col in GetComponents<Collider2D>())
        // {
        //     col.enabled = false;
        // }

        // Optional: destroy object after animation delay
        Destroy(gameObject, 2f); // Adjust time based on your death animation
    }

    void PlayAttackSound()
    {
        // 2. play sound
        if (hitClips != null && hitClips.Length > 0)
        {
            int index = Random.Range(0, hitClips.Length);
            
            // avoid immediate repeat if you have >1 clip
            if (hitClips.Length > 1 && index == lastClipIndex) index = (index + 1) % hitClips.Length;
            
            audioSource.pitch = Random.Range(0.95f, 1.05f); // tiny pitch scatter
            audioSource.PlayOneShot(hitClips[index]);
            audioSource.pitch = 1f;
            
            lastClipIndex = index;
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    
    bool GroundAhead()
    {
        // start a little in front of the feet, pointed straight down
        Vector2 origin = rb.position + new Vector2(facingRight ? ledgeOffsetX : -ledgeOffsetX, 0f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, ledgeCheckDepth, groundLayer);
        return hit.collider != null;
    }
    
#if UNITY_EDITOR     // draws gizmos in Scene view
    void OnDrawGizmosSelected()
    {
        // attack + aggro + stop radii
        Gizmos.color = Color.red;
        if (hitPoint != null) Gizmos.DrawWireSphere(hitPoint.position, hitRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
        
        // edge-check gizmo
        Gizmos.color = Color.green;
        Vector3 origin = transform.position +
                         new Vector3(facingRight ? ledgeOffsetX : -ledgeOffsetX, 0, 0);
        Gizmos.DrawLine(origin, origin + Vector3.down * ledgeCheckDepth);
    }
#endif
}
