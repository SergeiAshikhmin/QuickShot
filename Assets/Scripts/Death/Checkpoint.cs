using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public static Checkpoint activeCheckpoint = null;

    [Header("Visuals")]
    public Sprite inactiveSprite;
    public Sprite activeSprite;

    [Header("Player Respawn")]
    public GameObject playerPrefab; // Assign your Player prefab in the Inspector

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            SetAsActive();
        }
    }

    public void SetAsActive()
    {
        // Deactivate previous checkpoint
        if (activeCheckpoint != null && activeCheckpoint != this)
            activeCheckpoint.SetActive(false);

        activeCheckpoint = this;
        SetActive(true);
    }

    private void SetActive(bool isActive)
    {
        if (sr != null)
            sr.sprite = isActive ? activeSprite : inactiveSprite;
    }

    public void SpawnPlayer()
    {
        if (playerPrefab != null)
        {
            GameObject player = Instantiate(playerPrefab, transform.position, Quaternion.identity);

            // Assign the camera to follow the new player
            if (CameraFollowAssigner.Instance != null)
                CameraFollowAssigner.Instance.AssignFollowToPlayer();
        }
        else
        {
            Debug.LogError("No player prefab assigned to this checkpoint!");
        }
    }
}
