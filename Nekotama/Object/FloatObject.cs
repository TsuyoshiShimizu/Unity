using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatObject : MonoBehaviour
{
    [SerializeField] private float YDis = 0;
    [SerializeField] private float XOffset = 0;
    [SerializeField] private GameObject SObj = null;
    [SerializeField] private GameObject FObj = null;

    [SerializeField] private Direction DirA = Direction.Left;
    [SerializeField] private Direction DirB = Direction.Left;

    private enum Direction { Right, Left }

    private float YFix = 14.0f;


    public float GetLimitPointY()
    {
        return transform.position.y + YDis + YFix;
    }
 
    private void OnValidate()
    {
        if(DirA == Direction.Right && DirB == Direction.Right)
        {
            SObj.transform.eulerAngles = new Vector3(0, 180, 0) + transform.eulerAngles;
            SObj.transform.localPosition = new Vector3(0, 1.9f, 1);
            FObj.transform.eulerAngles = new Vector3(180, 180, 0) + transform.eulerAngles;
            FObj.transform.localPosition = new Vector3(XOffset, 10 + YDis, -1);
        }
        else if(DirA == Direction.Left && DirB == Direction.Left)
        {
            SObj.transform.eulerAngles = new Vector3(0, 0, 0) + transform.eulerAngles;
            SObj.transform.localPosition = new Vector3(0, 1.9f, -1);
            FObj.transform.eulerAngles = new Vector3(180, 0, 0) + transform.eulerAngles;       
            FObj.transform.localPosition = new Vector3(XOffset, 10 + YDis, 1);
        }
        else if (DirA == Direction.Right && DirB == Direction.Left)
        {
            SObj.transform.eulerAngles = new Vector3(0, 180, 0) + transform.eulerAngles;
            SObj.transform.localPosition = new Vector3(0, 1.9f, 1);
            FObj.transform.eulerAngles = new Vector3(180, 0, 0) + transform.eulerAngles;
            FObj.transform.localPosition = new Vector3(XOffset -4.0f, 10 + YDis, 1);
        }
        else
        {
            SObj.transform.eulerAngles = new Vector3(0, 0, 0) + transform.eulerAngles;
            SObj.transform.localPosition = new Vector3(0, 1.9f, -1);
            FObj.transform.eulerAngles = new Vector3(180, 180, 0) + transform.eulerAngles;
            FObj.transform.localPosition = new Vector3(XOffset + 4.0f, 10 + YDis, -1);
        }
    }
}
