using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuSpawner : MonoBehaviour
{
    public GameObject pauseMenuPrefab;

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
            return;

        if (FindObjectOfType<PauseMenuManager>() == null)
            Instantiate(pauseMenuPrefab);
    }
}
