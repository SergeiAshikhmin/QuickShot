using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnlockPopup : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;  // Drag TitleText (TMP) here
    public Button closeButton;         // Drag CloseButton here
    public Image weaponImageUI;        // Drag WeaponImage (Image) here (NOT the Button image)

    private System.Action onCloseCallback;

    public void Setup(string weaponID, System.Action onClose, Sprite weaponSprite)
    {
        onCloseCallback = onClose;

        if (titleText != null)
            titleText.text = $"You have unlocked the {weaponID}!";

        if (weaponImageUI != null)
        {
            weaponImageUI.sprite = weaponSprite;
            weaponImageUI.preserveAspect = true;
        }

        if (closeButton != null)
        {
            // Clear previous listeners and hook up close
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }
    }

    private void Close()
    {
        onCloseCallback?.Invoke();
        Destroy(gameObject);
    }
}
