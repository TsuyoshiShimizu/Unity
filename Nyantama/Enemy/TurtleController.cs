using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleController : MonoBehaviour
{
    [SerializeField] int AttackCoumt = 1;           //攻撃回数
    [SerializeField] float AttackCountNum = 0;      //攻撃回数増加係数
    [SerializeField] float Dis2D = 5.0f;            //敵が進む距離
    [SerializeField] GameObject[] AuraEffect = null;

    private bool Mode3D = true;             //3Dかどうか
    private bool Direction2D = true;        //初期の敵が進む方向
    private float MaxSpeed = 1.0f;          //移動のマックススピード
    private float ActiveDis = 10.0f;        //アクティブに変化する距離
    private float AttackDis = 5.0f;         //攻撃を行う距離
    private float AttackSpeed = 10;         //攻撃スピード

    private Animator animator;              //敵のアニメータ
    private Rigidbody rigid;                //敵の物理ボディ
    private bool StartFlag = false;

    private float BfoAttackTime = 2.0f;     //スピン待機時間
    private float AttackIntreval = 5.0f;    //次の攻撃を行うまでの間隔
    private float AttackDelta = 0;

    private Vector2 Speed;                          //敵のスピード
    private Vector3 PlayerDis;                      //プレイヤーまでの距離
   
    private float force = 20.0f;                    //移動に与える力
    private float ZPos;                             //初期のZ位置
    private Vector2 BfoPos;                         //直前の敵の位置

    private float XminPos;                          //2Dモード時の右最大移動量
    private float XmaxPos;                          //2Dモード時の左最大移動量

    private EnemyDamageController enemyDamageController;
    private int EStatas = 0;
    private int BfoEStatas = 0;
    private bool AttackPlaying = false;
    private bool AttackFlag = true;
    private int NowAttackCount = 0;
    private float AttackTime = 0;
    private Vector3 AttackV = Vector3.zero;
    private bool FinishSetup = false;

    private int AttackStatas = 0;
    private float AttackChangeDelta = 10f;

    private int ActiveNum = 0;
    private float KnockBackTime = 1.0f;
    private bool KnockBackFlag = false;

    private void Awake()
    {
        enemyDamageController = GetComponent<EnemyDamageController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        BfoPos = new Vector2(transform.position.x, transform.position.z);

        ZPos = transform.position.z;
        XminPos = transform.position.x - Dis2D;
        XmaxPos = transform.position.x + Dis2D;
        Invoke("ActiveStart", 2.7f);
        rigid.maxAngularVelocity = 30;
        AttackTime = AttackDis * 2 / AttackSpeed;
    }

    private void ActiveStart()
    {
        StartFlag = true;
    }

    public void Initialize()
    {
        Mode3D = enemyDamageController.Mode;
        Direction2D = enemyDamageController.Direction;
        int Lv = enemyDamageController.ELv;
        if (Lv >= 10) Lv = 1;
        Material mat = GetComponent<Renderer>().material;

        MaxSpeed = MaxSpeed + 0.1f * Lv;                            //移動スピード
        AttackSpeed = AttackSpeed + 1.5f * Lv;                      //攻撃スピード
        AttackCoumt = AttackCoumt + (int)(AttackCountNum * Lv);     //攻撃回数
        BfoAttackTime = BfoAttackTime - BfoAttackTime / 15f * Lv;   //攻撃溜め時間
        ActiveDis = ActiveDis + 1.2f * Lv;                          //アクティブになるプレイヤーの距離
        AttackDis = AttackDis + 1.2f * Lv;                          //攻撃可能になるプレイヤーの距離

        if (Lv == 2 || Lv == 5 || Lv == 8) mat.color = new Color32(170, 170, 170, 1);
        if (Lv == 3 || Lv == 6 || Lv == 9) mat.color = new Color32(100, 100, 100, 1);
        if (4 <= Lv && Lv <= 6) AuraEffect[1].SetActive(true);
        if (7 <= Lv && Lv <= 9) AuraEffect[0].SetActive(true);

        ActiveNum = enemyDamageController.ActiveTypeNum;
        FinishSetup = true;
    }

    private void FixedUpdate()
    {
        if (!FinishSetup && enemyDamageController.setupFinsh) Initialize();

        if (!StartFlag) return;
        //プレイヤーの敵の距離
        PlayerDis = PlayerController.PlayerPos - transform.position;
        Speed = new Vector2(rigid.velocity.x, rigid.velocity.z);

        //敵のアクティブフラグの変更
        if (!AttackPlaying)
        {
            if (PlayerDis.magnitude < AttackDis && AttackFlag) EStatas = 2;
            else if (PlayerDis.magnitude <= ActiveDis   //敵の位置がアクティブ範囲内
                || ActiveNum == 2                       //敵のタイプがアグレッシブの場合常にアクティブ以上
                || (EStatas >= 1 && ActiveNum == 1)     //敵のタイプがノーマルの場合一度アクティブになるとずっとアクティブ
                || enemyDamageController.DamageActive)  //敵が攻撃をくらった時にアクティブになる
                EStatas = 1;
            else EStatas = 0;
        }

        if (EStatas == 1) //歩行時
        {
            PlayMove();
        }
        else if (EStatas == 0)     //ノンアクティブ時の行動
        {
            PlayIdle();
        }
        else                      //攻撃時の処理
        {
            PlayAttack();
        }
        RecoverAttack();

        BfoPos = new Vector2(transform.position.x, transform.position.z);
    }

    /// <summary>
    /// 待機状態の処理
    /// </summary>
    private void PlayIdle()
    {
        if (EStatas != BfoEStatas)//変化時一回だけ実行
        {
            BfoEStatas = EStatas;
            animator.SetInteger("EStatas", EStatas);
        }
        rigid.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// 移動状態の処理
    /// </summary>
    private void PlayMove()
    {
        if (KnockBackFlag) return;

        if (EStatas != BfoEStatas)//変化時一回だけ実行
        {
            BfoEStatas = EStatas;
            animator.SetInteger("EStatas", EStatas);
        }
        if (Mode3D) //3Dモードの時
        {
            Vector2 Dir = new Vector2(PlayerDis.x, PlayerDis.z);
            if (Speed.magnitude <= MaxSpeed) rigid.AddForce(new Vector3(force * Dir.normalized.x, 0, force * Dir.normalized.y));
        }
        else     //2Dモードの時
        {
            if (transform.position.x <= XminPos)
            {
                Direction2D = true;
                transform.position = new Vector3(transform.position.x, transform.position.y, ZPos);
            }

            if (transform.position.x >= XmaxPos)
            {
                Direction2D = false;
                transform.position = new Vector3(transform.position.x, transform.position.y, ZPos);
            }

            if (Speed.magnitude <= MaxSpeed)
            {
                if (Direction2D) rigid.AddForce(new Vector3(force, 0, 0)); else rigid.AddForce(new Vector3(-force, 0, 0));
            }
        }
        Vector2 Pos = new Vector2(transform.position.x, transform.position.z);
        Vector2 DPos = Pos - BfoPos;
        if (DPos.magnitude > 0.01f) transform.rotation = Quaternion.LookRotation(new Vector3(DPos.x, 0, DPos.y));
        rigid.angularVelocity = Vector3.zero;
    }

    /// <summary>
    /// 攻撃状態の処理
    /// </summary>
    private void PlayAttack()
    {
        if (KnockBackFlag) return;

        if (EStatas != BfoEStatas)//変化時一回だけ実行
        {
            AttackPlaying = true;
            AttackFlag = false;
            AttackStatas = 0;
            BfoEStatas = EStatas;
            animator.SetInteger("EStatas", EStatas);

            AttackChangeDelta = BfoAttackTime;
           // Invoke("Attack", BfoAttackTime);
        }
        rigid.angularVelocity = Vector3.up * 25;
        rigid.velocity = AttackV;

        if (AttackPlaying)
        {
            AttackChangeDelta -= 0.02f;
            if(AttackChangeDelta <= 0)
            {
                if(AttackStatas == 0)       //待機状態終了
                {
                    AttackChangeDelta = AttackTime;
                    AttackStatas = 1;
                    if (AttackCoumt == NowAttackCount)
                    {
                        AttackPlaying = false;
                        NowAttackCount = 0;
                    }
                    else
                    {
                        NowAttackCount++;
                        if (Mode3D)
                        {
                            Vector2 PPos = new Vector2(PlayerController.PlayerPos.x, PlayerController.PlayerPos.z);
                            Vector2 EPos = new Vector2(transform.position.x, transform.position.z);
                            Vector2 AttackVec = (PPos - EPos).normalized;
                            AttackV = new Vector3(AttackVec.x, 0, AttackVec.y) * AttackSpeed;
                        }
                        else
                        {
                            bool Right = true;
                            if (PlayerController.PlayerPos.x < transform.position.x) Right = false;
                            if (Right) AttackV = Vector3.right * AttackSpeed; else AttackV = Vector3.left * AttackSpeed;
                        }
                    }
                }
                else if(AttackStatas == 1)  //突進状態終了
                {
                    AttackChangeDelta = BfoAttackTime;
                    AttackStatas = 0;
                    AttackV = Vector3.zero;
                    if(!Mode3D) transform.position = new Vector3(transform.position.x, transform.position.y, ZPos);
                }
            }
        }
    }

    /// <summary>
    /// 攻撃フラグ回復の処理
    /// </summary>
    private void RecoverAttack()
    {
        if (EStatas == 2) return;
        if (AttackFlag) return;
        AttackDelta += 0.02f;
        if (AttackDelta >= AttackIntreval)
        {
            AttackFlag = true;
            AttackDelta = 0;
        }
    }
    
    /// <summary>
    /// ノックバック処理
    /// </summary>
    public void KnockBack()
    {
        KnockBackFlag = true;
        rigid.Sleep();
        animator.SetTrigger("Hit");
        Vector2 HitVec = new Vector2(transform.position.x - PlayerController.PlayerPos.x
            , transform.position.z - PlayerController.PlayerPos.z).normalized;
        float PowX = 200;
        float PowY = 200;

        rigid.AddForce(HitVec.x * PowX, PowY, HitVec.y * PowX);

        Invoke("EndKnockBack", KnockBackTime);
    }
    private void EndKnockBack()
    {
        if (AttackPlaying) AttackChangeDelta = 0;
        KnockBackFlag = false;
    }
}
