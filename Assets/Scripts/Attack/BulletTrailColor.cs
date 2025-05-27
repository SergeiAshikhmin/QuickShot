using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class BulletTrailColor : MonoBehaviour
{
    public Color slowColor = Color.yellow;
    public Color fastColor = Color.red;
    public float maxSpeed = 20f;  // Set to the max speed your bullet reaches

    private TrailRenderer tr;
    private Rigidbody2D rb;

    void Awake()
    {
        tr = GetComponent<TrailRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float speed = rb.velocity.magnitude;
        // Interpolate color based on speed
        Color trailColor = Color.Lerp(slowColor, fastColor, Mathf.Clamp01(speed / maxSpeed));
        tr.startColor = trailColor;
        tr.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f); // fade out to transparent
    }
}
