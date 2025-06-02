using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawner : MonoBehaviour
{
    [Header("Respawn Settings")]
    public GameObject playerPrefab;
    public Transform spawnPoint;

    private void OnEnable()
    {
        GameManager.OnPlayerDeath += RespawnPlayer;
    }

    private void OnDisable()
    {
        GameManager.OnPlayerDeath -= RespawnPlayer;
    }

    private void RespawnPlayer()
    {
        // Optional: delay to allow death animation/sound
        Invoke(nameof(Spawn), 2f);
    }

    private void Spawn()
    {
        Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        GameManager.Instance.ResetRun(); // reset health, etc.
    }
}
