using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyController : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 0.5f;
    private Material skyboxMaterial;

    // Start is called before the first frame update
    void Start()
    {
        skyboxMaterial = RenderSettings.skybox;
    }

    // Update is called once per frame
    void Update()
    {
        skyboxMaterial.SetFloat("_Rotation", Mathf.Repeat(skyboxMaterial.GetFloat("_Rotation") + rotateSpeed * Time.deltaTime, 360f));
    }
}
