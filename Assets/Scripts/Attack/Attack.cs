using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    [Header("References")]
    public GameObject arrowPrefab;
    public Transform firePoint;

    [Header("Settings")]
    public float shootForce = 10f;
    public float shootCooldown = 0.5f;
    public float pushbackForce = 5f;

    [Header("Testing")]
    public bool useVelocityPush = false;

    private Rigidbody2D playerRB;
    private float lastShotTime;

    private void Awake()
    {
        // Find the player GameObject on the Player layer
        FindPlayerRB();
    }

    void Update()
    {
        // --- PAUSE LOCKOUT ---
        if (Time.timeScale == 0f)
            return;

        // Always re-find playerRB if lost (respawn case)
        if (playerRB == null)
            FindPlayerRB();

        // Aim toward mouse
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Shoot on click with cooldown
        if (Input.GetMouseButtonDown(0) && Time.time >= lastShotTime + shootCooldown)
        {
            Shoot();
            lastShotTime = Time.time;
        }
    }

    void Shoot()
    {
        // Instantiate and shoot the arrow
        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        rb.AddForce(firePoint.right * shootForce, ForceMode2D.Impulse);

        // Pushback player in the opposite direction
        if (playerRB != null)
        {
            Vector2 pushDirection = -firePoint.right.normalized;
            playerRB.AddForce(pushDirection * pushbackForce, ForceMode2D.Impulse);
        }
    }

    /// Finds and assigns the player Rigidbody2D on the Player layer.
    private void FindPlayerRB()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
        {
            if (go.layer == playerLayer && go.GetComponent<Rigidbody2D>() != null)
            {
                playerRB = go.GetComponent<Rigidbody2D>();
                return;
            }
        }
        if (playerRB == null)
        {
            Debug.LogWarning("No Rigidbody2D found on any GameObject on the Player layer!");
        }
    }
}
