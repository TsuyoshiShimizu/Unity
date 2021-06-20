using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoragonController : MonoBehaviour
{
    [SerializeField] private GameObject Flame = null;
    [SerializeField] private ParticleSystem particle = null;
    [SerializeField] private GameObject TaleAttack = null;
    [SerializeField] private GameObject Barea = null;
    [SerializeField] Material Lv2Mat = null;
    [SerializeField] Material Lv3Mat = null;
    [SerializeField] GameObject[] AuraEffect = null;


    //使用パラメータ
    private float ActiveDis = 18.0f;                    //アクティブに変化する距離
    private float AttackDis = 12.0f;                    //攻撃を行う距離
    private float MaxSpeed = 4.0f;                      //移動のマックススピード
 //   private float NoAttackTime = 2.0f;                  //ダメージ時の無敵時間

    private float DameDelta = 0;                        //ダメージによる無敵時間計測用
    private float GuardDelta = 100;                     //ガード状態の時間計測

    private bool AttackFlag = false;                    //攻撃可能か
    private bool MoveFlag = false;                      //移動可能か
    private float MoveForce = 20.0f;                    //移動に与える力
    private Vector2 BfoPos;                             //直前の敵の位置(敵の向き変更に使用)

    private float MoveDelta = 100;                      //次の行動開始
    private float AttackDelta = 100;                    //攻撃のリキャスト時間
    private float ReAttackTime = 4.0f;

    private int ActiveNum = 0;
    private bool ActiveFlag = false;

    // private int EnemyStatas = 0;                        //敵の状態
    // private bool DieFlag = false;
    // 0:待機状態  1:移動状態  2:攻撃状
    //使用コンポーネント
    private EnemyDamageController enemyDamageController;                //ダメージコントローラー
  //  private EnemyHitController EHCon;                   //ヒットコントローラ
    private Rigidbody rigid;                            //リジッド
    private Material material;                          //マテリアル
    private Shader TransShader;                         //シェーダ
    private Renderer rendar;                            //レンダー
    private Effekseer.EffekseerEmitter Effect;          //エフェクト
    private Animator animator;                          //アニメータ

    private bool Mode3D = false;

    private bool FinishSetup = false;

    //初期処理
    private void Start()
    {
        Effect = GetComponent<Effekseer.EffekseerEmitter>();
        rigid = GetComponent<Rigidbody>();
        material = GetComponent<Renderer>().material;
        TransShader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
        rendar = GetComponent<Renderer>();
        enemyDamageController = GetComponent<EnemyDamageController>();
     //   EHCon = GetComponentInChildren<EnemyHitController>();
        animator = GetComponent<Animator>();
        BfoPos = new Vector2(transform.position.x, transform.position.z);

        particle.Stop();
        enemyDamageController.GuardFlag = false;
        Invoke("StartMove", 2);
    }

    private void StartMove()
    {
       // EHCon.PConFlag = true;
        MoveFlag = true;
        AttackFlag = true;
    }

    /// <summary>
    /// 初期処理
    /// </summary>
    public void Initialize()
    {
        int Lv = enemyDamageController.ELv;
        Mode3D = enemyDamageController.Mode;

        if (!Mode3D)
        {
            ActiveDis = 30f;
            AttackDis = 20f;
        }

        Flame.GetComponent<ParticleDamage>().Damage = enemyDamageController.ePow;
        TaleAttack.GetComponent<DamageObject>().Damage = enemyDamageController.ePow;

        MaxSpeed = MaxSpeed + 0.5f * MaxSpeed * (Lv - 1) / 8;
        ReAttackTime = ReAttackTime - 0.5f * ReAttackTime * (Lv - 1) / 8;

        if (Lv == 2 || Lv == 5 || Lv == 8)
        {
            GetComponent<Renderer>().material = Lv2Mat;
            enemyDamageController.eName = "RedDragon";
        }

           
        if (Lv == 3 || Lv == 6 || Lv == 9)
        {
            GetComponent<Renderer>().material = Lv3Mat;
            enemyDamageController.eName = "GreenDragon";
        }
           
        if (4 <= Lv && Lv <= 6) AuraEffect[0].SetActive(true);
        if (7 <= Lv && Lv <= 9) AuraEffect[1].SetActive(true);

        ActiveNum = enemyDamageController.ActiveTypeNum;
        FinishSetup = true;
    }

    //攻撃を定義
    private void FlameAttack()
    {
        Flame.SetActive(true);
        GameManager.playStageSE(35);
    }

    private void FlameAttackEnd()
    {
        particle.Stop();
        Invoke("FlameAttackEnd2",3.0f);
    }

    private void FlameAttackEnd2()
    {
        Flame.SetActive(false);
    }

    private void SpinAttack()
    {
        Effect.Play(4);
    }

    private void SpinAttackEnd()
    {
        TaleAttack.SetActive(true);
        Invoke("SpinAttackEnd2", 0.5f);
    }

    private void SpinAttackEnd2()
    {
        TaleAttack.SetActive(false);
    }

    //メイン行動
    private void FixedUpdate()
    {
        if (!FinishSetup && enemyDamageController.setupFinsh) Initialize();

        //行動のリキャスト処理
        if (!MoveFlag)
        {
            MoveDelta -= 0.02f;
            if(MoveDelta <= 0)
            {
                MoveDelta = 100;
                MoveFlag = true;
            }
        }
        if (!AttackFlag)
        {
            AttackDelta -= 0.02f;
            if (AttackDelta <= 0)
            {
                AttackDelta = 100;
                AttackFlag = true;
            }
        }

        //ガードの解除処理
        if (enemyDamageController.GuardFlag)
        {
            GuardDelta -= 0.02f;
            if(GuardDelta <= 0)
            {
                GuardDelta = 100;
                Barea.SetActive(false);
                enemyDamageController.GuardFlag = false;
            }
        }

        //プレイヤーの敵の距離と現在の速度
        Vector3 PlayerDis = PlayerController.PlayerPos - transform.position;
        Vector2 Speed = new Vector2(rigid.velocity.x, rigid.velocity.z);

        if(AttackFlag && PlayerDis.magnitude <= AttackDis)
        {
            //攻撃処理
            AttackFlag = false;
            MoveFlag = false;
            animator.SetInteger("EnemyStatas", 0);
            // EnemyStatas = 2;
            enemyDamageController.GuardFlag = true;
            Barea.SetActive(true);

            int Rn = Random.Range(0,2);
            if (!Mode3D) Rn = 1;
            if(Rn == 0)     //スピン攻撃
            {
                GameManager.playStageSE(36);
                animator.SetTrigger("Attack1");
                MoveDelta = 3.0f;
                AttackDelta = ReAttackTime + MoveDelta;
                GuardDelta = 1.333f;

                Invoke("SpinAttack", 0.4f);
                Invoke("SpinAttackEnd", 0.8f);
            }
            else           //ブレス攻撃
            {
                GameManager.playStageSE(34);
                animator.SetTrigger("Attack2");
                MoveDelta = 5.0f;
                AttackDelta = ReAttackTime + MoveDelta;
                GuardDelta = 3.333f;

                Invoke("FlameAttack", 1.666f);
                Invoke("FlameAttackEnd", 3.333f);
            }
        }
        else if (MoveFlag)
        {
            //移動処理
            if(PlayerDis.magnitude <= ActiveDis || ActiveNum == 2 || (ActiveFlag && ActiveNum == 1) || enemyDamageController.DamageActive)
            {
                //移動処理
                //移動の力を加える
                ActiveFlag = true;
                if (Mode3D)
                {
                    if (animator.GetInteger("EnemyStatas") == 0) animator.SetInteger("EnemyStatas", 1);

                    Vector2 Dir = new Vector2(PlayerDis.x, PlayerDis.z);
                    if (Speed.magnitude <= MaxSpeed) rigid.AddForce(new Vector3(MoveForce * Dir.normalized.x, 0, MoveForce * Dir.normalized.y));

                    //向き変更の計算
                    Vector2 Pos = new Vector2(transform.position.x, transform.position.z);
                    Vector2 DPos = Pos - BfoPos;
                    if (DPos.magnitude > 0.01f) transform.rotation = Quaternion.LookRotation(new Vector3(DPos.x, 0, DPos.y));
                    rigid.angularVelocity = Vector3.zero;
                    BfoPos = new Vector2(transform.position.x, transform.position.z);
                }
            }
            else
            {
                //待機処理
                if (animator.GetInteger("EnemyStatas") == 1) animator.SetInteger("EnemyStatas", 0);
            }
        }
    }

}
