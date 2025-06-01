using UnityEngine;
using UnityEngine.SceneManagement;

public class EndPoint : MonoBehaviour
{
    [Header("Level Complete UI")]
    public GameObject levelCompleteUI; // Assign the UI panel in Inspector

    private bool triggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            triggered = true;
            ShowLevelCompleteUI();
        }
    }

    void ShowLevelCompleteUI()
    {
        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(true);
            Time.timeScale = 0f;

            // Disable ESC key pause by disabling the manager
            PauseMenuManager pauseManager = FindObjectOfType<PauseMenuManager>();
            if (pauseManager != null)
                pauseManager.enabled = false;
        }
        else
        {
            Debug.LogWarning("LevelCompleteUI is not assigned!");
        }
    }

    // Called by Retry Button
    public void RetryLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Called by Main Menu Button
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
