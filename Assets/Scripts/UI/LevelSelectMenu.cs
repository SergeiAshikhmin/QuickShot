using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectMenu : MonoBehaviour
{
    [System.Serializable]
    public class LevelButton
    {
        public string levelID;   // E.g., "Level1"
        public Button button;    // Drag the button here in Inspector
    }

    public List<LevelButton> levelButtons; // Fill in Inspector

    void OnEnable()
    {
        UpdateLevelButtons();
    }

    void UpdateLevelButtons()
    {
        foreach (var lb in levelButtons)
        {
            // For unlocked levels: PlayerPrefs.GetInt("LevelUnlocked_Level1", 0)
            bool unlocked = PlayerPrefs.GetInt("LevelUnlocked_" + lb.levelID, 0) == 1;
            lb.button.gameObject.SetActive(unlocked);
        }
    }

    // Assign this to each button's OnClick in the Inspector
    public void LoadLevel(string levelID)
    {
        Time.timeScale = 1f; // Unpause if paused
        SceneManager.LoadScene(levelID);
    }
}
