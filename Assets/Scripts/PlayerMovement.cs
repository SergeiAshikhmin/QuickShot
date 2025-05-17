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

    public float jumpForce = 10f;
    
    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;

    [Header("Ground Check")] 
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get horizontal input (-1 for left, 1 for right)
        moveInput = Input.GetAxis("Horizontal");
        
        if (Input.GetButtonDown("Jump") && isGrounded)
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    void FixedUpdate()
    {
        // Check if the player is grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        float targetSpeed = moveInput * maxSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = accelRate * speedDiff;

        rb.AddForce(Vector2.right * movement);

        // Limit speed
        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
    }
}
