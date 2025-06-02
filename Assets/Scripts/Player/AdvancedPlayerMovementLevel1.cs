using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedPlayerMovementLevel1 : MonoBehaviour
{

    [Header("Movement")]
    public float speed = 5f;

    [Header("Advanced Movement")]
    public float acceleration = 20f;
    public float deceleration = 40f;
    public float maxSpeed = 7f;

    [Header("Jump Tuning")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float jumpForce = 10f;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;
    public float coyoteTime = 0.1f;
    private float coyoteTimeCounter;

    [Header("Wall Check")]
    public Vector2 wallBoxSize = new Vector2(0.1f, 1f); // Width x Height
    private bool isTouchingWall;
    public float yOffset = 0.65f;

    private bool jumpRequested;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the player is grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        // Get horizontal input (-1 for left, 1 for right)
        moveInput = Input.GetAxis("Horizontal");

        if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0)
            jumpRequested = true;
        
        // Wall check using two raycasts (upper and lower)
        float xOffset = 0.5f * Mathf.Sign(moveInput);
        Vector2 rayOriginTop = transform.position + new Vector3(xOffset, yOffset);
        Vector2 rayOriginBottom = transform.position + new Vector3(xOffset, -yOffset);
        Vector2 rayDirection = Vector2.right * Mathf.Sign(moveInput);
        
        bool hitTop = Physics2D.Raycast(rayOriginTop, rayDirection, 0.1f, groundLayer);
        bool hitBottom = Physics2D.Raycast(rayOriginBottom, rayDirection, 0.1f, groundLayer);
        
        isTouchingWall = hitTop || hitBottom;
        
        Debug.DrawRay(rayOriginTop, rayDirection * 0.1f, Color.red);
        Debug.DrawRay(rayOriginBottom, rayDirection * 0.1f, Color.red);
    }

    void FixedUpdate()
    {
        if (jumpRequested) {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpRequested = false;
        }
        
        float targetSpeed = moveInput * maxSpeed;

        // Use deceleration when input opposes current velocity, otherwise use acceleration/braking rates
        float rate;
        if (Mathf.Abs(moveInput) > 0.01f) {
            // If changing direction, brake harder
            if (rb.velocity.x != 0f && Mathf.Sign(moveInput) != Mathf.Sign(rb.velocity.x))
                rate = deceleration;
            else
                rate = acceleration;
        } else {
            // No input: decelerate
            rate = deceleration;
        }

        // Move velocity towards target speed
        float newX = Mathf.MoveTowards(rb.velocity.x, targetSpeed, rate * Time.fixedDeltaTime);
        rb.velocity = new Vector2(newX, rb.velocity.y);
        
        // Apply extra gravity for better jump feel, even on the way up
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime);
        }
        else if (rb.velocity.y > 0)
        {
            rb.velocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1) * lowJumpMultiplier * Time.deltaTime);
        }
    }
}
