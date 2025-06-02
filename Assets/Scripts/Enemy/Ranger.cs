using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ranger : MonoBehaviour
{
    [Header("References")]
    public GameObject enemyArrowPrefab;
    public Transform firePoint;      // empty child placed at the bow’s tip
    Transform player;                // cached on Awake
    
    [Header("Behaviour")]
    public float aggroRange     = 8f;    // start shooting when player is inside
    public float shootCooldown  = 1.5f;  // seconds between arrows
    public float arrowSpeed     = 12f;
    public float aimTurnSpeed   = 720f;  // deg/sec the sprite can rotate (optional)

    float cooldown;                      

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    void Update()
    {
        if (!player) return;

        Vector2 toPlayer = player.position - transform.position;
        float   distance = toPlayer.magnitude;

        if (distance <= aggroRange)
        {
            Debug.Log("Player is in range!");
            AimAt(toPlayer);

            if (cooldown <= 0f)
            {
                Shoot(toPlayer);
                cooldown = shootCooldown;
            }
        }

        cooldown -= Time.deltaTime;
    }
    
    void AimAt(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.Euler(0, 0, angle);
        // Smooth rotate so the bow “tracks” the player
        firePoint.rotation = Quaternion.RotateTowards(firePoint.rotation, targetRot, 
            aimTurnSpeed * Time.deltaTime);
    }

    void Shoot(Vector2 dir)
    {
        GameObject arrow = Instantiate(enemyArrowPrefab, firePoint.position, firePoint.rotation);

        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        rb.gravityScale = 1f;                       // <-- turn gravity back on

        // add a constant upward boost so it arcs
        Vector2 launch = dir.normalized * arrowSpeed + Vector2.up * 3f;
        rb.velocity = launch;
        
        #if UNITY_EDITOR
        Debug.DrawRay(firePoint.position, dir.normalized * 0.5f, Color.red, 0.5f);
        #endif
    }
    
    
}
