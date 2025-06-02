using System;
using System.Collections;
using System.Collections.Generic;
using Enemy;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    public LayerMask enemyLayer;
    public int damage = 1;
    
    private Rigidbody2D rb;
    private bool hasHit = false;
    public float stickTime = 3f; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (hasHit) return;
        
        // Only rotate if the arrow is moving
        if (rb.velocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit) return;               // ignore ricochets
        hasHit = true;

        if (collision.collider.TryGetComponent(out IDamageable target))
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 2. Otherwise stick in whatever we hit (ground, wall, etc.)
        rb.velocity  = Vector2.zero;
        rb.simulated = false;
        transform.localScale = Vector3.one;   // avoid weird stretch if it hit on a diagonal

        Destroy(gameObject, stickTime);       // fall back despawn
    }

    void OnBecameInvisible()
    {
        if (hasHit) return;               // already scheduled
        Destroy(gameObject, stickTime);   // despawn after delay off-screen
    }
}
