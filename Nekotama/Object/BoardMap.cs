using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardMap : MonoBehaviour
{
    [SerializeField] private int Number = 0;
    private bool ContactFalg = true;
    private GameDirector Director;
    private float DeltaTime = 0;

    private void Start()
    {
        Director = GameObject.FindGameObjectWithTag("Director").GetComponent<GameDirector>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && ContactFalg)
        {
            ContactFalg = false;
            GameDirector.BoardNumber = Number;
            Director.OpenBoardMap();
        }
    }

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
}
