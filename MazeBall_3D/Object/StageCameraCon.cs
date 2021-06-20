using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// ステージカメラの透過処理を行う
/// </summary>
public class StageCameraCon : MonoBehaviour
{

    private float R = 1f;
    private float underP = 0.1f;
    private float uperP = 2.0f;

    private void Start()
    {
        float a = uperP - underP;
        float b = a * (GameManager.eventCount[3] / 100.0f);
        R = underP + b;
    }

    private void FixedUpdate()
    {
        //ステージオブジェクトがプレイヤーと重なった時の透過処理
        Ray ray = new Ray(transform.position, -(transform.position - PlayerCon.PlayerPos));
        float Pdis = (PlayerCon.PlayerPos - transform.position).magnitude;
        //if (Physics.Raycast(ray))
        if (Physics.SphereCast(ray, R))
        {
         //   RaycastHit[] hits = Physics.RaycastAll(ray);
            RaycastHit[] hits = Physics.SphereCastAll(ray, R);
            for (int i = 0; i < hits.Length; i++)
            {            
               if (hits[i].collider.tag == "StageObject" && hits[i].distance < Pdis - R * 1.5f)        
               {
                    ObjectTransparent TObj;
                    if (TObj = hits[i].collider.gameObject.GetComponent<ObjectTransparent>())
                    {
                        if (!TObj.TFlag) TObj.Transparent(); else TObj.TimerReset();                     
                    }
                }
            }
        }
    }
}
