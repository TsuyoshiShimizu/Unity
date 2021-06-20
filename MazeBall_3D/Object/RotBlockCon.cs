using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotBlockCon : MonoBehaviour
{
    [SerializeField] float RotateTime = 5.0f;
    [SerializeField] float RotateSita = 90.0f;
    [SerializeField] GameObject EffectObject = null;

    //1:下面　2;上面　3:正面　4:奥面　5:右側面　6:左側面
    private int SurNum = 0;
    
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player" && !StageCon.StageRotLock && SurNum != 0 && SurNum != StageCon.StageSurface)
        {
            StageCon.StageRotLock = true;
            StageCon.RotStage(SurNum, RotateTime, RotateSita);
            StageCon.RotEffect = EffectObject;
            EffectObject.SetActive(true);

          //  Debug.Log(gameObject.name + "  SurNum:" + SurNum + "  RTime:" + RotateTime + "  RSita:" + RotateSita);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //姿勢の判定
        Vector3 R = transform.eulerAngles;
        if(R.y == 0)
        {
            if (R == Vector3.zero) SurNum = 1;
            if (R.x == 90f && R.z == 0) SurNum = 3;
            if (R.x == 270f && R.z == 0) SurNum = 4;
            if (R.z == 90f && R.x == 0) SurNum = 5;
            if (R.z == 270f && R.x == 0) SurNum = 6;
        }
        else
        {
            if (R.y == 180f && R.z == 180f) SurNum = 2;
        }

       // Debug.Log(gameObject.name + ":" + R + "  SNum" + SurNum);
    }
}
