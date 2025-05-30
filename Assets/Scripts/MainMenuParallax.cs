using UnityEngine;

public class MainMenuParallax : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public RectTransform layer;
        public float movementMultiplier = 0.05f;

        [HideInInspector]
        public Vector2 initialPosition;
    }

    public ParallaxLayer[] layers;

    private Vector2 screenCenter;

    void Start()
    {
        screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        // Cache initial positions
        foreach (var layer in layers)
        {
            if (layer.layer != null)
                layer.initialPosition = layer.layer.anchoredPosition;
        }
    }

    void Update()
    {
        Vector2 mouseDelta = (Vector2)Input.mousePosition - screenCenter;

        foreach (var layer in layers)
        {
            if (layer.layer != null)
            {
                Vector2 offset = mouseDelta * layer.movementMultiplier;
                layer.layer.anchoredPosition = layer.initialPosition + offset;
            }
        }
    }
}
