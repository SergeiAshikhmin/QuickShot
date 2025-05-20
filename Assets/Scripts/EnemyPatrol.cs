using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float checkDistance = 0.6f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool movingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // Move
        rb.velocity = new Vector2((movingRight ? 1 : -1) * moveSpeed, rb.velocity.y);

        // Calculate a raycast point ahead of the enemy
        Vector2 rayOrigin = new Vector2(
            transform.position.x + (movingRight ? 0.4f : -0.4f), // slightly ahead of center
            transform.position.y - 0.5f                           // slightly below center
        );

        // Perform the raycast
        RaycastHit2D groundHit = Physics2D.Raycast(rayOrigin, Vector2.down, checkDistance, groundLayer);
        Debug.DrawRay(rayOrigin, Vector2.down * checkDistance, Color.red);

        // If there's no ground ahead and not jumping/falling too fast, flip
        if (!groundHit.collider && Mathf.Abs(rb.velocity.y) < 0.1f)
        {
            Flip();
        }
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
