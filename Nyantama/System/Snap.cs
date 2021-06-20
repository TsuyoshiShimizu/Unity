using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Custom Object/Snap")]
public class Snap : MonoBehaviour
{
    [SerializeField] private float snapMoveSpan = 0.5f;
    [SerializeField] private float snapRotSpan = 15f;
    [SerializeField] private float snapScaleSpan = 1f;
}
