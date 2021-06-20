using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicController : MonoBehaviour
{
    [SerializeField] private bool PlayOnStart = true;           //作成した瞬間に起動するか
    [SerializeField] private float StartContactTime = 0.0f;     //当たり判定の開始時間
    [SerializeField] private float EndContactTime = 2.0f;       //当たり判定消失までの時間
    [SerializeField] private float DeleteTime = 3.0f;           //当たり判定消失までの時間
    [SerializeField] private bool ContactDelete = false;        //敵と接触時にエフェクトを消失させるか

    [SerializeField] private bool FinishEffectFlag = false;         //エフェクト消失時に次のエフェクトを再生させるか
    [SerializeField] private GameObject FinishiEffect = null;   //終了時に再生させるエフェクト
    [SerializeField] private bool SEFlag = false;
    [SerializeField] private float SEPlayLag = 0f;

    [SerializeField] private int mtype = 0;
    public int MType { get { return mtype;  } }

    private GameObject homingObj;
    private bool homingFlag = false;

    public void MagicHit()
    {
        if (ContactDelete)
        {
            if (FinishEffectFlag)
            {
                GameObject EndFire = Instantiate(FinishiEffect, transform.position, transform.rotation);
            }
            Delete();
        }
    }


    //プレイヤースクリプトで実行
    public void PlayAttack()
    {
        GetComponent<Effekseer.EffekseerEmitter>().Play(0);
        Invoke("StartContact", StartContactTime);
        Invoke("EndContact", EndContactTime);
        if (SEFlag)
        {
            Invoke("SEPlay", SEPlayLag);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PlayOnStart)
        {
            GetComponent<Effekseer.EffekseerEmitter>().Play(0);
            Invoke("StartContact", StartContactTime);
            Invoke("EndContact", EndContactTime);
            Invoke("Delete", DeleteTime);
            if (SEFlag)
            {
                Invoke("SEPlay", SEPlayLag);
            }
        }
    }

    private void Awake()
    {
        GetComponent<Collider>().enabled = false;
    }

   

    private void StartContact()
    {
        GetComponent<Collider>().enabled = true;
    }

    private void EndContact()
    {
        GetComponent<Collider>().enabled = false;
    }

    private void SEPlay()
    {
        //GetComponent<AudioSource>().Play();
        if (gameObject.name == "FireBall(Clone)") GameManager.playLoopSE(1);
        if (gameObject.name == "FireBall_End(Clone)") GameManager.playStageSE(14);
        if (gameObject.name == "Thunder(Clone)") GameManager.playStageSE(15);
    }

    private void Delete()
    {
        if (gameObject.name == "FireBall(Clone)") GameManager.stopLoopSE();
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        Homing();
    }

    public void HomingStart(GameObject HomingObj)
    {
        homingObj = HomingObj;
        homingFlag = true;
    }

    private void Homing()
    {
        if (!homingFlag) return;
        if (homingObj == null) return;
        transform.position = homingObj.transform.position;
    }
}
