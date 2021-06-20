using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GoalColor : MonoBehaviour
{
    [SerializeField] private Color32 color = new Color32(255,255,255,255);

    private void Start()
    {
        GetComponent<Renderer>().material.color = color;
    }
}
