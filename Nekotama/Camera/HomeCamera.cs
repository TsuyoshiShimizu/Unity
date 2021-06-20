using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeCamera : MonoBehaviour
{
    private GameObject player;
    private int CameraPlace = 0;
    private Vector3 PScale;
    private Vector3 PPosition;

    //カメラ位置
    float C0minx = -2.0f;
    float C0maxx = 2.0f;
 //   float C0minz = 2.6f;
 //   float C0maxz = 30;

    float C0y = 2.6f;
    float C0z = -3.0f;
    float C0Cx = 0;
    float C0Cz = -1.3f;

    float C1x = -1.3f;
    float C1y = 2.4f;
    float C1z = -6.0f;
    float C1Cx = 0;
    float C1Cmaxz = -1.1f;
    float C1Cminz = -4.2f;

    float C2x = 0.7f;
    float C2y = 2.2f;
    float C2z = -5.5f;
    float C2Cx = -0.2f;
    float C2Cmaxz = -1.1f;
    float C2Cminz = -4.0f;

    float C3x = -3;
    float C3y = 2;
    float C3z = -9.7f;
    float C3Cz = -4.0f;

    float C4x = 2.2f;
    float C4y = 2;
    float C4z = -9.5f;
    float C4Cz = -3.5f;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < Players.Length; i++)
        {
            if (Players[i].name == "Player") player = Players[i];
        }
        PScale = transform.root.localScale;
        PPosition = transform.root.position;
    }

    private void FixedUpdate()
    {
        if (CameraPlace == 0)
        {
            if (C0minx <= player.transform.localPosition.x && player.transform.localPosition.x <= C0maxx )
            {
                transform.localPosition = new Vector3(player.transform.localPosition.x, C0y, C0z);
            }

            if (player.transform.localPosition.z <= C0Cz && player.transform.localPosition.x <= C0Cx)
            {
                CameraPlace = 1;
                ChangeCameraP(C1x, C1y, C1z);
                Debug.Log("カメラ1");
            }
            else if (player.transform.localPosition.z <= C0Cz)
            {
                CameraPlace = 2;
                ChangeCameraP(C2x, C2y, C2z);
                Debug.Log("カメラ2");
            }
            
        }

        if (CameraPlace == 1)
        {
            if (player.transform.localPosition.z >= C1Cmaxz)
            {
                CameraPlace = 0;
                ChangeCameraP(C0minx, C0y, C0z);
                Debug.Log("カメラ0");
            }
            if (player.transform.localPosition.x >= C1Cx)
            {
                CameraPlace = 2;
                ChangeCameraP(C2x, C2y, C2z);
                Debug.Log("カメラ2");
            }
            if (player.transform.localPosition.z <= C1Cminz)
            {
                CameraPlace = 3;
                ChangeCameraP(C3x, C3y, C3z);
                Debug.Log("カメラ3");
            }
        }

        if (CameraPlace == 2)
        {
            if (player.transform.localPosition.z >= C2Cmaxz)
            {
                CameraPlace = 0;
                ChangeCameraP(C0maxx, C0y, C0z);
                Debug.Log("カメラ0");
            }
            if (player.transform.localPosition.x <= C2Cx)
            {
                CameraPlace = 1;
                ChangeCameraP(C1x, C1y, C1z);
                Debug.Log("カメラ1");
            }
            if (player.transform.localPosition.z <= C2Cminz)
            {
                CameraPlace = 4;
                ChangeCameraP(C4x, C4y, C4z);
                Debug.Log("カメラ4");
            }
        }

        if (CameraPlace == 3)
        {
            if (player.transform.localPosition.z >= C3Cz)
            {
                CameraPlace = 1;
                ChangeCameraP(C1x, C1y, C1z);
                Debug.Log("カメラ1");
            }
        }
        if (CameraPlace == 4)
        {
            if (player.transform.localPosition.z >= C4Cz)
            {
                CameraPlace = 2;
                ChangeCameraP(C2x, C2y, C2z);
                Debug.Log("カメラ2");
            }
        }

    }

    private void Update()
    {
        
    }

    private void ChangeCameraP(float cx, float cy, float cz)
    {
        transform.localPosition = new Vector3(cx, cy, cz);
    }
}
