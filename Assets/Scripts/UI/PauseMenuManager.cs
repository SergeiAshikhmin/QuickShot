using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject levelSelectPanel;
    public GameObject weaponsPanel;
    public GameObject optionsPanel;
    public GameObject quitConfirmPanel;

    public Button resumeButton;
    public Button levelSelectButton;
    public Button weaponsButton;
    public Button optionsButton;
    public Button quitButton;

    public Button yesQuitButton;
    public Button noQuitButton;

    private bool isPaused = false;

    void Start()
    {
        levelSelectPanel.SetActive(false);
        weaponsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        quitConfirmPanel.SetActive(false);
        pausePanel.SetActive(false);

        resumeButton.onClick.AddListener(OnResume);
        levelSelectButton.onClick.AddListener(OpenLevelSelect);
        weaponsButton.onClick.AddListener(OpenWeapons);
        optionsButton.onClick.AddListener(OpenOptions);
        quitButton.onClick.AddListener(OpenQuitConfirm);
        yesQuitButton.onClick.AddListener(OnQuitConfirmed);
        noQuitButton.onClick.AddListener(CloseQuitConfirm);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
                Pause();
            else
                OnResume();
        }
    }

    private AdvancedPlayerMovement GetPlayerMovement()
    {
        return FindObjectOfType<AdvancedPlayerMovement>();
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);

        var player = GetPlayerMovement();
        if (player != null)
            player.enabled = false;
    }

    public void OnResume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        CloseAllSubmenus();

        var player = GetPlayerMovement();
        if (player != null)
            player.enabled = true;
    }

    void OpenLevelSelect()
    {
        CloseAllSubmenus();
        levelSelectPanel.SetActive(true);
    }

    void OpenWeapons()
    {
        CloseAllSubmenus();
        weaponsPanel.SetActive(true);
    }

    void OpenOptions()
    {
        CloseAllSubmenus();
        optionsPanel.SetActive(true);
    }

    void OpenQuitConfirm()
    {
        quitConfirmPanel.SetActive(true);
    }

    void CloseQuitConfirm()
    {
        quitConfirmPanel.SetActive(false);
    }

    void OnQuitConfirmed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    void CloseAllSubmenus()
    {
        levelSelectPanel.SetActive(false);
        weaponsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        quitConfirmPanel.SetActive(false);
    }
}
