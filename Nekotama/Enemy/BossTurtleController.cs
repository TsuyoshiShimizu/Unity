using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTurtleController : MonoBehaviour
{
    [SerializeField] Material Lv2Mat = null;
    [SerializeField] Material Lv3Mat = null;
    [SerializeField] GameObject[] AuraEffect = null;
    [SerializeField] GameObject Needle = null;

    private float ActiveDis = 200.0f;       //アクティブに変化する距離
    private float AttackDis = 15.0f;        //攻撃を行う距離
    private float MaxSpeed = 2.0f;          //移動のマックススピード
    private int AttackCoumt = 1;            //攻撃の連続使用数
    private float AttackSpeed = 5;          //攻撃のスピード

    private float[] StageChangeRate = new float[3] {0.9f, 0.6f, 0.3f};

    private int PlayAttackType = 0;                 //実行する攻撃

    private Animator animator;                      //敵のアニメータ
    private Rigidbody rigid;                        //敵の物理ボディ
    private Material mat;                           //敵の素材
    private bool StartFlag = false;                 //初回の行動開始用フラグ
    private int StageLv = 1;

    //次の攻撃可能までのリキャスト時間
    private float AttackIntreval = 10.0f;　          
    private float AttackDelta = 0;

    //攻撃のため時間
    private float BfoAttackTime = 2.0f;
    private float BfoAttackDelta = 100;

    //攻撃時間
    private float AttackTime = 0;               //(スピードから計算)突進の時間
    private float NeedleAttack1Time = 2.0f;     //トゲ1
    private float NeedleAttack2Time = 3.0f;     //トゲ2
    private float NeedleWaitTime = 0.5f;
    private float PlayAttackDelta = 100;

    private Vector3 AttackVelocity = Vector3.zero;

    private float force = 20.0f;                    //移動に与える力
    private Vector2 BfoPos;                         //直前の敵の位置(敵の向き変更に使用)

    private EnemyDamageController enemyDamageController;    //敵がくらうダメージの管理スクリプト
    private int EStatas = 0;                                //敵の状態
    private int BfoEStatas = 0;                             //敵の前の状態
    private bool AttackPlaying = false;                     //敵が攻撃中か
    private bool AttackFlag = true;                         //敵が攻撃可能か
    private int NowAttackCount = 0;                         //連続攻撃時の現在の回数               
    private Vector3 AttackVec = Vector3.zero;               //敵の攻撃中の速度を管理

    private bool FinishSetup = false;

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

        mat = GetComponent<Renderer>().material;
        rigid.maxAngularVelocity = 30;
        AttackTime = AttackDis * 2 / AttackSpeed;

        Invoke("ActiveStart", 2.7f);   //初回動作開始
    }

    private void ActiveStart()
    {
        StartFlag = true;
    }

    private void FixedUpdate()
    {
        if (!FinishSetup && enemyDamageController.setupFinsh) Initialize();

        if (StartFlag)
        {
            //プレイヤーの敵の距離
            Vector3 PlayerDis = PlayerController.PlayerPos - transform.position;
            Vector2 Speed = new Vector2(rigid.velocity.x, rigid.velocity.z);

            //敵のアクティブフラグの変更
            if (!AttackPlaying)
            {
                if (PlayerDis.magnitude < AttackDis && AttackFlag && EStatas == 1) EStatas = 2;
                else if (PlayerDis.magnitude <= ActiveDis) EStatas = 1;
                else EStatas = 0;
            }

            if (EStatas == 1) //歩行時
            {
                //アニメーションの切り替え
                if (EStatas != BfoEStatas)//変化時一回だけ実行
                {
                    BfoEStatas = EStatas;
                    animator.SetInteger("EStatas", EStatas);
                }

                //移動の力を加える
                Vector2 Dir = new Vector2(PlayerDis.x, PlayerDis.z);
                if (Speed.magnitude <= MaxSpeed) rigid.AddForce(new Vector3(force * Dir.normalized.x, 0, force * Dir.normalized.y));

                //向き変更の計算
                Vector2 Pos = new Vector2(transform.position.x, transform.position.z);
                Vector2 DPos = Pos - BfoPos;
                if (DPos.magnitude > 0.01f) transform.rotation = Quaternion.LookRotation(new Vector3(DPos.x, 0, DPos.y));
                rigid.angularVelocity = Vector3.zero;
                BfoPos = new Vector2(transform.position.x, transform.position.z);
            }
            else if (EStatas == 0)     //ノンアクティブ時の行動
            {
                //アニメーションの切り替え
                if (EStatas != BfoEStatas)//変化時一回だけ実行
                {
                    BfoEStatas = EStatas;
                    animator.SetInteger("EStatas", EStatas);
                }
                rigid.angularVelocity = Vector3.zero;             
            }
            else　//攻撃時の行動
            {
                if (EStatas != BfoEStatas)//変化時一回だけ実行
                {
                    BfoEStatas = EStatas;
                    animator.SetInteger("EStatas", EStatas);
                    AttackPlaying = true;
                    AttackFlag = false;
                    NowAttackCount = 0;
                    int Rand = Random.Range(0, 2);
                    if(PlayAttackType == 0)
                    {
                        if (Rand == 0) PlayAttackType = 1;
                        if (Rand == 1) PlayAttackType = 2;
                    }
                    else if (PlayAttackType == 1)
                    {
                        if (Rand == 0) PlayAttackType = 0;
                        if (Rand == 1) PlayAttackType = 2;
                    }
                    else
                    {
                        if (Rand == 0) PlayAttackType = 0;
                        if (Rand == 1) PlayAttackType = 1;
                    }

                    

                    AttackVelocity = Vector3.zero;
                    PlayAttack();
                }
                //攻撃中の回転
                rigid.angularVelocity = Vector3.up * 25;
                //攻撃中の移動速度
                rigid.velocity = AttackVelocity;

                if (AttackPlaying)
                {
                    BfoAttackDelta -= 0.02f;
                    PlayAttackDelta -= 0.02f;
                    if (BfoAttackDelta <= 0) //攻撃開始
                    {
                        BfoAttackDelta = 100;
                        if (PlayAttackType == 0) //突進攻撃
                        {
                            Debug.Log("突進攻撃");
                            Vector2 PPos = new Vector2(PlayerController.PlayerPos.x, PlayerController.PlayerPos.z);
                            Vector2 EPos = new Vector2(transform.position.x, transform.position.z);
                            Vector2 AttackVec = (PPos - EPos).normalized;
                            AttackVelocity = new Vector3(AttackVec.x, 0, AttackVec.y) * AttackSpeed;
                            PlayAttackDelta = (PPos - EPos).magnitude / AttackSpeed + 0.5f;
                        }
                        else if (PlayAttackType == 1) //トゲ攻撃1
                        {
                            Debug.Log("トゲ攻撃1");
                            PlayAttackDelta = NeedleAttack1Time;
                            AttackVelocity = Vector3.zero;

                            //トゲの出現位置計算
                            float DisXZ = 4;
                            float DisY = 0.5f;
                            float Ra = Random.Range(0f, 360f);
                            float FSita = Ra * Mathf.PI / 180f;
                            Vector3 TogePos = transform.position 
                                + new Vector3(DisXZ * Mathf.Cos(FSita), DisY, DisXZ * Mathf.Sin(FSita));

                            //トゲを生成
                            GameObject Toge = Instantiate(Needle, TogePos, transform.rotation);
                            Toge.GetComponent<DamageObject>().Damage = enemyDamageController.ePow;

                            //トゲの射出コルーチン起動
                            StartCoroutine( Shot1Needle(NeedleWaitTime, Toge) );

                            //トゲの消去コルーチン起動
                            StartCoroutine( DeleteNeedle(5.0f, Toge) );
                        }
                        else if (PlayAttackType == 2) //トゲ攻撃2
                        {
                            Debug.Log("トゲ攻撃2");
                            PlayAttackDelta = NeedleAttack2Time;
                            AttackVelocity = Vector3.zero;

                            //トゲの初期出現角と変化量を計算
                            float DisXZ = 4;
                            float DisY = 0.5f;
                            float Ra = Random.Range(0f, 360f);
                            float FSita = Ra * Mathf.PI / 180f;
   
                            float NeedleCount = 12;
                            float NSita = 360f / NeedleCount * Mathf.PI / 180f;

                            for(float n = 0; n < NeedleCount; n++)
                            {
                                Vector3 TogePos = transform.position
                                + new Vector3(DisXZ * Mathf.Cos(FSita + n * NSita), DisY, DisXZ * Mathf.Sin(FSita + n * NSita));

                                //トゲを生成
                                GameObject Toge = Instantiate(Needle, TogePos, transform.rotation);
                                Toge.GetComponent<DamageObject>().Damage = enemyDamageController.ePow;

                                //トゲの射出コルーチン起動
                                StartCoroutine(Shot2Needle(NeedleWaitTime + n * 0.1f, Toge));

                                //トゲの消去コルーチン起動
                                StartCoroutine(DeleteNeedle(5.0f, Toge));
                            }

                        }
                    }

                    if (PlayAttackDelta <= 0) //次の攻撃を開始
                    {
                        PlayAttack();
                        Debug.Log("次の行動");
                    }
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
        }
    }


    private void Initialize()
    {
        int Lv = enemyDamageController.ELv;
        for(int i = 0; i < StageChangeRate.Length; i++)
        {
            StageChangeRate[i] = StageChangeRate[i] + (1.0f - StageChangeRate[i]) / 12f * Lv;
        }

        if (Lv == 2 || Lv == 5 || Lv == 8) GetComponent<Renderer>().material = Lv2Mat;
        if (Lv == 3 || Lv == 6 || Lv == 9) GetComponent<Renderer>().material = Lv3Mat;
        if (4 <= Lv && Lv <= 6) AuraEffect[0].SetActive(true);
        if (7 <= Lv && Lv <= 9) AuraEffect[1].SetActive(true);
        FinishSetup = true;
    }

    private void PlayAttack()
    {
        if (AttackCoumt == NowAttackCount)  //連続攻撃終了時
        {
            AttackPlaying = false;
            BfoAttackDelta = 100;
            PlayAttackDelta = 100;
            NowAttackCount = 0;
            AttackVelocity = Vector3.zero;
            rigid.velocity = Vector3.zero;
            LevelChange();
            Debug.Log("攻撃終了");
        }
        else
        {
            NowAttackCount++;
            BfoAttackDelta = BfoAttackTime;
            PlayAttackDelta = 100;
            AttackVelocity = Vector3.zero;
        }
    }

    IEnumerator Shot1Needle(float delay,GameObject Toge)
    {
        yield return new WaitForSeconds(delay);

        //トゲの射出方向計算
        Vector2 TogeVec = new Vector2(PlayerController.PlayerPos.x, PlayerController.PlayerPos.z) 
            - new Vector2(Toge.transform.position.x, Toge.transform.position.z);
        Vector2 TogeVelocity = TogeVec.normalized * AttackSpeed;

        //トゲの射出
        Toge.GetComponent<Rigidbody>().velocity = new Vector3(TogeVelocity.x, 0, TogeVelocity.y);
    }

    IEnumerator Shot2Needle(float delay, GameObject Toge)
    {
        yield return new WaitForSeconds(delay);

        //トゲの射出方向計算
        Vector2 TogeVec = new Vector2(Toge.transform.position.x, Toge.transform.position.z)
            - new Vector2(transform.position.x, transform.position.z);
        Vector2 TogeVelocity = TogeVec.normalized * AttackSpeed;

        //トゲの射出
        Toge.GetComponent<Rigidbody>().velocity = new Vector3(TogeVelocity.x, 0, TogeVelocity.y);
    }

    IEnumerator DeleteNeedle(float delay,GameObject Toge)
    {
        yield return new WaitForSeconds(delay);

        //トゲの消去
        Destroy(Toge);
    }

    private void LevelChange()
    {
        float HpRate = enemyDamageController.eHP / enemyDamageController.eMaxHP;
        if (StageLv == 1 && HpRate <= StageChangeRate[0])
        {
            StageLv = 2;
            mat.color = new Color32(180, 180, 180, 1);
            MaxSpeed = 2.5f;
            AttackSpeed = 13;
            AttackCoumt = 2;
            AttackIntreval = 8.0f;
            BfoAttackTime = 1.7f;
            NeedleWaitTime = 0.4f;
        }

        if (StageLv == 2 && HpRate <= StageChangeRate[1])
        {
            StageLv = 3;
            mat.color = new Color32(110, 110, 110, 1);
            MaxSpeed = 3.0f;
            AttackSpeed = 16;
            AttackCoumt = 3;
            AttackIntreval = 6.0f;
            BfoAttackTime = 1.4f;
            NeedleAttack1Time = 1.0f;
            NeedleAttack2Time = 2.0f;
            NeedleWaitTime = 0.2f;
        }

        if (StageLv == 3 && HpRate <= StageChangeRate[2])
        {
            StageLv = 4;
            mat.color = new Color32(40, 40, 40, 1);
            MaxSpeed = 4.0f;
            AttackSpeed = 22;
            AttackCoumt = 5;
            AttackIntreval = 4.0f;
            BfoAttackTime = 1.0f;
            NeedleAttack1Time = 0.1f;
            NeedleAttack2Time = 0.8f;
            NeedleWaitTime = 0.1f;
        }
    }
}
