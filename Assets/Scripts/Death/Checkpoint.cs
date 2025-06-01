using UnityEngine;
using System.Collections.Generic;

public class Checkpoint : MonoBehaviour
{
    public static Checkpoint activeCheckpoint = null;

    [Header("Visuals")]
    public Sprite inactiveSprite;
    public Sprite activeSprite;

    [Header("Player Respawn")]
    public GameObject playerPrefab;

    private SpriteRenderer sr;

    // Internal saved scene snapshot
    private List<SavedObjectData> savedSceneState = new();

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
        if (activeCheckpoint != null && activeCheckpoint != this)
            activeCheckpoint.SetActive(false);

        activeCheckpoint = this;
        SetActive(true);
        SaveSceneState();
    }

    private void SetActive(bool isActive)
    {
        if (sr != null)
            sr.sprite = isActive ? activeSprite : inactiveSprite;
    }

    public void SpawnPlayer()
    {
        RestoreSceneState();

        if (playerPrefab != null)
        {
            GameObject player = Instantiate(playerPrefab, transform.position, Quaternion.identity);

            if (CameraFollowAssigner.Instance != null)
                CameraFollowAssigner.Instance.AssignFollowToPlayer();
        }
        else
        {
            Debug.LogError("No player prefab assigned to this checkpoint!");
        }
    }

    // === Scene Saving Logic ===
    private void SaveSceneState()
    {
        savedSceneState.Clear();

        SceneSaveable[] saveables = FindObjectsOfType<SceneSaveable>();
        foreach (var obj in saveables)
        {
            if (obj.prefabSource == null)
            {
                Debug.LogWarning($"Saveable object {obj.name} missing prefabSource reference.");
                continue;
            }

            savedSceneState.Add(new SavedObjectData
            {
                prefab = obj.prefabSource,
                position = obj.transform.position,
                rotation = obj.transform.rotation
            });
        }

        Debug.Log($"[Checkpoint] Saved {savedSceneState.Count} scene objects.");
    }

    private void RestoreSceneState()
    {
        // Destroy existing saveable objects
        foreach (var obj in FindObjectsOfType<SceneSaveable>())
        {
            Destroy(obj.gameObject);
        }

        // Respawn from saved state
        foreach (var saved in savedSceneState)
        {
            Instantiate(saved.prefab, saved.position, saved.rotation);
        }

        Debug.Log($"[Checkpoint] Restored {savedSceneState.Count} scene objects.");
    }

    // Serializable structure for saved object data
    private class SavedObjectData
    {
        public GameObject prefab;
        public Vector3 position;
        public Quaternion rotation;
    }
}
