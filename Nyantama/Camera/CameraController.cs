using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    private GameObject player;

    //カメラの距離
    float Cdx = 0.0f;
    float Cdy = 23.1f;
    float Cdz = -40.0f;
    //  float Cdy = 5.0f;
    // float Cdz = -30.0f;
    //カメラの角度


    // Start is called before the first frame update
    void Start()
    {
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i].name == "Player") player = Players[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 PlayerP = player.transform.position;
        transform.position = new Vector3(PlayerP.x + Cdx, PlayerP.y + Cdy, PlayerP.z + Cdz);
    }
}
