using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float jumpForce = 7f;

    [Header("Gap Detection Settings")]
    public float checkDistance = 0.6f;
    public float jumpRange = 2f;
    public float maxJumpHeight = 2f;
    public float maxDropHeight = 2.5f;

    [Header("Ground Check Settings")]
    public float groundRayLength = 0.1f;

    [Header("Environment Layers")]
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool movingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // Move horizontally
        rb.velocity = new Vector2((movingRight ? 1 : -1) * moveSpeed, rb.velocity.y);

        Vector2 origin = transform.position;
        Vector2 dir = movingRight ? Vector2.right : Vector2.left;

        // Step 0: Grounded check
        Vector2 groundCheckOrigin = origin + Vector2.down * 0.5f;
        RaycastHit2D groundedHit = Physics2D.Raycast(groundCheckOrigin, Vector2.down, groundRayLength, groundLayer);
        Debug.DrawRay(groundCheckOrigin, Vector2.down * groundRayLength, Color.yellow);
        bool isGrounded = groundedHit.collider != null;

        if (!isGrounded)
            return;

        // Step 1: Check for gap in front
        Vector2 gapCheckOrigin = origin + new Vector2(dir.x * 0.4f, -0.5f);
        RaycastHit2D groundHit = Physics2D.Raycast(gapCheckOrigin, Vector2.down, checkDistance, groundLayer);
        Debug.DrawRay(gapCheckOrigin, Vector2.down * checkDistance, Color.red);

        if (!groundHit.collider)
        {
            // Step 2a: Check up-forward for a platform
            Vector2 jumpUpOrigin = origin + new Vector2(dir.x * 0.6f, 0f);
            Vector2 jumpUpDirection = new Vector2(dir.x, 1f).normalized;
            RaycastHit2D jumpUpHit = Physics2D.Raycast(jumpUpOrigin, jumpUpDirection, jumpRange, groundLayer);
            Debug.DrawRay(jumpUpOrigin, jumpUpDirection * jumpRange, Color.green);

            if (jumpUpHit.collider && jumpUpHit.point.y - origin.y <= maxJumpHeight)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                return;
            }

            // Step 2b: Check ahead using OverlapBox for a reachable platform
            Vector2 boxCenter = origin + new Vector2(dir.x * (jumpRange * 0.75f), -0.5f);
            Vector2 boxSize = new Vector2(1f, 1f); // Width and height of the detection box
            Collider2D platformAhead = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundLayer);
            Debug.DrawLine(boxCenter - boxSize * 0.5f, boxCenter + new Vector2(boxSize.x, -boxSize.y), Color.magenta);
            Debug.DrawLine(boxCenter - boxSize * 0.5f, boxCenter + new Vector2(-boxSize.x, boxSize.y), Color.magenta);

            if (platformAhead != null)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                return;
            }

            // Step 3: Check forward-down for a lower platform
            Vector2 dropOrigin = origin + new Vector2(dir.x * 0.6f, 0f);
            Vector2 dropDirection = new Vector2(dir.x, -1f).normalized;
            RaycastHit2D dropHit = Physics2D.Raycast(dropOrigin, dropDirection, maxDropHeight, groundLayer);
            Debug.DrawRay(dropOrigin, dropDirection * maxDropHeight, Color.cyan);

            if (dropHit.collider && origin.y - dropHit.point.y <= maxDropHeight)
            {
                // Let it fall naturally
                return;
            }

            // Step 4: Nothing reachable — flip
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

    // Debug visualization in Scene View
    void OnDrawGizmosSelected()
    {
        Vector2 origin = transform.position;
        Vector2 dir = movingRight ? Vector2.right : Vector2.left;
        Vector2 boxCenter = origin + new Vector2(dir.x * (jumpRange * 0.75f), -0.5f);
        Vector2 boxSize = new Vector2(1f, 1f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
}
