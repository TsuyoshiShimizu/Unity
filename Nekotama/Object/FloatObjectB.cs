using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatObjectB : MonoBehaviour
{
    [SerializeField] private float YDis = 0;
    [SerializeField] private float ZDis = 0;
    [SerializeField] private float XOffsetA = 0;
    [SerializeField] private float XOffsetB = 0;
    [SerializeField] private Direction Dir = Direction.Left;
    [SerializeField] private ZDirection ZDir = ZDirection.Front;
    [SerializeField] private GameObject FObj = null;
    [SerializeField] private GameObject FObjA = null;
    [SerializeField] private GameObject FObjB = null;

    private float YFix = 14.0f;
    private float ZFix = 5.0f;

    private enum Direction { Right, Left }
    private enum ZDirection { Front,Back }

    public float GetLimitPointY()
    {
        return transform.position.y + YDis + YFix;
    }

    public float GetLimitPointZ()
    {
        if (ZDir == ZDirection.Front) return transform.position.z + ZDis + ZFix; else return transform.position.z - ZDis - ZFix;
    }

    public float GetZPos()
    {
        if (ZDir == ZDirection.Front) return transform.localPosition.z + ZDis; else return transform.localPosition.z - ZDis;
    }

    public bool GetFrontMoveFlag()
    {
        if (ZDir == ZDirection.Front) return true; else return false;
    }

    private void OnValidate()
    {
        if(Dir == Direction.Left && ZDir == ZDirection.Front)
        {
            FObj.transform.localPosition = new Vector3(0, 1.9f, -1);
            FObj.transform.eulerAngles = new Vector3(0, 0, 0);

            FObjA.transform.localPosition = new Vector3(XOffsetA + 6, 10 + YDis, 1.5f);
            FObjA.transform.eulerAngles = new Vector3(180,90,0);

            FObjB.transform.localPosition = new Vector3(XOffsetB + -2, 10 + YDis, ZDis -1.5f);
            FObjB.transform.eulerAngles = new Vector3(180, 270, 0);
        }
        else if(Dir == Direction.Left && ZDir == ZDirection.Back)
        {
            FObj.transform.localPosition = new Vector3(0, 1.9f, -1);
            FObj.transform.eulerAngles = new Vector3(0, 0, 0);

            FObjA.transform.localPosition = new Vector3(XOffsetA + -2, 10 + YDis, -1.5f);
            FObjA.transform.eulerAngles = new Vector3(180, 270, 0);

            FObjB.transform.localPosition = new Vector3(XOffsetB + 6, 10 + YDis, -ZDis + 1.5f);
            FObjB.transform.eulerAngles = new Vector3(180, 90, 0);      
        }
        else if (Dir == Direction.Right && ZDir == ZDirection.Front)
        {
            FObj.transform.localPosition = new Vector3(0, 1.9f, 1);
            FObj.transform.eulerAngles = new Vector3(0, 180, 0);

            FObjA.transform.localPosition = new Vector3(XOffsetA + 2, 10 + YDis, 1.5f);
            FObjA.transform.eulerAngles = new Vector3(180, 90, 0);

            FObjB.transform.localPosition = new Vector3(XOffsetB + -6, 10 + YDis, ZDis - 1.5f);
            FObjB.transform.eulerAngles = new Vector3(180, 270, 0);       
        }
        else
        {
            FObj.transform.localPosition = new Vector3(0, 1.9f, 1);
            FObj.transform.eulerAngles = new Vector3(0, 180, 0);

            FObjA.transform.localPosition = new Vector3(XOffsetA + -6, 10 + YDis, -1.5f);
            FObjA.transform.eulerAngles = new Vector3(180, 270, 0);

            FObjB.transform.localPosition = new Vector3(XOffsetB + 2, 10 + YDis, -ZDis + 1.5f);
            FObjB.transform.eulerAngles = new Vector3(180, 90, 0);
        }
    }
}
