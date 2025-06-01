using UnityEngine;
using UnityEngine.UI;

public class PopupWindow : MonoBehaviour
{
    [Header("References")]
    public GameObject popupPanel;  // Drag your PopupPanel UI GameObject here
    public Button closeButton;

    private AdvancedPlayerMovement playerMovement;

    void Start()
    {
        popupPanel.SetActive(true);          // Show on scene load
        Time.timeScale = 0f;                 // Pause the game
        closeButton.onClick.AddListener(ClosePopup);

        playerMovement = FindObjectOfType<AdvancedPlayerMovement>();
        if (playerMovement != null)
            playerMovement.enabled = false;
    }

    void ClosePopup()
    {
        popupPanel.SetActive(false);         // Hide popup
        Time.timeScale = 1f;                 // Resume the game

        if (playerMovement != null)
            playerMovement.enabled = true;
    }
}
