using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatSensor : MonoBehaviour
{
    [SerializeField] private bool ZMoveFlag = false;
    [SerializeField] private int SensorNumber = 0;
    [SerializeField] private float NextContactTime = 0;
    public int Number { get { return SensorNumber; } }
    private float delta = 0;
    private bool ContactFlag = true;
    public bool isContact { set { ContactFlag = value; } get { return ContactFlag; }  }
    public float LimitPoint { get { return LimitP; } }
    private float LimitP = 0;

    private float ZPosition = 0;
    public float ChangeZPos() { return ZPosition; }

    private bool ZFrontFlag = true;
    public bool ZFrontFlagGet() { return ZFrontFlag; }

    private void FixedUpdate()
    {
        if (!isContact)
        {
            delta += 0.02f;
            if(delta > NextContactTime)
            {
                delta = 0;
                isContact = true;
            }
        }
    }

    private void Start()
    {
        if(!ZMoveFlag && SensorNumber == 2) LimitP = GetComponentInParent<FloatObject>().GetLimitPointY();
        if(ZMoveFlag && SensorNumber == 2) LimitP = GetComponentInParent<FloatObjectB>().GetLimitPointY();
        if(SensorNumber == 5)
        {
            LimitP = GetComponentInParent<FloatObjectB>().GetLimitPointZ();
            ZFrontFlag = GetComponentInParent<FloatObjectB>().GetFrontMoveFlag();
        } 
        if(SensorNumber == 7) ZPosition = GetComponentInParent<FloatObjectB>().GetZPos();
    }
}
