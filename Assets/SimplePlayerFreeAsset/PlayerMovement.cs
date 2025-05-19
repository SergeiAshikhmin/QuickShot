using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    float horizontalInput;
    float moveSpeed = 10f;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    private void FixedUpdate()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        spriteRenderer.flipX = horizontalInput > 0;
        rb.velocity = new Vector2(horizontalInput * moveSpeed, rb.velocity.y);
    }
}
