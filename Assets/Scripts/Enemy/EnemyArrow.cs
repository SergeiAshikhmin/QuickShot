using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArrow : MonoBehaviour
{
    public int damage = 1;
    public float lifeTime = 6f;

    Rigidbody2D rb;
    bool hasHit;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifeTime);       // safety despawn
    }

    void Update()
    {
        if (hasHit) return;

        // rotate sprite to face travel direction
        if (rb.velocity.sqrMagnitude > 0.01f)
        {
            float ang = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(ang, Vector3.forward);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (hasHit) return;
        if (!col.CompareTag("Player")) return;

        hasHit = true;
        col.GetComponent<PlayerHealth>()?.TakeDamage(damage);

        rb.velocity  = Vector2.zero;
        rb.simulated = false;
        Destroy(gameObject, 2f);             // let it stick for flair
    }
}
