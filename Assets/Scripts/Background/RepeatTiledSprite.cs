using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RepeatTiledSprite : MonoBehaviour
{
    private float spriteWorldWidth;
    private Transform cam;

    private void Awake()
    {
        cam = Camera.main.transform;
        
        spriteWorldWidth = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void LateUpdate()
    {
        float deltaX = cam.position.x - transform.position.x;

        if (Mathf.Abs(deltaX) >= spriteWorldWidth)
        {
            float offset = spriteWorldWidth * Mathf.Sign(deltaX);
            transform.position += new Vector3(offset, 0f, 0f);
        }
    }
}
