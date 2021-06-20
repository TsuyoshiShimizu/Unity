using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EnemyDamageController : MonoBehaviour
{
    public float eHP { set; get;}       //敵のHP
    public float eMaxHP { set; get; }   //敵の最大HP
    public int ePow { set; get; }
    private int eDef = 1;               //敵の防御力
    private int eExp = 1;               //敵の経験値
    private int eCoin = 1;              //敵のコイン

  //  private bool DamageStop = true;        //敵が攻撃をくらうと硬直するか

    public bool DamageStopFlag { set; get; }

   // public float StopTime { set; get; }
    public float NoAttackTime { set; get; }
    public bool GuardFlag { set; get; }

    public bool setupFinsh { set; get; }
    public bool Mode { set; get; }
    public bool Direction { set; get; }
    public int ELv { set; get; }
    public int ActiveTypeNum { set; get; }
    public bool DamageActive { set; get; }
    public string eName { set; get; }
    //private float eMaxHp = 100;

    private Vector3 SummonPoint = Vector3.zero;
    private float DeleteDis = 100;
    private float DamageActiveDleta = 0;

    public bool StopFlag { private set; get; } 
    private bool DamageFlag = false;
    private Rigidbody rigid;
    private Material material;
    private Shader TransShader;
    private Renderer rendar;
    private Effekseer.EffekseerEmitter Effect;
    private EnemyGenerator EnemyGene;

    private bool deleteFlag = false;

    private float DameDelta = 0;
    private bool MetalFlag = false;

    //変更
    private EnemyHitController EHCon;
    private GameDirector Director;
    private bool kill = false;

    private void Awake()
    {
        setupFinsh = false;
        DamageStopFlag = false;
        NoAttackTime = 2.0f;
        EHCon = GetComponentInChildren<EnemyHitController>();
        DamageActive = false;
    }

    /// <summary>
    /// 初期処理
    /// </summary>
    void Start()
    {
        Effect = GetComponent<Effekseer.EffekseerEmitter>();
        rigid = GetComponent<Rigidbody>();
        material = GetComponent<Renderer>().material;
        Director = GameManager.DirectorObj.GetComponent<GameDirector>();
        TransShader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
        rendar = GetComponent<Renderer>();
        StopFlag = false;
        Invoke("DamageP", 0.5f);
    }

    /// <summary>
    /// 常時処理
    /// </summary>
    private void FixedUpdate()
    {
        enemyDistanceDelete();
        enemyRecoverNoDamage();
    }

    /// <summary>
    /// 衝突処理
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Attack")
        {
            if (DamageFlag && !GuardFlag)
            {
                DamageFlag = false;
                EHCon.PConFlag = false;
                bool ThunderFlag = false;
                bool MagicFlag = false;
                DamageActive = true;
                DamageActiveDleta = 0;

                if (other.gameObject.GetComponent<MagicController>() != null)
                {
                    MagicFlag = true;
                    MagicController MCon = other.gameObject.GetComponent<MagicController>();
                    if (MCon.MType == 1) ThunderFlag = true;
                    if (!ThunderFlag) MCon.MagicHit();
                }
                float Pow = GameManager.PlayerPow;
                float MPow = GameManager.PlayerMPow;
                if (GameManager.eventCount[9] >= 1) Pow += 1.5f;
                if (GameManager.eventCount[10] >= 1) MPow += 1.5f;

                if (ThunderFlag)//雷魔法
                {
                    if(MPow < eDef)
                    {
                        eHP -= GameManager.thnderMagicPower;
                    }
                    else
                    {
                        eHP -= GameManager.thnderMagicPower * MPow / eDef;
                    }             
                }
                else if (MagicFlag)
                {
                    eHP -= GameManager.normalPlayerAttackPower * MPow / eDef;
                }
                else
                {
                    eHP -= GameManager.normalPlayerAttackPower * Pow / eDef;
                }

                Director.viewEnemyGauge(eName, eMaxHP, eHP);

                if (eHP > 0)
                {
                    // if (DamageStop) LittleStop();
                    DamageStopFlag = true;
                    Effect.Play(1);
                    if(MetalFlag && !ThunderFlag ) GameManager.playStageSE(37); else GameManager.playStageSE(10);
                }
                else
                {
                    EnemyDelete(true);
                }
                Invoke("DamageP", NoAttackTime);
            }

            if (GuardFlag)
            {
                Effect.Play(3);
                GameManager.playStageSE(9);
            }
        }
    }

 

    /// <summary>
    /// ダメージによる無敵の点滅処理
    /// </summary>
    private void enemyRecoverNoDamage()
    {
        if (!DamageFlag)
        {
            DameDelta += 0.02f;
            if (DameDelta >= 0.1f)
            {
                DameDelta = 0;
                rendar.enabled = !rendar.enabled;
            }
        }
    }

    /// <summary>
    /// ダメージによるアクティブ切り替え処理
    /// </summary>
    private void DamageActiveRecover()
    {
        if (!DamageActive) return;
        DamageActiveDleta += 0.02f;
        if(DamageActiveDleta >= 10)
        {
            DamageActiveDleta = 0;
            DamageActive = false;
        }
    }

    /// <summary>
    /// 敵の消滅処理
    /// </summary>
    public void EnemyDelete(bool killFlag)
    {
        if (killFlag)
        {
            GameManager.playStageSE(11);
            Effect.Play(0);
            EnemyGene.EnemyDeleteCount(true);
            kill = true;
        }
        else
        {
            Effect.Play(2);
            EnemyGene.EnemyDeleteCount(false);
        }
        material.shader = TransShader;
        EHCon.StopTarget();                         //ターゲットを解除
        GameDirector.RemoveTarget(gameObject);      //ターゲットオブジェクトを消去
        GetComponent<Collider>().enabled = false;   //敵コライダーの無効化
        rigid.isKinematic = true;
        Color color = material.color;
        color.a = 0;
        material.color = color;
        Invoke("delete", 1.0f);
    }
    private void delete()
    {
        if (kill) Director.GetExp(eExp, eCoin);
        Destroy(gameObject);
    }

    /// <summary>
    /// ダメージ判定の回復処理
    /// </summary>
    private void DamageP()
    {
        DamageFlag = true;
        EHCon.PConFlag = true;
        rendar.enabled = true;
    }



    /// <summary>
    /// 敵のセットアップ
    /// </summary>
    /// <param name="GeneObj"></param>
    /// <param name="lv"></param>
    /// <param name="Ename"></param>
    /// <param name="mode"></param>
    /// <param name="direction"></param>
    /// <param name="deletedis"></param>
    public void EnemySetup(GameObject GeneObj, int lv, string Ename, bool mode, bool direction, float deletedis,bool metalFlag ,int AType)
    {
        SummonPoint = GeneObj.transform.position;
        EnemyGene = GeneObj.GetComponent<EnemyGenerator>();
        Mode = mode;
        Direction = direction;
        DeleteDis = deletedis;
        ELv = lv;

        int[] Estatas = GameManager.getEnemyStatas(lv, Ename);
        eExp = Estatas[0];
        eCoin = Estatas[1];
        eHP = Estatas[2];
        ePow = Estatas[3];
        eDef = Estatas[4];

        eMaxHP = Estatas[2];
        eName = Ename;

        EHCon.AttackPower = ePow;
        MetalFlag = metalFlag;
        ActiveTypeNum = AType;

        setupFinsh = true;
    }

    /// <summary>
    /// 敵の距離による消失処理
    /// </summary>
    private void enemyDistanceDelete()
    {
        if (deleteFlag) return;
        if (!setupFinsh) return;
        Vector3 EnemyDis = transform.position - SummonPoint;
        if (EnemyDis.magnitude < DeleteDis) return;
        deleteFlag = true;
        EnemyDelete(false);
    }
    
}
