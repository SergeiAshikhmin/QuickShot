using UnityEngine;
using UnityEngine.SceneManagement; // Needed for scene loading

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public GameObject optionsPanel;

    void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    public void ShowLevelSelect()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
        optionsPanel.SetActive(false);
    }

    public void ShowOptions()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Quit called");
    }

    // Load different scenes
    public void StartFuture()
    {
        SceneManager.LoadScene("Future");
    }

    public void StartTutorial()
    {
        SceneManager.LoadScene("tutorial-level");
    }

    public void StartLevel()
    {
        SceneManager.LoadScene("Scene3");
    }
}
