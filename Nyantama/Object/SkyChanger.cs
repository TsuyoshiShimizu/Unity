using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyChanger : MonoBehaviour
{
    [SerializeField] Material SkyMaterial = null;
    private bool ContactFalg = true;
    //  private float DeltaTime = 0;

    private void Awake()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
    private void OnTriggerStay(Collider other)
    {
        if (!GameDirector.GamePlay) return;
        if (other.gameObject.tag == "Player" && ContactFalg)
        {
            ContactFalg = false;
            if (SkyMaterial != null) RenderSettings.skybox = SkyMaterial;
            Destroy(gameObject);
        }
    }

    /*
    private void FixedUpdate()
    {
        if (!ContactFalg)
        {
            DeltaTime += 0.02f;
            if (DeltaTime >= 5)
            {
                DeltaTime = 0;
                ContactFalg = true;
            }
        }
    }
    */
}
