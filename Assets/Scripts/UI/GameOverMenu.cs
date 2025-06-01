using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    public GameObject gameOverMenu;
    private void OnEnable()
    {
        GameManager.OnPlayerDeath += ShowGameOverMenu;
    }

    private void OnDisable()
    {
        GameManager.OnPlayerDeath -= ShowGameOverMenu;
    }

    private void ShowGameOverMenu()
    {
        gameOverMenu.SetActive(true);
        Time.timeScale = 0; // pause the game
    }
    
    public void RestartLevel()
    {
        Time.timeScale = 1; // unpause the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
