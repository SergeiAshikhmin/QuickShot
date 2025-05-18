using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
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
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);

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

    void FixedUpdate()
    {
        // Check if the player is grounded
        // isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        float targetSpeed = moveInput * maxSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = accelRate * speedDiff;

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

        if (!isTouchingWall)
            rb.AddForce(Vector2.right * movement);

        // Limit speed
        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
    }
}
