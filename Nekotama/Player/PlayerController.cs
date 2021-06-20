using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    private GameDirector Director;
    [SerializeField] private GameObject Attack1 = null;
    [SerializeField] private GameObject Attack2 = null;
    [SerializeField] private GameObject Attack3 = null;
    [SerializeField] private GameObject FireBall = null;
    [SerializeField] private GameObject ThunderMagic = null;
    [SerializeField] private GameObject FootSensor = null;
    [SerializeField] private GameObject[] EffectObj = null;
    private float PlayerZP = 0f;                    //プレイヤーのZ位置の管理　2Dモード時Zの位置ずれ修正用に使用
    public float Pzp { get { return PlayerZP; } set { PlayerZP = value; } }


    private GameObject StartObjs;
  //  private List<int> StartN = new List<int>();

    //プレイヤーのコンポーネント
    private Rigidbody Rigid;                //プレイヤーの物理ボディ
    private Animator animetor;              //プレイヤーのアニメータ
    

    //プレイヤーの制御に使用
    private Vector3 PlayerP;                //プレイヤーの一コマ前の地点
    public static Vector3 PlayerPos;        //プレイヤーのワールド座標
    private int PStatas = 0;                //プレイヤーの状態（主にアニメの切り替えに使用）0:通常 1:歩き 2:走り 4:空中 5:ボール 6:突進
    private bool ActionFlag = true;         //移動系のアクションが使用できるか
    private bool CanMagic = true;           //詠唱時の魔法を使用できるか?（魔法の連射制限に使用）
    private bool MagicRFlag = false;        //詠唱状態が解除できるか

    private Vector2 PlayerSpeed = new Vector2(0, 0);    //プレイヤーのスピード
    private Vector2 JoyVec = new Vector2(0, 0);         //プレイヤーの移動用ジョイスティックの傾き
    private Vector2 PlayerDir = new Vector2(0, 0);      //プレイヤーの方向ベクトル
    private bool[] MoveKeyFlag = new bool[4] { false, false, false, false };
    public bool BallChineFalg { set; get; }

    //スプリング音の設定
    private bool SpringFlag = true;
    private float SpringDelta = 0;
    private float SpringInterval = 0.5f;

    //*****************空中状態のバク防止用
    private float AirDelta = 0;
    private float AirPosY = 0;
    private float AirCountDis = 0.01f;
    //*****************空中状態のバク防止用

    //プレイヤーのダメージ時の点滅処理
    private SkinnedMeshRenderer render;
    private float DameDelta = 0;

    //プレイヤーのカメラモード切替に使用
    public static bool ModeLock = false;
    private float ModeDelta = 0;
    private float ModeLockTime = 2.0f;
    private Camera2DController StageCamera;
    private bool CameraChangeEnable = false;

    public static Vector3 RebornPos = Vector3.zero;
    private bool RebornPosSaveFlag = true;
    private float RebornPosDelta = 0;

    //チャージアクションに関する変数
    private bool chrageActionFlag = false;
    private int chrageActionStatas = 0;
    private float chrageActionDelta = 0;
    private float chrageActionTime = 1.0f;
    private int chrageActionNum = 0;
    private int chrageEffectNum = 1;

    /// <summary>
    /// カメラの切り替え関数
    /// </summary>
    public void Change3DMode()
    {
        if (CameraChangeEnable && GameManager.StageNum != 1000)
        {
           // Debug.Log("3Dモードに切り替え");
            GameDirector.Player3DMode = true;
            Rigid.constraints = RigidbodyConstraints.FreezeRotation;
            StageCamera.Camera3DMode();
        }
    }
    public void Change2DMode()
    {
        if (CameraChangeEnable && GameManager.StageNum != 1000)
        {
           // Debug.Log("2Dモードに切り替え");
            GameDirector.Player3DMode = false;
            Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, Pzp);
            StageCamera.Camera2DMode();
        }
    }
    private void Change3DCamera()
    {
        if (CameraChangeEnable) StageCamera.Camera3DMode();
    }
    private void Change2DCamera()
    {
        if (CameraChangeEnable) StageCamera.Camera2DMode();
    }

    //プレイヤーの走り状態継続時間(移動キー入力を止めた時の余韻)
    private bool NoRunFlag = true;
    private float NoRunDelta = 0;
    private float NoRunInterbal = 0.7f;

    //プレイヤーのダメージ処理に使用
    static public bool PlayerDamgagFlag = true;
    private float damageDelta = 0;
    static public bool AvoidFlag = false;
    private float AvoidDelta = 0;
    private float AvoidTime = 0.73f;

   
    //センサのONOFF;
    private bool sensorLock = false;                    
    public bool SensorLock
    {
        private set
        {
            sensorLock = value;
         //   if (sensorLock) Debug.Log("センサーロック状態"); else Debug.Log("センサーロック解除");
        }
        get
        {
            return sensorLock;
        }
    }
    private bool onground = true;
    private Effekseer.EffekseerEmitter Effector;
    //着地判定
    public bool OnGround {
        set
        {
            if(SensorLock == false) onground = value;
            if (onground)
            {
              //  Debug.Log("地上状態");
                AirActionFlag = false;
                AirDelta = 0;
                if(FloatMode && !FloatMoveLock)
                {
                    FloatMode = false;
                    GameManager.playStageSE(39);
                    if (!GameDirector.Player3DMode) Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
                    EffectObj[13].SetActive(false);
                    PlayerGravity = Gravity;
                 //   Debug.Log("着地によるフロートモードの解除");
                    if (FloatStatas == 4) ResetFloat();
                    FloatStatas = 0; 
                }
            }
            else
            {
            //    Debug.Log("空中状態");
            }
        }
        get
        {
            return onground;
        }
    }
    private float PlayerGravity;

    //移動に使用するパラメータ
    private float MaxSpped = 7.0f;          //移動の最大スピード
    private float MaxWalkSpped = 1.5f;      //走るモーションへの切り替えスピード
    private float MiniWalkSpped = 0.01f;    //歩くモーションへの切り替えスピード
    private float walkForce = 20.0f;        //移動に使用する力
    private float AirForce = 6.0f;          //空中の移動で使用する力
    private float Gravity = 9.8f;           //デフォルトの重力加速度

    //座り設定
    private float Normaldelta = 0;          //通常状態でいる時間を管理
    private float SitTime = 5.0f;           //座り状態へ以降するまでの時間
    private bool SitFlag = false;           //現在の座っているかを管理

    //ジャンプに使用するパラメータ
    private float jumpForce = 350.0f;       //ジャンプに使用する上方向の力
    private float jumpWaitTime = 0.25f;     //ジャンプの力を加えるまでのラグ
    private float jumpXZForce = 100.0f;     //ジャンプに使用する横方向の力

    //ハイジャンプに使用するパラメータ
    private float highjumpForce = 500.0f;       //ハイジャンプに使用する上方向の力
    private float highjumpWaitTime = 1.00f;     //ハイジャンプの力を加えるまでのラグ
    private float highjumpXZForce = 100.0f;     //ハイジャンプに使用する横方向の力

    //空中でのスピード上限
    private float MaxAirSpeed = 6.0f;

    //攻撃設定
    //猫パンチ
    private float A1RigidTime = 1.2f;       //使用開始から次の行動までの硬直時間
    //スピンアタック
    private float A2RigidTime = 3.0f;       //リキャスト時間
    private bool Attack2Flag = true;        //攻撃が使用可能か
    //空中攻撃
    private bool AirActionFlag = false;

    //ローリング
    private float RollingForce = 380.0f;    //ローリングの力(通常時)
    private float Rolling2Force = 250.0f;   //ローリングの力(走っている時)
    private float RollinWaitTime = 0.2f;    //次の行動が可能か
    private bool PlayMagic = true;
    
    //ボール
    public bool BallFalg { get; set; }
    private Cinemachine.CinemachineDollyCart ChineStatas;
    private float CminSpeed = 0.03f;
    private float CpluSpeed = 0.06f;
    private float CMaxSpeed = 15.0f;

   // private float CpluSpeed = 0.02f;
  //  private float CMaxSpeed = 5.0f;
    private bool BallRFlag = false;

    //フロートボール
    private float FloatForce = 1f;          //フロート時に加える力
    private bool FloatFlag = false;         //フロート時かの判定
    private float FloatMaxSpped = 8.0f;     //フロート時のマックススピード
    private float FloatMass = 0.01f;        //フロート時のボール質量
    private float FloatGravity = 1f;        //フロートアクション時の重力
    private float FloatStatas = 0;          //フロートアクションの段階
    private float FloatDelta = 0f;          //フロートアクションの時間測定
    private float FloatS1Time = 1.0f;       //フロートアクションの受付時間
    private Vector3 FloatResetPos;

    //魔法
    private int FireMpPoint = 2;
    private int ThunderMpPoint = 10;

    //突進
    private bool RushRFlag = false;
    private float RushRInterval = 0.1f;
    private float RushRDelta = 0;
    private float RushFreezeTime = 1f;
    static public bool RushNoDamageFlag = false;
    private float RushMoveForce = 60;
    private float RushMaxSpeed = 20;
    private Vector3 RushVec = Vector3.right;
    private int RushDir = 1;                    //突進の進行方向  1:右　2:左  3:上  4:下
    private Vector3 RushPos = Vector3.zero;
    private float RushStopMove = 0.01f;
    private bool RushDirLock = false;

    private bool RushNoBrake = false;
    private bool RushBrakeFlag = false;
    private float RushBrakeForce = 20f;
    private float RushBrakeTime = 0.4f;

    /// <summary>
    /// 最初に実行
    /// </summary>
    private void Awake()
    {
        GameManager.PlayerObj = gameObject;
        StartObjs = GameObject.FindGameObjectWithTag("StartPoint");
    }
    void Start()
    {
        FirstPosition();
        Rigid = GetComponent<Rigidbody>();                              //プレイヤーの物理ボディ
        PlayerP = GetComponent<Transform>().localPosition;              //プレイヤーのローカル座標
        PlayerPos = GetComponent<Transform>().position;                 //プレイヤーのワールド座標
        animetor = GetComponent<Animator>();                            //プレイヤーのアニメータ
        render = GetComponentInChildren<SkinnedMeshRenderer>();         //プレイヤーのメッシュ
        Effector = GetComponent<Effekseer.EffekseerEmitter>();          //プレイヤーのエフェクタ
        //SE = GetComponent<AudioSource>();                               //プレイヤーのオーディオソース
        ChineStatas = GetComponent<Cinemachine.CinemachineDollyCart>(); //プレイヤーのシネマチェイン

        Director = GameManager.DirectorObj.GetComponent<GameDirector>();
        Movejoystick = GameObject.Find("MoveJoystick").GetComponent<FloatingJoystick>();        //移動用ジョイスティック
        Actionjoystick = GameObject.Find("ActionPad").GetComponent<FloatingJoystick2>();        //アクション用ジョイスティック                                                                                         
        var camera = GameObject.FindGameObjectWithTag("MainCamera");  
        //メインカメラを取得
        if (camera.GetComponent<Camera2DController>() != null)
        {
            StageCamera = camera.GetComponent<Camera2DController>();
            CameraChangeEnable = true;
        }
        else if ( GameManager.StageNum == 0) GameDirector.Player3DMode = true;
        //初期パラメータの設定
        if (!GameDirector.Player3DMode) Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        OnGround = false;
        PlayerZP = PlayerP.z;
        Rigid.useGravity = false;
        PlayerGravity = Gravity;
        BallChineFalg = false;
        BallFalg = false;

       // GameDirector.GamePlay = true;
        GameManager.PlayerBallFlag = false;
    }

    //キー入力などの処理を記述（毎フレーム処理）
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) MoveKeyFlag[0] = true;
        if (Input.GetKeyDown(KeyCode.Z)) MoveKeyFlag[1] = true;
        if (Input.GetKeyDown(KeyCode.X)) MoveKeyFlag[2] = true;
        if (Input.GetKeyDown(KeyCode.C)) MoveKeyFlag[3] = true;
        if (Input.GetKeyUp(KeyCode.S)) MoveKeyFlag[0] = false;
        if (Input.GetKeyUp(KeyCode.Z)) MoveKeyFlag[1] = false;
        if (Input.GetKeyUp(KeyCode.X)) MoveKeyFlag[2] = false;
        if (Input.GetKeyUp(KeyCode.C)) MoveKeyFlag[3] = false;

        if (Input.GetKeyDown(KeyCode.Space)) Debug.Log("P方向" + PlayerDir);
    }

    //メイン処理
    private void FixedUpdate()
    {
        //プレイヤーの状態を管理（スピードや向き）*******************************************
        //プレイヤーの速度
        PlayerSpeed.x = Rigid.velocity.x;
        PlayerSpeed.y = Rigid.velocity.z;
        float speed = PlayerSpeed.magnitude;

        //重力の計算
        Rigid.AddForce(new Vector3(0, -PlayerGravity, 0), ForceMode.Acceleration);

        //移動キーの入力
        float mx = Movejoystick.Horizontal;
        float my = Movejoystick.Vertical;
        JoyVec.x = mx;
        JoyVec.y = my;

        float angle = Mathf.Atan(my / mx);
        if (mx < 0)
        {
            if (my >= 0) angle += Mathf.PI; else angle -= Mathf.PI;
        }
        float AngleD = angle / Mathf.PI * 180.0f;
        float ARh = AngleRevision / 2.0f;

        //左右の角度制限
        if (Mathf.Abs(AngleD) <= ARh || Mathf.Abs(AngleD) >= 180 - ARh) JoyVec.y = 0;

        //上下の制限
        if (90 - ARh <= AngleD && AngleD <= 90 + ARh) JoyVec.x = 0;
        if (-90 - ARh <= AngleD && AngleD <= -90 + ARh) JoyVec.x = 0;

        if (MoveKeyFlag[0]) JoyVec.y = 1;
        if (MoveKeyFlag[1]) JoyVec.x = -1;
        if (MoveKeyFlag[2]) JoyVec.y = -1;
        if (MoveKeyFlag[3]) JoyVec.x = 1;
        bool KeyPush = false;
        for (int i = 0; i < 4; i++) { if (MoveKeyFlag[i]) KeyPush = true; }



        //    Debug.Log("ジョイスティック " + Movejoystick.Horizontal);

        //プレイヤーの方向を計算
        Vector2 diff2 = new Vector2(transform.localPosition.x, transform.localPosition.z) - new Vector2(PlayerP.x, PlayerP.z);
        if ( ( diff2.magnitude > 0.02f && OnGround && !BallFalg && !FloatFlag && (Movejoystick.isMoveStick || KeyPush) ) || (PStatas == 6 && !RushDirLock ) )
        {
            Vector2 DisVec = new Vector2(diff2.x, diff2.y);
            PlayerDir = DisVec.normalized;
            transform.rotation = Quaternion.LookRotation(new Vector3(diff2.y, 0, -diff2.x));
        }
        Vector3 diff3 = transform.localPosition - PlayerP;
        if ((BallFalg || FloatFlag) && diff3.magnitude > 0.02f)
        {
            transform.rotation = Quaternion.LookRotation(new Vector3(diff3.z, 0, -diff3.x));
        }

        //プレイヤーのダメージフラグ
        if (!PlayerDamgagFlag)
        {
            damageDelta += 0.02f;
            DameDelta += 0.02f;
            if (DameDelta >= 0.1f)
            {
                DameDelta = 0;
                render.enabled = !render.enabled;
            }

            if (damageDelta >= 2)
            {
                damageDelta = 0;
                PlayerDamgagFlag = true;
                render.enabled = true;
            }
        }
        if (AvoidFlag)
        {
            AvoidDelta += 0.02f;
            if (AvoidDelta >= AvoidTime)
            {
                AvoidFlag = false;
            }
        }

        //復活ポイントの当たり判定リセット
        if (!RebornPosSaveFlag)
        {
            RebornPosDelta += 0.02f;
            if (RebornPosDelta > 1.0f)
            {
                RebornPosDelta = 0;
                RebornPosSaveFlag = true;
            }
        }

        //**********空中状態バグ防止用

        if (!OnGround && !FloatFlag)
        {
            float DisY = transform.position.y - AirPosY;
            if (Mathf.Abs(DisY) < AirCountDis)
            {
                AirDelta += 0.02f;
            }
            else
            {
                AirDelta = 0;
            }
            if (AirDelta > 0.3f)
            {
                OnGround = true;
                Debug.Log("空中状態強制解除");
            }
            AirPosY = transform.position.y;
        }

        //スプリングオブジェクトの効果音
        if (!SpringFlag)
        {
            SpringDelta += 0.02f;
            if (SpringDelta > SpringInterval)
            {
                SpringDelta = 0;
                SpringFlag = true;
            }
        }

        //**********空中状態バグ防止用

        //プレイヤーのモード切替（2Dと3D）のロック解除
        if (ModeLock)
        {
            ModeDelta += 0.02f;
            if (ModeDelta >= ModeLockTime)
            {
                ModeDelta = 0;
                ModeLock = false;
            }
        }

        //ボール状態時のアニメスピード
        if (PStatas == 5 && ActionFlag && !BallChineFalg) animetor.speed = Rigid.velocity.magnitude / 5.0f;
        //ボールアクションを定義
        if (BallChineFalg)
        {
            float CSpeed = ChineStatas.m_Speed;
            //放置による減算
            if (!KeyPush && !Movejoystick.isMoveStick)
            {
                if (CSpeed > 0)
                {
                    CSpeed -= CminSpeed;
                    if (CSpeed < 0) CSpeed = 0;
                }
                else if (CSpeed < 0)
                {
                    CSpeed += CminSpeed;
                    if (CSpeed > 0) CSpeed = 0;
                }
            }
            //移動スティックによる加算
            else if (JoyVec.x > 0)
            {
                if (CSpeed < CMaxSpeed)
                {
                    if (CSpeed >= 0) CSpeed += CpluSpeed; else CSpeed = CSpeed + CpluSpeed * 2;
                    if (CSpeed > CMaxSpeed) CSpeed = CMaxSpeed;
                }
            }
            //移動スティックによる減算
            else if (JoyVec.x < 0)
            {
                if (CSpeed > -CMaxSpeed)
                {
                    if (CSpeed <= 0) CSpeed -= CpluSpeed; else CSpeed = CSpeed - CpluSpeed * 2;
                    if (CSpeed < -CMaxSpeed) CSpeed = -CMaxSpeed;
                }
            }
            ChineStatas.m_Speed = CSpeed;
            animetor.speed = Mathf.Abs(CSpeed / 4.0f);
        }

        //フロートボールのアクション
        if (FloatStatas == 1) FloatDelta += 0.02f;
        if (FloatDelta > FloatS1Time) FloatStatas = 0;
        if ((FloatStatas == 2 || FloatStatas == 3) && FloatMode && transform.position.y > PlayerFloatLimitP)
        {
            Debug.Log("上限によるフロートモードの解除");
            PlayerGravity = Gravity;
            GameManager.playStageSE(39);
            FloatMode = false;
            FloatMoveLock = false;
            if (!GameDirector.Player3DMode) Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
            else Rigid.constraints = RigidbodyConstraints.FreezeRotation;
            //   Effector.ChangeSpeed(5, 1);
            //  Effector.ChangeSize(5, 1);
            EffectObj[13].SetActive(false);
            FloatStatas = 0;
        }
        if (FloatStatas == 5 && (transform.position.z > PlayerFloatLimitP && ZFront) || (transform.position.z < PlayerFloatLimitP && !ZFront))
        {
            ResetFloat();
        }
        if (FloatStatas == 5)
        {
            if (-5 < Rigid.velocity.z && Rigid.velocity.z < 5) ResetFloat();
        }

        //歩く、走る、待機状態の変化
        if (OnGround)
        {
            //待機状態への変化
            if ((speed <= MiniWalkSpped || NoRunFlag) && (PStatas == 1 || PStatas == 2))
            {
                PStatas = 0;
                animetor.SetInteger("PlayerStatas", PStatas);
                NoRunDelta = 0;
                if (!GameDirector.Player3DMode) transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, PlayerZP);
            }
            //歩き状態への変化
            else if (MiniWalkSpped < speed && speed <= MaxWalkSpped && (PStatas == 0 || PStatas == 2) && (Movejoystick.isMoveStick || KeyPush))
            {
                PStatas = 1;
                animetor.SetInteger("PlayerStatas", PStatas);
            }
            //走り状態への変化
            else if (MaxWalkSpped < speed && (PStatas == 0 || PStatas == 1) && (Movejoystick.isMoveStick || KeyPush))
            {
                PStatas = 2;
                animetor.SetInteger("PlayerStatas", PStatas);
                NoRunFlag = false;
            }

            if (!NoRunFlag)
            {
                if (!Movejoystick.isMoveStick && !KeyPush)
                {
                    NoRunDelta += 0.02f;
                    if (NoRunDelta > NoRunInterbal)
                    {
                        NoRunDelta = 0;
                        NoRunFlag = true;
                    }
                }
                else
                {
                    NoRunDelta = 0;
                }
            }
        }

        
        //着地処理
        if (PStatas == 4 && OnGround && SensorLock == false)
        {
            PStatas = 0;
            animetor.SetInteger("PlayerStatas", PStatas);
        }

        //座りモーションに変更
        if (PStatas == 0 && SitFlag == false && ActionFlag)
        {
            Normaldelta += 0.02f;
        }
        else if (PStatas != 0)
        {
            Normaldelta = 0;
            SitFlag = false;
        }
        if (Normaldelta >= SitTime && SitFlag == false)
        {
            SitFlag = true;
            Normaldelta = 0;
            animetor.SetTrigger("Sit");
        }

        //プレイヤーの移動に関する処理  
        inputNormalMove();          //通常時およびボール時の移動の入力
        inputFloatBallMove();       //フロートボール時の移動の入力
        inputAirMove();             //空中の移動の入力

        //突進の処理
        RushActive();               //突進アクティブ時の処理

        //プレイヤー地点の更新
        PlayerP = transform.localPosition;
        PlayerPos = transform.position;

        //アクションパッドの処理
        chackCanUsePadAction();     //パッドアクションが使用できるか
        EnableActionPad();          //パッドアクション使用中に実行
        inputChrage();              //チャージアクションの入力
    }

    private bool FloatMoveLock = false;
    private bool FloatRLock = false;
    private void FloatMoveR2()
    {
        if (FloatStatas == 2) FloatMoveLock = false;
        if (!GameDirector.Player3DMode) Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        else Rigid.constraints = RigidbodyConstraints.FreezeRotation;
    }
    private void FloatMoveR3()
    {
        FloatMoveLock = false;
    }
    private void FloatMoveR4() { if (FloatStatas == 4) ResetFloat(); }
    private void FloatMoveR6() { if (FloatStatas == 6) ResetFloat(); }
    private void FloatMoveR8()
    {
        if (!GameDirector.Player3DMode) Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
        else Rigid.constraints = RigidbodyConstraints.FreezeRotation;
        FloatStatas = 0;
        FloatRLock = false;
        Director.UseIcon(2);
    }
    private float PlayerFloatLimitP = 0;
    private bool FloatMode = false;
    private bool ZFront = true;
    private void ResetFloat()
    {
        Director.PlayerHP -= 20;
        Effector.Play(8);
        GameManager.playStageSE(21);
        FloatMoveLock = true;
        Rigid.constraints = RigidbodyConstraints.FreezeAll;
        Rigid.transform.position = FloatResetPos;
        Invoke("ResetFloatAfter", 0.5f);
    }
    private void ResetFloatAfter()
    {
        if (!GameDirector.Player3DMode)
        {
            Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
            Change2DCamera();
        }  
        else Rigid.constraints = RigidbodyConstraints.FreezeRotation;
        FloatRLock = false;
        Director.UseIcon(2);
        FloatMoveLock = false;
    }

    //ジョイスティック
    private FloatingJoystick Movejoystick;
    private FloatingJoystick2 Actionjoystick;
   // private float MoveStartRange = 0.2f;
    private float AngleRevision = 45;


    
    //プレイヤーの開始位置記入
    private  void FirstPosition()
    {
        transform.position = StartObjs.transform.position;
        transform.eulerAngles = StartObjs.transform.eulerAngles + Vector3.down * 90;
    }

    //プレイヤーの衝突判定

    /// <summary>
    /// 接触中の処理
    /// </summary>
    /// <param name="other"></param>
    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag == "DamageObject" && PlayerDamgagFlag && !AvoidFlag && !RushNoDamageFlag && GameDirector.GamePlay)
        {
            PlayerDamgagFlag = false;
            Director.PlayerHP -= other.gameObject.GetComponent<DamageObject>().Damage;
            Effector.Play(8);
            GameManager.playStageSE(21);
        }
    }

    /// <summary>
    /// 接触時の処理
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "EnemyHiter" && PlayerDamgagFlag && !AvoidFlag && !RushNoDamageFlag && GameDirector.GamePlay)
        {
            if (other.GetComponent<EnemyHitController>().PConFlag)
            {
                PlayerDamgagFlag = false;
                int EPow = other.gameObject.GetComponent<EnemyHitController>().AttackPower;
                float PDef = GameManager.PlayerDef;
                if (GameManager.eventCount[11] >= 1) PDef *= 1.5f;
                int Damage = EPow * EPow / (int)PDef;
                if (Damage == 0) Damage = 1;
                Director.PlayerHP -= Damage;
                Effector.Play(8);
                GameManager.playStageSE(21);
            }  
        }

        if (other.gameObject.tag == "DamageObject" && PlayerDamgagFlag && !AvoidFlag && !RushNoDamageFlag && GameDirector.GamePlay)
        {
            PlayerDamgagFlag = false;
            Director.PlayerHP -= other.gameObject.GetComponent<DamageObject>().Damage;
            Effector.Play(8);
            GameManager.playStageSE(21);
        }

        //フロートアクションのセンサ処理
        if (FloatFlag)
        {
            if(other.gameObject.tag == "FloatSensor")
            {
                FloatSensor sensor = other.gameObject.GetComponent<FloatSensor>();
                if (sensor.isContact)
                {
                    sensor.isContact = false;
                    int num = sensor.Number;
                    if (num == 1)
                    {
                        Debug.Log("フロートアクション段階_1");
                        FloatStatas = 1;
                        FloatDelta = 0;
                        FloatResetPos = other.gameObject.transform.position;
                    }

                    if (num == 2 && FloatStatas == 1)
                    {
                        Debug.Log("フロートアクション段階_2");
                        FloatMoveLock = true;
                        Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                        FloatStatas = 2;
                        Invoke("FloatMoveR2", 0.3f);
                        PlayerGravity = FloatGravity;
                        PlayerFloatLimitP = sensor.LimitPoint;
                        FloatMode = true;
                        GameManager.playStageSE(38);
                        EffectObj[13].SetActive(true);
                    }

                    //二次元の変化
                    if (num == 3 && FloatStatas == 2)
                    {
                        Debug.Log("フロートアクション段階_3");
                        FloatStatas = 3;
                        FloatMoveLock = true;
                        Invoke("FloatMoveR3", 1.0f);
                    }

                    if (num == 4 && FloatStatas == 2)
                    {
                        Debug.Log("フロートアクション段階_4");
                        FloatRLock = true;
                        Director.NoUseIcon(2);
                        FloatStatas = 4;
                        Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX;
                        Invoke("FloatMoveR4", 1.0f);
                    }

                    if (num == 5 && FloatStatas == 4)
                    {
                        Debug.Log("フロートアクション段階_5");
                        ZFront = sensor.ZFrontFlagGet();
                        PlayerFloatLimitP = sensor.LimitPoint;
                        FloatStatas = 5;
                        Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
                        if (!GameDirector.Player3DMode) Change3DCamera();
                    }

                    if (num == 6 && FloatStatas == 5)
                    {
                        Debug.Log("フロートアクション段階_6");
                        FloatStatas = 6;
                        Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX;
                        Invoke("FloatMoveR6", 2.0f);
                    }

                    if (num == 7 && FloatStatas == 6)
                    {
                        Debug.Log("フロートアクション段階_7");
                        FloatStatas = 7;
                        Pzp = sensor.ChangeZPos();
                        if (!GameDirector.Player3DMode)
                        {
                            Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
                            Change2DCamera();
                        }
                        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, Pzp);
                        FloatRLock = false;
                        Director.UseIcon(2);
                    }

                    if (num == 8 && FloatStatas == 3)
                    {
                        Debug.Log("フロートアクション段階_8");
                        FloatStatas = 8;
                        Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
                        Invoke("FloatMoveR8", 1.0f);
                    }
                }
            }
        }

        //復活地点の記憶
        if (other.gameObject.tag == "SavePoint"　&& RebornPosSaveFlag)
        {
            RebornPosSaveFlag = false;
            RebornPos = other.transform.position;
        }
    }    

    /// <summary>
    /// プレイヤーのダメージ処理
    /// </summary>
    /// <param name="Damage"></param>
    public void PlayerDamage(int Damage)
    {
        Director.PlayerHP -= Damage;
        Effector.Play(8);
        GameManager.playStageSE(21);
    }

    //プレイヤーの基本アクションの処理

    //移動に関する処理
    /// <summary>
    /// 通常およびボールモード時の移動処理
    /// </summary>
    private void inputNormalMove()
    {
        if ((OnGround || (BallFalg && !BallChineFalg)) && !FloatFlag)
        {
            if (PlayerSpeed.magnitude < MaxSpped && ActionFlag)//スピード制限
            {
                //移動のための力を加える
                if (GameDirector.Player3DMode) Rigid.AddForce(new Vector3(JoyVec.x * walkForce, 0, JoyVec.y * walkForce));
                else Rigid.AddForce(new Vector3(JoyVec.x * walkForce, 0, 0));
            }
        }
    }
    /// <summary>
    /// フロートボール時の移動処理
    /// </summary>
    private void inputFloatBallMove()
    {
        if (FloatFlag && !FloatMoveLock)
        {
            float FloatX = JoyVec.x;
            float FloatY = JoyVec.y;
            if ((PlayerSpeed.x > FloatMaxSpped && JoyVec.x > 0) || (PlayerSpeed.x < -FloatMaxSpped && JoyVec.x < 0)) FloatX = 0;
            if ((PlayerSpeed.y > FloatMaxSpped && JoyVec.y > 0) || (PlayerSpeed.y < -FloatMaxSpped && JoyVec.y < 0)) FloatY = 0;
            if (GameDirector.Player3DMode) Rigid.AddForce(new Vector3(FloatX * FloatForce, 0, FloatY * FloatForce));
            else Rigid.AddForce(new Vector3(FloatX * FloatForce, 0, 0));
        }
    }
    /// <summary>
    /// 空中の移動
    /// </summary>
    private void inputAirMove()
    {
        if (ActionFlag && !onground && !BallChineFalg && !FloatFlag)
        {
            float AirX = JoyVec.x;
            float AirY = JoyVec.y;
            if ((PlayerSpeed.x > MaxAirSpeed && JoyVec.x > 0) || (PlayerSpeed.x < -MaxAirSpeed && JoyVec.x < 0)) AirX = 0;
            if ((PlayerSpeed.y > MaxAirSpeed && JoyVec.y > 0) || (PlayerSpeed.y < -MaxAirSpeed && JoyVec.y < 0)) AirY = 0;

            if (GameDirector.Player3DMode) Rigid.AddForce(new Vector3(AirX * AirForce, 0, AirY * AirForce));
            else Rigid.AddForce(new Vector3(AirX * AirForce, 0, 0));
        }
    }

    //移動パッドに関する記述***********************************************************

    /// <summary>
    /// 移動キーを入力しているかの判定
    /// </summary>
    /// <returns></returns>
    private bool getMoveKeyEnable()
    {
        bool isMoveKey = false;
        for (int i = 0; i < 4; i++) { if (MoveKeyFlag[i]) isMoveKey = true; }
        if (JoyVec != Vector2.zero) isMoveKey = true;
        return isMoveKey;
    }

    //アクションパッドに関する記述******************************************************

    /// <summary>
    /// アクションパッドの状態を更新する
    /// </summary>
    /// <param name="num"></param>
    private void ActionIconStatasChack(int num)
    {
        int[] PaletteNum = Director.PaletteNumber;
        for(int i = 0; i < PaletteNum.Length; i++)
        {
            if(PaletteNum[i] == num)
            {
                if (GameDirector.CanUseAction[num]) Director.UseIcon(i);
                else Director.NoUseIcon(i);
            }
        } 
    }

    //アクションパッド呼び出しに実行
    public void StartActionPad()
    {

    }
    //アクションパッド使用時に実行
    public void EnableActionPad()
    {
        if (Actionjoystick.isActionStick)
        {
            if (PStatas != 5 && PStatas != 6 && GameDirector.MagicType == 0)
            {
                int PANum = getPadPlaceNum();
                if ((chrageActionNum == 0 || chrageActionNum == PANum) && PANum >= 1)
                {
                    chrageActionNum = PANum;
                    chrageActionFlag = true;
                }
                else
                {
                    chrageActionFlag = false;
                }
            }
        }
    }

    /// <summary>
    /// アクションのチャージの有力
    /// </summary>
    private void inputChrage()
    {
        if (chrageActionFlag)
        {
            chrageActionDelta += 0.02f;
            if(chrageActionDelta >= chrageActionTime / 5 && chrageActionStatas == 0)
            {
                if (chrageActionNum >= 1) chrageEffectNum = chrageActionNum;
                chrageActionStatas = 1;
                EffectObj[chrageEffectNum - 1].SetActive(true);
            }
            if(chrageActionDelta >= chrageActionTime && chrageActionStatas == 1)
            {
                chrageActionStatas = 2;
                EffectObj[chrageEffectNum - 1].SetActive(false);
                EffectObj[chrageEffectNum + 3].SetActive(true);
                if (chrageActionNum == 1) Director.PaletteNumber = new int[5] { 0, 3, 4, 2, 6 };
                if (chrageActionNum == 2) Director.PaletteNumber = new int[5] { 0, 1, 5, 2, 6 };
                if (chrageActionNum == 3) Director.PaletteNumber = new int[5] { 0, 1, 4, 8, 6 };
                if (chrageActionNum == 4) Director.PaletteNumber = new int[5] { 0, 1, 4, 2, 7 };
                GameManager.playStageSE(20);
            }

            if (getMoveKeyEnable() && chrageActionStatas != 2) resetChrage();
        }
        else
        {
            resetChrage();
        }
    }

    /// <summary>
    /// アクションのチャージ状況をリセットする
    /// </summary>
    private void resetChrage()
    {
        chrageActionFlag = false;
        chrageActionDelta = 0;
        for(int i = 0; i < 8; i++)
        {
            EffectObj[i].SetActive(false);
        }
        chrageActionNum = 0;
        if (chrageActionStatas == 2) Director.PaletteNumber = new int[5] { 0, 1, 4, 2, 6 };
        chrageActionStatas = 0;
    }

    //アクションパッド終了時に実行
    public void EndActionPad() => SelectPadAction(getPadPlaceNum());

    /// <summary>
    /// アクションパッドの位置番号を取得
    /// </summary>
    /// <returns></returns>
    private int getPadPlaceNum()
    {
        float HX = Actionjoystick.HandlePos.x;
        float HZ = Actionjoystick.HandlePos.y;
        int ActionPlace = 0;
        float dd = Mathf.Sqrt(HX * HX + HZ * HZ);
        if (dd > 0.5f)
        {
            float y1 = Mathf.Abs(HX);
            float y2 = -y1;
            if (HZ > y1) ActionPlace = 1;
            else if (HZ < y2) ActionPlace = 2;
            else if (HX >= 0) ActionPlace = 3;
            else ActionPlace = 4;
        }
        return ActionPlace;
    }

    /// <summary>
    /// アクションパッドの実行
    /// </summary>
    /// <param name="ActionPlace"></param>
    private void SelectPadAction(int ActionPlace)
    {
        //特殊アクションの判定
        if (PStatas == 5 || PStatas == 6)
        {
            if (ActionPlace == 2) PlayPadAction(9);
        }
        else if (GameDirector.MagicType >= 1)
        {
            if (ActionPlace == 2) PlayPadAction(9);
            if (ActionPlace == 1) PlayPadAction(10);
        }
        //通常アクションの判定
        else PlayPadAction(Director.PaletteNumber[ActionPlace]);
    }

    /// <summary>
    /// パッドアクションの処理
    /// </summary>
    /// <param name="An"></param>
    private void PlayPadAction(int An)
    {
        if (An <= 8) resetChrage();

        if (An == 0)                 //攻撃
        {
            if (GameDirector.CanUseAction[0])
            {
                if (PStatas == 0)                  NormalAttack();     //通常攻撃を使用
                if (PStatas == 2 && Attack2Flag)   SpinAttack();       //スピン攻撃を使用
                if (AirActionFlag)                 AirSpinAttack();
            }
        }
        else if (An == 1)            //ジャンプ
        {
            if (GameDirector.CanUseAction[1]) Jump();             //ジャンプを使用
        }
        else if (An == 2)            //ローリング
        {
            if (GameDirector.CanUseAction[2]) Rolling();
        }
        else if (An == 3)            //ハイジャンプ
        {
            if (GameDirector.CanUseAction[3]) HighJump();
        }
        else if (An == 4)            //ボール
        {
            if (GameDirector.CanUseAction[4]) Ball();
        }
        else if (An == 5)            //マジック
        {
            if (GameDirector.CanUseAction[5]) FloatBall();
        }
        else if (An == 6)            //ファイア
        {
            if (GameDirector.CanUseAction[6]) Fire();
        }
        else if (An == 7)            //サンダー
        {
            if (GameDirector.CanUseAction[7]) Thunder();
        }
        else if (An == 8)           //突進
        {
            if (GameDirector.CanUseAction[8]) Rush();
        }
        else if (An == 9)          //アクション解除
        {
            if (PStatas == 5 && !BallChineFalg && !FloatMoveLock && BallRFlag && !FloatRLock) BallRelease();
            else if (GameDirector.MagicType >= 1 && MagicRFlag) MagicReleae();
            else if (PStatas == 6 && RushRFlag) RushRelease();
        }
        else if (An == 10)      //特殊アクションを実行
        {
            if (GameDirector.MagicType == 1 && Director.PlayerMP >= FireMpPoint && PlayMagic) PlayFire();
            else if (GameDirector.MagicType == 2 && Director.PlayerMP >= ThunderMpPoint && PlayMagic) PlayThunder();
        }
    }

    /// <summary>
    /// アクションを使用できるかの判定
    /// </summary>
    private void chackCanUsePadAction()
    {
        if ( (PStatas != 5 || PStatas != 6 ) && GameDirector.EnemyTargetFlag == false)
        {
            //攻撃
            if (GameDirector.CanUseAction[0])
            {
                if (((OnGround == false || PStatas == 4) && AirActionFlag == false) || PStatas == 1 || ActionFlag == false || (Attack2Flag == false && PStatas == 2))
                {
                    GameDirector.CanUseAction[0] = false;
                    ActionIconStatasChack(0);
                }
            }
            else
            {
                if (ActionFlag && (OnGround && (PStatas == 0 || (PStatas == 2 && Attack2Flag))) || AirActionFlag)
                {
                    GameDirector.CanUseAction[0] = true;
                    ActionIconStatasChack(0);
                }
            }

            //ジャンプ、ローリング、ハイジャンプ
            for (int i = 1; i <= 3; i++)
            {
                if (GameDirector.CanUseAction[i])
                {
                    if (OnGround == false || PStatas == 4 || ActionFlag == false)
                    {
                        GameDirector.CanUseAction[i] = false;
                        ActionIconStatasChack(i);
                    }
                }
                else
                {
                    if (OnGround && ActionFlag && PStatas <= 2)
                    {
                        GameDirector.CanUseAction[i] = true;
                        ActionIconStatasChack(i);
                    }
                }
            }

            //ボール、フロートボール
            for (int i = 4; i <= 5; i++)
            {
                if (GameDirector.CanUseAction[i])
                {
                    if (OnGround == false || PStatas >= 3 || ActionFlag == false)
                    {
                        GameDirector.CanUseAction[i] = false;
                        ActionIconStatasChack(i);
                    }
                }
                else
                {
                    if (OnGround && ActionFlag && 0 <= PStatas && PStatas <= 2)
                    {
                        GameDirector.CanUseAction[i] = true;
                        ActionIconStatasChack(i);
                    }
                }
            }

            //ファイアボール
            if (GameDirector.CanUseAction[6])
            {
                if (OnGround == false || PStatas >= 3 || ActionFlag == false || GameManager.PlayerMp < FireMpPoint)
                {
                    GameDirector.CanUseAction[6] = false;
                    ActionIconStatasChack(6);
                }
            }
            else
            {
                if (OnGround && ActionFlag && 0 <= PStatas && PStatas <= 2 && GameManager.PlayerMp >= FireMpPoint)
                {
                    GameDirector.CanUseAction[6] = true;
                    ActionIconStatasChack(6);
                }
            }

            //サンダー
            if (GameDirector.CanUseAction[7])
            {
                if (OnGround == false || PStatas >= 3 || ActionFlag == false || GameManager.PlayerMp < ThunderMpPoint)
                {
                    GameDirector.CanUseAction[7] = false;
                    ActionIconStatasChack(7);
                }
            }
            else
            {
                if (OnGround && ActionFlag && 0 <= PStatas && PStatas <= 2 && GameManager.PlayerMp >= ThunderMpPoint)
                {
                    GameDirector.CanUseAction[7] = true;
                    ActionIconStatasChack(7);
                }
            }

            //突進
            if (GameDirector.CanUseAction[8])
            {
                if (OnGround == false || PStatas >= 3 || ActionFlag == false)
                {
                    GameDirector.CanUseAction[8] = false;
                    ActionIconStatasChack(8);
                }
            }
            else
            {
                if (OnGround && ActionFlag && 0 <= PStatas && PStatas <= 2)
                {
                    GameDirector.CanUseAction[8] = true;
                    ActionIconStatasChack(8);
                }
            }
        }
        //詠唱時の魔法が使用できるかの定義
        if (GameDirector.MagicType >= 1)
        {
            if (CanMagic)
            {
                if ((GameDirector.MagicType == 1 && Director.PlayerMP < FireMpPoint) || (GameDirector.MagicType == 2 && Director.PlayerMP < ThunderMpPoint) || PlayMagic == false)
                {
                    CanMagic = false;
                    Director.NoUseIcon(1);
                }
            }
            else
            {
                if (((GameDirector.MagicType == 1 && Director.PlayerMP >= FireMpPoint) || (GameDirector.MagicType == 2 && Director.PlayerMP >= ThunderMpPoint)) && PlayMagic)
                {
                    CanMagic = true;
                    Director.UseIcon(1);
                }
            }
        }
    }

    //アクションパットに関する記述はここまで******************************************************


    //アクションパットのアクションを定義*******************************************************

    /// <summary>
    /// 攻撃の処理
    /// </summary>
    //ノーマル攻撃
    private void NormalAttack()
    {
     //   Debug.Log("通常攻撃");
        Normaldelta = 0;
        ActionFlag = false;
        SitFlag = false;
        animetor.SetTrigger("Attack01");
        Invoke("MovePossible", A1RigidTime);
        Invoke("AttackEffct1", 0.5f);
    }
    private void AttackEffct1()
    {
        Attack1.GetComponent<AttackController>().PlayAttack();
        GameManager.playStageSE(16);
    }
    //ダッシュ攻撃
    private void SpinAttack()
    {
  //      Debug.Log("スピン攻撃");
        Attack2Flag = false;
        ActionFlag = false;
        animetor.SetTrigger("Attack02");
        Invoke("Attack2Possible", A2RigidTime);
        Invoke("MovePossible", 16.0f / 30.0f);
        Attack2.GetComponent<AttackController>().PlayAttack();
        GameManager.playStageSE(33);
    }
    private void Attack2Possible()
    {
        Attack2Flag = true;
    }
    //空中攻撃
    private void AirSpinAttack()
    {
      //  Debug.Log("エアスピン攻撃");
        AirActionFlag = false;
        ActionFlag = false;
        animetor.SetTrigger("Attack03");
        Invoke("MovePossible", 16.0f / 30.0f);
        Attack3.GetComponent<AttackController>().PlayAttack();
        GameManager.playStageSE(33);
    }
    private void canAirAttack() { if (OnGround == false) AirActionFlag = true; }

    /// <summary>
    /// ジャンプの処理
    /// </summary>
    private void Jump()
    {
        OnGround = false;
        SensorLock = true;
        ActionFlag = false; 
        PStatas = 4;
        animetor.SetInteger("PlayerStatas", 4);
        animetor.SetTrigger("Jump");
        StopBody();
        Invoke("addJumpFoace", jumpWaitTime);
    }
    private void addJumpFoace()
    {
        Effector.Play(6);
        GameManager.playStageSE(26);
        if (GameDirector.Player3DMode) Rigid.AddForce(JoyVec.x * jumpXZForce, jumpForce, JoyVec.y * jumpXZForce);
        else Rigid.AddForce(JoyVec.x * jumpXZForce, jumpForce, 0);
        OnGround = false;
        Invoke("SensorActive", 0.3f);
        Invoke("canAirAttack", 0.3f);
        ActionFlag = true;
      //  Debug.Log("アクション使用禁止解除（ジャンプ使用時）");
    }

    /// <summary>
    /// ハイジャンプの処理
    /// </summary>
    private void HighJump()
    {
        OnGround = false;
        SensorLock = true;
        ActionFlag = false;
        PStatas = 4;
        animetor.SetInteger("PlayerStatas", 4);
        animetor.SetTrigger("HighJump");
        StopBody();
        Invoke("addHighJumpFoace", 0.05f);
    }
    private void addHighJumpFoace()
    {
        GameManager.playStageSE(25);
        if (GameDirector.Player3DMode) Rigid.AddForce(JoyVec.x * highjumpXZForce, highjumpForce, JoyVec.y * highjumpXZForce);
        else Rigid.AddForce(JoyVec.x * highjumpXZForce, highjumpForce, 0);
        Invoke("SensorActive", 0.3f);
        ActionFlag = true;
        Invoke("canAirAttack", 1.1f);
    }

    /// <summary>
    /// ローリングアクションの処理
    /// </summary>
    private void Rolling()
    {
        ActionFlag = false;
        Invoke("RollingPlay", RollinWaitTime);
        AvoidDelta = 0;
        AvoidFlag = true;
    }
    private void RollingPlay()
    {
        Effector.Play(0);
        animetor.SetTrigger("Rolling");
        GameManager.playStageSE(29);
        Vector2 RollingF = new Vector2(0, 0);
        if (JoyVec.magnitude >= 0.2)
        {
            if (PStatas == 2) RollingF = new Vector2(JoyVec.x * Rolling2Force, JoyVec.y * Rolling2Force);
            else RollingF = new Vector2(JoyVec.x * RollingForce, JoyVec.y * RollingForce);
        }
        else
        {
            if (PStatas == 2) RollingF = new Vector2(PlayerDir.x * Rolling2Force, PlayerDir.y * Rolling2Force);
            else RollingF = new Vector2(PlayerDir.x * RollingForce, PlayerDir.y * RollingForce);
        }
        if (GameDirector.Player3DMode == false) { RollingF.y = 0f; }
        Rigid.AddForce(RollingF.x, 0, RollingF.y);
        Invoke("MovePossible", 16.0f / 30.0f);
    }

    /// <summary>
    /// ボールアクションの処理
    /// </summary>
    private void Ball()
    {
        BallRFlag = false;
        Effector.Play(3);
        GameManager.playStageSE(18);
        PStatas = 5;
        ActionFlag = false;
        animetor.SetInteger("PlayerStatas", PStatas);
        GetComponents<Collider>()[0].enabled = false;
        GetComponents<Collider>()[1].enabled = false;
        Director.ChangePalette(2);
        Invoke("BallMode", 10.0f / 30.0f);
        BallFalg = true;
        FootSensor.transform.localScale = new Vector3(1, 10, 1);
    }

    /// <summary>
    /// フロートボールの処理
    /// </summary>
    private void FloatBall()
    {
        BallRFlag = false;
        //  Effector.Play(5);
        EffectObj[12].SetActive(true);
        GameManager.playStageSE(18);
        PStatas = 5;
        ActionFlag = false;
        FloatFlag = true;
        animetor.SetInteger("PlayerStatas", PStatas);
        GetComponents<Collider>()[0].enabled = false;
        GetComponents<Collider>()[1].enabled = false;
        Director.ChangePalette(2);
        Rigid.mass = FloatMass;
        Invoke("FloatBallMode", 10.0f / 30.0f);
        FootSensor.transform.localScale = new Vector3(1, 4, 1);
    }

    /// <summary>
    /// ボール、フロートボール共通処理
    /// </summary>
    private void BallMode()
    {
        ActionFlag = true;
        GameManager.PlayerBallFlag = true;
        BallRFlag = true;
        Director.UseIcon(2);
    }
    private void FloatBallMode()
    {
        ActionFlag = true;
        BallRFlag = true;
        Director.UseIcon(2);
    }
    private void BallRelease()
    {
        Effector.Play(4);
        BallFalg = false;
        GameManager.PlayerBallFlag = false;
        if (FloatFlag)
        {
            FloatFlag = false;
         //   Effector.Stop(5);
            Rigid.mass = 1.0f;
            PlayerGravity = Gravity;
            FloatMode = false;
            if(GameDirector.Player3DMode) Rigid.constraints = RigidbodyConstraints.FreezeRotation;
            else Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
            EffectObj[12].SetActive(false);
            EffectObj[13].SetActive(false);
            FloatStatas = 0;
        }
        FootSensor.transform.localScale = new Vector3(15, 10, 8);
        GameManager.playStageSE(17);
        PStatas = 0;
        animetor.SetInteger("PlayerStatas", PStatas);

        GetComponents<Collider>()[0].enabled = true;
        GetComponents<Collider>()[1].enabled = true;
        Director.ChangePalette(1);
        animetor.speed = 1.0f;
    }
    
    /// <summary>
    /// ファイア魔法の処理
    /// </summary>
    private void Fire()
    {
        ActionFlag = false;
        CanMagic = true;
        EffectObj[10].SetActive(true);
        GameManager.playStageSE(27);
        Director.ChangePalette(3);
        GameDirector.EnemyTargetFlag = true;
        GameDirector.MagicType = 1;
        Director.NoUseIcon(2);
        Invoke("CanRelease", 1.0f);
    }
    private void PlayFire()
    {
        PlayMagic = false;
        Director.PlayerMP -= FireMpPoint;
        GameObject Fire = Instantiate(FireBall) as GameObject;
        float FireSpeed = 10.0f;
        Fire.transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
        if (GameDirector.isTarget())
        {
            if(GameDirector.GetTarget() != null)
            {
                GameObject ATarget = GameDirector.GetTarget();
                Vector3 FireVec = ATarget.transform.position - Fire.transform.position;
                FireVec = FireVec.normalized;
                Fire.GetComponent<Rigidbody>().velocity = FireVec * FireSpeed;
            }
        }
        else
        {
            Fire.GetComponent<Rigidbody>().velocity = new Vector3(PlayerDir.x * FireSpeed, 0, PlayerDir.y * FireSpeed);
        }
        Invoke("AMagic", 1.0f);

    }

    /// <summary>
    /// サンダー魔法の処理
    /// </summary>
    private void Thunder()
    {
        ActionFlag = false;
        CanMagic = true;
        EffectObj[11].SetActive(true);
        GameManager.playStageSE(27);
        Director.ChangePalette(4);
        GameDirector.EnemyTargetFlag = true;
        GameDirector.MagicType = 2;
        Director.NoUseIcon(2);
        Invoke("CanRelease", 1.0f);
    }
    private void PlayThunder()
    {
        PlayMagic = false;
        Director.PlayerMP -= ThunderMpPoint;
        float Dis = 4.0f;
        if (GameDirector.isTarget())
        {
            GameObject[] ATargets = GameDirector.GetTargets();
            for (int i = 0; i < ATargets.Length; i++)
            {
                GameObject Thunder = Instantiate(ThunderMagic, ATargets[i].transform.position, Quaternion.identity);
                Thunder.GetComponent<MagicController>().HomingStart(ATargets[i]);
            }
        }
        else
        {
            Instantiate(ThunderMagic, transform.position + Vector3.left * Dis, Quaternion.identity);
            Instantiate(ThunderMagic, transform.position + Vector3.right * Dis, Quaternion.identity);
            Instantiate(ThunderMagic, transform.position + Vector3.forward * Dis, Quaternion.identity);
            Instantiate(ThunderMagic, transform.position + Vector3.back * Dis, Quaternion.identity);
            Instantiate(ThunderMagic, transform.position, Quaternion.identity);
        }
        Invoke("AMagic", 2.0f);
    }

    /// <summary>
    /// 魔法詠唱解除処理
    /// </summary>
    private void MagicReleae()
    {
        ActionFlag = true;
        MagicRFlag = false;
        GameDirector.EnemyTargetFlag = false;
        Director.ChangePalette(1);
        GameDirector.RemoveAllTarget();

        if (GameDirector.MagicType == 1) EffectObj[10].SetActive(false); else if (GameDirector.MagicType == 2) EffectObj[11].SetActive(false);

        GameDirector.MagicType = 0;
        int[] an = Director.PaletteNumber;
        if (GameDirector.CanUseAction[an[1]]) Director.UseIcon(1); else Director.NoUseIcon(1);
        GameManager.playStageSE(17);
    }
    
    private void AMagic() { PlayMagic = true; }

    /// <summary>
    /// 突進処理
    /// </summary>
    private void Rush()
    {
        RushRDelta = 0;
        ActionFlag = false;
        RushRFlag = false;
        RushNoBrake = false;
        PStatas = 6;

        //方向計算
        if( Mathf.Abs(PlayerDir.x) >= Mathf.Abs(PlayerDir.y) || !GameDirector.Player3DMode)
        {
            if(PlayerDir.x >= 0)
            {
                RushDir = 1;
                RushVec = Vector3.right;
            }
            else
            {
                RushDir = 2;
                RushVec = Vector3.left;
            }
        }
        else
        {
            if(PlayerDir.y >= 0)
            {
                RushDir = 3;
                RushVec = Vector3.forward;
            }
            else
            {
                RushDir = 4;
                RushVec = Vector3.back;
            }
        }

        RushPos = transform.position;

        animetor.SetInteger("PlayerStatas", 6);
        animetor.SetTrigger("Rush");
        Director.ChangePalette(2);
        Director.NoUseIcon(2);
        RushNoDamageFlag = true;
        EffectObj[8].SetActive(true);
        GameManager.playStageSE(30);
    }

    /// <summary>
    /// 突進状態の解除
    /// </summary>
    private void RushRelease()
    {
        RushNoDamageFlag = false;
        animetor.SetTrigger("Rush_End");
        Director.ChangePalette(1);
        Invoke("RushEnd", RushFreezeTime);
        EffectObj[8].SetActive(false);
        EffectObj[9].SetActive(true);
        RushBrakeFlag = true;
        Invoke("stopRushBrake", RushBrakeTime);

        GameManager.stopLoopSE();
        GameManager.playStageSE(31);
    }

    private void stopRushBrake()
    {
        RushBrakeFlag = false;
    }

    private void RushEnd()
    {
        PStatas = 0;
        animetor.SetInteger("PlayerStatas", 0);
        ActionFlag = true;
        RushDirLock = false;
        EffectObj[9].SetActive(false);
        GameManager.stopLoopSE();
    }

    /// <summary>
    /// 突進状態時の処理
    /// </summary>
    private void RushActive()
    {
        if(PStatas == 6)
        {
            //解除を有効にする
            RushRDelta += 0.02f;
            if(RushRDelta >= RushRInterval && !RushRFlag)
            {
                RushRDelta = 0;
                CanRelease();
                RushDirLock = true;
                GameManager.playLoopSE(0);
            }

            //突進移動の処理
            if (RushNoDamageFlag)
            {
                bool StopFalg = false;
                if (RushRFlag)
                {
                    Vector3 Pos = transform.position;
                    if (RushDir == 1 && Pos.x - RushPos.x < RushStopMove) StopFalg = true;
                    if (RushDir == 2 && Pos.x - RushPos.x > -RushStopMove) StopFalg = true;
                    if (RushDir == 3 && Pos.z - RushPos.z < RushStopMove) StopFalg = true;
                    if (RushDir == 4 && Pos.z - RushPos.z > -RushStopMove) StopFalg = true;
                }
                if (StopFalg)
                {
                    RushNoBrake = true;
                    RushRelease();
                }
                else
                {
                    if (PlayerSpeed.magnitude < RushMaxSpeed)//スピード制限
                    {
                        //移動のための力を加える
                        Rigid.AddForce(RushVec * RushMoveForce);
                    }
                }
                RushPos = transform.position;
            }

            if(RushBrakeFlag && !RushNoBrake)
            {
                Rigid.AddForce(-RushVec * RushBrakeForce);
            }
        }
    }


    /// <summary>
    /// アクション共通使用関数
    /// </summary>
    //センサを有効にする
    private void SensorActive()
    {
        SensorLock = false;
    }

    //通常状態に戻るコマンドを有効にする
    private void CanRelease()
    {
        MagicRFlag = true;
        RushRFlag = true;
        Director.UseIcon(2);
    }

    //アクションパットのアクションを定義はここまで*******************************************************


    //複数のアクションに使用する関数
    //移動フラグを有効にする
    private void MovePossible() { ActionFlag = true; }

    //プレイヤーの動作をリセット
    private void StopBody()
    {
        Rigid.isKinematic = true;
        Invoke("NoStopBody", 0.05f);
    }
    private void NoStopBody() { Rigid.isKinematic = false; }


    //他スクリプトで使用する関数
    /*
    public void PlaySE(int Num)
    {
        SE.PlayOneShot(PlayerSE[Num]);
    }
    */
    public void PlayerEffect(int Num)
    {
        Effector.Play(Num);
    }
}
