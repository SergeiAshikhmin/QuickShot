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
    public Sprite hitSprite;
    public GameObject destroyedPrefab;  // Drag your Impact03 prefab here

    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private SpriteRenderer sr;
    private bool isDestroyed = false;
    private bool canSpawn = true;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (normalSprite && sr) sr.sprite = normalSprite;
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

    void OnTriggerEnter2D(Collider2D other)
{
    if (isDestroyed) return;
    if (other.CompareTag("Projectile"))
    {
        // Disable the projectile collider immediately so only one hit is counted
        Collider2D projCol = other.GetComponent<Collider2D>();
        if (projCol) projCol.enabled = false;

        Destroy(other.gameObject);

        hitPoints--;

        if (hitPoints > 0)
        {
            StartCoroutine(FlashHit());
        }
        else if (hitPoints == 0)
        {
            StartCoroutine(ExplodeAndDestroy());
        }
    }
}


    IEnumerator FlashHit()
    {
        if (hitSprite && sr) sr.sprite = hitSprite;
        yield return new WaitForSeconds(0.2f);
        if (normalSprite && sr && !isDestroyed) sr.sprite = normalSprite;
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
