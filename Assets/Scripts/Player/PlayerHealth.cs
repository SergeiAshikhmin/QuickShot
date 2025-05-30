using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;
    [Tooltip("How many flashes (red ↔︎ white) per hit")]
    public int flashCount = 3;
    [Tooltip("Seconds each color stays on screen")]
    public float flashInterval = 0.1f;
    
    int currentHealth;
    SpriteRenderer[] renderers; // allow for children with their own sprites
    Color originalColor;
    bool isInvincible = false;
    void Awake()
    {
        currentHealth = maxHealth;
        renderers = GetComponentsInChildren<SpriteRenderer>();
        originalColor = renderers[0].color;          // assumes they all share the same tint
    }
    
    /// <summary>Called from EnemyController when the player gets hurt.</summary>
    public void TakeDamage(int amount)
    {
        
        if (isInvincible) return;
        isInvincible = true;
        
        CameraShake.Instance?.Shake();
        
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            // TODO: trigger death sequence here
            Debug.Log("Player died");
        }

        StopAllCoroutines();          // cancel any previous flash
        StartCoroutine(Flash());
    }
    
    IEnumerator Flash()
    {
        for (int i = 0; i < flashCount; i++)
        {
            SetTint(Color.red);
            yield return new WaitForSeconds(flashInterval);
            SetTint(Color.white);
            yield return new WaitForSeconds(flashInterval);
        }
        SetTint(originalColor);       // restore
        isInvincible = false;
    }

    void SetTint(Color c)
    {
        foreach (var r in renderers) r.color = c;
    }
    
}
