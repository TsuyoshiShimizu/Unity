using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSlimeController : MonoBehaviour
{
    [SerializeField] Material Lv2Mat = null;
    [SerializeField] Material Lv3Mat = null;
    [SerializeField] GameObject[] AuraEffect = null;

    private float MaxSpeed = 2.0f;         //移動のマックススピード
    private float AttackSpeed = 5;         //攻撃のスピード
    private float JumpHight = 6;           //ジャンプ攻撃の高さ
    private float ActiveDis = 200.0f;       //アクティブに変化する距離
    private float AttackDis = 12.0f;       //攻撃を行う距離
    private int AttackCoumt = 1;           //攻撃の連続使用数

    private float AttackIntreval = 6.0f;　          //次の攻撃を行うまでの間隔
    private float NextAttackTime = 3.0f;            //次のジャンプの使用
    private float EnemyGravity = 9.8f;              //ジャンプ時の落下加速度
    
    private Animator animator;                      //敵のアニメータ
    private Rigidbody rigid;                        //敵の物理ボディ
    private Material mat;
    private bool StartFlag = false;                 //初回の行動開始用フラグ
    private int EnemyStage = 1;

    private float Gravity = 9.8f;
    private float AttackDelta = 0;                  //次の攻撃可能までのリキャスト時間
    private float PlayAttackDelta = 100;            //連続攻撃のインターバル
    private float AttackJumpDelta = 100;            //ジャンプのタメ時間
    private float AttackMoveDelta = 100;            //ジャンプの時間
    private bool JumpFlag = false;                  //ジャンプ使用中
    

    private Vector2 Speed;                          //敵のスピード
    private Vector3 PlayerDis;                      //プレイヤーまでの距離

    private float force = 20.0f;                    //移動に与える力
    private Vector2 BfoPso;                         //直前の敵の位置(敵の向き変更に使用)

    private EnemyDamageController enemyDamageController;    //敵がくらうダメージの管理スクリプト
    private int EStatas = 0;                                //敵の状態
    private int BfoEStatas = 0;                             //敵の前の状態
    private bool AttackPlaying = false;                     //敵が攻撃中か
    private bool AttackFlag = true;                         //敵が攻撃可能か
    private int NowAttackCount = 0;                         //連続攻撃時の現在の回数               
    private Vector3 AttackVec = Vector3.zero;                 //敵の攻撃中の速度を管理

    private bool FinishSetup = false;
    private float StopTime = 2.0f;
    private float DamageStopDelta = 0;

    private void Awake()
    {
        enemyDamageController = GetComponent<EnemyDamageController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();                                
        rigid = GetComponent<Rigidbody>();
        mat = GetComponent<Renderer>().material;      
        BfoPso = new Vector2(transform.position.x, transform.position.z);
        Invoke("ActiveStart", 1.5f);   //初回動作開始
    }

    private void ActiveStart()
    {
        StartFlag = true;
    }

    private void FixedUpdate()
    {
        if (!FinishSetup && enemyDamageController.setupFinsh) Initialize();

        //ダメージが与えられた時に非アクティブにする処理
        if (enemyDamageController.DamageStopFlag)
        {
            DamageStopDelta += 0.02f;
            if (DamageStopDelta >= StopTime)
            {
                DamageStopDelta = 0;
                enemyDamageController.DamageStopFlag = false;
            }
            return;
        }

        rigid.AddForce(new Vector3(0, -Gravity, 0), ForceMode.Acceleration);

        if (StartFlag　&& !enemyDamageController.StopFlag)
        {
            //プレイヤーの敵の距離
            PlayerDis = PlayerController.PlayerPos - transform.position;
            Speed = new Vector2(rigid.velocity.x, rigid.velocity.z);

            //敵のアクティブフラグの変更
            if (!AttackPlaying)
            {
                if (PlayerDis.magnitude < AttackDis && AttackFlag && EStatas == 1) EStatas = 2;
                else if (PlayerDis.magnitude <= ActiveDis) EStatas = 1;
                else EStatas = 0;
            }
            if (EStatas == 1) //歩行時
            {
                if (EStatas != BfoEStatas)//変化時一回だけ実行
                {
                    BfoEStatas = EStatas;
                    animator.SetBool("RunFlag", true);
                }

                //移動の力を加える
                Vector2 Dir = new Vector2(PlayerDis.x, PlayerDis.z);
                if (Speed.magnitude <= MaxSpeed) rigid.AddForce(new Vector3(force * Dir.normalized.x, 0, force * Dir.normalized.y));
            }
            else if (EStatas == 0)     //ノンアクティブ時の行動
            {
                if (EStatas != BfoEStatas)//変化時一回だけ実行
                {
                    BfoEStatas = EStatas;
                    animator.SetBool("RunFlag", false);
                }
            }
            else　//攻撃時の行動
            {
                if (EStatas != BfoEStatas)//変化時一回だけ実行
                {
                    BfoEStatas = EStatas;
                    AttackPlaying = true;
                    AttackFlag = false;
                    NowAttackCount = 0;
                    PlayJumpAttack();
                }

                if (AttackPlaying)
                {
                    AttackJumpDelta -= 0.02f;
                    if(JumpFlag) AttackMoveDelta -= 0.02f;
                    PlayAttackDelta -= 0.02f;
                    if (AttackJumpDelta <= 0) //ジャンプ使用
                    {
                        AttackJumpDelta = 100;
                        PlayAttackDelta = 100;
                        Vector3 MovePos = PlayerController.PlayerPos + Vector3.up * JumpHight;  //移動目標地点
                        Vector3 MoveVec = MovePos - transform.position;                         //移動ベクトル
                        AttackVec = MoveVec.normalized * AttackSpeed;                           //攻撃の速度ベクトルの計算
                        AttackMoveDelta = MoveVec.magnitude / AttackSpeed;                      //ジャンプ攻撃の移動時間を計算
                        Gravity = 0;                                               //重力の影響をなくす
                        JumpFlag = true;

                        Debug.Log("ジャンプ使用");
                    }
                    if(AttackMoveDelta <= 0) //ジャンプ移動終了
                    {
                        JumpFlag = false;
                        rigid.velocity = Vector3.zero;
                        Gravity = EnemyGravity;
                        AttackJumpDelta = 100;
                        AttackMoveDelta = 100;
                        PlayAttackDelta = NextAttackTime;
                        Debug.Log("ジャンプ終了");
                    }
                    if(PlayAttackDelta <= 0) //次のジャンプを使用
                    {
                        PlayJumpAttack();
                        Debug.Log("次の行動");
                    }
                    if(JumpFlag) rigid.velocity = AttackVec;  //ジャンプの移動
                }
            }

            //攻撃フラグを再度有効にする設定
            if (EStatas != 2 && !AttackFlag)
            {
                AttackDelta += 0.02f;
                if (AttackDelta >= AttackIntreval)
                {
                    AttackFlag = true;
                    AttackDelta = 0;
                }
            }
            //向き変更の計算
            Vector2 Pos = new Vector2(transform.position.x, transform.position.z);
            Vector2 DPos = Pos - BfoPso;
            if (DPos.magnitude > 0.01f) transform.rotation = Quaternion.LookRotation(new Vector3(DPos.x, 0, DPos.y));
            BfoPso = new Vector2(transform.position.x, transform.position.z);
        }
    }

    private void Initialize()
    {
        int Lv = enemyDamageController.ELv;

        StopTime = 1.5f - 0.15f * Lv;         //ダメージを受けたこのによる硬直時間
        MaxSpeed = MaxSpeed + 0.2f * Lv;                            //移動スピード
        AttackSpeed = AttackSpeed + 1f * Lv;
        AttackCoumt = AttackCoumt + (int)(0.4f * Lv);
        AttackIntreval = AttackIntreval - 0.2f * Lv;
        NextAttackTime = NextAttackTime - 0.2f * Lv;
        EnemyGravity = EnemyGravity + Lv * 2f;

        if (Lv == 2 || Lv == 5 || Lv == 8) GetComponent<Renderer>().material = Lv2Mat;
         if (Lv == 3 || Lv == 6 || Lv == 9) GetComponent<Renderer>().material = Lv3Mat;
        if (4 <= Lv && Lv <= 6) AuraEffect[0].SetActive(true);
        if (7 <= Lv && Lv <= 9) AuraEffect[1].SetActive(true);
        FinishSetup = true;
    }

    private void PlayJumpAttack()
    {
        if (AttackCoumt == NowAttackCount)
        {
            AttackPlaying = false;
            AttackJumpDelta = 100;
            AttackMoveDelta = 100;
            PlayAttackDelta = 100;
            NowAttackCount = 0;
            animator.SetTrigger("AttackEnd");
            Gravity = 9.8f;
            LevelChange();
            Debug.Log("攻撃終了");
        }
        else
        {
            NowAttackCount++;
            animator.SetTrigger("Attack");
            AttackJumpDelta = 1.333f;
            AttackMoveDelta = 100;
            PlayAttackDelta = 100;
        }
    }

    private void LevelChange()
    {
        float HpRate = enemyDamageController.eHP / enemyDamageController.eMaxHP;

        Debug.Log("HpRate " + HpRate);

        if (EnemyStage == 1 && HpRate <= 0.8f)
        {
            EnemyStage = 2;
            mat.color = new Color32(200, 200, 200, 1);
            MaxSpeed *= 1.5f;
            AttackSpeed *= 2;
            AttackCoumt += 1;
            AttackIntreval *= 0.8f;
            NextAttackTime *= 0.5f;
            EnemyGravity += 10f;
        }

        if (EnemyStage == 2 && HpRate <= 0.5f)
        {
            EnemyStage = 3;
            mat.color = new Color32(150, 150, 150, 1);
            MaxSpeed *= 1.3f;
            AttackSpeed *= 1.5f;
            AttackCoumt += 1;
            AttackIntreval *= 0.8f;
            NextAttackTime *= 0.5f;
            EnemyGravity += 30f;
        }

        if (EnemyStage == 3 && HpRate <= 0.2f)
        {
            EnemyStage = 4;
            mat.color = new Color32(100, 100, 100, 1);
            MaxSpeed *= 1.25f;
            AttackSpeed *= 1.25f;
            AttackCoumt += 1;
            AttackIntreval *= 0.8f;
            NextAttackTime *= 0.5f;
            EnemyGravity += 50f;
        }
    }
}
