using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WispController : MonoBehaviour
{
    [SerializeField] bool Homing = false;
    [SerializeField] GameObject Fire = null;
    [SerializeField] GameObject HomingFire = null;
    [SerializeField] GameObject[] AuraEffect = null;

    private float Speed = 3.0f;            //移動のマックススピード
    private float RDis = 8.0f;             //移動範囲
    private float AttackInterval = 6;
    private float FireSpeed = 5f;
    private bool Mode3D = true;    //3Dかどうか
    private float FireDis = 10;
    private int SplitN = 360;                               //円運動の分割数（3Dの時だけ使用）
    private float Dtime;                                    //1コマの移動時間
    private float Ftime;                                    //初期移動の時間
    private List<Vector3> NextPos = new List<Vector3>();    //移動地点

    private int NP;                                         //次の移動地点の番号
    private bool AttackLock = true;                         //攻撃フラグの時間加算が有効か
    private bool MoveLock = true;　　　　　　　　　　　　　 //移動地点変更フラグの時間加算が有効か
    private float AttackDelta = 0;　　　　　　　　　　　　　//攻撃フラグのデルタ時間
    private float MoveDelta = 0;                            //移動地点変更フラグのデルタ時間
    private Vector3 BfoV;

    private float BfoAttackTime = 3.0f;
    private float AfterAttackTime = 3.0f;

    private bool Revars = true;　　　　　　　　　　　　　　 //どっち方向に回転させるか(3Dの場合のみ)
    private Rigidbody Rigid;
    private Animator animator;                              //敵のアニメータ
    private GameObject FireBall;

    private EnemyDamageController enemyDamageController;

    private bool FinishSetup = false;
    private int ActiveNum = 0;

    private Vector2 BfoPso;                                 //直前の敵の位置

    //敵のレベル設定
    public void Initialize()
    {
        Mode3D = enemyDamageController.Mode;
        int Lv = enemyDamageController.ELv;
        Material mat = GetComponent<Renderer>().material;

        Speed = Speed + 0.7f * Lv;
        if (!Mode3D) Speed *= 0.5f;
        AttackInterval = AttackInterval - 0.4f * Lv;
        BfoAttackTime = BfoAttackTime - 0.2f * Lv;
        AfterAttackTime = AfterAttackTime - 0.1f * Lv;
        FireSpeed = FireSpeed + 0.4f * Lv;

        if (Lv == 2 || Lv == 5 || Lv == 8) mat.color = new Color32(170, 170, 170, 1);
        if (Lv == 3 || Lv == 6 || Lv == 9) mat.color = new Color32(100, 100, 100, 1);
        if (4 <= Lv && Lv <= 6) AuraEffect[1].SetActive(true);
        if (7 <= Lv && Lv <= 9) AuraEffect[0].SetActive(true);
        MovePoint();

        ActiveNum = enemyDamageController.ActiveTypeNum;
        FireDis = (ActiveNum + 1.0f) * FireDis;
       // FireDis

        FinishSetup = true;
    }

    private void Awake()
    {
        enemyDamageController = GetComponent<EnemyDamageController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Rigid = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void MovePoint()
    {
        Vector3 FPos = transform.position;
        Ftime = RDis / Speed;
        if (Mode3D)
        {
            NP = Random.Range(0, SplitN);
            for (int i = 0; i < SplitN; i++)
            {
                float sita = (float)i / (float)SplitN * 2 * Mathf.PI;
                Vector3 Pos = FPos + Vector3.right * RDis * Mathf.Sin(sita) + Vector3.back * RDis * Mathf.Cos(sita);
                NextPos.Add(Pos);
            }
            Dtime = (NextPos[0] - NextPos[1]).magnitude / Speed;
            if (Random.Range(0, 2) == 0) Revars = false;
        }
        else
        {
            NP = Random.Range(0, 2);
            NextPos.Add(FPos + Vector3.right * RDis);
            NextPos.Add(FPos + Vector3.left * RDis);
            Dtime = RDis * 2 / Speed;
            SplitN = 2;
        }
        BfoPso = new Vector2(transform.position.x, transform.position.z);
        Invoke("FirstMove", 3.3f);
        MoveDelta = Dtime;
    }

    private void FirstMove()
    {
        Rigid.velocity = (NextPos[NP] - transform.position) / Ftime;
        Invoke("ActiveAction", Ftime);
    }
    private void ActiveAction()
    {
        AttackLock = false;
        MoveLock = false;
    }

    private void PlayAttack()
    {
        animator.SetTrigger("Attack");
        if(Homing) FireBall.GetComponent<HomingFire>().FireShot(); else FireBall.GetComponent<LineFire>().FireShot();
        Invoke("endAttack", AfterAttackTime);
        Invoke("FireDeleta", 3);
    }

    private void endAttack()
    {
        MoveLock = false;
        AttackLock = false;
        Rigid.velocity = BfoV;
    }

    private void FireDeleta()
    {
        Destroy(FireBall);
    }

    private void FixedUpdate()
    {
        if (!FinishSetup && enemyDamageController.setupFinsh) Initialize();

        if (!AttackLock)
        {
            AttackDelta += 0.02f;
            if (AttackDelta >= AttackInterval)
            {
                float Pdis = (PlayerController.PlayerPos - transform.position).magnitude;
                if (Pdis <= FireDis)
                {
                    AttackDelta = 0;
                    BfoV = Rigid.velocity;
                    MoveLock = true;
                    AttackLock = true;
                    Rigid.velocity = Vector3.zero;
                    Invoke("PlayAttack", BfoAttackTime);
                    if (Homing)
                    {
                        FireBall = Instantiate(HomingFire) as GameObject;
                        FireBall.GetComponent<HomingFire>().FireSpeed = FireSpeed;
                    }
                    else
                    {
                        FireBall = Instantiate(Fire) as GameObject;
                        FireBall.GetComponent<LineFire>().FireSpeed = FireSpeed;
                    }
                    FireBall.transform.position = transform.position;
                    FireBall.GetComponent<DamageObject>().Damage = enemyDamageController.ePow ;                
                }
            }
        }

        if (!MoveLock)
        {
            MoveDelta += 0.02f;
            if(MoveDelta >= Dtime)
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

        //敵の向き
        Vector2 PPos = new Vector2(transform.position.x, transform.position.z);
        Vector2 DPos = PPos - BfoPso;
        if (DPos.magnitude > 0.01f) transform.rotation = Quaternion.LookRotation(new Vector3(DPos.x, 0, DPos.y));
        BfoPso = new Vector2(transform.position.x, transform.position.z);
    }

}
