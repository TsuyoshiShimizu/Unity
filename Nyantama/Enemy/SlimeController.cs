using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeController : MonoBehaviour
{
    //インスペクターから操作(レッド、ブルースライムの違い)
    [SerializeField] float MaxSpeed = 2.0f;         //移動のマックススピード
    [SerializeField] float Dis2D = 5.0f;            //敵が進む距離
    [SerializeField] float ActivDis = 8.0f;         //アクティブに変化する距離
    [SerializeField] float NonActiveDis = 20.0f;    //ノンアクティブに変化する距離

    //レベルによる見た目に変化に使用
    [SerializeField] GameObject[] AuraEffect = null; 
    
    //召喚器から変更
    private bool Mode3D = true;                     //3Dかどうか 
    bool Direction2D = true;                        //初期の敵が進む方向

    //その他
    private Animator animator;                      //敵のアニメータ
    private Rigidbody rigid;                        //敵の物理ボディ
    private bool StartFlag = false; 
    private bool ActiveFlag = false;                //アクティブフラグ
    private bool BfoActiveFlag = false;             //直前のアクティブフラグ
    private float force = 20.0f;                    //移動に与える力
    private float ZPos;                             //初期のZ位置
    private Vector2 BfoPso;                         //直前の敵の位置
    private float XminPos;
    private float XmaxPos;
    private bool FinishSetup = false;
    private float StopTime = 2.0f;
    private float DamageStopDelta = 0;

    private int ActiveNum = 0;

    private EnemyDamageController enemyDamageController;

    private void Awake()
    {
        enemyDamageController = GetComponent<EnemyDamageController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        BfoPso = new Vector2(transform.position.x, transform.position.z);

        ZPos = transform.position.z;
        XminPos = transform.position.x - Dis2D;
        XmaxPos = transform.position.x + Dis2D;
        Invoke("ActiveStart", 2.0f);
    }

    private void FixedUpdate()
    {
        if (!FinishSetup && enemyDamageController.setupFinsh) Initialize();

        if (StartFlag && FinishSetup)
        {
            //プレイヤーの敵の距離
            Vector3 PlayerDis = PlayerController.PlayerPos - transform.position;
            Vector2 Speed = new Vector2(rigid.velocity.x, rigid.velocity.z);

            //ダメージが与えられた時に非アクティブにする処理
            if (enemyDamageController.DamageStopFlag)
            {
                DamageStopDelta += 0.02f;
                if(DamageStopDelta >= StopTime)
                {
                    DamageStopDelta = 0;
                    enemyDamageController.DamageStopFlag = false;
                }
                return;
            }

            //敵のアクティブフラグの変更
            if (PlayerDis.magnitude <= ActivDis) ActiveFlag = true;
            if (PlayerDis.magnitude >= NonActiveDis && ActiveNum == 0 && !enemyDamageController.DamageActive) ActiveFlag = false;

            if (ActiveFlag) //アクティブ時の行動
            {
                if (ActiveFlag != BfoActiveFlag)//変化時一回だけ実行
                {
                    BfoActiveFlag = ActiveFlag;
                    animator.SetBool("RunFlag", true);
                }
                if (Mode3D) //3Dモードの時
                {
                    Vector2 Dir = new Vector2(PlayerDis.x, PlayerDis.z);
                    if (Speed.magnitude <= MaxSpeed) rigid.AddForce(new Vector3(force * Dir.normalized.x, 0, force * Dir.normalized.y));
                }
                else
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
                Vector2 DPos = Pos - BfoPso;
                if (DPos.magnitude > 0.01f) transform.rotation = Quaternion.LookRotation(new Vector3(DPos.x, 0, DPos.y));

            }
            else      //ノンアクティブ時の行動
            {
                if (ActiveFlag != BfoActiveFlag)//変化時一回だけ実行
                {
                    BfoActiveFlag = ActiveFlag;
                    animator.SetBool("RunFlag", false);
                }
                if (ActiveNum == 2 || enemyDamageController.DamageActive) ActiveFlag = true;
            }
            BfoPso = new Vector2(transform.position.x, transform.position.z);
        }
    }


    private void ActiveStart()
    {
        StartFlag = true;
    }

    private void Initialize()
    {
        Mode3D = enemyDamageController.Mode;
        Direction2D = enemyDamageController.Direction;

        int Lv = enemyDamageController.ELv;
        if (Lv >= 10) Lv = 1;

        Material mat = GetComponent<Renderer>().material;


        StopTime = 1.5f - 0.15f * Lv;                   //ダメージを受けたこのによる硬直時間
        MaxSpeed = MaxSpeed + 0.1f * Lv;                //移動スピード

        if (Lv == 2 || Lv == 5 || Lv == 8) mat.color = new Color32(170, 170, 170, 1);
        if (Lv == 3 || Lv == 6 || Lv == 9) mat.color = new Color32(100, 100, 100, 1);
        if (4 <= Lv && Lv <= 6) AuraEffect[1].SetActive(true);
        if (7 <= Lv && Lv <= 9) AuraEffect[0].SetActive(true);

        ActiveNum = enemyDamageController.ActiveTypeNum;
        FinishSetup = true;
    }

    public void KnockBack()
    {
        rigid.Sleep();
        animator.SetTrigger("Hit");
        Vector2 HitVec = new Vector2(transform.position.x - PlayerController.PlayerPos.x
            , transform.position.z - PlayerController.PlayerPos.z).normalized;
        float PowX = 200;
        float PowY = 200;

        rigid.AddForce(HitVec.x * PowX, PowY, HitVec.y * PowX);
    }
}
