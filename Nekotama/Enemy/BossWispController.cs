using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossWispController : MonoBehaviour
{
    [SerializeField] private GameObject[] FireBalls = null;
    [SerializeField] private GameObject FireBody = null;
    [SerializeField] Material Lv2Mat = null;
    [SerializeField] Material Lv3Mat = null;
    [SerializeField] GameObject[] AuraEffect = null;

    private float Speed = 3.0f;            //移動のマックススピード
    private float RDis = 16.0f;             //回転移動の半径
    private float AttackInterval = 10.0f;   //攻撃のインターバル

    private float FireDis = 100;                             //攻撃を行う距離
    private int SplitN = 360;                               //円運動の分割数（3Dの時だけ使用）
    private float Dtime;                                    //1コマの移動時間
    private float Ftime;                                    //初期移動の時間
    private List<Vector3> NextPos = new List<Vector3>();    //移動地点

    private int NP;                                         //次の移動地点の番号
    private bool AttackLock = true;                         //攻撃フラグの時間加算が有効か
    private bool MoveLock = true;　　　　　　　　　　　　　 //移動地点変更フラグの時間加算が有効か
    private float AttackDelta = 0;　　　　　　　　　　　　　//攻撃フラグのデルタ時間
    private float MoveDelta = 0;                            //移動地点変更フラグのデルタ時間
    private bool Revars = true;　　　　　　　　　　　　　　 //どっち方向に回転させるか(3Dの場合のみ)

    private int StageNum = 0;　                             //現在の段階
    private bool HitFlag = false;                           //攻撃を受けた場合の処理

    private Vector3 BfoV;

    private bool BfoAttackFlag = false;
    private bool AfterAttackFlag = false;
    private float BfoAttackTime = 3.0f;
    private float AfterAttackTime = 3.0f;
    private float AttackWaitDelta = 0;

    private Rigidbody Rigid;                                //敵の物理特性
    private Animator animator;                              //敵のアニメータ
    private SphereCollider[] body;                          //敵の物理ボディ
    private SkinnedMeshRenderer mesh;                       //敵のメッシュ
  //  private AudioSource aud;

    private Material mat;

    //  private GameObject FireBall;

    private EnemyDamageController enemyDamageController;
    private EnemyHitController enemyHitController;
    private Vector2 BfoPso;                                 //直前の敵の位置


    private Vector3 PosDistance = new Vector3(40, 15, 40);  //移動地点の距離
    private Vector3[] SPos = new Vector3[5];

    private bool FDameLock = true;                                  //初回時のダメージフラグ有効かを判定する            
    private float MoveSpeed = 20;                                   //地点移動の速度
    private int PNum = -1;
    private int LV = 1;

    //ファイアのステータス
    private int[] FireN = new int[3] {4,1,2};
    private float[] FireSpeed = new float[3] { 10, 5, 6};
    private float[] FireWaitTime = new float[3] { 2, 2, 2 };
    private float[] FireLifeTime = new float[3] { 10, 10, 10 };

    private bool FinishSetup = false;

    private void Awake()
    {
        enemyDamageController = GetComponent<EnemyDamageController>();
        enemyHitController = GetComponentInChildren<EnemyHitController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //コンポーネントのパッケージング
        Rigid = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        body = GetComponentsInChildren<SphereCollider>();
        mesh = GetComponent<SkinnedMeshRenderer>();
        mat = GetComponent<Renderer>().material;

        Vector3 FPos = transform.position;   //初期位置
        Ftime = RDis / Speed;              　//初期位置までの移動時間             

        //回転移動の軌跡計算
        NP = Random.Range(0, SplitN);
        for (int i = 0; i < SplitN; i++)
        {
            float sita = (float)i / (float)SplitN * 2 * Mathf.PI;
            Vector3 Pos = FPos + Vector3.right * RDis * Mathf.Sin(sita) + Vector3.back * RDis * Mathf.Cos(sita);
            NextPos.Add(Pos);
        }
        Dtime = (NextPos[0] - NextPos[1]).magnitude / Speed;
        if (Random.Range(0, 2) == 0) Revars = false;
        //回転移動の軌跡計算

        //基準移動地点の計算
        SPos[0] = new Vector3(FPos.x + PosDistance.x, FPos.y + PosDistance.y, FPos.z + PosDistance.z);
        SPos[1] = new Vector3(FPos.x + PosDistance.x, FPos.y + PosDistance.y, FPos.z - PosDistance.z);
        SPos[2] = new Vector3(FPos.x - PosDistance.x, FPos.y + PosDistance.y, FPos.z - PosDistance.z);
        SPos[3] = new Vector3(FPos.x - PosDistance.x, FPos.y + PosDistance.y, FPos.z + PosDistance.z);
        SPos[4] = new Vector3(FPos.x, FPos.y + 2 * PosDistance.y, FPos.z);

        Debug.Log("Vec1 " + FPos);
        Debug.Log("Vec2 " + PosDistance);

        BfoPso = new Vector2(transform.position.x, transform.position.z);   //現在の位置(向き変更用に使用)

        Invoke("FirstMove", 3.3f);      //初期の移動開始関数
        MoveDelta = Dtime;              //回転の1コマの移動時間
    }

    private void FirstMove()
    {
        Rigid.velocity = (NextPos[NP] - transform.position) / Ftime;    //初期移動の速度
        Invoke("ActiveAction", Ftime);                                  //回転移動の開始 
    }

    private void ActiveAction()
    {
        AttackLock = false;                                                 
        MoveLock = false;
    }

    private void FixedUpdate()
    {
        if (!FinishSetup && enemyDamageController.setupFinsh) Initialize();

        if (!enemyHitController.PConFlag && !FDameLock && !HitFlag && StageNum <= 1)
        {
            HitFlag = true;
            BfoAttackFlag = false;
            AfterAttackFlag = false;
            AttackWaitDelta = 0;
            float EnemyHP = enemyDamageController.eHP;
            float EnemyMaxHP = enemyDamageController.eMaxHP;

            // ステージの段階を上げるかの判定
            if (StageNum == 0 && EnemyHP / EnemyMaxHP <= 0.5f) StageNum = 1;
            if (StageNum == 1 && EnemyHP / EnemyMaxHP <= 0.2f) StageNum = 2;

            //レベルの段階を上げるかの判定
            if (LV == 1 && EnemyHP / EnemyMaxHP <= 0.5f)
            {
                LV = 2;
                mat.color = new Color32(200, 200, 200, 1);
                Speed = 4;
                AttackInterval = 8;
                AfterAttackTime = 5.0f;
                FireN = new int[3] { 4, 2, 3 };
                FireSpeed = new float[3] { 11, 6, 7 };
                FireWaitTime = new float[3] { 3, 3, 3 };
                FireLifeTime = new float[3] { 9, 9, 9 };
            }
            if (LV == 2 && EnemyHP / EnemyMaxHP <= 0.4f)
            {
                LV = 3;
                mat.color = new Color32(150, 150, 150, 1);
                Speed = 5;
                AttackInterval = 6;
                AfterAttackTime = 4.0f;
                FireN = new int[3] { 6, 3, 4 };
                FireSpeed = new float[3] { 12, 6, 8 };
                FireWaitTime = new float[3] { 3, 3, 3 };
                FireLifeTime = new float[3] { 8, 8, 8 };
            }
            if (LV == 3 && EnemyHP / EnemyMaxHP <= 0.3f)
            {
                LV = 4;
                mat.color = new Color32(100, 100, 100, 1);
                Speed = 6;
                AttackInterval = 5;
                AfterAttackTime = 3.0f;
                FireN = new int[3] { 8, 4, 5 };
                FireSpeed = new float[3] { 13, 7, 9 };
                FireWaitTime = new float[3] { 2, 2, 2 };
                FireLifeTime = new float[3] { 7, 7, 7 };
            }
            if (LV == 4 && EnemyHP / EnemyMaxHP <= 0.2f)
            {
                LV = 5;
                mat.color = new Color32(50, 50, 50, 1);
                Speed = 7;
                AttackInterval = 5;
                AfterAttackTime = 2.0f;
                FireN = new int[3] { 10, 5, 6 };
                FireSpeed = new float[3] { 15, 8, 10 };
                FireWaitTime = new float[3] { 2, 2, 2 };
                FireLifeTime = new float[3] { 6, 6, 6 };
            }

            //移動処理を記入
            MoveStart();
        }

        if(!enemyHitController.PConFlag && FDameLock) //初回のダメージフラグのバグ防止
        {
            FDameLock = false;
        }

        //攻撃に関する記述
        if (!AttackLock)
        {
            AttackDelta += 0.02f;
            if (AttackDelta >= AttackInterval)
            {
                float Pdis = (PlayerController.PlayerPos - transform.position).magnitude; //プレイヤーと敵の距離
                if (Pdis <= FireDis)
                {
                    AttackDelta = 0;                            //攻撃タイマーの初期化
                    BfoV = Rigid.velocity;                      //現在の速度を記憶
                    MoveLock = true;                            //移動のロック
                    AttackLock = true;                          //攻撃のロック
                    Rigid.velocity = Vector3.zero;              //移動速度ゼロ

                    int RA = Random.Range(0, 3);                                //攻撃種類
                    float FSita = Random.Range(0, 360) / 180.0f * Mathf.PI;     //初期角度
                    float DSita = 2 * Mathf.PI / (float)FireN[RA];              //変動角度
                    float DisFire = 10;                                         //ファイア作成距離

                    for (int i = 0; i < FireN[RA]; i++)
                    {
                        float Sita = FSita + DSita * i;
                        Vector3 FirePos = transform.position + Vector3.right * DisFire * Mathf.Sin(Sita) + Vector3.up * ( DisFire * Mathf.Cos(Sita) + 2 );
                        GameObject FB = Instantiate(FireBalls[RA], FirePos, transform.rotation);
                        FB.GetComponent<BossFire>().Initialize(FireSpeed[RA], FireWaitTime[RA], FireLifeTime[RA]);
                    }
                    GameManager.playStageSE(7);
                    BfoAttackTime = FireWaitTime[RA];
                    BfoAttackFlag = true;
                }
            }
        }

        //移動に関する記述
        if (!MoveLock)
        {
            MoveDelta += 0.02f;
            if (MoveDelta >= Dtime)
            {
                MoveDelta = 0;
                if (Revars)
                {
                    NP -= 1;
                    if (NP < 0) NP = SplitN - 1;
                }
                else
                {
                    NP += 1;
                    if (NP >= SplitN) NP = 0;
                }
                Rigid.velocity = (NextPos[NP] - transform.position) / Dtime;
            }
        }

        if (BfoAttackFlag)
        {
            AttackWaitDelta += 0.02f;
            if(AttackWaitDelta >= BfoAttackTime)
            {
                AttackWaitDelta = 0;
                BfoAttackFlag = false;
                AfterAttackFlag = true;
                animator.SetTrigger("Attack");
                GameManager.playStageSE(8);
            }
        }

        if (AfterAttackFlag)
        {
            AttackWaitDelta += 0.02f;
            if (AttackWaitDelta >= AfterAttackTime)
            {
                AttackWaitDelta = 0;
                AfterAttackFlag = false;
                MoveLock = false;
                AttackLock = false;
                Rigid.velocity = BfoV;
            }
        }

        //敵の向き
        Vector2 PPos = new Vector2(transform.position.x, transform.position.z);
        Vector2 DPos = PPos - BfoPso;
        if (DPos.magnitude > 0.01f) transform.rotation = Quaternion.LookRotation(new Vector3(DPos.x, 0, DPos.y));
        BfoPso = new Vector2(transform.position.x, transform.position.z);
    }

    //次の起点への移動開始処理
    private void MoveStart()
    {
        //当たり判定の無効化
        body[0].enabled = false;
        body[1].enabled = false;

        //通常移動と攻撃のロック
        MoveLock = true;
        AttackLock = true;
        MoveDelta = 0;
        AttackDelta = 0;

        //移動速度をゼロ
        Rigid.velocity = Vector3.zero;
        Invoke("MoveNextPoint", 2.5f);
    }

    private void MoveNextPoint()
    {
        //炎化処理
        FireBody.SetActive(true);       
        mesh.enabled = false;

        //移動に関する計算
        //移動地点番号の判別
        if(StageNum == 1 && PNum == -1)
        {
            PNum = Random.Range(0, 4);
        }
        else if(StageNum == 1)
        {
            int RN = Random.Range(0, 3);
            if (PNum == RN) PNum = 3; else PNum = RN;
        }
        else
        {
            PNum = 4;
        }

        //移動パラメータの計算
        Vector3 MoveVec = SPos[PNum] - transform.position;              //移動距離のベクトル
        Vector3 MoveV = MoveVec.normalized * MoveSpeed;                 //移動速度のベクトル
        float NextTime = MoveVec.magnitude / MoveSpeed;                 //移動時間
        Rigid.velocity = MoveV;                                         //敵に速度をセット

        //回転移動の軌跡計算
        NextPos.Clear();
        NP = Random.Range(0, SplitN);
        for (int i = 0; i < SplitN; i++)
        {
            float sita = (float)i / (float)SplitN * 2 * Mathf.PI;
            Vector3 Pos = SPos[PNum] + Vector3.right * RDis * Mathf.Sin(sita) + Vector3.back * RDis * Mathf.Cos(sita);
            NextPos.Add(Pos);
        }
        Dtime = (NextPos[0] - NextPos[1]).magnitude / Speed;
        if (Random.Range(0, 2) == 0) Revars = false;

        Invoke("FinishMove", NextTime);
    }

    private void FinishMove()
    {
        Rigid.velocity = Vector3.zero;
        body[0].enabled = true;
        body[1].enabled = true;
        FireBody.SetActive(false);
        mesh.enabled = true;

        MoveLock = false;
        AttackLock = false;
        HitFlag = false;
    }

    private void Initialize()
    {
        int Lv = enemyDamageController.ELv;
        if (Lv == 2 || Lv == 5 || Lv == 8) GetComponent<Renderer>().material = Lv2Mat;
        if (Lv == 3 || Lv == 6 || Lv == 9) GetComponent<Renderer>().material = Lv3Mat;
        if (4 <= Lv && Lv <= 6) AuraEffect[0].SetActive(true);
        if (7 <= Lv && Lv <= 9) AuraEffect[1].SetActive(true);

        FinishSetup = true;
    }
}
