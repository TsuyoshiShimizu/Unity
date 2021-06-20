using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitController : MonoBehaviour
{
    [SerializeField] GameObject TargetEffect = null;
    public int AttackPower = 10;
    public bool PConFlag = false;
    private bool TargetFlag = false;
  //  private Camera camera;

    //ターゲットの処理
    public void onEnemyClick()
    {
        if (TargetFlag) //ターゲットを解除
        {
            StopTarget();
            GameManager.playStageSE(13);
            GameDirector.RemoveTarget(gameObject);
        }
        else            //ターゲットする
        {
            StartTarget();
            GameManager.playStageSE(12);
            if (GameDirector.MagicType == 1)
            {
                GameDirector.ChangeTarget(gameObject);
            }
            else if (GameDirector.MagicType == 2)
            {
                GameDirector.AddTarget(gameObject);
            }
        }
    }

    public void onEnemyDrag()
    {
        if (TargetFlag) return;
        StartTarget();
        GameManager.playStageSE(12);
        GameDirector.AddTarget(gameObject);
    }

    private void StartTarget()
    {
        TargetFlag = true;
        TargetEffect.SetActive(true);
       // Effect.Play(0);
    }
    public void StopTarget()
    {
        TargetFlag = false;
        TargetEffect.SetActive(false);
      //  Effect.Stop(0);
    }


    private void FixedUpdate()
    {
        //詠唱解除によるターゲット解除の監視
        if (GameDirector.EnemyTargetFlag == false && TargetFlag) StopTarget();
    }
}
