using UnityEngine;
using UnityEngine.SceneManagement;

public class ForceBowInLevel1 : MonoBehaviour
{
    private const string EquippedKey = "EquippedWeapon";
    private const string TempKey = "PreviousEquippedWeapon";

    void Awake()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Level-1")
        {
            string equipped = PlayerPrefs.GetString(EquippedKey, "Bow");

            if (equipped != "Bow")
            {
                PlayerPrefs.SetString(TempKey, equipped);
                PlayerPrefs.SetString(EquippedKey, "Bow");
                PlayerPrefs.Save();
                Debug.Log("ForceBowInLevel1: Overriding equipped weapon to Bow.");
            }
        }
    }
}
