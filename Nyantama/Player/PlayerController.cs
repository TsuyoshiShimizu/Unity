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
    [SerializeField] private SphereCollider LegCollider = null;
    [SerializeField] private BoxCollider SensorCollider = null;
    [SerializeField] private GameObject[] ComboAttack = null;
    // private float PlayerZP = 0f;                    /
    public float Pzp { set; get; } //プレイヤーのZ位置の管理　2Dモード時Zの位置ずれ修正用に使用

    static public Vector3 SavePoint = Vector3.zero; //プレイヤーのセーブポイント

    private GameObject StartObjs;
    //  private List<int> StartN = new List<int>();

    //プレイヤーのコンポーネント
    private Rigidbody Rigid;                //プレイヤーの物理ボディ
    private Animator animetor;              //プレイヤーのアニメータ
    private CapsuleCollider[] PCollider;

    //  private Transform Parenttransform;

    //プレイヤーの制御に使用
    //  private Vector3 PlayerP;                //プレイヤーの一コマ前の地点
    public static Vector3 PlayerPos;        //プレイヤーのワールド座標

    private int PStatas = 0;                //プレイヤーの状態（主にアニメの切り替えに使用）0:通常 1:歩き 2:走り 4:空中 5:ボール 6:突進
    private bool ActionFlag = true;         //移動系のアクションが使用できるか
    private bool CanMagic = true;           //詠唱時の魔法を使用できるか?（魔法の連射制限に使用）
    private bool MagicRFlag = false;        //詠唱状態が解除できるか

    private Vector2 PlayerSpeed = new Vector2(0, 0);    //プレイヤーのスピード
    private Vector2 JoyVec = new Vector2(0, 0);         //プレイヤーの移動用ジョイスティックの傾き
                                                        //  private Vector2 PlayerDir = new Vector2(0, 0);      //プレイヤーの方向ベクトル
    private bool[] MoveKeyFlag = new bool[4] { false, false, false, false };
    public bool BallChineFalg { set; get; }

    private bool isMoveKey = false;

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

    private bool MoveStcickFlag = false;

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

    public static float ComboTime = 0;
   // public static float FinishComboTime = 0;
  //  public static float RollingComboTime = 0;
    private bool ComboFlag = false;
   // private bool FinishComboFlag = false;
  //  private bool RollingComboFlag = false;

    private int ComboStackNum = 0;
    private bool ComboStackFlag = false;
    private bool ComboAttackFlag = false;
   // public static bool PanishmentFlag = false;
    private bool ChargeAttackFlag = false;
    static public int ComboCount = 0;
    static public bool[] ComboPlusFlag = new bool[] { true, true, true, true, true, true, true };
    private bool FinishComboFlag = false;

    private bool ComboIconChangeFlag = false;

    private float SpinTimer = 0;

    private int JumpAttackAnimetionCount = 0;

    private bool tutorialFlag = false;
    private int tutorialType = 0;
    private int tutorialStage = 0;
    private Animator movePadAnimator;
    private Animator actionPadAnimator;

    private bool tutorialMoveLock = false;          //チュートリアル時に移動をロックするか
    private bool tutorialActionLock = false;        //チュートリアル時にアクションをロックするか
    private bool tutorialMultiLock = false;         //チュートリアル時の時間停止を移動、アクションの両方実行した時に解除するか
    private bool tutorialMultiMoveLock = false;     //チュートリアル時の時間停止マルチロックの移動スティックの条件を満たしているか
    private enum MoveEnable { none, up, down, right, left ,enable }
    private enum ActionEnable { none, tap, up, down, right, left ,holdup, holddown, holdright, holdleft }
    private MoveEnable tutorialMoveLockStatas = MoveEnable.none;            //移動ロックの種類
    private ActionEnable tutorialActionLockStatas = ActionEnable.none;      //アクションロックの種類
    private bool tutorialAnimeFlag = false;                                 //チュートリアルアニメを表示しているか
    private bool tutorialStoping = false;                                   //チュートリアルでの停止を行っているか
    private bool tutorialStopFlag = false;                                  //チュートリアルでの停止を行うか
    private bool tutorialNextStage = false;                                 //チュートリアルでアクションを使用した時に次のステージに進むか
    private bool tutorialMoveStopRelease = false;                           //チュートリアルで有効な移動スティック状態を行った時に停止フラグを解消するか
    private int tutorialTargetCount = 0;

    /// <summary>
    /// カメラの切り替え関数
    /// </summary>
    public void Change3DMode()
    {
        if (CameraChangeEnable && GameManager.StageNum != 1000)
        {
           // Debug.Log("3Dモードに切り替え");
            GameDirector.Player3DMode = true;
            Change3DRigid();
            StageCamera.Camera3DMode();
        }
    }
    public void Change2DMode()
    {
        if (CameraChangeEnable && GameManager.StageNum != 1000)
        {
           // Debug.Log("2Dモードに切り替え");
            GameDirector.Player3DMode = false;
            Change2DRigid();
            transform.position = new Vector3(transform.position.x, transform.position.y, Pzp);
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

    private void Change2DRigid() => Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
    private void Change3DRigid() => Rigid.constraints = RigidbodyConstraints.FreezeRotation;

    //プレイヤーの走り状態継続時間(移動キー入力を止めた時の余韻)
    private bool NoRunFlag = true;
    private float NoRunDelta = 0;
    private float NoRunInterbal = 0.7f;

    //プレイヤーのダメージ処理に使用
    static public bool PlayerDamgagFlag = true;
    private float damageDelta = 0;
    static public bool AvoidFlag = false;
    private float AvoidDelta = 0;

    private GameObject Enhance;
   
    //センサのONOFF;
    private bool sensorLock = false;                    
    public bool SensorLock
    {
        private set
        {
            sensorLock = value;
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
                if (AirActionFlag)
                {
                    AirActionFlag = false;
                   // ResetCombo();
                }
                AirDelta = 0;
                if(FloatMode && !FloatMoveLock)
                {
                    FloatMode = false;
                    GameManager.playStageSE(39);
                    if (!GameDirector.Player3DMode) Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
                    EffectObj[7].SetActive(false);
                    PlayerGravity = Gravity;
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
    private float MaxSpped = 7.0f;              //移動の最大スピード
    private float MaxSpeedScale = 1.0f;         //移動スピードの掛け率
    private float MaxWalkSpped = 1.5f;          //走るモーションへの切り替えスピード
    private float MiniWalkSpped = 0.01f;        //歩くモーションへの切り替えスピード
    private float walkForce = 20.0f;            //移動に使用する力
    private float walkForceScale = 1.0f;
    private float AirForce = 6.0f;              //空中の移動で使用する力
    private float Gravity = 9.8f;               //デフォルトの重力加速度
    private Vector3 GravityVec = Vector3.down;  //重力方向

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
  //  private float highjumpWaitTime = 1.00f;     //ハイジャンプの力を加えるまでのラグ
    private float highjumpXZForce = 100.0f;     //ハイジャンプに使用する横方向の力

    //空中でのスピード上限
    private float MaxAirSpeed = 6.0f;

    //攻撃設定
    //猫パンチ
    private float A1RigidTime = 1.2f;       //使用開始から次の行動までの硬直時間
    //スピンアタック
    private float A2RigidTime = 2.0f;       //リキャスト時間
   // private bool Attack2Flag = true;        //攻撃が使用可能か
    //空中攻撃
    private bool AirActionFlag = false;

    //ローリング
    private float RollingForce = 380.0f;    //ローリングの力(通常時)
    private float Rolling2Force = 250.0f;   //ローリングの力(走っている時)
    private float RollinWaitTime = 0.2f;    //次の行動が可能か
    private float AvoidTime = 0.73f;
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
        PlayerPos = GetComponent<Transform>().position;                 //プレイヤーのワールド座標
        animetor = GetComponent<Animator>();                            //プレイヤーのアニメータ
        render = GetComponentInChildren<SkinnedMeshRenderer>();         //プレイヤーのメッシュ
        Effector = GetComponent<Effekseer.EffekseerEmitter>();          //プレイヤーのエフェクタ
        ChineStatas = GetComponent<Cinemachine.CinemachineDollyCart>(); //プレイヤーのシネマチェイン
        PCollider = GetComponents<CapsuleCollider>();
        Chine = GetComponent<Cinemachine.CinemachineDollyCart>();

        //  Parenttransform = transform.parent;

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
        Pzp = transform.position.z;
        Rigid.useGravity = false;
        PlayerGravity = Gravity;
        BallChineFalg = false;
        BallFalg = false;
        ComboTime = 0;
       // FinishComboTime = 0;
        ComboCount = 0;
        ComboPlusFlag = new bool[] { true, true, true, true, true, true, true };

        GameManager.PlayerBallFlag = false;

        KingSetUp();            //キング状態の設定
        
    }

    //キー入力などの処理を記述（毎フレーム処理）**************************************
    void Update()
    {
        WarpMove(Time.deltaTime);
        ComboTimeCount(Time.deltaTime);
        if (SpinTimer > 0) SpinTimer -= Time.deltaTime;

        bool isMKey = false;
        
        if (Input.GetKeyDown(KeyCode.S)) MoveKeyFlag[0] = true;
        if (Input.GetKeyDown(KeyCode.Z)) MoveKeyFlag[1] = true; 
        if (Input.GetKeyDown(KeyCode.X)) MoveKeyFlag[2] = true;
        if (Input.GetKeyDown(KeyCode.C)) MoveKeyFlag[3] = true;
        if (Input.GetKey(KeyCode.S)) isMKey = true;
        if (Input.GetKey(KeyCode.Z)) isMKey = true;
        if (Input.GetKey(KeyCode.X)) isMKey = true;
        if (Input.GetKey(KeyCode.C)) isMKey = true;
        if (Movejoystick.isMoveStick) isMKey = true;
        if (Input.GetKeyUp(KeyCode.S)) MoveKeyFlag[0] = false;
        if (Input.GetKeyUp(KeyCode.Z)) MoveKeyFlag[1] = false;
        if (Input.GetKeyUp(KeyCode.X)) MoveKeyFlag[2] = false;
        if (Input.GetKeyUp(KeyCode.C)) MoveKeyFlag[3] = false;

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


        MoveStcickFlag = isMKey;
        bool isTouch = isMKey;
        if (Actionjoystick.isActionStick) isTouch = true;
        tutorialShowChange(isTouch);
        tutorialMoveStopChange();
        tutorialTarget();

        if (isMoveKey != isMKey)
        {
            isMoveKey = isMKey;
            if (!GameManager.PlayerKingBuffer) return;
            if (RushNoDamageFlag) return;
            if (isMoveKey) NormalMat(); else StopMat();
        }
    }

    //メイン処理*******************************************************************
    private void FixedUpdate()
    {
        //プレイヤーの状態を管理（スピードや向き）*******************************************
        //プレイヤーの速度
        PlayerSpeed.x = Rigid.velocity.x;
        PlayerSpeed.y = Rigid.velocity.z;
        float speed = PlayerSpeed.magnitude;

        //重力の計算
        Rigid.AddForce(PlayerGravity * GravityVec, ForceMode.Acceleration);

        /*
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
        */

        PlayerDirectionUpdate(); //プレイヤーの方向を計算

        //プレイヤーのダメージフラグ
        if (!PlayerDamgagFlag)
        {
            damageDelta += 0.02f;
            DameDelta += 0.02f;
            //点滅処理
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
                if (ComboAttackFlag)
                {
                    ComboAttackFlag = false;
                    ComboAttack[3].SetActive(false);
                }
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
            if (!MoveStcickFlag)
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
            EffectObj[7].SetActive(false);
            FloatStatas = 0;
        }
     
        //フロート限界点を超えた時の処理
        if (FloatStatas == 5)
        {
            if ( (-5 < Rigid.velocity.z && Rigid.velocity.z < 5) 
                || (transform.position.z > PlayerFloatLimitP && ZFront) 
                || (transform.position.z < PlayerFloatLimitP && !ZFront))
            {
                Debug.Log("Float_limit");
                FloatStatas = 0;
                ResetFloat();
            }
        }

        float PlayerMoveSpeed = 0;
        float SpeedValue = new Vector2(Rigid.velocity.x, Rigid.velocity.z).magnitude;
        if (!NoRunFlag && OnGround)
        {
            PlayerMoveSpeed = SpeedValue;
        }

        //座りモーションに変更
        if (PStatas == 0 && ActionFlag && !MoveStcickFlag)
        {
            Normaldelta += 0.02f;
           // Debug.Log("Sit_Add");
        }
        else
        {
            Normaldelta = 0;
            SitFlag = false;
        }

        if (Normaldelta >= SitTime && SitFlag == false)
        {
            SitFlag = true;
            Normaldelta = 0;
           // Debug.Log("Sit_Change");
        }
        if(SitFlag) PlayerMoveSpeed = animetor.GetFloat("speed") - 0.1f;

        animetor.SetFloat("speed", PlayerMoveSpeed);
        if (!MoveStcickFlag)
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
            NoRunFlag = false;
        }


        //着地処理
        if (PStatas == 4 && OnGround && SensorLock == false)
        {
            PStatas = 0;
            animetor.SetTrigger("Standard");
        }

        //プレイヤーの移動に関する処理  
     //   inputNormalMove();          //通常時およびボール時の移動の入力
    //    inputFloatBallMove();       //フロートボール時の移動の入力
     //   inputAirMove();             //空中の移動の入力

        inputMoveForce();
        inputMagnetBallMove();

        //プレイヤー地点の更新
        PlayerPos = transform.position;

        //突進の処理
        RushActive();               //突進アクティブ時の処理

        MagnetRelease();            //マグネット状態時の解除を管理
        MagnetChangeRelease();      //マグネットの切り替えを管理

        //アクションパッドの処理
        chackCanUsePadAction();     //パッドアクションが使用できるか
        EnableActionPad();          //パッドアクション使用中に実行
       // inputChrage();              //チャージアクションの入力
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
        Debug.Log("フロートダメージ");
        Director.PlayerHP -= GameManager.PlayerMaxHp / 2;
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


    
    /// <summary>
    /// プレイヤーの開始地点の変更処理
    /// </summary>
    private  void FirstPosition()
    {
        if(GameManager.FieldNum != 0 && GameManager.StageNum == 0)
        {
            if(GameManager.FieldPoint == Vector3.zero)
            {
                transform.position = StartObjs.transform.position;
                transform.eulerAngles = StartObjs.transform.eulerAngles + Vector3.down * 90;
            }
            else
            {
                transform.position = GameManager.FieldPoint;
                transform.rotation = GameManager.FieldRot;
            }
        }
        else
        {
            if (SavePoint == Vector3.zero && StartObjs != null)
            {
                transform.position = StartObjs.transform.position;
                transform.eulerAngles = StartObjs.transform.eulerAngles + Vector3.down * 90;
            }
            else
            {
                transform.position = SavePoint;
                transform.rotation = Quaternion.LookRotation(Vector3.right);
            }
        }
        GameDirector.RebornPos = transform.position;
    }

    //プレイヤーの衝突判定

    //物理衝突あり(isTrigger = false)
    /// <summary>
    /// 衝突開始処理
    /// </summary>
    /// <param name="other"></param>
    private void OnCollisionEnter(Collision other)
    {
        ContactRushBlock(other.gameObject);       //ラッシュブロックに接触時
    }

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

    //物理衝突なし(isTrigger = true)
    /// <summary>
    /// 接触開始の処理
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
                float PDef = GameManager.PlayerDef * GameManager.BufferDef;
                int Damage = EPow * EPow / (int)PDef;
                if (Damage == 0) Damage = 1;
                if (EPow == 0) Damage = 0;
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

                    //フロートブロックの登りはじめを検知
                    if (num == 1)
                    {
                        Debug.Log("フロートアクション段階_1");
                        FloatStatas = 1;
                        FloatDelta = 0;
                        FloatResetPos = other.gameObject.transform.position;
                    }

                    //フロートブロックの登り終わりを検知
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
                        EffectObj[7].SetActive(true);
                    }

                    //上にあるノーマルフロートブロックの通過開始を検知
                    if (num == 3 && FloatStatas == 2)
                    {
                        Debug.Log("フロートアクション段階_3");
                        FloatStatas = 3;
                        FloatMoveLock = true;
                        Invoke("FloatMoveR3", 1.0f);
                    }

                    //上にあるノーマルフロートブロックの通過終了を検知
                    if (num == 8 && FloatStatas == 3)
                    {
                        Debug.Log("フロートアクション段階_8");
                        FloatStatas = 8;
                        Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
                        Invoke("FloatMoveR8", 1.0f);
                    }

                    //上にあるｚ方向移動フロートブロックの通過開始を検知
                    if (num == 4 && FloatStatas == 2)
                    {
                        Debug.Log("フロートアクション段階_4");
                        FloatRLock = true;
                        Director.NoUseIcon(2);
                        FloatStatas = 4;
                        Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX;
                        Invoke("FloatMoveR4", 1.0f);
                    }

                    //上にあるｚ方向移動フロートブロックの通過終わりを検知
                    if (num == 5 && FloatStatas == 4)
                    {
                        Debug.Log("フロートアクション段階_5");
                        ZFront = sensor.ZFrontFlagGet();
                        PlayerFloatLimitP = sensor.LimitPoint;
                        FloatStatas = 5;
                        Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
                        if (!GameDirector.Player3DMode) Change3DCamera();
                    }

                    //z方向移動後のフロートブロック通過開始を検知
                    if (num == 6 && FloatStatas == 5)
                    {
                        Debug.Log("フロートアクション段階_6");
                        FloatStatas = 6;
                        Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX;
                        Invoke("FloatMoveR6", 2.0f);
                    }

                    //z方向移動後のフロートブロック通過終わりを検知
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
                        transform.position = new Vector3(transform.position.x, transform.position.y, Pzp);
                        FloatRLock = false;
                        Director.UseIcon(2);
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

        ContactWarp(other.gameObject);
    }

    /// <summary>
    /// 接触中の処理
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay(Collider other)
    {
        GameObject CObj = other.gameObject;
        ContactWind(CObj);
        ContactMagnet(CObj);
        ContactTutorialBlock(CObj);
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
    private void inputMoveForce()
    {
        if (tutorialMoveLock)
        {
            if (tutorialMoveLockStatas == MoveEnable.up && JoyVec.y > 0.5f)
            {
                JoyVec.x = 0;
                JoyVec.y = 1;
            }
            else if (tutorialMoveLockStatas == MoveEnable.down && JoyVec.y < -0.5f)
            {
                JoyVec.x = 0;
                JoyVec.y = -1;
            }
            else if (tutorialMoveLockStatas == MoveEnable.right && JoyVec.x > 0.5f)
            {
                JoyVec.y = 0;
                JoyVec.x = 1;
            }
            else if (tutorialMoveLockStatas == MoveEnable.left && JoyVec.x < -0.5f)
            {
                JoyVec.y = 0;
                JoyVec.x = -1;
            }
            else return;
            }
                inputNormalMove();
                inputAirMove();
                inputFloatBallMove();
            }


    /// <summary>
    /// 通常およびボールモード時の地上での移動処理
    /// </summary>
    private void inputNormalMove()
    {
        if (MagnetFlag) return;
        if (JoyVec == Vector2.zero) return; //移動キーを入力していない時
        if (FloatFlag) return;              //フロート状態の時
        if (!ActionFlag) return;
        if (OnGround || (BallFalg && !BallChineFalg))
        {

            float GroundX = JoyVec.x;
            float GroundY = JoyVec.y;
            
            if ((PlayerSpeed.x > MaxSpped * MaxSpeedScale && JoyVec.x > 0) || (PlayerSpeed.x < -MaxSpped * MaxSpeedScale && JoyVec.x < 0)) GroundX = 0;
            if ((PlayerSpeed.y > MaxSpped * MaxSpeedScale && JoyVec.y > 0) || (PlayerSpeed.y < -MaxSpped * MaxSpeedScale && JoyVec.y < 0)) GroundY = 0;

            if (GameDirector.Player3DMode) Rigid.AddForce(new Vector3(GroundX * walkForce * walkForceScale, 0, GroundY * walkForce * walkForceScale));
            else Rigid.AddForce(new Vector3(GroundX * walkForce * walkForceScale, 0, 0));
        }
    }
    /// <summary>
    /// フロートボール時の移動処理
    /// </summary>
    private void inputFloatBallMove()
    {
        if (MagnetFlag) return;
        if (JoyVec == Vector2.zero) return; //移動キーを入力していない時
        if (!FloatFlag) return;             //フロートモードではない時
        if (FloatMoveLock) return;

        float FloatX = JoyVec.x;
        float FloatY = JoyVec.y;
        
        if ((PlayerSpeed.x > FloatMaxSpped && JoyVec.x > 0) || (PlayerSpeed.x < -FloatMaxSpped && JoyVec.x < 0)) FloatX = 0;
        if ((PlayerSpeed.y > FloatMaxSpped && JoyVec.y > 0) || (PlayerSpeed.y < -FloatMaxSpped && JoyVec.y < 0)) FloatY = 0;

        if (GameDirector.Player3DMode) Rigid.AddForce(new Vector3(FloatX * FloatForce, 0, FloatY * FloatForce));
        else Rigid.AddForce(new Vector3(FloatX * FloatForce, 0, 0));
    }
    
    //マグネットボールの移動を定義
    private void inputMagnetBallMove()
    {
        if (!MagnetFlag) return;
        if (JoyVec == Vector2.zero) return;

        Vector3 PSpeed = Rigid.velocity;
        Vector3 RightVec = new Vector3(-MagnetVec.y, MagnetVec.x, 0).normalized;
        Vector3 ForwardVec = new Vector3(0, MagnetVec.z, -MagnetVec.y).normalized;
        Vector3 MagVec = JoyVec.x * RightVec + JoyVec.y * ForwardVec;

        if(PSpeed.magnitude > MagnetMaxSpeed)
        {
            if ( (PSpeed.x > 0 && MagVec.x > 0) || (PSpeed.x < 0 && MagVec.x < 0) ) MagVec.x = 0;
            if ( (PSpeed.y > 0 && MagVec.y > 0) || (PSpeed.y < 0 && MagVec.y < 0) ) MagVec.y = 0;
            if ( (PSpeed.z > 0 && MagVec.z > 0) || (PSpeed.z < 0 && MagVec.z < 0) ) MagVec.z = 0;
        }

        if (GameDirector.Player3DMode) Rigid.AddForce(MagVec * MagnetForce);
        else Rigid.AddForce(new Vector3(MagVec.x * MagnetForce, MagVec.y * MagnetForce, 0));
    }
  
    /// <summary>
    /// 空中の移動
    /// </summary>
    private void inputAirMove()
    {
        if (JoyVec == Vector2.zero) return;
        if (!ActionFlag) return;
        if (FloatFlag) return;
        if (BallFalg) return;
        if (OnGround) return;

        float AirX = JoyVec.x;
        float AirY = JoyVec.y;

        if (PlayerSpeed.magnitude > MaxAirSpeed)
        {
            if ((PlayerSpeed.x > 0 && AirX > 0) || (PlayerSpeed.x < 0 && AirX < 0)) AirX = 0;
            if ((PlayerSpeed.y > 0 && AirY > 0) || (PlayerSpeed.y < 0 && AirY < 0)) AirY = 0;
        }

        if (GameDirector.Player3DMode) Rigid.AddForce(new Vector3(AirX * AirForce, 0, AirY * AirForce));
        else Rigid.AddForce(new Vector3(AirX * AirForce, 0, 0));

      //  Debug.Log("空中移動");
    }

    /// <summary>
    /// プレイヤーの方向を計算する
    /// </summary>
    /// <param name="KeyPush"></param>
    private void PlayerDirectionUpdate()
    {
        //プレイヤーの方向を計算
        if (PStatas == 6 && RushDirLock) return; //ラッシュ中に向き変更を無効化する場合
        Direction2DUpdate();
        Direction2DUpdate2();
        Direction3D();
    }

    //2次元変位量で向きを2次元的に変化させる時
    private void Direction2DUpdate()
    {
        Vector2 diff2 = new Vector2(transform.position.x, transform.position.z) - new Vector2(PlayerPos.x, PlayerPos.z);

        if (BallFalg) return;                   //ボール状態時
        if (FloatFlag) return;                  //フロートボール状態
        if (!OnGround) return;                  //地上にいない時
        if (PStatas == 6) return;
        
        if (diff2.magnitude < 0.02f) return;                //変異量が0.02より小さい時
        if (!MoveStcickFlag) return;  //移動キーを使用していない時

        transform.rotation = Quaternion.LookRotation(new Vector3(diff2.x, 0, diff2.y));
    }
    //3次元変位量で向きを2次元的に変化させる時
    private void Direction2DUpdate2()
    {
        if(BallFalg || FloatFlag || (PStatas == 6 && !RushNoDamageFlag && !RushDirLock) )
        {
            Vector3 diff3 = transform.position - PlayerPos;
            if (diff3.magnitude <= 0.02f) return;               //移動変位量が0の時
            transform.rotation = Quaternion.LookRotation(new Vector3(diff3.x, 0, diff3.z));
        }
    }
    //3次元変位量で向きを3次元的に変化させる時
    private void Direction3D()
    {
        if (PStatas != 6) return;                           //突進状態以外の時
        if (!RushNoDamageFlag) return;                      //突進時無敵状態じゃない時

        Vector3 diff3 = transform.position - PlayerPos;
        if (diff3.magnitude <= 0.02f) return;               //移動変位量が0の時
        transform.rotation = Quaternion.LookRotation(diff3, -GravityVec);
    }


    //移動パッドに関する記述***********************************************************

    /// <summary>
    /// 移動キーを入力しているかの判定
    /// </summary>
    /// <returns></returns>
    private bool getMoveKeyEnable()
    {
        bool MoveKey = false;
        for (int i = 0; i < 4; i++) { if (MoveKeyFlag[i]) MoveKey = true; }
        if (Movejoystick.isMoveStick) MoveKey = true;
        return MoveKey;
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
                if (chrageActionNum == PANum)
                {
                    chrageActionFlag = true;
                    inputChrage();
                }
                else
                {
                    resetCharge();
                }
                chrageActionNum = PANum;
            }
            else
            {
                resetCharge();
            }
        }
    }

    /// <summary>
    /// アクションのチャージの入力
    /// </summary>
    private void inputChrage()
    {
        if (WarpLock) return;
        if (chrageActionFlag)
        {
            chrageActionDelta += 0.02f;
            if(chrageActionDelta >= chrageActionTime / 5 && chrageActionStatas == 0)
            {
                chrageEffectNum = chrageActionNum;
                chrageActionStatas = 1;
                EffectObj[0].GetComponent<EffectColorChange>().ChangeColor(chrageEffectNum);
                EffectObj[1].GetComponent<EffectColorChange>().ChangeColor(chrageEffectNum);
                EffectObj[0].SetActive(true);

                //if (chrageActionNum >= 1) chrageEffectNum = chrageActionNum;
                // EffectObj[chrageEffectNum - 1].SetActive(true);
            }
            if (chrageActionDelta >= chrageActionTime && chrageActionStatas == 1)
            {
                chrageActionStatas = 2;
                EffectObj[0].SetActive(false);
                EffectObj[1].SetActive(true);
                if (chrageActionNum == 1) Director.PaletteNumber = new int[5] { 0, 3, 4, 2, 6 };
                if (chrageActionNum == 2) Director.PaletteNumber = new int[5] { 0, 1, 5, 2, 6 };
                if (chrageActionNum == 3) Director.PaletteNumber = new int[5] { 0, 1, 4, 8, 6 };

                if (chrageActionNum == 4) Director.PaletteNumber = new int[5] { 0, 1, 4, 2, 7 };
                GameManager.playStageSE(20);
            }
            if (getMoveKeyEnable() && chrageActionStatas != 2) resetCharge();
        }
    }

    /// <summary>
    /// アクションのチャージ状況をリセットする
    /// </summary>
    private void resetCharge()
    {
        if (!chrageActionFlag) return;
        chrageActionFlag = false;
        chrageActionDelta = 0;
        EffectObj[0].SetActive(false);
        EffectObj[1].SetActive(false);
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
        PlayActionPadAction(An);
        if (An <= 8) resetCharge();
    }

    private void PlayActionPadAction(int num)
    {
        if (tutorialActionLock)
        {
            if (tutorialActionLockStatas != ActionEnable.tap && num == 0) return;
            if (tutorialActionLockStatas != ActionEnable.up && (num == 1 || num == 10)) return;
            if (tutorialActionLockStatas != ActionEnable.down && (num == 4 || num == 9)) return;
            if (tutorialActionLockStatas != ActionEnable.right && num == 2) return;
            if (tutorialActionLockStatas != ActionEnable.left && num == 6) return;
            if (tutorialActionLockStatas != ActionEnable.holdup && num == 3) return;
            if (tutorialActionLockStatas != ActionEnable.holddown && num == 5) return;
            if (tutorialActionLockStatas != ActionEnable.holdright && num == 8) return;
            if (tutorialActionLockStatas != ActionEnable.holdleft && num == 7) return;
            
            if (tutorialStopFlag && tutorialMultiLock && !tutorialMultiMoveLock) return;
            else if (tutorialStopFlag && ( (tutorialMultiLock && tutorialMultiMoveLock) || !tutorialMultiLock ) ) 
            {
                Time.timeScale = 1;
                tutorialMultiLock = false;
                tutorialStoping = false;
                tutorialStopFlag = false;
                Debug.Log("Action_Stop_R");
            }
           // else if ()
          
        }

        //コンボによる発動技の変更
        if(num == 6)
        {
            if (ComboCount >= 3) num = 7;
        }

        if (num == 0)                 //攻撃
        {
            if (GameDirector.CanUseAction[0] && !GameDirector.ActionLockFlag[0])
            {
                if (PStatas == 0 && chrageActionStatas == 2) ChargeAttackFlag = true;
                if (PStatas == 0 && !MoveStcickFlag) NormalAttack();     //通常攻撃を使用
                if (PStatas == 0 && MoveStcickFlag) SpinAttack();       //スピン攻撃を使用
                if (AirActionFlag) AirSpinAttack();
            }
        }
        else if (num == 1)            //ジャンプ
        {
            if (GameDirector.CanUseAction[1] && !GameDirector.ActionLockFlag[1]) Jump();             //ジャンプを使用
        }
        else if (num == 2)            //ローリング
        {
            if (GameDirector.CanUseAction[2] && !GameDirector.ActionLockFlag[3]) Rolling();
        }
        else if (num == 3)            //ハイジャンプ
        {
            if (GameDirector.CanUseAction[3] && !GameDirector.ActionLockFlag[5]) HighJump();
        }
        else if (num == 4)            //ボール
        {
            if (GameDirector.CanUseAction[4] && !GameDirector.ActionLockFlag[2]) Ball();
        }
        else if (num == 5)            //フロート
        {
            if (GameDirector.CanUseAction[5] && !GameDirector.ActionLockFlag[6]) FloatBall();
        }
        else if (num == 6)            //ファイア
        {
            if (GameDirector.CanUseAction[6] && !GameDirector.ActionLockFlag[4]) Fire();
        }
        else if (num == 7)            //サンダー
        {
            if (GameDirector.CanUseAction[7] && !GameDirector.ActionLockFlag[8]) Thunder();
        }
        else if (num == 8)           //突進
        {
            if (GameDirector.CanUseAction[8] && !GameDirector.ActionLockFlag[7]) Rush();
        }
        else if (num == 9)          //アクション解除
        {
            if (PStatas == 5 && !BallChineFalg && !FloatMoveLock && BallRFlag && !FloatRLock) BallRelease();
            else if (GameDirector.MagicType >= 1 && MagicRFlag) MagicReleae();
            else if (PStatas == 6 && RushRFlag) RushRelease();
        }
        else if (num == 10)      //特殊アクションを実行
        {
            if (GameDirector.MagicType == 1 && Director.PlayerMP >= FireMpPoint && PlayMagic) PlayFire();
            else if (GameDirector.MagicType == 2 && Director.PlayerMP >= ThunderMpPoint && PlayMagic) PlayThunder();
        }

        if(tutorialNextStage && tutorialActionLock)
        {
            tutorialNextStage = false;
            tutorialStage += 1;
            tutorialPlay();
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
            if (GameDirector.CanUseAction[0])//使用不可条件
            {
                if (((OnGround == false || PStatas == 4) && AirActionFlag == false) || ActionFlag == false || (SpinTimer > 0 && PStatas == 0 && MoveStcickFlag))
                {
                    GameDirector.CanUseAction[0] = false;
                    ActionIconStatasChack(0);
                }
            }
            else//使用可能条件
            {
                if (ActionFlag && OnGround && PStatas == 0 && ( !MoveStcickFlag || ( MoveStcickFlag && SpinTimer <= 0 ) ) || AirActionFlag)
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
        Normaldelta = 0;
        ActionFlag = false;
        SitFlag = false;
        ComboPlusFlag = new bool[] { true, true, true, true, true, true, true };
        if (ComboCount >= 3)
        {
            ResetCombo();
            ChargeAttackFlag = false;
            FinishComboFlag = true;
        }
        else if (ComboCount <= 2) //コンボ技、チャージ技の使用
        {
            if (ComboFlag || GameManager.PlayerKingBuffer || ChargeAttackFlag)
            {
                ResetCombo();
                ChargeAttackFlag = false;
                ComboAttackFlag = true;
            }
        }
        ComboCount = 0;
        animetor.SetTrigger("Attack01");
    }
    public void AttackEffect1()
    {
        float XDis = 0.6f;
        float YDis = 0.8f;
        Vector3 AttackPos = transform.position + transform.forward * XDis + transform.up * YDis;
        if (FinishComboFlag)
        {
            Instantiate(ComboAttack[8], AttackPos, transform.rotation);
            GameManager.playStageSE(43);
        }
        else if (ComboAttackFlag)
        {
            Instantiate(ComboAttack[0], AttackPos, transform.rotation);
            GameManager.playStageSE(43);
        }
        else
        {
            Instantiate(Attack1, AttackPos, transform.rotation);
            GameManager.playStageSE(16);
        }  
    }

    public void NormalAttackEnd()
    {
        animetor.speed = 1;
        ActionFlag = true;
        ComboAttackFlag = false;
        FinishComboFlag = false;
        if (ComboStackFlag)
        {
            ComboStackFlag = false;
            if (ComboStackNum == 0 && !GameDirector.ActionLockFlag[0]) NormalAttack();
            if (ComboStackNum == 1 && !GameDirector.ActionLockFlag[1]) Jump();
            if (ComboStackNum == 2 && !GameDirector.ActionLockFlag[3]) Rolling();
            if (ComboStackNum == 4 && !GameDirector.ActionLockFlag[2]) Ball();
            if (ComboStackNum == 6 && !GameDirector.ActionLockFlag[4]) Fire();
        }
    }
    //ダッシュ攻撃
    private void SpinAttack()
    {
        //Attack2Flag = false;
        ActionFlag = false;
        animetor.SetTrigger("Attack02");
        SpinTimer = A2RigidTime;
       // Invoke("Attack2Possible", A2RigidTime);
        if (ComboCount >= 3)
        {
            ResetCombo();
            ChargeAttackFlag = false;
            ComboCount = 0;
            Attack2.SetActive(true);
            GameManager.playStageSE(33);
            GameManager.playStageSE(51);
            Instantiate(ComboAttack[9], transform.position, transform.rotation);
        }
        else if ((ComboFlag && ComboPlusFlag[1]) || GameManager.PlayerKingBuffer || ChargeAttackFlag)
        {
            ComboAttackFlag = true;
            ChargeAttackFlag = false;
            ResetCombo();
            GameManager.playStageSE(47);
            ComboAttack[6].SetActive(true);
        }
        else
        {
            ComboPlusFlag = new bool[] { true, true, true, true, true, true, true };
            ComboCount = 0;
            Attack2.SetActive(true);
            GameManager.playStageSE(33);
        }
    }
    /*
    private void Attack2Possible()
    {
        Attack2Flag = true;
    }
    */

    public void SpinAttackEnd()
    {
        MovePossible();
        if (ComboAttackFlag)
        {
            ComboAttack[6].SetActive(false);
            ComboAttackFlag = false;
            if (ComboCount >= 3) SpinTimer = 0;
        }
        else
        {
            if (ComboFlag) SpinTimer = 0;
            Attack2.SetActive(false);
        }
    }

    //空中攻撃
    private void AirSpinAttack()
    {
        AirActionFlag = false;
        
        ActionFlag = false;
        animetor.SetTrigger("Attack03");
        
        if (ComboCount >= 3)
        {
            ComboCount = 0;
            ResetCombo();
            GameObject Panishment =  Instantiate(ComboAttack[5], transform.position, transform.rotation);
            Panishment.GetComponent<Panishment>().ChangeVelocity(transform.forward);
        }
        else
        {//コンボ技を
            if ((ComboFlag && ComboPlusFlag[3]) || GameManager.PlayerKingBuffer)
            {
                ComboAttackFlag = true;;
                ResetCombo();
                GameManager.playStageSE(47);
                ComboAttack[7].SetActive(true);
            }
            else
            {
                ComboPlusFlag = new bool[] { true, true, true, true, true, true, true };
                ComboCount = 0;
                GameManager.playStageSE(33);
                Attack3.SetActive(true);
            }
        }
    }
    private void canAirAttack() { if (!OnGround) AirActionFlag = true; }

    public void AirSpinAttackEnd()
    {
        MovePossible();
        if (ComboAttackFlag)
        {
            ComboAttack[7].SetActive(false);
            ComboAttackFlag = false;
            if (ComboCount >= 3) canAirAttack();
        }
        else
        {
            Attack3.SetActive(false);
            if (ComboFlag) canAirAttack();
        }
    }


    /// <summary>
    /// ジャンプの処理
    /// </summary>
    private void Jump()
    {
        OnGround = false;
        SensorLock = true;
        ActionFlag = false; 
        PStatas = 4;
        if ( (ComboFlag && ComboPlusFlag[4]) || GameManager.PlayerKingBuffer)
        {
            ComboAttackFlag = true;
            ResetCombo();
        }

        Rigid.Sleep();

        if (ComboAttackFlag)
        {
            JumpAttackAnimetionCount = 0;
            animetor.SetTrigger("JumpAttack");
            ComboAttackFlag = false;
        }
        else
        {
            animetor.SetTrigger("Jump");
            Invoke("addJumpFoace", jumpWaitTime);
        }
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
        animetor.SetTrigger("HighJump");
        Rigid.Sleep();
       // StopBody();
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

    public void StartJumpAttackAnime()
    {
        if(JumpAttackAnimetionCount == 0)
        {
            GameManager.playStageSE(47);
            if (GameDirector.Player3DMode) Rigid.AddForce(JoyVec.x * jumpXZForce, jumpForce, JoyVec.y * jumpXZForce);
            else Rigid.AddForce(JoyVec.x * jumpXZForce, jumpForce, 0);
            ComboAttack[4].SetActive(true);
        }
    }

    public void FinishJumpAttackAnime()
    {
        JumpAttackAnimetionCount++;
        if(JumpAttackAnimetionCount == 1)
        {
            ActionFlag = true;
            OnGround = false;
        }
        else if (JumpAttackAnimetionCount == 2)
        {
          //  PanishmentFlag = false;
            ComboAttack[4].SetActive(false);
            animetor.SetTrigger("JumpAttackEnd");
            SensorLock = false;
            canAirAttack();
        }
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

        if ( (ComboFlag && ComboPlusFlag[2]) || GameManager.PlayerKingBuffer)
        {
            ComboAttackFlag = true;
            ResetCombo();
        }
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
            if (PStatas == 2) RollingF = new Vector2(transform.forward.x * Rolling2Force, transform.forward.z * Rolling2Force);
            else RollingF = new Vector2(transform.forward.x * RollingForce, transform.forward.z * RollingForce);
        }
        if (GameDirector.Player3DMode == false) { RollingF.y = 0f; }
        Rigid.AddForce(RollingF.x, 0, RollingF.y);
       // Invoke("MovePossible", 16.0f / 30.0f);

        if (ComboAttackFlag)
        {
            ComboAttack[3].SetActive(true);
            GameManager.playStageSE(47);
        }
    }

    public void FinishRolling()
    {
        ActionFlag = true;
        if (ComboStackFlag)
        {
            ComboStackFlag = false;
            if (ComboStackNum == 0 && !GameDirector.ActionLockFlag[0]) NormalAttack();
            if (ComboStackNum == 1 && !GameDirector.ActionLockFlag[1]) Jump();
            if (ComboStackNum == 2 && !GameDirector.ActionLockFlag[3]) Rolling();
            if (ComboStackNum == 4 && !GameDirector.ActionLockFlag[2]) Ball();
            if (ComboStackNum == 6 && !GameDirector.ActionLockFlag[4]) Fire();
        }
    }

    /// <summary>
    /// ボールアクションの処理
    /// </summary>
    private void Ball()
    {
        BallRFlag = false;
        if (StageCamera != null) StageCamera.ChangeCRange(3);
        Effector.Play(3);
        GameManager.playStageSE(18);
        PStatas = 5;
        ActionFlag = false;
        animetor.SetTrigger("Ball");
       // animetor.SetInteger("PlayerStatas", PStatas);
        PCollider[0].enabled = false;
        PCollider[1].enabled = false;
        Director.ChangePalette(2);
        Invoke("BallMode", 10.0f / 30.0f);
        BallFalg = true;
        FootSensor.transform.localScale = new Vector3(1, 10, 1);
        ComboCount = 0;
        if (ComboFlag || GameManager.PlayerKingBuffer)
        {
            ComboAttackFlag = true;
            ComboAttack[2].SetActive(true);
            ResetCombo();
            GameManager.playStageSE(46);
        }
    }

    /// <summary>
    /// フロートボールの処理
    /// </summary>
    private void FloatBall()
    {
        BallRFlag = false;
        if (StageCamera != null) StageCamera.ChangeCRange(3);
        EffectObj[6].SetActive(true);
        GameManager.playStageSE(18);
        PStatas = 5;
        ActionFlag = false;
        FloatFlag = true;
        animetor.SetTrigger("Ball");
        PCollider[0].enabled = false;
        PCollider[1].enabled = false;
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
        MagnetFlag = false;
        MagnetDelta = 0;
        PlayerGravity = Gravity;
        if (FloatFlag)
        {
            FloatFlag = false;
            Rigid.mass = 1.0f;
            FloatMode = false;
            if(GameDirector.Player3DMode) Rigid.constraints = RigidbodyConstraints.FreezeRotation;
            else Rigid.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
            EffectObj[6].SetActive(false);
            EffectObj[7].SetActive(false);
            FloatStatas = 0;
        }
        FootSensor.transform.localScale = new Vector3(15, 10, 8);
        GameManager.playStageSE(17);
        PStatas = 0;
        animetor.SetTrigger("Standard");

        PCollider[0].enabled = true;
        PCollider[1].enabled = true;
        Director.ChangePalette(1);
        animetor.speed = 1.0f;
        if (GameManager.PlayerKingBuffer) StopMat();

        Invoke("RetrunCRange", 1.0f);
        if (ComboAttackFlag)
        {
            ComboAttackFlag = false;
            ComboAttack[2].SetActive(false);
        }
        //if (StageCamera != null) StageCamera.ChangeCRange(0.8f);
    }

    private void RetrunCRange() { if (StageCamera != null) StageCamera.ChangeCRange(0.8f); }
    
    /// <summary>
    /// ファイア魔法の処理
    /// </summary>
    private void Fire()
    {
        if (!GameManager.PlayerKingBuffer)
        {
            MaxSpeedScale = 0.5f;
            walkForceScale = 0.7f;
        }
        CanMagic = true;
        EffectObj[4].SetActive(true);
        GameManager.playStageSE(27);
        Director.ChangePalette(3);
        GameDirector.EnemyTargetFlag = true;
        GameDirector.MagicType = 1;
        Director.NoUseIcon(2);
        Invoke("CanRelease", 1.0f);
        if (ComboFlag || GameManager.PlayerKingBuffer)
        {
            ComboAttackFlag = true;
            ResetCombo();
            Enhance = Instantiate(ComboAttack[1], transform.position, transform.rotation);
            GameManager.playStageSE(45);
        }
        ComboCount = 0;
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
            Fire.GetComponent<Rigidbody>().velocity = new Vector3(transform.forward.x * FireSpeed, 0, transform.forward.z * FireSpeed);
        }
        Invoke("AMagic", 1.0f);
    }

    /// <summary>
    /// サンダー魔法の処理
    /// </summary>
    private void Thunder()
    {
        if (!GameManager.PlayerKingBuffer)
        {
            MaxSpeedScale = 0.5f;
            walkForceScale = 0.7f;
        }
        CanMagic = true;
        EffectObj[5].SetActive(true);
        GameManager.playStageSE(27);
        Director.ChangePalette(4);
        GameDirector.EnemyTargetFlag = true;
        GameDirector.MagicType = 2;
        Director.NoUseIcon(2);
        Invoke("CanRelease", 1.0f);

        if (ComboCount >= 3 || GameManager.PlayerKingBuffer)
        {
            ComboAttackFlag = true;
            ResetCombo();
            Enhance = Instantiate(ComboAttack[10], transform.position, transform.rotation);
            GameManager.playStageSE(52);
        }
        ComboCount = 0;
    }
    private void PlayThunder()
    {
        PlayMagic = false;
        Director.PlayerMP -= ThunderMpPoint;
        EffectObj[13].SetActive(true);
        float Dis = 4.0f;

        if (GameDirector.isTarget())
        {
            GameObject[] ATargets = GameDirector.GetTargets();
            for (int i = 0; i < ATargets.Length; i++)
            {
                GameObject Thunder = Instantiate(ThunderMagic, ATargets[i].transform.position, Quaternion.identity);
                Thunder.GetComponent<ThunderCon>().init(gameObject, ATargets[i]);
               // Thunder.GetComponent<MagicController>().HomingStart(ATargets[i]);
            }
        }
        else
        {
            Instantiate(ThunderMagic, transform.position + Vector3.left * Dis, Quaternion.identity).GetComponent<ThunderCon>().init(gameObject, Vector3.left * Dis);
            Instantiate(ThunderMagic, transform.position + Vector3.right * Dis, Quaternion.identity).GetComponent<ThunderCon>().init(gameObject, Vector3.right * Dis);
            Instantiate(ThunderMagic, transform.position + Vector3.forward * Dis, Quaternion.identity).GetComponent<ThunderCon>().init(gameObject, Vector3.forward * Dis);
            Instantiate(ThunderMagic, transform.position + Vector3.back * Dis, Quaternion.identity).GetComponent<ThunderCon>().init(gameObject, Vector3.back * Dis);
            Instantiate(ThunderMagic, transform.position, Quaternion.identity).GetComponent<ThunderCon>().init(gameObject, Vector3.zero);
        }
        Invoke("AMagic", 2.0f);
    }

    /// <summary>
    /// 魔法詠唱解除処理
    /// </summary>
    private void MagicReleae()
    {
        ActionFlag = true;
        if (!GameManager.PlayerKingBuffer)
        {
            MaxSpeedScale = 1.0f;
            walkForceScale = 1.0f;
        }
        if (ComboAttackFlag)
        {
            ComboAttackFlag = false;
            Destroy(Enhance);
        }
        MagicRFlag = false;
        GameDirector.EnemyTargetFlag = false;
        Director.ChangePalette(1);
        GameDirector.RemoveAllTarget();

        if (GameDirector.MagicType == 1) EffectObj[4].SetActive(false); else if (GameDirector.MagicType == 2) EffectObj[5].SetActive(false);

        GameDirector.MagicType = 0;
        int[] an = Director.PaletteNumber;
        if (GameDirector.CanUseAction[an[1]]) Director.UseIcon(1); else Director.NoUseIcon(1);
        GameManager.playStageSE(17);
    }
    
    private void AMagic()
    {
        PlayMagic = true;
        EffectObj[13].SetActive(false);
    }

    //突進********************************************************************************
    private bool RushRFlag = false;                 //突進状態解除フラグが有効か
    private float RushRInterval = 0.5f;
    private float RushRFirstInterval = 0.1f;
    private float RushRChangeInterval = 0.5f;
    private float RushRDelta = 0;                   //突進状態解除フラグのタイマー
    private float RushFreezeTime = 1f;              //突進解除時の硬直時間
    static public bool RushNoDamageFlag = false;    //突進状態時のダメージ無効フラグ
    private float RushMoveForce = 60;               //突進時にプレイヤーを動かす力
    private float RushMaxSpeed = 20;                //突進時の最大スピード
    private Vector3 RushVec = Vector3.right;        //突進時に加える力の方向
                                                    // private int RushDir = 1;                        //突進の進行方向  1:右　2:左  3:上  4:下
    private Vector3 RushPos = Vector3.zero;         //突進時の直前の位置を記憶
    private float RushStopMove = 0.01f;             //突進時に停止する時の移動距離限界
    private bool RushDirLock = false;               //突進終了時のプレイヤー向き変更をロックするか

    private bool RushNoBrake = false;               //突進終了時にブレーキをしないフラグ(衝突による解除が起きた場合に使用)
    private bool RushBrakeFlag = false;             //突進終了時のブレーキフラグ
    private float RushBrakeForce = 20f;             //突進終了時のブレーキに使用する力
    private float RushBrakeTime = 0.4f;             //突進終了時のブレーキをかける時間
    /// <summary>
    /// 突進処理開始
    /// </summary>
    private void Rush()
    {
        ActionFlag = false;                 //アクション使用禁止     
        RushNoBrake = false;                //アクションのブレーキなしフラグをfalse
        PStatas = 6;                        //プレイヤー状態を突進状態に変更
        RushNoDamageFlag = true;            //ダメージを無効にする
        RushPos = transform.position;       //突進ポジションの更新
        RushRInterval = RushRFirstInterval;
        RushDirLock = false;

        Vector3 LookVec = transform.forward;
        if (Mathf.Abs(LookVec.x) >= Mathf.Abs(LookVec.z) || !GameDirector.Player3DMode)
        {
            if (LookVec.x >= 0) RushVec = Vector3.right; else RushVec = Vector3.left;
        }
        else
        {
            if (LookVec.z >= 0) RushVec = Vector3.forward; else RushVec = Vector3.back;
        }

        //アニメーションを変更
       // animetor.SetInteger("PlayerStatas", 6);
        animetor.SetTrigger("Rush");

        Director.ChangePalette(2);          //アクションパレットタイプを変更
        RushNoCanRelease();                 //アクション解除を無効化する

        if(GameManager.PlayerKingBuffer) NormalMat();   //キング状態ならプレイヤーの物理属性をスタンダードに変更                        
        EffectObj[2].SetActive(true);       //ラッシュエフェクトを表示
        GameManager.playStageSE(30);        //ラッシュのSEを再生
    }

    /// <summary>
    /// 突進状態の解除
    /// </summary>
    private void RushRelease()
    {
        RushNoDamageFlag = false;
        GravityVec = Vector3.down;
        if (GameManager.PlayerKingBuffer) StopMat();
        if (!RushNoBrake) animetor.SetTrigger("Rush_End");
        Director.ChangePalette(1);
        Invoke("RushEnd", RushFreezeTime);
        EffectObj[2].SetActive(false);
        EffectObj[3].SetActive(true);
        RushBrakeFlag = true;
        Invoke("stopRushBrake", RushBrakeTime);

        GameManager.stopLoopSE();
        GameManager.playStageSE(31);
    }
    private void stopRushBrake()
    {
        RushBrakeFlag = false;
        RushDirLock = false;
        if (RushNoBrake) animetor.SetTrigger("Standard");
        if (!GameDirector.Player3DMode) Change2DRigid();
    }
    private void RushEnd()
    {
        PStatas = 0;
        ActionFlag = true;
        EffectObj[3].SetActive(false);
        GameManager.stopLoopSE();
    }

    /// <summary>
    /// 突進状態時の処理
    /// </summary>
    private void RushActive()
    {
        if (PStatas != 6) return;   //プレイヤーの状態が6

        //解除を有効にする
        RushRDelta += 0.02f;
        if (RushRDelta >= RushRInterval && !RushRFlag)
        {
            RushDirLock = true;
            CanRelease();
            GameManager.playLoopSE(0);
        }

        //突進移動の処理
        if (RushNoDamageFlag)
        {
            bool StopFalg = false;
            if (RushRFlag && !WarpLock)
            {
                Vector3 Pos = transform.position;
                Vector3 DeltaVec = transform.position - RushPos;
                for(int i = 0; i <= 2; i++)
                {
                    if(RushVec[i] != 0)
                    {
                        float StopDis = RushVec[i] * RushStopMove;
                        if(StopDis > 0)
                        {
                            if(DeltaVec[i] < StopDis)
                            {
                                StopFalg = true;
                                Debug.Log("突進リリースON");
                            }

                                
                        }
                        else if (StopDis < 0)
                        {
                            if (DeltaVec[i] > StopDis)
                            {
                                StopFalg = true;
                                Debug.Log("突進リリースON");
                            }
                        }
                    }
                }
            }

            if (StopFalg)
            {
                RushNoBrake = true;
                RushRelease();
            }
            else
            {
                if (Rigid.velocity.magnitude < RushMaxSpeed)//スピード制限
                {
                    //移動のための力を加える
                    Rigid.AddForce(RushVec * RushMoveForce);
                }
            }
            RushPos = transform.position;


        }

        if (RushBrakeFlag && !RushNoBrake)
        {
            Rigid.AddForce(-RushVec * RushBrakeForce);
        }
    }

    /// <summary>
    /// ラッシュの解除不可にする
    /// </summary>
    private void RushNoCanRelease()
    {
        RushRDelta = 0;                     //アクション解除フラグのタイマーを初期化
        RushRFlag = false;                  //アクション解除禁止
        RushDirLock = false;
        Director.NoUseIcon(2);              //アクションパレッドの解除アイコンを使用不可に変更
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

    /// <summary>
    /// キング状態のセットアップ
    /// 移動速度、ジャンプ力アップ、猫パンチ強化、魔法移動、神速ダッシュの反動軽減
    /// 滑る量を減らす、回避アクションの無敵時間延長、チャージの時間、ダッシュ攻撃の再使用時間
    /// </summary>
    private void KingSetUp()
    {
        if (GameManager.PlayerKingBuffer)
        {
            EffectObj[8].SetActive(true);

            MaxSpeedScale = 2;              //移動の最大スピードの掛け率変更
            walkForceScale = 1.5f;              //移動に使用する力
            AirForce = 12.0f;               //空中の移動で使用する力 
            MaxAirSpeed = 10.0f;            //空中でのスピード上限
            jumpWaitTime = 0.1f;            //ジャンプの力を加えるまでのラグ
            jumpForce = 450.0f;             //ジャンプに使用する上方向の力
            highjumpForce = 650.0f;         //ハイジャンプの時に加える力
            A2RigidTime = 0.8f;             //リキャスト時間(ダッシュ攻撃)
            AvoidTime = 1f;                 //回避の無敵時間
            FloatForce = 1.2f;                //フロート時に加える力
            FloatMaxSpped = 20.0f;           //フロート時のマックススピード
            chrageActionTime = 0.5f;        //チャージの時間
            StopMat();
        }
    }


    private void StopMat()
    {
        PCollider[0].material.dynamicFriction = 2;
        PCollider[0].material.staticFriction = 1;
        PCollider[0].material.frictionCombine = PhysicMaterialCombine.Maximum;
        PCollider[1].material.dynamicFriction = 2;
        PCollider[1].material.staticFriction = 1;
        PCollider[1].material.frictionCombine = PhysicMaterialCombine.Maximum;
      //  Debug.Log("停止マテリアル");
    }

    private void NormalMat()
    {
        PCollider[0].material.dynamicFriction = 0.6f;
        PCollider[0].material.staticFriction = 0.6f;
        PCollider[0].material.frictionCombine = PhysicMaterialCombine.Average;
        PCollider[1].material.dynamicFriction = 0.6f;
        PCollider[1].material.staticFriction = 0.6f;
        PCollider[1].material.frictionCombine = PhysicMaterialCombine.Average;
     //   Debug.Log("通常マテリアル");
    }

    public void KingAnimationSpeed(float Speed)
    {
        if (GameManager.PlayerKingBuffer)
        {
            animetor.speed = Speed;
        }
    }

    public void AnimationSpeed(float Speed)
    {
        animetor.speed = Speed;
    }

    public void KingAninaRetrun()
    {
        animetor.speed = 1;
        if (GameManager.PlayerKingBuffer)
        {
            animetor.speed = 1;
        }
    }


    //***************************************************特殊ギミックとの接触**************************
    /// <summary>
    /// ラッシュブロックと接触時
    /// </summary>
    /// <param name="ContactObj"></param>
    private void ContactRushBlock(GameObject ContactObj)
    {
        if (PStatas != 6) return;
        if (!RushRFlag) return;
        if (!RushNoDamageFlag) return;
        if (ContactObj.tag == "RushBlock" && RushRFlag)
        {
            RushBlock RBlock = ContactObj.GetComponent<RushBlock>();
            Vector3 RVec = RBlock.RushVec;
            if (RushVec == RVec) return;
            RushRInterval = RushRChangeInterval;
            RushNoCanRelease();
            RBlock.PlayEffect();

            Vector3 PPos = transform.position;
            Vector3 BPos = ContactObj.transform.position;
            float XDis = Mathf.Abs(BPos.x - PPos.x);
            float YDis = Mathf.Abs(BPos.y - PPos.y);
            float ZDis = Mathf.Abs(BPos.z - PPos.z);

            if (!GameDirector.Player3DMode && RVec.z != 0)
            {
                Change3DRigid();
                if(YDis > 2.5f)
                {
                    Vector3 ChangePPos = transform.position;
                    ChangePPos.x = RBlock.transform.position.x;
                    ChangePPos.z = RBlock.transform.position.z;
                    transform.position = ChangePPos;
                }
            }
                

            if (!GameDirector.Player3DMode && RVec.z == 0) Change2DRigid();
            GameManager.playStageSE(40);
                        
            Rigid.isKinematic = true;

            if (!GameDirector.Player3DMode) Pzp = ContactObj.transform.position.z;

            
            Vector3 NextGravityVec = Vector3.down;

            
          
            if(Mathf.Abs(RVec.x) == 1 || Mathf.Abs(RVec.y) == 1 || Mathf.Abs(RVec.z) == 1)
            {
                if (XDis > YDis && XDis > ZDis)
                {
                    if(PPos.x <= BPos.x)
                    {
                        if(Mathf.Abs(RVec.x) == 1) NextGravityVec = Vector3.down;
                        else NextGravityVec = Vector3.right;
                    }
                    else
                    {
                        if (Mathf.Abs(RVec.x) == 1) NextGravityVec = Vector3.down;
                        else NextGravityVec = Vector3.left;
                    }
                }

                if (ZDis > XDis && ZDis > YDis)
                {
                    if (PPos.z <= BPos.z)
                    {
                        if (Mathf.Abs(RVec.z) == 1) NextGravityVec = Vector3.down;
                        else NextGravityVec = Vector3.forward;
                    }
                    else
                    {
                        if (Mathf.Abs(RVec.z) == 1) NextGravityVec = Vector3.down;
                        else NextGravityVec = Vector3.back;
                    }
                }

                if (YDis > XDis && YDis > ZDis)
                {
                    if (PPos.y <= BPos.y)
                    {
                        if (Mathf.Abs(RVec.y) == 1)
                        {
                            if(Mathf.Abs(RushVec.y) == 1) NextGravityVec = GravityVec;
                            else
                            {
                                if(XDis >= ZDis)
                                {
                                    if(PPos.x <= BPos.x) NextGravityVec = Vector3.right;
                                    else NextGravityVec = Vector3.left;
                                }
                                else
                                {
                                    if (PPos.z <= BPos.z) NextGravityVec = Vector3.forward;
                                    else NextGravityVec = Vector3.back;
                                }
                            }
                        }
                        else NextGravityVec = Vector3.up;
                    }
                    else
                    {
                        if (Mathf.Abs(RVec.y) == 1)
                        {
                            if (Mathf.Abs(RushVec.y) == 1) NextGravityVec = GravityVec;
                            else
                            {
                                if (XDis >= ZDis)
                                {
                                    if (PPos.x <= BPos.x) NextGravityVec = Vector3.right;
                                    else NextGravityVec = Vector3.left;
                                }
                                else
                                {
                                    if (PPos.z <= BPos.z) NextGravityVec = Vector3.forward;
                                    else NextGravityVec = Vector3.back;
                                }
                            }
                        }
                        else NextGravityVec = Vector3.down;
                    }
                }
            }

            RushVec = RVec;
            if (GravityVec != NextGravityVec)
            {
                float FixFloat = 0.5f;
                Vector3 FixPPos = PPos - GravityVec * FixFloat - NextGravityVec * FixFloat;
                transform.position = FixPPos;
            }

            GravityVec = NextGravityVec;
            if (!GameDirector.Player3DMode && (RVec == Vector3.right || RVec == Vector3.left))
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, Pzp);
            }

            Invoke("ContactRushBlockAfter", 0.05f);
        }
    }
    private void ContactRushBlockAfter()
    {
        Rigid.isKinematic = false;
    }

    /// <summary>
    /// 竜巻と接触時
    /// </summary>
    /// <param name="ContactObj"></param>
    private void ContactWind(GameObject ContactObj)
    {
        if (!FloatFlag) return;
        if (ContactObj.tag != "Wind") return;
        float coefficient = 0.1f;
        Vector3 relativeVelocity = ContactObj.GetComponent<WindObject>().WindVector - Rigid.velocity;
        Rigid.AddForce(coefficient * relativeVelocity);
    }

    private float MagnetDelta = 0;
    private float MagnetRTime = 0.1f;
    private float MagnetForce = 15f;
    private float MagnetMaxSpeed = 12.0f;
    private bool MagnetFlag = false;
    private bool MagnetChangeLock = false;
    private float MagnetChangeInterTime = 3.0f;
    private float MagnetChangeDelta = 0;
    private Vector3 MagnetVec = Vector3.down;
    private string MagnetName = "";

    private Cinemachine.CinemachineDollyCart Chine;

    /// <summary>
    /// 光の道と接触
    /// </summary>
    public void ContactLightLoad()
    {

    }

    /// <summary>
    /// 光の道状態開始
    /// </summary>
    public void StartLightBall(Cinemachine.CinemachineSmoothPath ChinePath, bool startFlag)
    {
        Chine.m_Path = ChinePath;
        if (startFlag)
        {
            Chine.m_Position = 0;
            Chine.m_Speed = 3;
        }
        else
        {
            Chine.m_Position = 10000;
            Chine.m_Speed = -3;
        }
        Chine.enabled = true;
        Rigid.isKinematic = true;
        Director.NoUseIcon(2);
        BallChineFalg = true;
    }

    /// <summary>
    /// 光の道状態解除
    /// </summary>
    public void EndLightBall(float ZPos)
    {
        Pzp = ZPos;
        transform.position = new Vector3(transform.position.x, transform.position.y , Pzp);
        Chine.enabled = false;
        Rigid.isKinematic = false;
        Director.UseIcon(2);
        BallChineFalg = false;
    }

    /// <summary>
    /// 磁力球と接触時
    /// </summary>
    /// <param name="ContactObj"></param>
    private void ContactMagnet(GameObject ContactObj)
    {
        if (!BallFalg) return;
        if (ContactObj.tag != "Magnet") return;
        string Name = ContactObj.name;
        if (!MagnetFlag || !MagnetChangeLock || (MagnetChangeLock && Name == MagnetName) )
        {
            if (MagnetName != Name)
            {
                Debug.Log("切り替えのロック "　+　Name);
                MagnetChangeDelta = 0;
                MagnetChangeLock = true;
            }
                
            MagnetName = Name;
            MagnetFlag = true;
            MagnetDelta = 0;
            PlayerGravity = 0;
            float magnetPow = ContactObj.GetComponent<MagnetObject>().MagnetPow;
            MagnetVec = (ContactObj.transform.position - transform.position).normalized;
            Rigid.AddForce(magnetPow * MagnetVec, ForceMode.Acceleration);
        }
    }
    private void MagnetRelease()
    {
        if (!MagnetFlag) return;
        MagnetDelta += 0.02f;
        if(MagnetDelta >= MagnetRTime)
        {
            MagnetDelta = 0;
            MagnetFlag = false;
            PlayerGravity = Gravity;
        }
    }
    private void MagnetChangeRelease()
    {
        if (!MagnetChangeLock) return;
        MagnetChangeDelta += 0.02f;
        if(MagnetChangeDelta >= MagnetChangeInterTime)
        {
            MagnetChangeDelta = 0;
            MagnetChangeLock = false;
        }
    }

    private bool WarpFlag = false;                  //ワープ中か
    private bool WarpLock = false;                  //ワープによる行動の制限
    private float WarpTime = 2.0f;                  //ワープ時間
    private Vector3 WarpDeltaVec = Vector3.zero;    //ワープの微小移動距離
    private Vector3 WarpStartPos = Vector3.zero;    //ワープ開始地点
    private Vector3 WarpEndPos = Vector3.zero;      //ワープの終了地点
    private float WarpDeltaTime = 0;                //ワープの時間

  
    /// <summary>
    /// ワープと接触開始時に実行
    /// </summary>
    /// <param name="ContactObj"></param>
    private void ContactWarp(GameObject ContactObj)
    {
        if (WarpFlag) return;
        if (ContactObj.tag != "Warp") return;

        WarpMoveStart(ContactObj.GetComponent<Warp>().WarpMovePos);
    }

    /// <summary>
    ///　ワープ開始
    /// </summary>
    /// <param name="MPos"></param>
    public void WarpMoveStart(Vector3 MPos)
    {
        WarpLock = true;
        //各状態に応じた処理を記入
        //魔法詠唱解除
        if (GameDirector.MagicType != 0) MagicReleae();

        //フロートエフェクト解除
        if (FloatFlag) EffectObj[6].SetActive(false);

        //オーバーフロートエフェクト解除
        if (FloatMode) EffectObj[7].SetActive(false);

        //チャージエフェクト解除
        resetCharge();

        //ラッシュエフェクト解除
        if (PStatas == 6) EffectObj[2].SetActive(false);

        if (GameManager.PlayerKingBuffer) EffectObj[8].SetActive(false);


        //動けなくしてアクションを禁止する
        Rigid.isKinematic = true;
        ActionFlag = false;

        //コライダーを無効化する
        PCollider[0].enabled = false;
        PCollider[1].enabled = false;
        LegCollider.enabled = false;
        SensorCollider.enabled = false;

        //ダメージ処理のリセット
        damageDelta = 0;
        PlayerDamgagFlag = true;

        //プレイヤーのメッシュを消し、エフェクトを出す
        EffectObj[9].SetActive(true);
        render.enabled = false;
        GameManager.playStageSE(41);

        //移動時間と微小移動距離の計算
        WarpDeltaTime = 0;
        WarpStartPos = transform.position;
        WarpEndPos = MPos;
        WarpDeltaVec = (WarpEndPos - WarpStartPos) / WarpTime;
        WarpFlag = true;
    }

    /// <summary>
    /// ワープ移動処理 (Updateに記述)
    /// </summary>
    private void WarpMove(float Deltatime)
    {
        if (!WarpFlag) return;
        WarpDeltaTime += Deltatime;
        if(WarpDeltaTime < WarpTime)
        {
            transform.position = WarpStartPos + WarpDeltaVec * WarpDeltaTime;
        }
        else
        {
            transform.position = WarpEndPos;
            FinishWarp();
        }
    }

    /// <summary>
    /// ワープ終了時の処理
    /// </summary>
    private void FinishWarp()
    {
        GameDirector.GamePlay = true;
        //プレイヤーのメッシュを表示、エフェクトを消す
        EffectObj[9].SetActive(false);
        render.enabled = true;
        //  GameManager.playStageSE(40);

        LegCollider.enabled = true;
        SensorCollider.enabled = true;
        //コライダーを有効化する
        if (!BallFalg && !FloatFlag)
        {
            PCollider[0].enabled = true;
            PCollider[1].enabled = true;
        }

        //動けるようにしてアクションを使用可能にする
        Rigid.isKinematic = false;

        //フロートエフェクト
        if (FloatFlag) EffectObj[6].SetActive(true);

        //オーバーフロートエフェクト
        if(FloatMode) EffectObj[7].SetActive(true);

        if(GameManager.PlayerKingBuffer) EffectObj[8].SetActive(true);

        //ラッシュエフェクト
        if (PStatas == 6)
        {
            EffectObj[2].SetActive(true);
        }
        else
        {
            ActionFlag = true;
        }

        WarpFlag = false;
        Invoke("WarpLockOFF", 0.5f);
    }
    private void WarpLockOFF() => WarpLock = false;

    /// <summary>
    /// 復活してワープするアクションを実行(トレーニング用)
    /// </summary>
    /// <param name="RebornP"></param>
    public void RebornWarp(Vector3 RebornP)
    {
        if (RushNoDamageFlag) RushRelease();
        if (BallChineFalg) EndLightBall(Pzp);
        if (FloatStatas >= 2)
        {
            FloatStatas = 0;
            ResetFloatAfter();
        }
        WarpMoveStart(RebornP);
    }

    public void PlayerEffect(int Num)
    {
        Effector.Play(Num);
    }

    private void ComboTimeCount(float DeltaTime)
    {
        if (ComboTime > 0) ComboTime -= DeltaTime;
        if(!ComboFlag && ComboTime > 0) //コンボ受付開始
        {
            ComboFlag = true;
            GameManager.playStageSE(42);
            if (ComboCount == 1) EffectObj[10].SetActive(true);
            else if (ComboCount == 2) EffectObj[11].SetActive(true);
            else if (ComboCount >= 3) EffectObj[12].SetActive(true);


            for (int i = 0; i < Director.PaletteNumber.Length; i++)
            {
                Director.UseIcon(i);
            }

            if(ComboCount >= 3 && !ComboIconChangeFlag)
            {
                ComboIconChangeFlag = true;
                Director.PaletteNumber = new int[5] { 0, 1, 4, 2, 7 };
            }
        }

        if(ComboFlag && ComboTime <= 0) //時間経過によるコンボ受付終了
        {
            ComboCount = 0;
            ResetCombo();
        }
    }

    private void ResetCombo()
    {
        ComboFlag = false;
        ComboTime = 0;
        EffectObj[10].SetActive(false);
        EffectObj[11].SetActive(false);
        EffectObj[12].SetActive(false);

        if (ComboIconChangeFlag)
        {
            ComboIconChangeFlag = false;
            Director.PaletteNumber = new int[5] { 0, 1, 4, 2, 6 };
        }
        
    }




    private void TutorialAnimePlay()
    {
        if (movePadAnimator == null) movePadAnimator = Director.GetMovePadAnime();
        if (actionPadAnimator == null) actionPadAnimator = Director.GetActionPadAnime();
        movePadAnimator.SetTrigger("Right");
        actionPadAnimator.SetTrigger("Right");
    }

    /// <summary>
    /// チュートリアルブロックと接触時
    /// </summary>
    /// <param name="ContactObj"></param>
    private void ContactTutorialBlock(GameObject ContactObj)
    {
        if (ContactObj.tag != "TutorialBlock") return;
        if (movePadAnimator == null) movePadAnimator = Director.GetMovePadAnime();
        if (actionPadAnimator == null) actionPadAnimator = Director.GetActionPadAnime();
        TutorialBlock TBlock = ContactObj.GetComponent<TutorialBlock>();
        if (TBlock.TutorialFinsh)
        {
            tutorialFinish();
        }
        else
        {
            int TType = TBlock.TutorialNum;
            int TStage = TBlock.TutorialStage;
            if (tutorialType == 0) tutorialType = TType;
            if(tutorialType == TType && TStage == tutorialStage + 1)
            {
                tutorialFlag = true;
                tutorialStage = TStage;
                tutorialPlay();
            }
        }
        GameManager.playSystemSE(1);
        Destroy(ContactObj);
    }

    /// <summary>
    /// チュートリアル終了
    /// </summary>
    private void tutorialFinish()
    {
        tutorialFlag = false;
        tutorialType = 0;
        tutorialStage = 0;
        tutorialMoveLock = false;
        tutorialActionLock = false;
        tutorialAnimeFlag = false;
        tutorialMultiLock = false;
        tutorialMoveLock = false;
        tutorialStopFlag = false;
        Time.timeScale = 1;
        Director.hideMoveTutorial();
        Director.hideActionTutorial();
        GameDirector.TargetLock = false;
    }

    /// <summary>
    /// チュートリアルの条件、ステータスを変更
    /// </summary>
    private void tutorialPlay()
    {
        if (!tutorialFlag) return;
        Debug.Log("Start_Tutorial");
        if (tutorialType == 1 && tutorialStage == 1)
        {
            Director.ChangeMoveTutorialText("左にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.left;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Left");
          
        }
        else if (tutorialType == 1 && tutorialStage == 2)
        {
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
            Rigid.Sleep();
        }
        else if (tutorialType == 1 && tutorialStage == 3)
        {
            Director.ChangeMoveTutorialText("上にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.up;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Up");
            Rigid.Sleep();
        }
        else if (tutorialType == 1 && tutorialStage == 4)
        {
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
            Rigid.Sleep();
        }
        else if (tutorialType == 1 && tutorialStage == 5)
        {
            Director.ChangeMoveTutorialText("下にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.down;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Down");
            Rigid.Sleep();
        }
        else if (tutorialType == 2 && tutorialStage == 1)
        {
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
            Rigid.Sleep();
        }
        else if (tutorialType == 2 && tutorialStage == 2)
        {
            Director.ChangeMoveTutorialText("右にスワイプしながら");
            Director.ChangeActionTutorialText("上にフリックする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.up;
            if (Director.GetMoveAnimeIsActiove()) Director.showActionTutorial();
            PlayMoveTAnime("Right");
            PlayActionTAnime("Up");
            tutorialMultiLock = true;
            tutorialNextStage = true;
            tutorialStopFlag = true;
            Rigid.Sleep();
        }
        else if (tutorialType == 2 && tutorialStage == 3)
        {
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
            Director.hideActionTutorial();
            tutorialStopFlag = true;
            tutorialMultiLock = false;
        }
        else if (tutorialType == 3 && tutorialStage == 1)
        {
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
            tutorialStopFlag = true;
        }
        else if (tutorialType == 3 && tutorialStage == 2)
        {
            Director.ChangeMoveTutorialText("タッチしないで");
            Director.ChangeActionTutorialText("タップする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.tap;
            if (Director.GetMoveAnimeIsActiove())  Director.showActionTutorial();
            PlayMoveTAnime("NoHold");
            PlayActionTAnime("Tap");
            tutorialStopFlag = true;
            tutorialMultiLock = true;
            tutorialNextStage = true;
            Rigid.Sleep();
        }
        else if (tutorialType == 3 && tutorialStage == 3)
        {
            tutorialFinish();
        }
        else if (tutorialType == 4 && tutorialStage == 1)
        {
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
            tutorialStopFlag = true;
        }
        else if (tutorialType == 4 && tutorialStage == 2)
        {
            Director.ChangeMoveTutorialText("上にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.up;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Up");
            tutorialStopFlag = true;
            Rigid.Sleep();
        }
        else if (tutorialType == 4 && tutorialStage == 3)
        {
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
            tutorialStopFlag = true;
            Rigid.Sleep();
        }
        else if (tutorialType == 4 && tutorialStage == 4)
        {
            Director.ChangeMoveTutorialText("タッチしながら");
            Director.ChangeActionTutorialText("タップする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.enable;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.tap;
            if (Director.GetMoveAnimeIsActiove()) Director.showActionTutorial();
            PlayMoveTAnime("Hold");
            PlayMoveTAnime("Tap");
            tutorialMultiLock = true;
            tutorialNextStage = true;
            tutorialStopFlag = true;
           // Rigid.Sleep();
        }
        else if (tutorialType == 4 && tutorialStage == 5)
        {
            tutorialFinish();
        }
        else if (tutorialType == 5 && tutorialStage == 1)
        {
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
            tutorialStopFlag = true;
        }
        else if (tutorialType == 5 && tutorialStage == 2)
        {
            Director.ChangeActionTutorialText("上にフリックする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.up;
            if (Director.GetMoveAnimeIsActiove())
            {
                Director.showActionTutorial();
                Director.hideMoveTutorial();
                PlayActionTAnime("Up");
            }
            tutorialStopFlag = true;
            //Rigid.Sleep();
        }
        else if (tutorialType == 5 && tutorialStage == 3)
        {
            Director.ChangeActionTutorialText("タップする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.tap;
            PlayActionTAnime("Tap");
            tutorialStopFlag = true;
            tutorialNextStage = true;
        }
        else if (tutorialType == 5 && tutorialStage == 4)
        {
            tutorialFinish();
        }
        else if (tutorialType == 6 && tutorialStage == 1)
        {
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
            tutorialStopFlag = true;
        }
        else if (tutorialType == 6 && tutorialStage == 2)
        {
            Director.ChangeActionTutorialText("右にフリックする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.right;
            if (Director.GetMoveAnimeIsActiove())
            {
                Director.showActionTutorial();
                Director.hideMoveTutorial();
                PlayActionTAnime("Right");
            }
            tutorialStopFlag = true;
        }
        else if (tutorialType == 6 && tutorialStage == 3)
        {
            tutorialFinish();
        }
        else if (tutorialType == 7 && tutorialStage == 1)
        {
            if(BallFalg) BallRelease();
            Director.ChangeMoveTutorialText("右にスワイプ");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
            Rigid.Sleep();
        }
        else if (tutorialType == 7 && tutorialStage == 2)
        {
            Director.ChangeActionTutorialText("下にフリックする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.down;
            if (Director.GetMoveAnimeIsActiove()) Director.showActionTutorial();
            Director.hideMoveTutorial();
            PlayActionTAnime("Down");
            Rigid.Sleep();

            tutorialNextStage = true;
        }
        else if (tutorialType == 7 && tutorialStage == 3)
        {
            Director.ChangeMoveTutorialText("右にスワイプ");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            if (Director.GetActionAnimeIsActiove()) Director.showMoveTutorial();
            Director.hideActionTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 7 && tutorialStage == 4)
        {
            Director.ChangeActionTutorialText("下にフリックする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.down;
            
            tutorialNextStage = true;
            if (Director.GetMoveAnimeIsActiove()) Director.showActionTutorial();
            Director.hideMoveTutorial();
            PlayActionTAnime("Down2");
            Rigid.Sleep();

            tutorialNextStage = true;
        }
        else if (tutorialType == 7 && tutorialStage == 5)
        {
            tutorialFinish();
        }
        else if (tutorialType == 8 && tutorialStage == 1)
        {
            if (BallFalg) BallRelease();
            Director.ChangeActionTutorialText("下にフリックする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.down;
            PlayActionTAnime("Down");
            Rigid.Sleep();

            tutorialNextStage = true;
        }
        else if (tutorialType == 8 && tutorialStage == 2)
        {
            Director.ChangeMoveTutorialText("右にスワイプ");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            if (Director.GetActionAnimeIsActiove()) Director.showMoveTutorial();
            Director.hideActionTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 8 && tutorialStage == 3)
        {
            Director.ChangeActionTutorialText("下にフリックする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.down;
            if (Director.GetMoveAnimeIsActiove()) Director.showActionTutorial();
            Director.hideMoveTutorial();
            PlayActionTAnime("Down2");
            Rigid.Sleep();

            tutorialNextStage = true;
        }
        else if (tutorialType == 8 && tutorialStage == 4)
        {
            tutorialFinish();
        }
        else if (tutorialType == 9 && tutorialStage == 1)
        {
            if (BallFalg) BallRelease();
            Director.ChangeActionTutorialText("下にフリックする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.down;
            PlayActionTAnime("Down");
            Rigid.Sleep();

            tutorialNextStage = true;
        }
        else if (tutorialType == 9 && tutorialStage == 2)
        {
            Director.ChangeMoveTutorialText("右にスワイプ");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            if (Director.GetActionAnimeIsActiove()) Director.showMoveTutorial();
            Director.hideActionTutorial();
            PlayMoveTAnime("Right");

            tutorialStopFlag = true;
        }
        else if (tutorialType == 10 && tutorialStage == 1)
        {
            if(GameDirector.MagicType != 0) MagicReleae();
            Director.ChangeMoveTutorialText("右にスワイプ");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
            Rigid.Sleep();
            GameDirector.TargetLock = true;
        }
        else if (tutorialType == 10 && tutorialStage == 2)
        {
            Director.ChangeActionTutorialText("左にフリックする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.left;
            tutorialNextStage = true;
            if (Director.GetMoveAnimeIsActiove()) Director.showActionTutorial();
            Director.hideMoveTutorial();
            PlayActionTAnime("Left");
            Rigid.Sleep();
            tutorialNextStage = true;
        }
        else if (tutorialType == 10 && tutorialStage == 3)
        {
            Director.ChangeActionTutorialText("上にフリックする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.up;
            PlayActionTAnime("Up2");
            tutorialNextStage = true;
        }
        else if (tutorialType == 10 && tutorialStage == 4)
        {
            Director.ChangeActionTutorialText("下にフリックする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.down;
            PlayActionTAnime("Down3");
            tutorialNextStage = true;
        }
        else if (tutorialType == 10 && tutorialStage == 5)
        {
            tutorialFinish();
        }
        else if (tutorialType == 11 && tutorialStage == 1)
        {
            if (GameDirector.MagicType != 0) MagicReleae();
            Director.ChangeMoveTutorialText("右にスワイプ");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
            Rigid.Sleep();
        }
        else if (tutorialType == 11 && tutorialStage == 2)
        {
            Director.ChangeActionTutorialText("左にフリックする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.left;
            if (Director.GetMoveAnimeIsActiove()) Director.showActionTutorial();
            Director.hideMoveTutorial();
            PlayActionTAnime("Left");
            Rigid.Sleep();
            tutorialNextStage = true;
        }
        else if (tutorialType == 11 && tutorialStage == 3)
        {
            Director.ChangeActionTutorialText("スイッチをタップする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayActionTAnime("Tap2");
            tutorialTargetCount = 1;
        }
        else if (tutorialType == 11 && tutorialStage == 4)
        {
            Director.ChangeActionTutorialText("上にフリックする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.up;
            PlayActionTAnime("Up2");
            tutorialNextStage = true;
        }
        else if (tutorialType == 11 && tutorialStage == 5)
        {
            Director.ChangeActionTutorialText("下にフリックする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.down;
            PlayActionTAnime("Down3");
            tutorialNextStage = true;
        }
        else if (tutorialType == 11 && tutorialStage == 6)
        {
            tutorialFinish();
        }
        else if (tutorialType == 12 && tutorialStage == 1)
        {
            if (GameDirector.MagicType != 0) MagicReleae();
            Director.ChangeMoveTutorialText("右にスワイプ");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
            Rigid.Sleep();
        }
        else if (tutorialType == 12 && tutorialStage == 2)
        {
            Director.ChangeActionTutorialText("上にスワイプする\nチャージが完了するまでホールド\nチャージ完了後に指を離す");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.holdup;
            if (Director.GetMoveAnimeIsActiove()) Director.showActionTutorial();
            Director.hideMoveTutorial();
            PlayActionTAnime("HoldUp");
            Rigid.Sleep();
 
        }
        else if (tutorialType == 12 && tutorialStage == 3)
        {
            if (GameDirector.MagicType != 0) MagicReleae();
            Director.ChangeMoveTutorialText("右にスワイプ");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            if (Director.GetActionAnimeIsActiove()) Director.showMoveTutorial();
            Director.hideActionTutorial();
            tutorialStopFlag = true;
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 13 && tutorialStage == 1)
        {
            if (GameDirector.MagicType != 0) MagicReleae();
            Director.ChangeActionTutorialText("上にスワイプする\nチャージが完了するまでホールド\nチャージ完了後に指を離す");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.holdup;
            PlayMoveTAnime("HoldUp");
            Rigid.Sleep();
            tutorialNextStage = true;
        }
        else if (tutorialType == 13 && tutorialStage == 2)
        {
            if (GameDirector.MagicType != 0) MagicReleae();
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            if (Director.GetActionAnimeIsActiove()) Director.showMoveTutorial();
            PlayMoveTAnime("Right");
            Director.hideActionTutorial();
            tutorialStopFlag = true;
        }
        else if (tutorialType == 14 && tutorialStage == 1)
        {
            if (BallFalg || FloatFlag) BallRelease();
            Director.ChangeActionTutorialText("下にスワイプする\nチャージが完了するまでホールド\nチャージ完了後に指を離す");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.holddown;
            PlayActionTAnime("HoldDown");
            Rigid.Sleep();
            tutorialNextStage = true;
        }
        else if (tutorialType == 14 && tutorialStage == 2)
        {
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            if (Director.GetActionAnimeIsActiove()) Director.showMoveTutorial();
            PlayMoveTAnime("Right");
            Director.hideActionTutorial();
        }
        else if (tutorialType == 14 && tutorialStage == 3)
        {
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
            tutorialStopFlag = true;
        }
        else if (tutorialType == 14 && tutorialStage == 4)
        {
            Director.ChangeMoveTutorialText("左にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.left;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Left");
            tutorialStopFlag = true;
        }
        else if (tutorialType == 14 && tutorialStage == 5)
        {
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
            tutorialStopFlag = true;
        }
        else if (tutorialType == 15 && tutorialStage == 1)
        {
            if (BallFalg || FloatFlag) BallRelease();
            Director.ChangeActionTutorialText("下にスワイプする\nチャージが完了するまでホールド\nチャージ完了後に指を離す");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.holddown;
            PlayActionTAnime("HoldDown");
            Rigid.Sleep();
            tutorialNextStage = true;
        }
        else if (tutorialType == 15 && tutorialStage == 2)
        {
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            if (Director.GetActionAnimeIsActiove()) Director.showMoveTutorial();
            PlayMoveTAnime("Right");
            Director.hideActionTutorial();
        }
        else if (tutorialType == 15 && tutorialStage == 3)
        {
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
            tutorialStopFlag = true;
        }
        else if (tutorialType == 15 && tutorialStage == 4)
        {
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            Director.hideMoveTutorial();
            tutorialStopFlag = false;
        }
        else if (tutorialType == 15 && tutorialStage == 5)
        {
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            tutorialStopFlag = true;
            tutorialMoveStopRelease = true;
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
            Rigid.Sleep();
        }
        else if (tutorialType == 15 && tutorialStage == 6)
        {
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
            tutorialStopFlag = true;
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 16 && tutorialStage == 1)
        {
            Director.ChangeActionTutorialText("右にスワイプする\nチャージが完了するまでホールド\nチャージ完了後に指を離す");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.holdright;
            PlayActionTAnime("HoldRight");
            tutorialNextStage = true;
            Rigid.Sleep();
        }
        else if (tutorialType == 16 && tutorialStage == 2)
        {
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            Director.hideActionTutorial();
        }
        else if (tutorialType == 17 && tutorialStage == 1)
        {
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 17 && tutorialStage == 2)
        {
            Director.ChangeMoveTutorialText("上にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.up;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Up");
            Rigid.Sleep();
        }
        else if (tutorialType == 17 && tutorialStage == 3)
        {
            Director.ChangeActionTutorialText("右にスワイプする\nチャージが完了するまでホールド\nチャージ完了後に指を離す");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.holdright;
            if (Director.GetMoveAnimeIsActiove()) Director.showActionTutorial();
            PlayActionTAnime("HoldRight");
            Director.hideMoveTutorial();
            tutorialNextStage = true;
            Rigid.Sleep();
        }
        else if (tutorialType == 17 && tutorialStage == 4)
        {
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            Director.hideActionTutorial();
        }
        else if (tutorialType == 18 && tutorialStage == 1)
        {
            Director.ChangeMoveTutorialText("左にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.left;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Left");
        }
        else if (tutorialType == 18 && tutorialStage == 2)
        {
            Director.ChangeActionTutorialText("右にスワイプする\nチャージが完了するまでホールド\nチャージ完了後に指を離す");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.holdright;
            if (Director.GetMoveAnimeIsActiove()) Director.showActionTutorial();
            PlayActionTAnime("HoldRight");
            Director.hideMoveTutorial();
            tutorialNextStage = true;
            Rigid.Sleep();
        }
        else if (tutorialType == 18 && tutorialStage == 3)
        {
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            Director.hideActionTutorial();
        }
        else if (tutorialType == 18 && tutorialStage == 4)
        {
            Director.ChangeActionTutorialText("下にフリックする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.down;
            Director.showActionTutorial();
            PlayActionTAnime("Down2");
            tutorialStopFlag = true;
            tutorialNextStage = true;
        }
        else if (tutorialType == 18 && tutorialStage == 5)
        {
            tutorialFinish();
        }
        else if (tutorialType == 19 && tutorialStage == 1)
        {
            if (GameDirector.MagicType != 0) MagicReleae();
            Director.ChangeMoveTutorialText("右にスワイプする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
            tutorialStopFlag = true;
            Rigid.Sleep();
            GameDirector.TargetLock = true;
        }
        else if (tutorialType == 19 && tutorialStage == 2)
        {
            Director.ChangeActionTutorialText("左にスワイプする\nチャージが完了するまでホールド\nチャージ完了後に指を離す");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.holdleft;
            if (Director.GetMoveAnimeIsActiove()) Director.showActionTutorial();
            PlayActionTAnime("HoldLeft");
            Director.hideMoveTutorial();
            tutorialStopFlag = false;
            tutorialNextStage = true;
            Rigid.Sleep();
        }
        else if (tutorialType == 19 && tutorialStage == 3)
        {
            Director.ChangeActionTutorialText("上にフリックする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.up;
            PlayActionTAnime("Up3");
            tutorialNextStage = true;
        }
        else if (tutorialType == 19 && tutorialStage == 4)
        {
            Director.ChangeActionTutorialText("下にフリックする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.down;
            PlayActionTAnime("Down4");
            tutorialNextStage = true;
        }
        else if (tutorialType == 19 && tutorialStage == 5)
        {
            tutorialFinish();
        }
        else if (tutorialType == 20 && tutorialStage == 1)
        {
            if (GameDirector.MagicType != 0) MagicReleae();
            Director.ChangeMoveTutorialText("右にスワイプ");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.right;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayMoveTAnime("Right");
            Rigid.Sleep();
        }
        else if (tutorialType == 20 && tutorialStage == 2)
        {
            Director.ChangeActionTutorialText("左にスワイプする\nチャージが完了するまでホールド\nチャージ完了後に指を離す");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.holdleft ;
            if (Director.GetMoveAnimeIsActiove()) Director.showActionTutorial();
            Director.hideMoveTutorial();
            PlayActionTAnime("HoldLeft");
            Rigid.Sleep();
            tutorialNextStage = true;
        }
        else if (tutorialType == 20 && tutorialStage == 3)
        {
            Director.ChangeActionTutorialText("敵をスワイプで\nターゲッティングする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.none;
            PlayActionTAnime("Swipe");
            tutorialTargetCount = 7;
        }
        else if (tutorialType == 20 && tutorialStage == 4)
        {
            Director.ChangeActionTutorialText("上にフリックする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.up;
            PlayActionTAnime("Up3");
            tutorialNextStage = true;
        }
        else if (tutorialType == 20 && tutorialStage == 5)
        {
            Director.ChangeActionTutorialText("下にフリックする");
            tutorialMoveLock = true;
            tutorialMoveLockStatas = MoveEnable.none;
            tutorialActionLock = true;
            tutorialActionLockStatas = ActionEnable.down;
            PlayActionTAnime("Down4");
            tutorialNextStage = true;
        }
        else if (tutorialType == 20 && tutorialStage == 6)
        {
            tutorialFinish();
        }

        if (tutorialStopFlag)
        {
            tutorialStoping = true;
            Time.timeScale = 0;
        }
    }

    private void ShowTutorialAnime()
    {
        if (tutorialType == 1 && tutorialStage == 1)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Left");
        }
        else if (tutorialType == 1 && tutorialStage == 2)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 1 && tutorialStage == 3)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Up");
        }
        else if (tutorialType == 1 && tutorialStage == 4)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 1 && tutorialStage == 5)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Down");
        }
        else if (tutorialType == 2 && tutorialStage == 1)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 2 && tutorialStage == 2)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
            Director.showActionTutorial();
            PlayActionTAnime("Up");
        }
        else if (tutorialType == 2 && tutorialStage == 3)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 3 && tutorialStage == 1)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 3 && tutorialStage == 2)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("NoHold");
            Director.showActionTutorial();
            PlayActionTAnime("Tap");
        }
        else if (tutorialType == 4 && tutorialStage == 1)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 4 && tutorialStage == 2)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Up");
        }
        else if (tutorialType == 4 && tutorialStage == 3)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 4 && tutorialStage == 4)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Hold");
            Director.showActionTutorial();
            PlayActionTAnime("Tap");
        }
        else if (tutorialType == 5 && tutorialStage == 1)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 5 && tutorialStage == 2)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Up"); ;
        }
        else if (tutorialType == 5 && tutorialStage == 3)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Tap"); ;
        }
        else if (tutorialType == 6 && tutorialStage == 1)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 6 && tutorialStage == 2)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Right"); ;
        }
        else if (tutorialType == 7 && tutorialStage == 1)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 7 && tutorialStage == 2)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Down"); ;
        }
        else if (tutorialType == 7 && tutorialStage == 3)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 7 && tutorialStage == 4)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Down2"); ;
        }
        else if (tutorialType == 8 && tutorialStage == 1)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Down"); ;
        }
        else if (tutorialType == 8 && tutorialStage == 2)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 8 && tutorialStage == 3)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Down2"); ;
        }
        else if (tutorialType == 9 && tutorialStage == 1)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Down"); ;
        }
        else if (tutorialType == 9 && tutorialStage == 2)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 10 && tutorialStage == 1)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 10 && tutorialStage == 2)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Left"); ;
        }
        else if (tutorialType == 10 && tutorialStage == 3)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Up2"); ;
        }
        else if (tutorialType == 10 && tutorialStage == 4)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Down3"); ;
        }
        else if (tutorialType == 11 && tutorialStage == 1)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 11 && tutorialStage == 2)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Left"); ;
        }
        else if (tutorialType == 11 && tutorialStage == 3)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Tap2"); ;
        }
        else if (tutorialType == 11 && tutorialStage == 4)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Up2"); ;
        }
        else if (tutorialType == 11 && tutorialStage == 5)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Down3"); ;
        }
        else if (tutorialType == 12 && tutorialStage == 1)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 12 && tutorialStage == 2)
        {
            Director.showActionTutorial();
            PlayActionTAnime("HoldUp"); ;
        }
        else if (tutorialType == 12 && tutorialStage == 3)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 13 && tutorialStage == 1)
        {
            Director.showActionTutorial();
            PlayActionTAnime("HoldUp"); ;
        }
        else if (tutorialType == 13 && tutorialStage == 2)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 14 && tutorialStage == 1)
        {
            Director.showActionTutorial();
            PlayActionTAnime("HoldDown"); ;
        }
        else if (tutorialType == 14 && tutorialStage == 2)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 14 && tutorialStage == 3)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 14 && tutorialStage == 4)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Left");
        }
        else if (tutorialType == 14 && tutorialStage == 5)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 15 && tutorialStage == 1)
        {
            Director.showActionTutorial();
            PlayActionTAnime("HoldDown"); ;
        }
        else if (tutorialType == 15 && tutorialStage == 2)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 15 && tutorialStage == 3)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 15 && tutorialStage == 6)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 16 && tutorialStage == 1)
        {
            Director.showActionTutorial();
            PlayActionTAnime("HoldRight"); ;
        }
        else if (tutorialType == 17 && tutorialStage == 1)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 17 && tutorialStage == 2)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Up");
        }
        else if (tutorialType == 17 && tutorialStage == 3)
        {
            Director.showActionTutorial();
            PlayActionTAnime("HoldRight"); ;
        }
        else if (tutorialType == 18 && tutorialStage == 1)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Left");
        }
        else if (tutorialType == 18 && tutorialStage == 2)
        {
            Director.showActionTutorial();
            PlayActionTAnime("HoldRight"); ;
        }
        else if (tutorialType == 18 && tutorialStage == 4)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Down2"); ;
        }
        else if (tutorialType == 19 && tutorialStage == 1)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 19 && tutorialStage == 2)
        {
            Director.showActionTutorial();
            PlayActionTAnime("HoldLeft"); ;
        }
        else if (tutorialType == 19 && tutorialStage == 3)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Up3"); ;
        }
        else if (tutorialType == 19 && tutorialStage == 4)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Down4"); ;
        }
        else if (tutorialType == 20 && tutorialStage == 1)
        {
            Director.showMoveTutorial();
            PlayMoveTAnime("Right");
        }
        else if (tutorialType == 20 && tutorialStage == 2)
        {
            Director.showActionTutorial();
            PlayActionTAnime("HoldLeft"); ;
        }
        else if (tutorialType == 20 && tutorialStage == 3)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Swipe"); ;
        }
        else if (tutorialType == 20 && tutorialStage == 4)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Up3"); ;
        }
        else if (tutorialType == 20 && tutorialStage == 5)
        {
            Director.showActionTutorial();
            PlayActionTAnime("Down4"); ;
        }

    }

    /// <summary>
    /// チュートリアルアニメの表示、非表示
    /// </summary>
    /// <param name="TouchFlag"></param>
    private void tutorialShowChange(bool TouchFlag)
    {
        if (!tutorialFlag) return;
        if(TouchFlag && tutorialAnimeFlag) //チュートリアルアニメひ非表示にする
        {
            tutorialAnimeFlag = false;
            Director.hideActionTutorial();
            Director.hideMoveTutorial();
        }
        else if(!TouchFlag && !tutorialAnimeFlag) //チュートリアルアニメを表示する
        {
            tutorialAnimeFlag = true;
            ShowTutorialAnime();
        }
    }

    private void tutorialMoveStopChange()
    {
        if (!tutorialFlag) return;
        if (!tutorialStopFlag) return;
        bool EnableTap = false;
        if (tutorialMoveLockStatas == MoveEnable.enable && getMoveKeyEnable()) EnableTap = true;
        if (tutorialMoveLockStatas == MoveEnable.up && JoyVec.y >= 0.5f) EnableTap = true;
        if (tutorialMoveLockStatas == MoveEnable.down && JoyVec.y <= -0.5f) EnableTap = true;
        if (tutorialMoveLockStatas == MoveEnable.right && JoyVec.x >= 0.5f) EnableTap = true;
        if (tutorialMoveLockStatas == MoveEnable.left && JoyVec.x <= -0.5f) EnableTap = true;

        if (tutorialMultiLock)
        {
            if (tutorialMoveLockStatas == MoveEnable.none && !getMoveKeyEnable()) EnableTap = true;
            if (EnableTap && !tutorialMultiMoveLock) tutorialMultiMoveLock = true;
            else if (!EnableTap && tutorialMultiMoveLock) tutorialMultiMoveLock = false;
        }
        else
        {
            if(EnableTap && tutorialStoping) //停止状態を解除
            {
                tutorialStoping = false;
                Time.timeScale = 1;
                if (tutorialMoveStopRelease)
                {
                    tutorialMoveStopRelease = false;
                    tutorialStopFlag = false;
                }
            }
            else if (!EnableTap && !tutorialStoping)
            {
                tutorialStoping = true;
                Time.timeScale = 0;
            }
        }
    }

    private void PlayMoveTAnime(string name)
    {
        if(Director.GetMoveAnimeIsActiove()) movePadAnimator.SetTrigger(name);
    }
    private void PlayActionTAnime(string name)
    {
        if (Director.GetActionAnimeIsActiove()) actionPadAnimator.SetTrigger(name);
    }

    private void tutorialTarget()
    {
        if (tutorialTargetCount == 0) return;
        if(tutorialTargetCount == Director.getTargetCount())
        {
            tutorialTargetCount = 0;
            GameDirector.TargetLock = true;
            tutorialStage += 1;
            tutorialPlay();
        }
    }

}
