using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{

    [Header("References")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public GameObject playerPrefab;

    [Header("Settings")] 
    public float shootForce = 10f;
    public float shootCooldown = 0.5f;
    public float pushbackForce = 5;
    
    private Rigidbody2D playerRB;

    private void Awake()
    {
        playerRB = playerPrefab.GetComponent<Rigidbody2D>();
    }


    // Update is called once per frame
    void Update()
    {
        // Aim toward mouse
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Shoot on click with cooldown
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // Instantiate and shoot the arrow
        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        rb.AddForce(firePoint.right * shootForce, ForceMode2D.Impulse);
        
        // Pushback player in the opposite direction
        Vector2 pushDirection = -firePoint.right.normalized;
        playerRB.AddForce(pushDirection * pushbackForce, ForceMode2D.Impulse);
    }
}
