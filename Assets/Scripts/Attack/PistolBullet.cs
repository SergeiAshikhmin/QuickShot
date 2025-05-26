using UnityEngine;

public class PistolBullet : MonoBehaviour
{
    private Rigidbody2D rb;
    public float ricochetDamping = 0.9f; // Controls energy loss on bounce
    public float minSpeedToSurvive = 1.0f; // Bullet is destroyed if speed drops below this
    public LayerMask groundLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Rotate to face movement
        if (rb.velocity.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // Destroy bullet if it slows down too much
        if (rb.velocity.magnitude < minSpeedToSurvive)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ricochet if ground, else destroy
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            // Ricochet!
            Vector2 inDirection = rb.velocity.normalized;
            Vector2 inVelocity = rb.velocity;
            Vector2 normal = collision.contacts[0].normal;
            Vector2 reflectDir = Vector2.Reflect(inDirection, normal);
            rb.velocity = reflectDir * inVelocity.magnitude * ricochetDamping;
            // If speed is already too low, destroy immediately (optional quick kill)
            if (rb.velocity.magnitude < minSpeedToSurvive)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            // Not ground: destroy immediately
            Destroy(gameObject);
        }
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject, 1f); // Cleanup if off screen
    }
}
