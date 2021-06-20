using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearBlock : MonoBehaviour
{

    [HideInInspector] [SerializeField] MeshRenderer MRend = null;
    [SerializeField] private GameObject XWall = null;
    [SerializeField] private GameObject YWall = null;
    [SerializeField] private GameObject ZWall = null;

    private Vector3 MaxPos = Vector3.zero;
    private Vector3 MinPos = Vector3.zero;
    private Vector3 FixMaxPos = Vector3.zero;
    private Vector3 FixMinPos = Vector3.zero;
    private bool WallFlagX = false;
    private bool WallFlagY = false;
    private bool WallFlagZ = false;
    private float WallDeltaX = 0;
    private float WallDeltaY = 0;
    private float WallDeltaZ = 0;
    private float WallTime = 0.1f;
    private float FixRange = 0.8f;

    private void Start()
    {
        MRend.enabled = false;
        MaxPos = transform.position + transform.lossyScale / 2f;
        MinPos = transform.position - transform.lossyScale / 2f;
        FixMaxPos = MaxPos - Vector3.one * FixRange;
        FixMinPos = MinPos + Vector3.one * FixRange;
    }

    private void OnCollisionStay(Collision other)
    {
        if(other.gameObject.tag == "Player")
        {
            
            int[] intP = new int[] { 0, 0, 0 };
            Vector3 PPos = PlayerController.PlayerPos;
            for (int i = 0; i <= 2; i++)
            {
                if      (PPos[i] <= MinPos[i])  intP[i] = 1;
                else if (PPos[i] <= MaxPos[i])  intP[i] = 2;
                else                            intP[i] = 3;
            }

            if(intP[0] != 2)
            {
                WallDeltaX = 0;
                WallFlagX = true;

                float ax = 0;
                if (intP[0] == 1) ax = MinPos.x; else ax = MaxPos.x;
                float ay = PPos.y;
                if (ay < FixMinPos.y) ay = FixMinPos.y;
                if (ay > FixMaxPos.y) ay = FixMaxPos.y;
                float az = PPos.z;
                if (az < FixMinPos.z) az = FixMinPos.z;
                if (az > FixMaxPos.z) az = FixMaxPos.z;

                XWall.transform.position = new Vector3(ax, ay, az);
                if (!XWall.activeSelf) XWall.SetActive(true);
                
            }

            if (intP[1] != 2)
            {
                WallDeltaY = 0;
                WallFlagY = true;

                float ay = 0;
                if (intP[1] == 1) ay = MinPos.y; else ay = MaxPos.y;
                float ax = PPos.x;
                if (ax < FixMinPos.x) ax = FixMinPos.x;
                if (ax > FixMaxPos.x) ax = FixMaxPos.x;
                float az = PPos.z;
                if (az < FixMinPos.z) az = FixMinPos.z;
                if (az > FixMaxPos.z) az = FixMaxPos.z;

                YWall.transform.position = new Vector3(ax, ay, az);
                if (!YWall.activeSelf) YWall.SetActive(true);
                
            }
            if (intP[2] != 2)
            {
                WallDeltaZ = 0;
                WallFlagZ = true;

                float az = 0;
                if (intP[2] == 1) az = MinPos.z; else az = MaxPos.z;
                float ax = PPos.x;
                if (ax < FixMinPos.x) ax = FixMinPos.x;
                if (ax > FixMaxPos.x) ax = FixMaxPos.x;
                float ay = PPos.y;
                if (ay < FixMinPos.y) ay = FixMinPos.y;
                if (ay > FixMaxPos.y) ay = FixMaxPos.y;

                if (!ZWall.activeSelf) ZWall.SetActive(true);
                ZWall.transform.position = new Vector3(ax, ay, az);
            }
        }
    }

    private void Update()
    {
        if (WallFlagX)
        {
            WallDeltaX += Time.deltaTime;
            if (WallDeltaX >= WallTime)
            {
                WallFlagX = false;
                WallDeltaX = 0;
                XWall.SetActive(false);
            }
        }

        if (WallFlagY)
        {
            WallDeltaY += Time.deltaTime;
            if (WallDeltaY >= WallTime)
            {
                WallFlagY = false;
                WallDeltaY = 0;
                YWall.SetActive(false);
            }
        }

        if (WallFlagZ)
        {
            WallDeltaZ += Time.deltaTime;
            if (WallDeltaZ >= WallTime)
            {
                WallFlagZ = false;
                WallDeltaZ = 0;
                ZWall.SetActive(false);
            }
        }
    }
}
