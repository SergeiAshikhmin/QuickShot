using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float speed = 2f;                // Bullet speed, set by enemy or in prefab
    public float lifetime = 5f;             // Seconds before auto-destroy (editable in Inspector)
    public GameObject impactPrefab;         // Assign Impact02.prefab in Inspector

    private float timer = 0f;
    private Vector2 direction;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        // Move bullet
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        // Increment timer and destroy if over lifetime
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            PlayImpactAndDestroy();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Destroy if colliding with something on Player layer or tagged Player
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") || other.CompareTag("Player"))
        {
            PlayImpactAndDestroy();
        }
    }

    void PlayImpactAndDestroy()
    {
        if (impactPrefab)
        {
            Instantiate(impactPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
