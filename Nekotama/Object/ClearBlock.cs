using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearBlock : MonoBehaviour
{
    [SerializeField] MeshRenderer MRend = null;

    private void Awake()
    {
        MRend.enabled = false;
    }
}
