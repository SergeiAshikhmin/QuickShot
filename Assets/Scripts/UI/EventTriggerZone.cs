using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventTriggerZone : MonoBehaviour
{
    [Header("Invisible Wall")]
    public GameObject invisibleWall; // Assign the invisible wall GameObject

    [Header("Enemy Wave Prefabs")]
    public GameObject wave1Prefab;
    public GameObject wave2Prefab;
    public GameObject wave3aPrefab;
    public GameObject wave3bPrefab;

    [Header("Spawn Points")]
    public Transform wave1SpawnPoint;
    public Transform wave2SpawnPoint;
    public Transform wave3aSpawnPoint;
    public Transform wave3bSpawnPoint;

    [Header("Final Spawn After Combat")]
    public GameObject finalPrefab;
    public Transform finalSpawnPoint;

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;

            if (invisibleWall != null)
                invisibleWall.SetActive(true);

            StartCoroutine(HandleCombatSequence());
        }
    }

    IEnumerator HandleCombatSequence()
    {
        // === Wave 1 ===
        GameObject wave1 = Instantiate(wave1Prefab, wave1SpawnPoint.position, Quaternion.identity);
        yield return new WaitUntil(() => wave1 == null);

        // === Wave 2 ===
        GameObject wave2 = Instantiate(wave2Prefab, wave2SpawnPoint.position, Quaternion.identity);
        yield return new WaitUntil(() => wave2 == null);

        // === Wave 3 ===
        GameObject wave3a = Instantiate(wave3aPrefab, wave3aSpawnPoint.position, Quaternion.identity);
        GameObject wave3b = Instantiate(wave3bPrefab, wave3bSpawnPoint.position, Quaternion.identity);
        yield return new WaitUntil(() => wave3a == null && wave3b == null);

        // === Unlock and Final Reward ===
        if (invisibleWall != null)
            invisibleWall.SetActive(false);

        if (finalPrefab != null && finalSpawnPoint != null)
            Instantiate(finalPrefab, finalSpawnPoint.position, Quaternion.identity);
    }
}
