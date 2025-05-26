using UnityEngine;

public class PlayerPrefsResetter : MonoBehaviour
{
    public void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs wiped!");
    }
}
