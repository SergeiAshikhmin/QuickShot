using System;
using System.Collections;
using System.Collections.Generic;
using Enemy;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ranger : MonoBehaviour, IDamageable
{
    /* ───────── Inspector ───────── */
    [Header("References")]
    public Transform  player;                 // drag in or find by tag
    public GameObject enemyArrowPrefab;       // projectile with EnemyArrow script
    public Transform  firePoint;              // empty at bow tip
    public LayerMask  groundLayer;            // for ledge checks
    [SerializeField]  AudioClip[] hitClips;   // impact SFX

    [Header("Stats")]
    public int    health          = 3;
    public float  hitPause        = 0.3f;

    [Header("Movement")]
    public float  moveSpeed       = 3f;
    public float  aggroRadius     = 8f;
    public float  stopDistance    = 4f;       // ranger stops farther away than knight
    public float  edgeOffsetX     = 0.4f;
    public float  ledgeCheckDepth = 1f;
    
    [Header("Ranged Attack")]
    public float  shootCooldown   = 1.5f;
    public float  arrowSpeed      = 12f;
    public float  gravityScale    = 1f;       // >0 for arcing arrows
    public float  upwardBoost     = 3f;       // lift added to make an arc
    public int    damage          = 1;        // arrow deals this to player

    /* ───────── Internals ───────── */
    Rigidbody2D    rb;
    AudioSource    audioSrc;
    Animator       anim;
    bool           facingRight = true;
    float          lastShotTime;
    int            lastClip = -1;        
    
    public  event Action OnEnemyDied;

    /* ───────── Unity ───────── */
    void Awake()
    {
        rb       = GetComponent<Rigidbody2D>();
        audioSrc = GetComponent<AudioSource>();
        anim     = GetComponent<Animator>();

        if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    void FixedUpdate()
    {
        if (!player) return;

        float dist = Vector2.Distance(transform.position, player.position);

        /* 0. Face target & ledge guard */
        FacePlayer();
        if (!GroundAhead())
        {
            Halt();
            return;
        }

        /* 1. Chase until stopDistance */
        if (dist < aggroRadius && dist > stopDistance)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.velocity = new Vector2(dir.x * moveSpeed, rb.velocity.y);
        }
        else
        {
            Halt();
        }
        anim.SetBool("IsWalking", Mathf.Abs(rb.velocity.x) > 0.1f);

        /* 2. Shoot if in range */
        if (dist <= stopDistance && Time.time >= lastShotTime + shootCooldown)
        {
            anim.SetTrigger("Attack");        // play bow animation
            Shoot();                          // arrow spawns instantly (or via AnimEvent; your choice)
            lastShotTime = Time.time;
        }
    }
    
    /* ───────── Actions ───────── */
    void Shoot()
    {
        if (!enemyArrowPrefab || !firePoint) return;

        Vector2 dir = (player.position - firePoint.position).normalized;

        var arrow = Instantiate(enemyArrowPrefab, firePoint.position, Quaternion.identity);
        var arrowRb = arrow.GetComponent<Rigidbody2D>();
        if (arrowRb)
        {
            arrowRb.gravityScale = gravityScale;
            arrowRb.velocity     = dir * arrowSpeed + Vector2.up * upwardBoost;
        }

        // optional: set arrow damage
        // arrow.GetComponent<EnemyArrow>()?.SetDamage(damage);
    }

    public void TakeDamage(int amt)
    {
        if (health <= 0) return;

        health -= amt;
        anim.SetTrigger("TakeHit");
        PlayHitSound();
        StartCoroutine(HitPauseRoutine());

        if (health <= 0) Die();
    }

    /* ───────── Helpers ───────── */
    IEnumerator HitPauseRoutine()
    {
        float savedSpeed = moveSpeed;
        moveSpeed = 0;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(hitPause);
        moveSpeed = savedSpeed;
    }

    void Die()
    {
        anim.SetBool("IsWalking", false);
        anim.SetTrigger("Die");
        rb.velocity = Vector2.zero;
        moveSpeed   = 0;
        this.enabled = false;

        foreach (Collider2D c in GetComponents<Collider2D>())
            c.enabled = false;

        rb.bodyType = RigidbodyType2D.Static;
        OnEnemyDied?.Invoke();
        Destroy(gameObject, 2f);
    }

    void PlayHitSound()
    {
        if (hitClips == null || hitClips.Length == 0) return;

        int idx = Random.Range(0, hitClips.Length);
        if (hitClips.Length > 1 && idx == lastClip) idx = (idx + 1) % hitClips.Length;

        audioSrc.pitch = Random.Range(0.95f, 1.05f);
        audioSrc.PlayOneShot(hitClips[idx]);
        audioSrc.pitch = 1f;
        lastClip = idx;
    }

    void FacePlayer()
    {
        float dirX = player.position.x - transform.position.x;
        if ((dirX > 0 && !facingRight) || (dirX < 0 && facingRight))
        {
            facingRight = !facingRight;
            Vector3 s = transform.localScale;
            s.x *= -1;
            transform.localScale = s;
        }
    }

    void Halt() => rb.velocity = new Vector2(0, rb.velocity.y);

    bool GroundAhead()
    {
        Vector2 origin = rb.position + new Vector2(facingRight ? edgeOffsetX : -edgeOffsetX, 0);
        return Physics2D.Raycast(origin, Vector2.down, ledgeCheckDepth, groundLayer);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        Gizmos.color = Color.green;
        Vector3 o = transform.position + new Vector3(facingRight ? edgeOffsetX : -edgeOffsetX, 0);
        Gizmos.DrawLine(o, o + Vector3.down * ledgeCheckDepth);
    }
#endif
    
    
}
