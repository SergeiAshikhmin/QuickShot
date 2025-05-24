using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ParallaxFactor : MonoBehaviour
{
    [Range(0f, 1f)] public float factor = 0.3f;
}
