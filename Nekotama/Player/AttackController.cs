using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    [SerializeField] private bool PlayOnStart = true;           //作成した瞬間に起動するか
    [SerializeField] private float StartContactTime = 0.0f;     //当たり判定の開始時間
    [SerializeField] private float EndContactTime = 2.0f;       //当たり判定消失までの時間
    [SerializeField] private float DeleteTime = 3.0f;           //当たり判定消失までの時間
   
  //  [SerializeField] private bool FinishEffectFlag = false;     //エフェクト消失時に次のエフェクトを再生させるか
    [SerializeField] private bool SEFlag = false;
    [SerializeField] private float SEPlayLag = 0f;
    private bool MRFlag = false;

    private GameDirector Director;
    
    private void OnTriggerEnter(Collider other)
    {
       // Debug.Log("テストA"　+ other.gameObject.tag);

        if ( (other.gameObject.tag == "Enemy" || other.gameObject.tag == "EnemyHiter") && MRFlag)
        {
           // Debug.Log("MP回復");
            MRFlag = false;
            float MPow = GameManager.PlayerMPow;
            if (GameManager.eventCount[10] >= 1) MPow *= 1.5f;
            Director.PlayerMP += MPow / 10f + 1f;
        }
    }
    
    //プレイヤースクリプトで実行
    public void PlayAttack()
    {
        MRFlag = true;
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
        Director = GameManager.DirectorObj.GetComponent<GameDirector>();
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
        MRFlag = false;
    }

    private void SEPlay()
    {
        GetComponent<AudioSource>().Play();
    }


    private void Delete()
    {
        Debug.Log("消失");
        Destroy(gameObject);
    }
}
