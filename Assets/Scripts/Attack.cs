using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{

    public GameObject arrowPrefab;
    public Transform firePoint;

    [Header("Charging Settings")] 
    public float minForce = 5f;
    public float maxForce = 10f;
    public float chargeSpeed = 10f;

    private float currentForce;
    private bool isCharging = false;

    // Update is called once per frame
    void Update()
    {
        // Get the mouse position in world space
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // Calculate the direction from the bow to the mouse
        Vector2 direction = mousePos - transform.position;
        
        // Rotate the bow to face the mouse
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (Input.GetMouseButton(0))
        {
            isCharging = true;
            currentForce = minForce;
        }
        
        if (Input.GetMouseButton(0) && isCharging) // While holding
        {
            currentForce += chargeSpeed * Time.deltaTime;
            currentForce = Mathf.Clamp(currentForce, minForce, maxForce);
        }

        if (Input.GetMouseButtonUp(0) && isCharging) // Release to shoot
        {
            Shoot(currentForce);
            isCharging = false;
        }
    }

    void Shoot(float force)
    {
        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = arrow.GetComponent<Rigidbody2D>();
        rb.AddForce(firePoint.right * force, ForceMode2D.Impulse);
    }
}
