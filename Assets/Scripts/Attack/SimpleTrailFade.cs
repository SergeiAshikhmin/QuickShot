using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class SimpleTrailFade : MonoBehaviour
{
    public Color trailColor = Color.yellow; // Pick your trail color

    void Awake()
    {
        TrailRenderer tr = GetComponent<TrailRenderer>();

        // Build a gradient that fades from solid to transparent
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(trailColor, 0.0f),
                new GradientColorKey(trailColor, 1.0f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1.0f, 0.0f), // Fully opaque at the start
                new GradientAlphaKey(0.0f, 1.0f)  // Fully transparent at the end
            }
        );
        tr.colorGradient = gradient;
    }
}
