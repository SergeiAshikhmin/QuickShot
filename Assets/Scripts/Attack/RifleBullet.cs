using UnityEngine;

public class RifleBullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 2f;
    private float timer = 0f;
    private Vector2 direction;
    public int maxRicochets = 3; // Optional: limit ricochets
    private int ricochets = 0;
    public float minSpeed = 0.5f; // Destroy if speed drops below this

    public LayerMask groundLayer; // Set this in the Inspector to your Ground layer

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        // Move
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        timer += Time.deltaTime;
        if (timer >= lifetime)
            Destroy(gameObject);

        // Destroy if nearly stopped
        if ((direction * speed).magnitude < minSpeed)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Hit player
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") || other.CompareTag("Player"))
        {
            Destroy(gameObject);
            return;
        }

        // Ricochet logic for ground
if (((1 << other.gameObject.layer) & groundLayer.value) != 0)
{
    Vector2 bulletPos = (Vector2)transform.position;
    Vector2 collisionNormal = (bulletPos - other.ClosestPoint(bulletPos)).normalized;
    direction = Vector2.Reflect(direction, collisionNormal);

    ricochets++;
    if (ricochets > maxRicochets)
    {
        Destroy(gameObject);
    }
}

    }
}
