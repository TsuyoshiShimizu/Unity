using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchController : MonoBehaviour
{
    [SerializeField] private int SwitchNumber = 0;
    [SerializeField] private GameObject ONSwitchObj = null;
    [SerializeField] private GameObject OFFSwitchObj = null;

    private bool SwitchON = false;
    private bool SwitchFlag = true;
    public bool SFlag { get { return SwitchFlag; } }
    private float TimeDelta = 0;

    public void SwitchChange()
    {
        if (SwitchFlag)
        {
            SwitchFlag = false;
            if (SwitchON)
            {
                SwitchON = false;
                OFFSwitchObj.SetActive(true);
            }
            else
            {
                SwitchON = true;
                ONSwitchObj.SetActive(true);
            }
            EventManager.eventManager.PlayObjAction(SwitchNumber);
           // GameDirector.PlayObjAction(SwitchNumber);

        }
    }

    private void FixedUpdate()
    {
        if (!SwitchFlag)
        {
            TimeDelta += 0.02f;
            if(TimeDelta >= 1)
            {
                TimeDelta = 0;
                SwitchFlag = true;
            }
        }
    }
}
