using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [Header("Effects")]
    public GameObject deathEffectPrefab;

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

        // Tell the checkpoint to spawn a new player
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
}
