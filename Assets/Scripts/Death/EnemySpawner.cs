using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;
    public int targetEnemyCount = 3;
    public int hitPoints = 3;

    [Header("Spawn Options")]
    public float spawnRadius = 1f;
    public float spawnCooldown = 2f;

    [Header("Visuals")]
    public Sprite normalSprite;
    public GameObject destroyedPrefab;     // Drag your Impact03 prefab here
    public GameObject hitEffectPrefab;     // Drag your HitEffect prefab here

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private SpriteRenderer sr;
    private bool isDestroyed = false;
    private bool canSpawn = true;
    private int projectileLayer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (normalSprite && sr) sr.sprite = normalSprite;
        projectileLayer = LayerMask.NameToLayer("Projectile");
    }

    void Update()
    {
        if (isDestroyed) return;
        spawnedEnemies.RemoveAll(e => e == null);

        if (canSpawn && spawnedEnemies.Count < targetEnemyCount)
        {
            StartCoroutine(SpawnEnemyAfterDelay(spawnCooldown));
        }
    }

    IEnumerator SpawnEnemyAfterDelay(float delay)
    {
        canSpawn = false;
        yield return new WaitForSeconds(delay);
        SpawnEnemy();
        canSpawn = true;
    }

    void SpawnEnemy()
    {
        if (!enemyPrefab || isDestroyed) return;
        Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        spawnedEnemies.Add(enemy);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDestroyed) return;
        if (collision.gameObject.layer == projectileLayer)
        {
            Destroy(collision.gameObject);

            hitPoints--;

            if (hitPoints > 0)
            {
                // Spawn a hit effect at the point of contact
                if (hitEffectPrefab)
                    Instantiate(hitEffectPrefab, collision.contacts[0].point, Quaternion.identity);
            }
            else if (hitPoints == 0)
            {
                StartCoroutine(ExplodeAndDestroy());
            }
        }
    }

    IEnumerator ExplodeAndDestroy()
    {
        isDestroyed = true;

        // Hide the spawner visually before destroy (optional)
        if (sr) sr.enabled = false;

        // Instantiate the destroyed prefab at this position/rotation
        if (destroyedPrefab)
            Instantiate(destroyedPrefab, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.5f); // Let the prefab play effect if needed

        Destroy(gameObject);
    }
}
