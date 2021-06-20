using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DescriptionArea : MonoBehaviour
{
    [SerializeField] private int Number = 0;
    [SerializeField] private bool DestroyFlag = true;
    [SerializeField] private bool ViewFlag = false;
    [SerializeField] private bool SoundFlag = false;
    private bool ContactFalg = true;
    private GameDirector Director;
    private float DeltaTime = 0;

    private void Awake()
    {
        if(!ViewFlag)  GetComponent<MeshRenderer>().enabled = false;
    }

    private void Start()
    {
        Director = GameObject.FindGameObjectWithTag("Director").GetComponent<GameDirector>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!GameDirector.GamePlay) return;
        if (other.gameObject.tag == "Player" && ContactFalg)
        {
            ContactFalg = false;
            GameDirector.DNumber = Number;
            Director.StartDescription(SoundFlag);
            if (DestroyFlag) Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (!ContactFalg)
        {
            DeltaTime += 0.02f;
            if(DeltaTime >= 5)
            {
                DeltaTime = 0;
                ContactFalg = true;
            }
        }
    }
}
