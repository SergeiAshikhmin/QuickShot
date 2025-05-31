using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTriggerZone : MonoBehaviour
{
    public EnemyController watchedEnemy;       // The enemy to monitor
    public GameObject leftEnemyPrefab;         // Prefab to spawn on the left
    public GameObject rightEnemyPrefab;        // Prefab to spawn on the right
    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;

    private bool hasTriggered = false;

    private void Start()
    {
        if (watchedEnemy) watchedEnemy.OnEnemyDied += HandleEnemyDeath;
        else Debug.LogWarning("No enemy assigned to watchedEnemy");
    }

    private void HandleEnemyDeath()
    {
        if (hasTriggered) return;
        hasTriggered = true;

        StartCoroutine(SpawnAfterDelay());
    }

    IEnumerator SpawnAfterDelay()
    {
        yield return new WaitForSeconds(3f);

        if (leftEnemyPrefab && leftSpawnPoint)
        {
            GameObject leftEnemy =  Instantiate(leftEnemyPrefab, leftSpawnPoint.position, Quaternion.identity);
            EnemyController ec = leftEnemy.GetComponent<EnemyController>();
            if (ec)
            {
                ec.player = GameObject.FindGameObjectWithTag("Player").transform;
                ec.aggroRadius = 20f;
            }
        }

        if (rightEnemyPrefab && rightSpawnPoint)
        {
            GameObject rightEnemy = Instantiate(rightEnemyPrefab, rightSpawnPoint.position, Quaternion.identity);
            EnemyController ec = rightEnemy.GetComponent<EnemyController>();
            if (ec)
            {
                ec.player = GameObject.FindGameObjectWithTag("Player").transform;
                ec.aggroRadius = 20f;
            }    
        }
        
    }

    private void OnDestroy()
    {
        if (watchedEnemy) watchedEnemy.OnEnemyDied -= HandleEnemyDeath;
    }
}
