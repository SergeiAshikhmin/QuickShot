using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Audio")]
    [Tooltip("Assign several different hit sounds; a random one plays each hit")]
    [SerializeField] private AudioClip[] hitClips;
    
    AudioSource audioSource;
    int lastClipIndex = -1;
    
    [Header("Stats")]
    [Header("Invincibility")]
    [Tooltip("How long the player stays invincible after a hit (seconds)")]
    public float invincibilityDuration = 1f;
    [Tooltip("Blink interval while invincible (seconds)")]
    public float invincibilityBlinkInterval = 0.12f;
    
    bool isInvincible = false;
    SpriteRenderer[] renderers; // allow for children with their own sprites
    Color originalColor;
    void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
        originalColor = renderers[0].color;          // assumes they all share the same tint
        
        audioSource = GetComponent<AudioSource>();
    }
    
    /// <summary>Called from EnemyController when the player gets hurt.</summary>
    public void TakeDamage(int amount)
    {
        
        if (isInvincible) return;
        isInvincible = true;
        
        // 1. camera jolt
        CameraShake.Instance?.Shake();
        GameManager.Instance.DamagePlayer(amount);
        
        // 2. play sound
        // if (hitClips != null && hitClips.Length > 0)
        // {
        //     int index = Random.Range(0, hitClips.Length);
        //     
        //     // avoid immediate repeat if you have >1 clip
        //     if (hitClips.Length > 1 && index == lastClipIndex) index = (index + 1) % hitClips.Length;
        //     
        //     audioSource.pitch = Random.Range(0.95f, 1.05f); // tiny pitch scatter
        //     audioSource.PlayOneShot(hitClips[index]);
        //     audioSource.pitch = 1f;
        //     
        //     lastClipIndex = index;
        // }
        
        StopAllCoroutines();          // cancel any previous effect
        StartCoroutine(InvincibilityFlash());
    }
    
    IEnumerator InvincibilityFlash()
    {
        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            foreach (var r in renderers) r.enabled = !r.enabled;   // simple blink
            yield return new WaitForSeconds(invincibilityBlinkInterval);
            elapsed += invincibilityBlinkInterval;
        }
        foreach (var r in renderers) r.enabled = true;             // make sure sprites are visible
        isInvincible = false;
    }
    
}
