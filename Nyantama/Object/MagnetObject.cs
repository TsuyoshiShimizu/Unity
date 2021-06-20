using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetObject : MonoBehaviour
{
    public float MagnetPow = 1.0f;
    [SerializeField] private float MagnetAreaSize = 4.0f;
    [HideInInspector]
    [SerializeField] private GameObject MagEffect = null;
    [HideInInspector]
    [SerializeField] private GameObject MagArea = null;
    
    private enum Mode { Mode3D, Mode2D }

    // Start is called before the first frame update
    void Start()
    {
        MagArea.transform.localScale = Vector3.one * MagnetAreaSize;
        GetComponent<SphereCollider>().radius = MagnetAreaSize * 0.5f ;
        MagEffect.transform.localScale = Vector3.one * MagnetAreaSize * 0.1f;
    }

    private void OnValidate()
    {
        MagArea.transform.localScale = Vector3.one * MagnetAreaSize;
    }
}
