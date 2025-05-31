using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevUnlock : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
{
    PlayerPrefs.SetInt("LevelUnlocked_Movement", 1);
    PlayerPrefs.SetInt("LevelUnlocked_Tutorial", 1);
    PlayerPrefs.SetInt("LevelUnlocked_Future", 1);
    PlayerPrefs.SetInt("WeaponUnlocked_Bow", 1);
    PlayerPrefs.SetInt("WeaponUnlocked_LaserPistol", 1);
    PlayerPrefs.Save();
}


    // Update is called once per frame
    void Update()
    {
        
    }
}
