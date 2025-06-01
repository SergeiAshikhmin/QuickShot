using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }
    private CinemachineImpulseSource impulse;

    private void Awake()
    {
        Instance = this;
        impulse = GetComponent<CinemachineImpulseSource>();
    }
    
    /// <summary>Kick the camera; strength = 1 is “normal hit”.</summary>
    public void Shake(float strength = 1f) => impulse.GenerateImpulse(strength);
}
