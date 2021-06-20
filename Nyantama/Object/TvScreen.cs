using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TvScreen : MonoBehaviour
{
    [SerializeField] private Texture ScreenImage = null;
    [HideInInspector]
    [SerializeField] private GameObject ScreenObj = null;

    // Start is called before the first frame update
    void Start()
    {
        Renderer renderer = ScreenObj.GetComponent<Renderer>();
        renderer.material.SetTexture("_MainTex", ScreenImage);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
