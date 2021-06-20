using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentSwitchTorch : MonoBehaviour
{
    [SerializeField] private float GoOutTime = 0;
    [SerializeField] private int SwitchNum = 0;
    [SerializeField] private bool OFFFlag = false;

    private ChirdTorchSwitch[] CTSwitch;
    private bool ONState = false;

    // Start is called before the first frame update
    void Start()
    {
        CTSwitch = GetComponentsInChildren<ChirdTorchSwitch>();
        for(int i = 0; i < CTSwitch.Length; i++)
        {
            CTSwitch[i].Initialize(GoOutTime);
        }
    }

    public void SwitchCheck()
    {
        bool SwitchON = true;
        for (int i = 0; i < CTSwitch.Length; i++)
        {
            if (!CTSwitch[i].TorchFlag) SwitchON = false;
        }

        if (SwitchON)
        {
            GameDirector.PlayObjAction(SwitchNum);
            ONState = true;
            if (!OFFFlag)
            {
                for (int i = 0; i < CTSwitch.Length; i++)
                {
                    CTSwitch[i].TorchLock = true;
                }
            }
        }
        else
        {
            if(ONState && OFFFlag)
            {
                ONState = false;
                GameDirector.PlayObjAction(SwitchNum);
            }
        }
    }

  
}
