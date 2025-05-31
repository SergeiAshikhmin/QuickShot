using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

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

    [Header("Back Buttons")]
    public Button backLevelSelectButton;
    public Button backWeaponsButton;
    public Button backOptionsButton;

    [Header("Ammo Counter UI")]
    public TMP_Text ammoCounter;
    public Image reloadSpinner;
    public TMP_Text reloadText;

    [Header("Reload Spinner Animation")]
    public Sprite[] reloadFrames;
    public float spinnerFPS = 12f;

    [Header("Charge UI")]
    public Slider chargeSlider;
    public TMP_Text chargeText;
    public TMP_Text chargeOverheatText;  // 🔥 New field

    private bool isPaused = false;
    private float spinnerTimer = 0f;
    private int spinnerFrameIndex = 0;

    void Start()
    {
        levelSelectPanel.SetActive(false);
        weaponsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        quitConfirmPanel.SetActive(false);
        pausePanel.SetActive(false);

        if (ammoCounter != null) ammoCounter.gameObject.SetActive(false);
        if (reloadSpinner != null) reloadSpinner.gameObject.SetActive(false);
        if (reloadText != null) reloadText.gameObject.SetActive(false);
        if (chargeSlider != null) chargeSlider.gameObject.SetActive(false);
        if (chargeText != null) chargeText.gameObject.SetActive(false);
        if (chargeOverheatText != null) chargeOverheatText.gameObject.SetActive(false);

        resumeButton.onClick.AddListener(OnResume);
        levelSelectButton.onClick.AddListener(OpenLevelSelect);
        weaponsButton.onClick.AddListener(OpenWeapons);
        optionsButton.onClick.AddListener(OpenOptions);
        quitButton.onClick.AddListener(OpenQuitConfirm);
        yesQuitButton.onClick.AddListener(OnQuitConfirmed);
        noQuitButton.onClick.AddListener(CloseQuitConfirm);

        if (backLevelSelectButton) backLevelSelectButton.onClick.AddListener(CloseLevelSelectPanel);
        if (backWeaponsButton)     backWeaponsButton.onClick.AddListener(CloseWeaponsPanel);
        if (backOptionsButton)     backOptionsButton.onClick.AddListener(CloseOptionsPanel);
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

        UpdateAmmoCounter();
        UpdateChargeUI();
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

    void CloseQuitConfirmPanel()
    {
        quitConfirmPanel.SetActive(false);
        if (pausePanel.activeSelf) SetPauseButtonsInteractable(true);
    }

    void SetPauseButtonsInteractable(bool interactable)
    {
        resumeButton.interactable = interactable;
        levelSelectButton.interactable = interactable;
        weaponsButton.interactable = interactable;
        optionsButton.interactable = interactable;
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

    void UpdateChargeUI()
    {
        if (!chargeSlider || !chargeText || !chargeOverheatText) return;

        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        bool foundLaserWeapon = false;

        foreach (GameObject obj in allObjects)
        {
            if (obj.activeInHierarchy && obj.layer == LayerMask.NameToLayer("Weapon"))
            {
                foreach (var comp in obj.GetComponents<MonoBehaviour>())
                {
                    if (comp is LaserPistol laser)
                    {
                        if (laser.IsOverheated)
                        {
                            float remainingCooldown = Mathf.Max(0f, laser.OverheatCooldownRemaining);
                            chargeSlider.gameObject.SetActive(false);
                            chargeText.gameObject.SetActive(false);
                            chargeOverheatText.gameObject.SetActive(true);
                            chargeOverheatText.text = $"OVERHEATED ({remainingCooldown:F1}s)";
                        }
                        else
                        {
                            float percent = Mathf.Clamp01(laser.CurrentCharge / laser.maxCharge);
                            chargeSlider.value = percent;
                            chargeText.text = $"{(int)(percent * 100)}%";

                            chargeSlider.gameObject.SetActive(true);
                            chargeText.gameObject.SetActive(true);
                            chargeOverheatText.gameObject.SetActive(false);
                        }

                        foundLaserWeapon = true;
                        break;
                    }
                }
            }
            if (foundLaserWeapon) break;
        }

        if (!foundLaserWeapon)
        {
            chargeSlider.gameObject.SetActive(false);
            chargeText.gameObject.SetActive(false);
            chargeOverheatText.gameObject.SetActive(false);
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
