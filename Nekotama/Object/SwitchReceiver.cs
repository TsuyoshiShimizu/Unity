using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchReceiver : MonoBehaviour
{
    [SerializeField] private GameObject[] EffectObj = null;
    private bool TargetFlag = false;
 //   private Effekseer.EffekseerEmitter Effect;
    private SwitchController Controller;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Attack" && Controller.SFlag)
        {
            if (other.gameObject.GetComponent<MagicController>() != null)
            {
                MagicController MCon = other.gameObject.GetComponent<MagicController>();
                if (MCon.MType != 1) MCon.MagicHit();       
            }

            StopTarget();
            GameDirector.RemoveTarget(gameObject);
            Controller.SwitchChange();
            GameManager.playSystemSE(1);
          //  EffectObj[1].SetActive(false);
            EffectObj[1].SetActive(true);
          //  Effect.Play(1);
            GetComponent<Collider>().enabled = false;
            GetComponent<MeshRenderer>().enabled = false;
            Invoke("OffSwitch", 1.0f);
        }
    }

    private void OffSwitch()
    {
        gameObject.SetActive(false);
        GetComponent<Collider>().enabled = true;
        GetComponent<MeshRenderer>().enabled = true;
    }

    public void onSwitchClick()
    {
        if (TargetFlag) //ターゲットを解除
        {
            StopTarget();
            GameManager.playStageSE(13);
            GameDirector.RemoveTarget(this.gameObject);
        }
        else            //ターゲットする
        {
            StartTarget();
            GameManager.playStageSE(12);
            if (GameDirector.MagicType == 1)
            {
                GameDirector.ChangeTarget(this.gameObject);
            }
            else if (GameDirector.MagicType == 2)
            {
                GameDirector.AddTarget(this.gameObject);
            }
        }
    }

    public void onSwitchDrag()
    {
        if (TargetFlag) return;
        StartTarget();
        GameManager.playStageSE(12);
        GameDirector.AddTarget(gameObject);
    }

    private void StartTarget()
    {
        TargetFlag = true;
        EffectObj[0].SetActive(true);
    }

    public void StopTarget()
    {
        TargetFlag = false;
        EffectObj[0].SetActive(false);
    }

    private void FixedUpdate()
    {
        if (GameDirector.EnemyTargetFlag == false && TargetFlag) StopTarget();
    }

    // Start is called before the first frame update
    void Start()
    {
       // Effect = GetComponent<Effekseer.EffekseerEmitter>();
      //  aud = GetComponent<AudioSource>();
        Controller = GetComponentInParent<SwitchController>();
    }
}
