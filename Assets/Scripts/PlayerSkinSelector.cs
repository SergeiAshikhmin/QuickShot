using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkinSelector : MonoBehaviour
{
    [Tooltip("Index 0 = Pink, 1 = Blue, 2 = White")]
    public AnimatorOverrideController[] availableSkins;

    private Animator _anim;

    void Awake()
    {
        _anim = GetComponent<Animator>();
        // Apply Default Skin - Pink
        ChooseSkin(0); 
    }

    // Call this from a UI dropdown, settings menu, or another script
    public void ChooseSkin(int index)
    {
        if (index < 0 || index >= availableSkins.Length) return;
        _anim.runtimeAnimatorController = availableSkins[index];
    }
}
