using System;
using System.Collections;
using System.Collections.Generic;
using Enemy;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool hasHit = false;
    private bool isWaitingToDespawn = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Only rotate if the arrow is moving
        if (rb.velocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
{
    if (hasHit) return;
    hasHit = true;

    // Check if the object we hit implements IDamageable
    if (collision.collider.TryGetComponent(out IDamageable damageable))
    {
        damageable.TakeDamage(1); // or pass your own damage variable
    }

    // Stop movement and stick the arrow
    rb.velocity = Vector2.zero;
    rb.simulated = false;

    // Optional: remove collider to avoid future collisions
    Collider2D col = GetComponent<Collider2D>();
    if (col) Destroy(col);

    transform.localScale = Vector3.one;

    Destroy(gameObject, 3f); // Arrow despawns after sticking
}


    void OnBecameInvisible()
    {
        if (isWaitingToDespawn) return;
        
        isWaitingToDespawn = true;
        Invoke(nameof(SelfDestruct), 3f);
    }

    void SelfDestruct()
    {
        Destroy(gameObject);
    }
}
