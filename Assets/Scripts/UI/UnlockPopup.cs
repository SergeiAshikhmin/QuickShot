using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UnlockPopup : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public Button closeButton;

    private System.Action onClose;

    // Call this from UnlockPoint to initialize the popup
    public void Setup(string weaponID, System.Action onCloseCallback)
    {
        messageText.text = $"You unlocked the {weaponID}!";
        onClose = onCloseCallback;
        closeButton.onClick.RemoveAllListeners(); // Ensure no duplicate listeners
        closeButton.onClick.AddListener(ClosePopup);
        gameObject.SetActive(true); // Show popup
    }

    void ClosePopup()
    {
        onClose?.Invoke(); // Callback to UnlockPoint for re-enabling game
        Destroy(gameObject); // Destroy popup UI
    }
}

