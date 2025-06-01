using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxRoot : MonoBehaviour
{
    private Transform cam;
    private Vector3 prevCamPos;

    private void Awake()
    {
        cam = Camera.main.transform;
        prevCamPos = cam.position;
    }

    private void LateUpdate()
    {
        Vector3 camDelta = cam.position - prevCamPos;

        foreach (ParallaxFactor layer in GetComponentsInChildren<ParallaxFactor>())
        {
            layer.transform.position += new Vector3(camDelta.x * layer.factor, 0f, 0f);
        }
        
        prevCamPos = cam.position;
    }
}
