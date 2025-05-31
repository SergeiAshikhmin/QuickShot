using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LaserBullet : MonoBehaviour
{
    public float lifetime = 5f;
    public LayerMask destroyOnContact;

    void Start()
    {
        Destroy(gameObject, lifetime); // fallback
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & destroyOnContact) != 0)
        {
            Destroy(gameObject);
        }
    }
}
