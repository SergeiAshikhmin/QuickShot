using UnityEngine;

public class RifleBullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 2f;
    private float timer = 0f;
    private Vector2 direction;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        timer += Time.deltaTime;
        if (timer >= lifetime)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") || other.CompareTag("Player"))
            Destroy(gameObject);
    }
}
