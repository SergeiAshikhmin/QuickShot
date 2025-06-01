using UnityEngine;
using Cinemachine;

public class CameraZone : MonoBehaviour
{
    [Header("Zone Cameras")]
    public CinemachineVirtualCamera cameraFromLeft;
    public CinemachineVirtualCamera cameraFromRight;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        float playerX = other.transform.position.x;
        float zoneX = transform.position.x;

        if (playerX < zoneX)
        {
            // Entering from the left
            if (cameraFromLeft != null)
                CameraZoneManager.Instance.SwitchToCamera(cameraFromLeft);
        }
        else
        {
            // Entering from the right
            if (cameraFromRight != null)
                CameraZoneManager.Instance.SwitchToCamera(cameraFromRight);
        }
    }

    // No need for OnTriggerExit2D unless handling exit behavior
}
