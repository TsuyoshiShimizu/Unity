using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCon : MonoBehaviour
{
    [SerializeField] GameObject DamageEffect = null;

    //プレイヤーの共通使用パラメータ
    static public Vector3 PlayerPos = Vector3.zero;     //プレイヤーの位置
    static public Vector3 StageRot = Vector3.zero;
    static public Vector3 GravityVector = Vector3.down;
    static public Vector3 BackVector = Vector3.back;
    static public Vector3 RightVector = Vector3.right;

   // static public bool StageRotateFlag = true;
    
    //プレイヤーのアクションに使用するパラメータ
    private float MaxSpeed = 15.0f;                     //最大移動速度
    private float MoveForce = 20.0f;                    //移動させる力
    private Vector3 MoveForceVec = Vector3.zero;        //移動させる力ベクトル
    private float GravityForce = 9.8f;                  //重力の力

    private float DDetla = 0; 
    private float Dinter = 1.0f;

    private Vector2 MVec = Vector2.zero;
    static public Vector2 RotPosture = Vector2.zero; 

    //プレイヤーのコンポーネントのパッケージングに使用
    private Rigidbody Rigid;

#if UNITY_EDITOR
    private GameObject Phone = null;
#endif

    private void Awake()
    {
        PlayerPos = transform.position;
        GameManager.PlayerBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Rigid = GetComponent<Rigidbody>();

#if UNITY_EDITOR
        Phone = GameObject.FindWithTag("DebugObject");
#endif
        /*
        //基準面の違いによる回転角度の計算
        Vector3 SPosture = GameManager.StandardPosture.normalized;
        if (SPosture.z <= 0)
        {
            RotPosture.x = Mathf.Asin(SPosture.y);
            RotPosture.y = Mathf.Asin(SPosture.x / Mathf.Cos(RotPosture.x));
        }
        else
        {
            if (Mathf.Abs(SPosture.y) >= Mathf.Abs(SPosture.x))
            {
                if (SPosture.y >= 0) RotPosture.x = Mathf.PI - Mathf.Asin(SPosture.y);
                else RotPosture.x = -Mathf.PI - Mathf.Asin(SPosture.y);
                RotPosture.y = Mathf.Asin(SPosture.x / Mathf.Cos(RotPosture.x));
            }
            else
            {
                RotPosture.x = Mathf.Asin(SPosture.y);
                if (SPosture.x >= 0) RotPosture.y = Mathf.PI - Mathf.Asin(SPosture.x / Mathf.Cos(RotPosture.x));
                else RotPosture.y = -Mathf.PI - Mathf.Asin(SPosture.x / Mathf.Cos(RotPosture.x));
            }
        }
        RotPosture = -RotPosture;
        */
    }

private void Update()
    {
        PlayerPos = transform.position;

        //移動キーの入力**********************************************************
        MVec = Vector2.zero;
        if (Input.GetKey(KeyCode.S)) MVec.y = 1;
        if (Input.GetKey(KeyCode.X)) MVec.y = -1;
        if (Input.GetKey(KeyCode.Z)) MVec.x = -1;
        if (Input.GetKey(KeyCode.C)) MVec.x = 1;
        //移動キーの入力**********************************************************
    }

    private void FixedUpdate()
    {
        //移動に関する記述******************************************************
        Vector3 Posture = Input.acceleration;
        if(Posture != Vector3.zero)
        {
            float ax = Posture.x; float ay = Posture.y; float az = Posture.z;
            float ox = RotPosture.x; float oy = RotPosture.y;

            float rx = ax * Mathf.Cos(oy) - az * Mathf.Sin(oy);
            float ry = ay * Mathf.Cos(ox) - ax * Mathf.Sin(ox) * Mathf.Sin(oy) - az * Mathf.Sin(ox) * Mathf.Cos(oy);

            MVec.x = rx * (1 + GameManager.eventCount[2] / 50.0f);
            MVec.y = ry * (1 + GameManager.eventCount[2] / 50.0f);

            //  MVec.x = Input.acceleration.x * (1 + GameManager.eventCount[2] / 50.0f);
            //   MVec.y = Input.acceleration.y * (1 + GameManager.eventCount[2] / 50.0f);

            if (MVec.magnitude > 1) MVec.Normalize();
        }
        MoveForceVec = (MVec.x * RightVector + MVec.y * BackVector) * MoveForce;

        Vector3 PlayerSpeed = Rigid.velocity;       //プレイヤーのスピード
        if(PlayerSpeed.magnitude < MaxSpeed && GameManager.StageStartFlag)
        {
            Rigid.AddForce(new Vector3(MoveForceVec.x, MoveForceVec.y, MoveForceVec.z));
        }
        //移動に関する記述******************************************************

#if UNITY_EDITOR
        DDetla += 0.02f;

        float Ax = Input.acceleration.x;
        float Ay = Input.acceleration.y;

        float Bx = 0;
        float By = 0;
        if (Ax > 1) Bx = 1; else if (Ax < -1) Bx = -1; else Bx = Ax;
        if (Ay > 1) By = 1; else if (Ay < -1) By = -1; else By = Ay;
        float Ax2 = Mathf.Asin(Bx);
        float Ay2 = Mathf.Asin(By);

        float Ax3 = Ax2 * 180 / Mathf.PI;
        float Ay3 = Ay2 * 180 / Mathf.PI;

        if(Phone != null)
        {
            Phone.transform.eulerAngles = new Vector3(Ay3, 0, -Ax3);
        }
        /*
        if (DDetla >= Dinter)
        {
            DDetla = 0;
            


            // Debug.Log("Ax:" + Ax + " Ay:" + Ay);
            // Debug.Log("Asitax:" + Ax2 + " Asitay:" + Ay2);
           // Debug.Log("Asitax:" + Ax3 + " Asitay:" + Ay3);
        }
        */
#endif



        //重力の計算
        Rigid.AddForce(GravityVector * GravityForce, ForceMode.Acceleration);
    }

    public void Damage()
    {
        DamageEffect.SetActive(true);
        GameManager.playStageSE(3);
    }
}
