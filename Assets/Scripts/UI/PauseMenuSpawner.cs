using UnityEngine;

public class PauseMenuSpawner : MonoBehaviour
{
    public GameObject pauseMenuPrefab;

    void Start()
    {
        if (FindObjectOfType<PauseMenuManager>() == null)
            Instantiate(pauseMenuPrefab);
    }
}
