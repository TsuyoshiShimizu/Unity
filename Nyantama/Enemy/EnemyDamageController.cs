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

    private bool GuradEffectFlag = true;
    private float GuradEffectInterval = 1.0f;
    private float GuradEffectDelta = 0;

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

    private int HitType = 1;

    private Collider SelfCollider;
    private Collider HitterCollider;

    private void Awake()
    {
        setupFinsh = false;
        DamageStopFlag = false;
        NoAttackTime = 1.0f;
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

        SelfCollider = GetComponent<Collider>();
        HitterCollider = GetComponent<Collider>();
    }

    /// <summary>
    /// 常時処理
    /// </summary>
    private void FixedUpdate()
    {
        enemyDistanceDelete();
        enemyRecoverNoDamage();
    }

    private void Update()
    {
        if (GuradEffectFlag) return;
        GuradEffectDelta += Time.deltaTime;
        if(GuradEffectDelta >= GuradEffectInterval)
        {
            GuradEffectDelta = 0;
            GuradEffectFlag = true;
        }
    }

    /// <summary>
    /// 衝突処理
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
       // AttackHit(other.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        AttackHit(other.gameObject);
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
            EHCon.gameObject.SetActive(false);

            rigid.isKinematic = true;
            SelfCollider.enabled = false;
            HitterCollider.enabled = false;
        }
        else
        {
            Effect.Play(2);
            EnemyGene.EnemyDeleteCount(false);
        }
        material.shader = TransShader;
        EHCon.StopTarget();                         //ターゲットを解除
        GameDirector.RemoveTarget(EHCon.gameObject);      //ターゲットオブジェクトを消去
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

        if (eName == "Slime") HitType = 2;
        if (eName == "BlueSlime") HitType = 2;
        if (eName == "Turtle") HitType = 2;
        if (eName == "RedTurtle") HitType = 2;
        if (eName == "GoldSlime") HitType = 2;
        if (eName == "MetalSlime") HitType = 2;
        if (eName == "GoldTurtle") HitType = 2;
        if (eName == "MetalTurtle") HitType = 2;

       // if (HitType == 2) NoAttackTime = 1.0f;

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

    /// <summary>
    /// 攻撃ヒット処理
    /// </summary>
    /// <param name="AttackObj"></param>
    private void AttackHit(GameObject AttackObj)
    {
        if (AttackObj.tag != "Attack") return;

        //ダメージ処理
        if (DamageFlag && !GuardFlag && eHP > 0)
        {
            DamageFlag = false;
            EHCon.PConFlag = false;
            DamageActive = true;
            DamageActiveDleta = 0;
            bool MpRecoverFlag = false;

            float Pow = GameManager.PlayerPow * GameManager.BufferPow;
            float MPow = GameManager.PlayerMPow * GameManager.BufferMPow;
            string AttackName = AttackObj.name;
          //  bool ThunderFlag = false;
            int AttackPow = 10;
            int AttackType = 1;     //1:通常攻撃　2:魔法攻撃　3:雷魔法攻撃

            if (AttackName.Contains("Thunder"))
            {
                Debug.Log("Hit Thunder");
                AttackPow = 20;
                AttackType = 3;
            }
            else if(AttackName == "FireBall(Clone)" || AttackName == "FireBall_End(Clone)")
            {
                Debug.Log("Hit Fire");
                AttackObj.GetComponent<MagicController>().MagicHit();
                AttackPow = 10;
                AttackType = 3;
            }
            else if (AttackName == "Attack1(Clone)")
            {
                Debug.Log("Hit NormalAttack");
                AttackPow = 10;
                AttackType = 1;
                MpRecoverFlag = true;
                if (PlayerController.ComboPlusFlag[0])
                {
                    if (!GameManager.PlayerKingBuffer) PlayerController.ComboPlusFlag[0] = false;
                    PlayerController.ComboCount += 1;
                }
                PlayerController.ComboTime += 1.5f;
            }
            else if (AttackName == "Attack2")
            {
                Debug.Log("Hit SpinAttack");
                AttackPow = 10;
                AttackType = 1;
                MpRecoverFlag = true;
                if (PlayerController.ComboPlusFlag[5])
                {
                    if (!GameManager.PlayerKingBuffer) PlayerController.ComboPlusFlag[5] = false;
                    PlayerController.ComboCount += 1;
                }
                PlayerController.ComboTime += 1.5f;

            }
            else if (AttackName == "Attack3")
            {
                Debug.Log("Hit AirAttack");
                AttackPow = 10;
                AttackType = 1;
                MpRecoverFlag = true;
                if (PlayerController.ComboPlusFlag[6])
                {
                    if (!GameManager.PlayerKingBuffer) PlayerController.ComboPlusFlag[6] = false;
                    PlayerController.ComboCount += 1;
                }
                PlayerController.ComboTime += 1.5f;
            }
            else if (AttackName == "ComboPunch")
            {
                AttackPow = 10;
                AttackType = 1;
                if (GameManager.PlayerKingBuffer)
                {
                    PlayerController.ComboCount += 1;
                    PlayerController.ComboTime = 1.5f;
                }
            }
            else if (AttackName == "Grave")
            {
                AttackPow = 15;
                AttackType = 1;
                if (GameManager.PlayerKingBuffer)
                {
                    PlayerController.ComboCount += 1;
                    PlayerController.ComboTime = 1.5f;
                }
            }
            else if(AttackName == "FireEnhance(Clone)")
            {
                AttackPow = 8;
                AttackType = 2;
            }
            else if (AttackName == "ThunderEnhance(Clone)")
            {
                AttackPow = 8;
                AttackType = 3;
            }
            else if(AttackName == "ElectricityBall")
            {
                AttackPow = 8;
                AttackType = 3;
            }
            else if (AttackName == "RollingBall")
            {
                Debug.Log("Hit RollingAttack");
                AttackPow = 10;
                AttackType = 1;
                if (PlayerController.ComboPlusFlag[2])
                {
                    if(!GameManager.PlayerKingBuffer) PlayerController.ComboPlusFlag[2] = false;
                    PlayerController.ComboCount += 1;
                }
                PlayerController.ComboTime = 1.5f;
                //PlayerController.RollingComboTime = 1.5f;
            }
            else if (AttackName == "JumpComboAttack")
            {
                Debug.Log("Hit JumpCombogAttack");
                AttackPow = 10;
                AttackType = 1;
                if (PlayerController.ComboPlusFlag[4])
                {
                    if (!GameManager.PlayerKingBuffer) PlayerController.ComboPlusFlag[4] = false;
                    PlayerController.ComboCount += 1;
                }
                PlayerController.ComboTime = 1.5f;
                // if (PlayerController.PanishmentFlag) PlayerController.FinishComboTime = 20f;
            }
            else if (AttackName == "PanishmentAttack(Clone)")
            {
                Debug.Log("Hit PanishmentAttack");
                AttackPow = 5;
                AttackType = 1;
            }
            else if (AttackName == "PanishmentFinish")
            {
                Debug.Log("Hit PanishmentFinish");
                AttackPow = 30;
                AttackType = 1;
            }
            else if (AttackName == "SpinComboAttack")
            {
                Debug.Log("Hit SpinComboAttack");
                AttackPow = 10;
                AttackType = 1;
                if (PlayerController.ComboPlusFlag[1])
                {
                    if (!GameManager.PlayerKingBuffer) PlayerController.ComboPlusFlag[1] = false;
                    PlayerController.ComboCount += 1;    
                }
                PlayerController.ComboTime = 1.5f;
            }
            else if (AttackName == "AirSpinComboAttack")
            {
                Debug.Log("Hit AirSpinComboAttack");
                AttackPow = 10;
                AttackType = 1;
                if (PlayerController.ComboPlusFlag[3])
                {
                    if (!GameManager.PlayerKingBuffer) PlayerController.ComboPlusFlag[3] = false;
                    PlayerController.ComboCount += 1;    
                }
                PlayerController.ComboTime = 1.5f;
            }
            else if (AttackName == "FinishGrave")
            {
                Debug.Log("Hit FinishGrave");
                AttackPow = 15;
                AttackType = 1;
            }
            else if(AttackName == "Tornado")
            {
                Debug.Log("Hit Tornado");
                AttackPow = 12;
                AttackType = 1;
            }
            else
            {
                Debug.Log("AttackName " + AttackName);
                AttackPow = 10;
                AttackType = 1;
            }

            //敵のダメージ処理
            if(AttackType == 1) eHP -= AttackPow * Pow / eDef;
            else if(AttackType == 2) eHP -= AttackPow * MPow / eDef;
            else if(AttackType == 3)
            {
                if (MPow < eDef) eHP -= AttackPow; else eHP -= AttackPow * MPow / eDef;
            }


            Director.viewEnemyGauge(eName, eMaxHP, eHP);

            if (MpRecoverFlag)
            {
                Director.PlayerMP += MPow / 10f + 1f;
            }

            if (eHP > 0)
            {
                Change3DMode();                 //ドラゴン専用
                DamageStopFlag = true;         
                Effect.Play(1);                 //ヒットエフェクト
                //ヒット効果音
                if (MetalFlag && AttackType != 3) GameManager.playStageSE(37); else GameManager.playStageSE(10);

                //攻撃ヒットによるノックバック処理
                if(HitType == 2)
                {
                    if(eName == "Slime" || eName == "BlueSlime")
                    {
                        GetComponent<SlimeController>().KnockBack();
                    }

                    if (eName == "Turtle" || eName == "RedTurtle")
                    {
                        GetComponent<TurtleController>().KnockBack();
                    }
                }
            }
            else
            {
                EnemyDelete(true);
            }
            Invoke("DamageP", NoAttackTime);
        }

        //ガード処理
        if (GuardFlag && GuradEffectFlag)
        {
            GuradEffectFlag = false;
            Effect.Play(3);
            GameManager.playStageSE(9);
        }
    }

    private void Change3DMode()
    {
        if (eName != "BlueDragon" && eName != "RedDragon" && eName != "GreenDragon") return;
        if (Mode) return;
        if (ActiveTypeNum == 0) return;
        Mode = true;
        DoragonController DCon = GetComponent<DoragonController>();
        if (DCon != null) DCon.ChangeMode();
    }
}
