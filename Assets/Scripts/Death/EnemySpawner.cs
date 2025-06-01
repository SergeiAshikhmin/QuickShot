using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyType
    {
        public GameObject prefab;
        public int spawnThreshold = 3;
        public float spawnRateSeconds = 2f;
        public Transform spawnPoint; // Assign manually in Inspector
        [HideInInspector] public List<GameObject> spawnedEnemies = new();
    }

    [Header("Enemy Types")]
    public List<EnemyType> enemyTypes = new();

    [Header("Animation")]
    public float floatDownSpeed = 1.5f;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.2f;

    [Header("Spawner Health")]
    public int hitPoints = 3;

    [Header("Visuals")]
    public Sprite normalSprite;
    public GameObject destroyedPrefab;
    public GameObject hitEffectPrefab;

    private SpriteRenderer sr;
    private bool isDestroyed = false;
    private int projectileLayer;
    private bool hasLanded = false;
    private Vector3 landedPosition;
    private Color baseColor = Color.white;
    private int maxHealth;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (normalSprite && sr) sr.sprite = normalSprite;
        projectileLayer = LayerMask.NameToLayer("Projectile");
        maxHealth = hitPoints;
    }

    void Start()
    {
        StartCoroutine(FloatDownUntilGrounded());
        StartCoroutine(UpdateHurtFlash());
    }

    void Update()
    {
        if (isDestroyed || !hasLanded) return;
        transform.position = landedPosition;
    }

    IEnumerator FloatDownUntilGrounded()
    {
        while (!CheckIfGrounded())
        {
            transform.position += Vector3.down * floatDownSpeed * Time.deltaTime;
            yield return null;
        }

        hasLanded = true;
        landedPosition = transform.position;

        BeginSpawningFromAssignedPoints();
    }

    bool CheckIfGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        Debug.DrawRay(transform.position, Vector2.down * groundCheckDistance, hit.collider ? Color.green : Color.red, 1.0f);
        return hit.collider != null;
    }

    void BeginSpawningFromAssignedPoints()
    {
        foreach (var type in enemyTypes)
        {
            if (type.spawnPoint == null)
            {
                Debug.LogWarning($"[{name}] Enemy type {type.prefab.name} is missing a spawn point!");
                continue;
            }

            StartCoroutine(SpawnLoop(type));
        }
    }

    IEnumerator SpawnLoop(EnemyType type)
    {
        while (!isDestroyed)
        {
            type.spawnedEnemies.RemoveAll(e => e == null);

            if (type.spawnedEnemies.Count < type.spawnThreshold)
            {
                GameObject enemy = Instantiate(type.prefab, type.spawnPoint.position, Quaternion.identity);
                type.spawnedEnemies.Add(enemy);
            }

            yield return new WaitForSeconds(type.spawnRateSeconds);
        }
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
                if (hitEffectPrefab)
                    Instantiate(hitEffectPrefab, collision.contacts[0].point, Quaternion.identity);
            }
            else
            {
                StartCoroutine(ExplodeAndDestroy());
            }
        }
    }

    IEnumerator ExplodeAndDestroy()
    {
        isDestroyed = true;

        if (sr) sr.enabled = false;

        if (destroyedPrefab)
            Instantiate(destroyedPrefab, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    IEnumerator UpdateHurtFlash()
    {
        while (true)
        {
            if (sr != null && hitPoints < maxHealth && hitPoints > 0 && !isDestroyed)
            {
                float zone = (float)(maxHealth - hitPoints) / maxHealth;
                float pulseSpeed = Mathf.Lerp(1.5f, 10f, zone);
                float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                Color flashColor = Color.Lerp(baseColor, Color.red, t);
                sr.color = flashColor;
            }
            else if (sr != null)
            {
                sr.color = baseColor;
            }

            yield return null;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        foreach (var type in enemyTypes)
        {
            if (type.spawnPoint != null)
                Gizmos.DrawWireSphere(type.spawnPoint.position, 0.3f);
        }
    }
}
