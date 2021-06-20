using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchSwitch : MonoBehaviour
{
    [SerializeField] private float LightIntensity = 3.0f;
    [SerializeField] private float LightRange = 30.0f;
    [SerializeField] private float GoOutTime = 0;
    [SerializeField] private int SwitchNum = 0;
    [SerializeField] private bool SpotLightFlag = false;
    [SerializeField] GameObject Fire = null;
    [SerializeField] GameObject TargetEffect = null;
    [SerializeField] GameObject SpotLightObj = null;
  //  [SerializeField] AudioClip[] TorchSE = null;

    private bool SwitchFlag = true;
    private bool TargetFlag = false;

  //  private Effekseer.EffekseerEmitter Effect;
  //  private AudioSource aud;

    private float GoOutDelta = 0;

    private void Start()
    {
        Light TorchLight = Fire.GetComponent<Light>();
        TorchLight.intensity = LightIntensity;
        TorchLight.range = LightRange;
        SpotLightObj.SetActive(SpotLightFlag);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Attack" && SwitchFlag && other.gameObject.GetComponent<MagicController>() != null )
        {
            SwitchFlag = false;

            StopTarget();
            GameDirector.RemoveTarget(gameObject);
            if (SwitchNum > 0) GameDirector.PlayObjAction(SwitchNum);
            Fire.SetActive(true);
            GameManager.playStageSE(7);
        }
    }

    private void FixedUpdate()
    {
        if (GoOutTime > 0 && !SwitchFlag)
        {
            GoOutDelta += 0.02f;
            if(GoOutDelta > GoOutTime)
            {
                GoOutDelta = 0;
                SwitchFlag = true;
                if (SwitchNum > 0) GameDirector.PlayObjAction(SwitchNum);
                Fire.SetActive(false);
            }
        }

        if (GameDirector.EnemyTargetFlag == false && TargetFlag) StopTarget();
    }

    public void onClick()
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

    public void onTorchDrag()
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
    }

    public void StopTarget()
    {
        TargetFlag = false;
        TargetEffect.SetActive(false);
    }
}
