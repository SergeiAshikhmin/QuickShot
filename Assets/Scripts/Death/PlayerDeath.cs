using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [Header("Effects")]
    public GameObject deathEffectPrefab;

    [Header("Death Pulse Settings")]
    public float killRadius = 3f;
    public float pulseDuration = 0.1f;
    public GameObject pulsePrefab; // Assign a prefab with CircleCollider2D (Trigger) on KillEnemy layer

    private bool isDead = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isDead && collision.gameObject.layer == LayerMask.NameToLayer("Death"))
        {
            Die();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDead && other.gameObject.layer == LayerMask.NameToLayer("Death"))
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (deathEffectPrefab != null)
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        EmitKillPulse(); // 💥 Launch the kill pulse

        if (Checkpoint.activeCheckpoint != null)
        {
            Checkpoint.activeCheckpoint.SpawnPlayer();
        }
        else
        {
            Debug.LogWarning("No active checkpoint to respawn the player!");
        }

        Destroy(gameObject);
    }

    void EmitKillPulse()
    {
        if (pulsePrefab == null)
        {
            Debug.LogWarning("Pulse prefab not assigned!");
            return;
        }

        GameObject pulse = Instantiate(pulsePrefab, transform.position, Quaternion.identity);
        CircleCollider2D circle = pulse.GetComponent<CircleCollider2D>();
        if (circle != null)
        {
            circle.radius = killRadius;
        }

        Destroy(pulse, pulseDuration);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, killRadius);
    }
}
