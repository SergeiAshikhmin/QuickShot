using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
public class Checkpoint : MonoBehaviour
{
    public static Checkpoint activeCheckpoint = null;

    [Header("Player Respawn")]
    public GameObject playerPrefab;

    private Animator anim;
    private SpriteRenderer sr;

    // Internal saved scene snapshot
    private List<SavedObjectData> savedSceneState = new();

    void Awake()
    {
        anim = GetComponent<Animator>();
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
        if (anim != null)
            anim.SetBool("IsActive", isActive);
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
        foreach (var obj in FindObjectsOfType<SceneSaveable>())
        {
            Destroy(obj.gameObject);
        }

        foreach (var saved in savedSceneState)
        {
            Instantiate(saved.prefab, saved.position, saved.rotation);
        }

        Debug.Log($"[Checkpoint] Restored {savedSceneState.Count} scene objects.");
    }

    private class SavedObjectData
    {
        public GameObject prefab;
        public Vector3 position;
        public Quaternion rotation;
    }
}
