using UnityEngine;

public class TimeDestroy : MonoBehaviour
{
    public float time = 0.2f;
    void Start() => Destroy(gameObject, time);
}
