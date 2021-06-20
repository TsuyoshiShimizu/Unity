using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChirdTorchSwitch : MonoBehaviour
{
 //   private int SwitchNum = 0;
    [SerializeField] GameObject Fire = null;
    [SerializeField] AudioClip[] TorchSE = null;

    private float GoOutTime = 0;

    private bool SwitchFlag = true;
    private bool TargetFlag = false;

    public bool TorchFlag { set; get; }
    public bool TorchLock { set; get; }

    private Effekseer.EffekseerEmitter Effect;
    private AudioSource aud;

    private ParentSwitchTorch PSTorch;

    private float GoOutDelta = 0;

    private void Start()
    {
        Effect = GetComponent<Effekseer.EffekseerEmitter>();
        aud = GetComponent<AudioSource>();
        PSTorch = GetComponentInParent<ParentSwitchTorch>();
        TorchFlag = false;
        TorchLock = false;
    }

    public void Initialize(float goouttime)
    {
        GoOutTime = goouttime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Attack" && SwitchFlag && other.gameObject.GetComponent<MagicController>() != null )
        {
            SwitchFlag = false;
            StopTarget();
            GameDirector.RemoveTarget(gameObject);
            TorchFlag = true;
            PSTorch.SwitchCheck();
            Fire.SetActive(true);
            aud.PlayOneShot(TorchSE[2]);
        }
    }

    private void FixedUpdate()
    {
        if (GoOutTime > 0 && !SwitchFlag && !TorchLock)
        {
            GoOutDelta += 0.02f;
            if(GoOutDelta > GoOutTime)
            {
                GoOutDelta = 0;
                SwitchFlag = true;
                TorchFlag = false;
                PSTorch.SwitchCheck();
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
            aud.PlayOneShot(TorchSE[1]);
            GameDirector.RemoveTarget(gameObject);
        }
        else            //ターゲットする
        {
            StartTarget();
            aud.PlayOneShot(TorchSE[0]);
            if (GameDirector.MagicType == 1)
            {
                GameDirector.ChangeTarget(gameObject);
                Debug.Log("ターゲット切り替え");
            }
            else if (GameDirector.MagicType == 2)
            {
                GameDirector.AddTarget(gameObject);
                Debug.Log("ターゲット追加");
            }
        }
    }

    public void onMTorchDrag()
    {
        if (TargetFlag) return;
        StartTarget();
        GameManager.playStageSE(12);
        GameDirector.AddTarget(gameObject);
    }

    private void StartTarget()
    {
        TargetFlag = true;
        Effect.Play(0);
    }

    public void StopTarget()
    {
        TargetFlag = false;
        Effect.Stop(0);
    }
}
