using UnityEngine;
using Cinemachine;

public class CameraZoneManager : MonoBehaviour
{
    public static CameraZoneManager Instance;

    // (Optional) Assign your default/follow camera here
    public CinemachineVirtualCamera defaultCamera;

    private CinemachineVirtualCamera currentActiveCamera;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Called by zones
    public void SwitchToCamera(CinemachineVirtualCamera newCamera)
    {
        if (currentActiveCamera != null)
            currentActiveCamera.Priority = 10;

        currentActiveCamera = newCamera;
        if (currentActiveCamera != null)
            currentActiveCamera.Priority = 20;
    }

    // Optionally call this to return to the default cam
    public void SwitchToDefaultCamera()
    {
        if (currentActiveCamera != null)
            currentActiveCamera.Priority = 10;
        if (defaultCamera != null)
            defaultCamera.Priority = 20;

        currentActiveCamera = defaultCamera;
    }
}
