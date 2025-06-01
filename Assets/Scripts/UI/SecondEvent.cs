using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondEvent : MonoBehaviour
{
    [Header("Invisible Wall")]
    public GameObject invisibleWall;

    [Header("Enemy Prefabs")]
    public GameObject enemyPrefabA;
    public GameObject enemyPrefabB;

    [Header("Spawn Points")]
    public Transform spawnPointA;
    public Transform spawnPointB;

    [Header("Destroy Target")]
    public GameObject objectToDestroy;

    [Header("Event Object Toggles")]
    public List<GameObject> objectsToDisableWhileActive = new();
    public List<GameObject> objectsToActivateWhileActive = new();

    [Header("Spawn Settings")]
    public float spawnRate = 3f;

    [Header("Completion Spawn")]
    public GameObject completedPrefab;
    public Transform completedPrefabSpawnPoint;

    private bool triggered = false;
    private GameObject currentEnemyA;
    private GameObject currentEnemyB;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;

            if (invisibleWall != null)
                invisibleWall.SetActive(true);

            foreach (var go in objectsToDisableWhileActive)
                if (go) go.SetActive(false);

            foreach (var go in objectsToActivateWhileActive)
                if (go) go.SetActive(true);

            StartCoroutine(HandleEventWave());
        }
    }

    IEnumerator HandleEventWave()
    {
        while (objectToDestroy != null)
        {
            if (currentEnemyA == null)
            {
                yield return new WaitForSeconds(spawnRate);
                if (objectToDestroy == null) break; // double check after delay
                currentEnemyA = Instantiate(enemyPrefabA, spawnPointA.position, Quaternion.identity);
            }

            if (objectToDestroy == null) break;

            if (currentEnemyB == null)
            {
                yield return new WaitForSeconds(spawnRate);
                if (objectToDestroy == null) break;
                currentEnemyB = Instantiate(enemyPrefabB, spawnPointB.position, Quaternion.identity);
            }

            yield return null;
        }

        // === Object destroyed - conclude event ===
        if (invisibleWall != null)
            invisibleWall.SetActive(false);

        foreach (var go in objectsToDisableWhileActive)
            if (go) go.SetActive(true);

        foreach (var go in objectsToActivateWhileActive)
            if (go) go.SetActive(false);

        // === Spawn completion prefab ===
        if (completedPrefab != null)
        {
            Vector3 spawnPos = completedPrefabSpawnPoint != null
                ? completedPrefabSpawnPoint.position
                : transform.position;

            Instantiate(completedPrefab, spawnPos, Quaternion.identity);
        }
    }
}
