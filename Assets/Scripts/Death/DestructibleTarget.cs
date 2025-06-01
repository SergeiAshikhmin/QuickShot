using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DestructibleTarget : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Damage Feedback")]
    public Sprite hitEffectSprite;
    public float hitEffectDuration = 0.08f;
    public GameObject impactPrefab;

    private SpriteRenderer sr;
    private Color baseColor = Color.white;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        StartCoroutine(UpdateHurtFlash());
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            HandleHit(other.ClosestPoint(transform.position));
            Destroy(other.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Projectile"))
        {
            Vector2 impactPos = collision.contacts.Length > 0 ? collision.contacts[0].point : (Vector2)transform.position;
            HandleHit(impactPos);
            Destroy(collision.gameObject);
        }
    }

    void HandleHit(Vector2 impactPoint)
    {
        if (hitEffectSprite != null)
        {
            GameObject hitObj = new GameObject("HitEffect");
            hitObj.transform.position = impactPoint;
            var spriteR = hitObj.AddComponent<SpriteRenderer>();
            spriteR.sprite = hitEffectSprite;
            spriteR.sortingOrder = 999;
            Destroy(hitObj, hitEffectDuration);
        }

        currentHealth--;
        if (currentHealth <= 0)
        {
            Die(impactPoint);
        }
    }

    void Die(Vector2 impactPoint)
    {
        if (impactPrefab)
            Instantiate(impactPrefab, impactPoint, Quaternion.identity);
        
        Destroy(gameObject); // triggers event completion in SecondEvent
    }

    IEnumerator UpdateHurtFlash()
    {
        while (true)
        {
            if (sr != null && currentHealth < maxHealth)
            {
                float t = Mathf.PingPong(Time.time * 4f, 1f);
                float damageRatio = 1f - ((float)currentHealth / maxHealth);
                Color flashColor = Color.Lerp(baseColor, Color.red, t * damageRatio);
                sr.color = flashColor;
            }
            else if (sr != null)
            {
                sr.color = baseColor;
            }

            yield return null;
        }
    }
}
