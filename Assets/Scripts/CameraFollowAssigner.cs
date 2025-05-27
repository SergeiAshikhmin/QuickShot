// CameraFollowAssigner.cs
using UnityEngine;
using Cinemachine;

public class CameraFollowAssigner : MonoBehaviour
{
    public static CameraFollowAssigner Instance { get; private set; }

    public CinemachineVirtualCamera virtualCamera;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        // Optional: DontDestroyOnLoad(this.gameObject); // If you want to persist across scenes
    }

    public void AssignFollowToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (virtualCamera != null && player != null)
        {
            virtualCamera.Follow = player.transform;
        }
    }
}
