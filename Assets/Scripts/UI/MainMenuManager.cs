using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject levelSelectPanel;
    public GameObject weaponsPanel;
    public GameObject optionsPanel;
    public GameObject quitConfirmPanel;
    public GameObject skinPanel;

    public Button levelSelectButton;
    public Button weaponsButton;
    public Button optionsButton;
    public Button skinButton;
    public Button quitButton;
    public Button yesQuitButton;
    public Button noQuitButton;

    [Header("Back Buttons")]
    public Button backLevelSelectButton;
    public Button backWeaponsButton;
    public Button backOptionsButton;
    public Button backSkinButton;

    [Header("Ammo Counter UI")]
    public TMP_Text ammoCounter;
    public Image reloadSpinner;
    public TMP_Text reloadText;

    [Header("Reload Spinner Animation")]
    public Sprite[] reloadFrames;
    public float spinnerFPS = 12f;

    private bool isPaused = false;
    private float spinnerTimer = 0f;
    private int spinnerFrameIndex = 0;
    private bool isMainMenu;

    void Start()
    {
        isMainMenu = SceneManager.GetActiveScene().name == "MainMenu";

        levelSelectPanel.SetActive(false);
        weaponsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        quitConfirmPanel.SetActive(false);
        skinPanel.SetActive(false);
        pausePanel.SetActive(true); // Always open on MainMenu

        if (ammoCounter != null) ammoCounter.gameObject.SetActive(false);
        if (reloadSpinner != null) reloadSpinner.gameObject.SetActive(false);
        if (reloadText != null) reloadText.gameObject.SetActive(false);

        levelSelectButton.onClick.AddListener(OpenLevelSelect);
        weaponsButton.onClick.AddListener(OpenWeapons);
        optionsButton.onClick.AddListener(OpenOptions);
        skinButton.onClick.AddListener(OpenSkinPanel);
        quitButton.onClick.AddListener(OpenQuitConfirm);
        yesQuitButton.onClick.AddListener(OnQuitConfirmed);
        noQuitButton.onClick.AddListener(CloseQuitConfirm);

        if (backLevelSelectButton) backLevelSelectButton.onClick.AddListener(CloseLevelSelectPanel);
        if (backWeaponsButton) backWeaponsButton.onClick.AddListener(CloseWeaponsPanel);
        if (backOptionsButton) backOptionsButton.onClick.AddListener(CloseOptionsPanel);
        if (backSkinButton) backSkinButton.onClick.AddListener(CloseSkinPanel);

        if (isMainMenu)
        {
            Time.timeScale = 0f;
        }
    }

    void Update()
    {
        if (!isMainMenu && Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
                Pause();
            else
                OnResume();
        }

        UpdateAmmoCounter();
        AnimateReloadSpinner();
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
        SetPauseButtonsInteractable(true);

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
        SetPauseButtonsInteractable(false);
    }

    void OpenWeapons()
    {
        CloseAllSubmenus();
        weaponsPanel.SetActive(true);
        SetPauseButtonsInteractable(false);
    }

    void OpenOptions()
    {
        CloseAllSubmenus();
        optionsPanel.SetActive(true);
        SetPauseButtonsInteractable(false);
    }

    void OpenSkinPanel()
    {
        CloseAllSubmenus();
        skinPanel.SetActive(true);
        SetPauseButtonsInteractable(false);
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

        if (isMainMenu)
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    void CloseAllSubmenus()
    {
        levelSelectPanel.SetActive(false);
        weaponsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        skinPanel.SetActive(false);
        quitConfirmPanel.SetActive(false);
        if (pausePanel.activeSelf) SetPauseButtonsInteractable(true);
    }

    void CloseLevelSelectPanel()
    {
        levelSelectPanel.SetActive(false);
        if (pausePanel.activeSelf) SetPauseButtonsInteractable(true);
    }

    void CloseWeaponsPanel()
    {
        weaponsPanel.SetActive(false);
        if (pausePanel.activeSelf) SetPauseButtonsInteractable(true);
    }

    void CloseOptionsPanel()
    {
        optionsPanel.SetActive(false);
        if (pausePanel.activeSelf) SetPauseButtonsInteractable(true);
    }

    void CloseSkinPanel()
    {
        skinPanel.SetActive(false);
        if (pausePanel.activeSelf) SetPauseButtonsInteractable(true);
    }

    void CloseQuitConfirmPanel()
    {
        quitConfirmPanel.SetActive(false);
        if (pausePanel.activeSelf) SetPauseButtonsInteractable(true);
    }

    void SetPauseButtonsInteractable(bool interactable)
    {
        levelSelectButton.interactable = interactable;
        weaponsButton.interactable = interactable;
        optionsButton.interactable = interactable;
        skinButton.interactable = interactable;
        quitButton.interactable = interactable;
    }

    void UpdateAmmoCounter()
    {
        if (!ammoCounter || !reloadSpinner || !reloadText) return;

        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        bool foundAmmoWeapon = false;

        foreach (GameObject obj in allObjects)
        {
            if (obj.activeInHierarchy && obj.layer == LayerMask.NameToLayer("Weapon"))
            {
                foreach (var comp in obj.GetComponents<MonoBehaviour>())
                {
                    if (comp is IAmmoWeapon ammoWeapon && ammoWeapon.ShowAmmo)
                    {
                        var isReloadingProp = comp.GetType().GetProperty("isReloading");
                        bool isReloading = isReloadingProp != null && (bool)isReloadingProp.GetValue(comp);

                        if (isReloading)
                        {
                            ammoCounter.gameObject.SetActive(false);
                            if (reloadSpinner) reloadSpinner.gameObject.SetActive(true);
                            if (reloadText) reloadText.gameObject.SetActive(true);
                            foundAmmoWeapon = true;
                        }
                        else
                        {
                            ammoCounter.text = $"{ammoWeapon.CurrentAmmo}";
                            ammoCounter.gameObject.SetActive(true);
                            if (reloadSpinner) reloadSpinner.gameObject.SetActive(false);
                            if (reloadText) reloadText.gameObject.SetActive(false);
                            foundAmmoWeapon = true;
                        }
                        break;
                    }
                }
            }
            if (foundAmmoWeapon) break;
        }

        if (!foundAmmoWeapon)
        {
            ammoCounter.gameObject.SetActive(false);
            if (reloadSpinner) reloadSpinner.gameObject.SetActive(false);
            if (reloadText) reloadText.gameObject.SetActive(false);
        }
    }

    void AnimateReloadSpinner()
    {
        if (reloadSpinner != null && reloadSpinner.gameObject.activeSelf && reloadFrames != null && reloadFrames.Length > 0)
        {
            spinnerTimer += Time.unscaledDeltaTime;
            if (spinnerTimer >= 1f / spinnerFPS)
            {
                spinnerTimer = 0f;
                spinnerFrameIndex = (spinnerFrameIndex + 1) % reloadFrames.Length;
                reloadSpinner.sprite = reloadFrames[spinnerFrameIndex];
            }
        }
    }
}
