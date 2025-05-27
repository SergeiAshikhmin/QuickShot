using UnityEngine;

public class UpgradedBackgroundController : MonoBehaviour
{
    [Header("Camera to Follow")]
    public Camera targetCamera;  // Assign your Main Camera here, or leave empty for auto

    [Header("Offset (optional)")]
    public Vector3 offset = Vector3.zero; // Use this to adjust Z-depth, e.g. (0,0,10)

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;
        // Keep background centered on the camera, preserving original z or using offset.z if set
        Vector3 camPos = targetCamera.transform.position;
        transform.position = new Vector3(
            camPos.x + offset.x,
            camPos.y + offset.y,
            (offset == Vector3.zero) ? transform.position.z : camPos.z + offset.z
        );
    }
}
