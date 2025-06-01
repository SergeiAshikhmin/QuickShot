using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallDetector : MonoBehaviour
{
    public Transform respawnPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = respawnPoint.position;
            
            // reset velociyt
            other.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            
            // reset health 
            GameManager.Instance.ResetRun();
            
            Debug.Log("Player fell and respawned.");
        }
    }
}
