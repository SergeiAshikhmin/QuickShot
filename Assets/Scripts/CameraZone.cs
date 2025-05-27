using UnityEngine;
using Cinemachine;

public class CameraZone : MonoBehaviour
{
    public CinemachineVirtualCamera zoneCamera;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Tell the manager to switch to this zone camera
            CameraZoneManager.Instance.SwitchToCamera(zoneCamera);
        }
    }

    // REMOVE OnTriggerExit2D, or leave it empty.
}
