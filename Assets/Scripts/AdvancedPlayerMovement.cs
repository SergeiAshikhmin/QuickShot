using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedPlayerMovement : MonoBehaviour
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
    public Vector2 wallBoxSize = new Vector2(0.1f, 1f);
    private bool isTouchingWall;
    public float yOffset = 0.65f;

    private bool jumpRequested;

    [Header("Animator")]
    private Animator animator;

    [Header("Weapon")]
    public Transform weaponTransform;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        coyoteTimeCounter = isGrounded ? coyoteTime : coyoteTimeCounter - Time.deltaTime;

        // Movement input
        moveInput = Input.GetAxis("Horizontal");

        // Flip player sprite based on direction
        Vector3 s = transform.localScale;
        if (moveInput < 0)
            s.x = -Mathf.Abs(s.x);
        else if (moveInput > 0)
            s.x = Mathf.Abs(s.x);
        transform.localScale = s;

        // Animate
        animator.SetBool("IsWalking", Mathf.Abs(moveInput) > 0.01f);

        // Jump
        if (Input.GetButtonDown("Jump") && coyoteTimeCounter > 0)
            jumpRequested = true;

        // Wall check
        float xOffset = 0.5f * Mathf.Sign(moveInput);
        Vector2 rayOriginTop = transform.position + new Vector3(xOffset, yOffset);
        Vector2 rayOriginBottom = transform.position + new Vector3(xOffset, -yOffset);
        Vector2 rayDirection = Vector2.right * Mathf.Sign(moveInput);
        bool hitTop = Physics2D.Raycast(rayOriginTop, rayDirection, 0.1f, groundLayer);
        bool hitBottom = Physics2D.Raycast(rayOriginBottom, rayDirection, 0.1f, groundLayer);
        isTouchingWall = hitTop || hitBottom;
        Debug.DrawRay(rayOriginTop, rayDirection * 0.1f, Color.red);
        Debug.DrawRay(rayOriginBottom, rayDirection * 0.1f, Color.red);

        // ✅ Weapon flip compensation + mouse aim
        if (weaponTransform != null)
        {
            // 1. Aim at mouse
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 aimDir = mousePos - weaponTransform.position;
            float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
            weaponTransform.rotation = Quaternion.Euler(0f, 0f, angle);

            // 2. Flip weapon back if player is flipped
            Vector3 ws = weaponTransform.localScale;
            ws.x = (transform.localScale.x < 0) ? -1f : 1f;
            ws.y = 1f; // ensure upright
            weaponTransform.localScale = ws;
        }
    }

    void FixedUpdate()
    {
        if (jumpRequested)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpRequested = false;
        }

        float targetSpeed = moveInput * maxSpeed;
        float rate;

        if (Mathf.Abs(moveInput) > 0.01f)
        {
            if (rb.velocity.x != 0f && Mathf.Sign(moveInput) != Mathf.Sign(rb.velocity.x))
                rate = deceleration;
            else
                rate = acceleration;
        }
        else
        {
            rate = deceleration;
        }

        float newX = Mathf.MoveTowards(rb.velocity.x, targetSpeed, rate * Time.fixedDeltaTime);
        rb.velocity = new Vector2(newX, rb.velocity.y);

        // Better jump fall
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime);
        }
        else if (rb.velocity.y > 0)
        {
            rb.velocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1) * lowJumpMultiplier * Time.deltaTime);
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"Velocity: {rb.velocity}");
        GUI.Label(new Rect(10, 30, 300, 20), $"Speed: {rb.velocity.magnitude:F2}");
    }
}
