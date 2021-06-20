using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using TMPro;
using UnityEngine.Video;
#if UNITY_IOS
using UnityEngine.iOS;
#else
using UnityEngine.Android;
#endif

//データ保存箇所　ゲームオーバー時　ゲームクリア時　パレット変更時　スキル習得時

public class GameDirector : MonoBehaviour
{
    //ゲームオブジェクトを取得
    [SerializeField] private Canvas ControllerWindow = null;       //メインゲーム画面のUI
    [SerializeField] private Canvas StatasWindow = null;           //ステータス画面のUI
    [SerializeField] private Canvas VideoWindow = null;            //動画再生用UI
    [SerializeField] private Image[] PadIcon = null;               //アクションパットのアイコン
    [SerializeField] private Sprite[] PaletteIconSprite = null;    //アクションパット/パレットのスプライト
    [SerializeField] private Sprite[] PaletteSprite = null;        //パレットのバックスプライト
    [SerializeField] private Image Pad = null;                     //パットのイメージ
    [SerializeField] private Image HPGauge = null;            //HPゲージのイメージ
    [SerializeField] private Image MPGauge = null;            //MPゲージのイメージ
    [SerializeField] private GameObject[] EventUI = null;          //イベントUI
    [SerializeField] private GameObject[] SkillUI = null;
    [SerializeField] private GameObject[] ShopUI = null;           //ショップUI
    [SerializeField] private GameObject BackButton = null;
    [SerializeField] private TextMeshProUGUI CoinText = null;
    [SerializeField] private Button GameOverExitB = null;
    [SerializeField] private Button ClearNext = null;
    [SerializeField] private Text CMCoinText = null;
    [SerializeField] private Button CMCoinButton = null;
    [SerializeField] private Button TweetButton = null;
    [SerializeField] private Text TextViewText = null;

#if UNITY_EDITOR
    private bool Editor = true;
#else
    private bool Editor = false;
#endif

    public static bool GamePlay = false;
    public static bool CmFlag = false;
    public static bool CmCoinFlag = false;
    public static int CmGetCoin = 0;
    //  private int CmN = 3;

    private float SpringReTime = 0.5f;
    private float SpringDelta = 0;

    [SerializeField] private VideoClip[] Movie = null;
    [SerializeField] private GameObject[] VideoUI = null;
    //                                     0  1  2  3  4  5  6    7   8   9  10  11  12  13  14  15  16  17  18  19  20  21  22  23  24  25
    //ビデオの開始番号　　　　　　　　　　　　　　　　　　　　　 Me　B1  B2  B3  FM  FM  FM  HJ  HJ  HJ  FB  FB  FB  FB  R1  R2  R3  TM  TM 
    private int[] VideoN = new int[]     { 0, 2, 3, 6, 7, 9, 11, 12, 13, 15, 17, 18, 20, 21, 22, 24, 26, 27, 28, 29, 32, 34, 35, 36, 39, 40};
    //ビデオの連数
    private int[] VideoCount = new int[] { 2, 1, 3, 1, 2, 2,  1,  1,  2,  2,  1,  2,  1,  1,  2,  2,  1,  1,  1,  3,  2,  1,  1,  3,  1,  1};             
    public static int DNumber = 0;
    private int VideoPage = 1;
    private bool SoundFlag = false;
    private VideoPlayer VPlayer;
    private Button[] VButton;

    [SerializeField] private GameObject BoardView = null;
    [SerializeField] private Image BoardImage = null;
    [SerializeField] private Sprite[] BoardSprites = null;
    public static int BoardNumber = 0;
    public static bool[] CanUseAction = new bool[9] {true ,true ,true, true, true, true, true, true ,true};    //アクションが使用可能の状態か 
    public static bool Player3DMode = true;

    [SerializeField] private Text[] StatasText = null;    //ステータスのテキストを管理
    [SerializeField] private GameObject[] LevelUpText = null;
    [SerializeField] private GameObject StatasExitButton = null;

    [SerializeField] private GameObject EnemyHpGauge = null;
    [SerializeField] private Image EnemyHpGaugeMater = null;
    [SerializeField] private Text EnemyHpGaugeName = null;
    private float EnemyGaugeViewTime = 3.0f;
    private float EnemyGaugeDelta = 0;
    private bool EnemyGaugeViewFlag = false;


    //ショップに使用する変数
    private int ShopNum = 0;                                    //商品ナンバー
    private int ShopPrice = 0;                                  //商品価格
    private GameObject ShopItem;                                //購入するアイテムのゲームオブジェクト

    //オブジェクトのスイッチアクション
    private static List<GameObject> ActionObj = new List<GameObject>();
    private static List<List<int>> AObjSwitchNum = new List<List<int>>();
    private static List<List<int>> StackSwitchNum = new List<List<int>>();

    public static void InputActionObj(GameObject obj,int[] SwitchNum)
    {
        ActionObj.Add(obj);
        List<int> SwitchList = new List<int>();
        List<int> SwitchList2 = new List<int>();
        for (int i = 0; i < SwitchNum.Length; i++)
        {
            SwitchList.Add(SwitchNum[i]);
            SwitchList2.Add(SwitchNum[i]);
        }
        AObjSwitchNum.Add(SwitchList);
        StackSwitchNum.Add(SwitchList2);
    }
    public static void PlayObjAction(int SwitchNum)
    {

        for (int i = 0; i < ActionObj.Count; i++)
        {
            //スイッチ番号の切り替え処理
            for (int j = 0; j < AObjSwitchNum[i].Count; j++)
            {
                if (StackSwitchNum[i][j] == SwitchNum)
                {
                    if(AObjSwitchNum[i][j] == 0)
                    {
                        AObjSwitchNum[i][j] = StackSwitchNum[i][j];
                    }
                    else
                    {
                        AObjSwitchNum[i][j] = 0;
                    }
                }    
            }

            //スイッチアクションを実行するかの確認(スイッチ番号がオール0で実行)
            bool SwithRun = true;
            for (int j = 0; j < AObjSwitchNum[i].Count; j++)
            {
                if (AObjSwitchNum[i][j] != 0) SwithRun = false;
            }

            if (SwithRun)
            {
                ActionObj[i].GetComponent<MoveObject2>().ChangeSwitch();
                AObjSwitchNum[i] = StackSwitchNum[i];
            }
        }

    }
    public static void ResetActionObj()
    {
        Debug.Log("リセットアクション");
        ActionObj.Clear();
        AObjSwitchNum.Clear();
        StackSwitchNum.Clear();
    }

    //セットアップおよび更新*******************************************************************************************
    //初回実行
    private void Awake()
    {
        GameManager.DirectorObj = gameObject;
        GameManager.playStageBGM();
        Player3DMode = true;

       // GameManager.resetGameData();
    }

    void Start()
    {
        StatasWindow.gameObject.SetActive(false);       //ステータス画面のUIを無効化    
        ChangeActionPad();                              //アクションパット、パレット変更を反映
        PlayerHP = GameManager.PlayerHp;                //プレイヤーのHPを読み込み
        PlayerMP = GameManager.PlayerMp;                //プレイヤーのMPを読み込み
        EnemyTargetFlag = false;                        //ターゲットフラグを無効化                        
        MagicType = 0;                                  //マジック未詠唱状態
        GameManager.saveGameData();                     //プレイヤーデータの保存
        CoinUpdate();                                   //コインの表示を更新

        VPlayer = VideoUI[0].GetComponent<VideoPlayer>();
        VPlayer.SetTargetAudioSource(0, GameManager.getBGMAudioSoruce());

        VButton = new Button[] { VideoUI[4].GetComponent<Button>(), VideoUI[5].GetComponent<Button>(), VideoUI[6].GetComponent<Button>() };
        GameManager.SpringFlag = true;

        Invoke("LoadViewClose", 2.5f);
    }

    
    private void LoadViewClose()
    {
        GameManager.LoadViewClose();
        GamePlay = true;
    }
    

    //0.02秒ごとに実行
    private void FixedUpdate()
    {
        //MP自然回復設定************************************************************************
        MpDelta += 0.02f;
        if (MpDelta >= MpRTime)
        {
            MpDelta = 0;
            float MPow = GameManager.PlayerMPow;
            if (GameManager.eventCount[10] >= 1) MPow *= 1.5f;
            PlayerMP += MPow / 40f;
        }

        if (PlayerController.PlayerPos.y < -3 && GamePlay) PlayerHP -= 10;

        if (EnemyGaugeViewFlag)
        {
            EnemyGaugeDelta += 0.02f;
            if(EnemyGaugeDelta >= EnemyGaugeViewTime)
            {
                EnemyGaugeViewFlag = false;
                EnemyHpGauge.SetActive(false);
                EnemyGaugeDelta = 0;
            }
        }

        if (!GameManager.SpringFlag)
        {
            SpringDelta += 0.02f;
            if (SpringDelta >= SpringReTime)
            {
                SpringDelta = 0;
                GameManager.SpringFlag = true;
            }
        }
    }
    //セットアップおよび更新*********************************************************************************************


    //プレイヤーのステータス関連************************************************************************************************
    public int PlayerHP {
        set
        {
            GameManager.PlayerHp = value;
            GameManager.saveGameData();
            if (GameManager.PlayerHp > GameManager.PlayerMaxHp) GameManager.PlayerHp = GameManager.PlayerMaxHp;
            if (GameManager.PlayerHp < 0)
            {
                GameManager.PlayerHp = 0;
            }
            ChangeHPGauge();
            if (GameManager.PlayerHp == 0)
            {
                GamePlay = false;
                PlayerController.PlayerDamgagFlag = false;
                Time.timeScale = 0.2f;
                Invoke("GameOver", 0.4f);
            }
        }
        get { return GameManager.PlayerHp; }
    }
    public float PlayerMP {
        set
        {
            GameManager.PlayerMp = value;
            GameManager.saveGameData();
            if (GameManager.PlayerMp > GameManager.PlayerMaxMp) GameManager.PlayerMp = GameManager.PlayerMaxMp;
            if (GameManager.PlayerMp < 0) GameManager.PlayerMp = 0;
            ChangeMPGauge();
        }
        get { return GameManager.PlayerMp; }
    }
    //MP自然回復設定
    private float MpRTime = 1.0f;
    private float MpDelta = 0f;
    private void ChangeHPGauge() { HPGauge.fillAmount = (float)GameManager.PlayerHp / GameManager.PlayerMaxHp; }
    private void ChangeMPGauge() { MPGauge.fillAmount = GameManager.PlayerMp / GameManager.PlayerMaxMp; }

    //HP、MP全回復
    public void AllRecver(bool Save)
    {
        PlayerHP = GameManager.PlayerMaxHp;
        PlayerMP = GameManager.PlayerMaxMp;
        if(Save) GameManager.saveGameData();
    }

    /// <summary>
    /// ステータス画面の更新
    /// </summary>
    private void UpdataStatasView()
    {
        StatasText[1].text = "HP:    " + GameManager.PlayerHp.ToString() + "    /    " + GameManager.PlayerMaxHp.ToString();
        StatasText[2].text = "MP:    " + GameManager.PlayerMp.ToString("f2") + "    /    " + GameManager.PlayerMaxMp.ToString("f2");

        string PowText = "攻撃力:  " + GameManager.PlayerPow.ToString();
        string MPowText = "魔力:  " + GameManager.PlayerMPow.ToString();
        string DefText = "防御力:  " + GameManager.PlayerDef.ToString();
        if (GameManager.eventCount[9] >= 1) PowText += " + " + GameManager.PlayerPow / 2;
        if (GameManager.eventCount[10] >= 1) MPowText += " + " + GameManager.PlayerMPow / 2;
        if (GameManager.eventCount[11] >= 1) DefText += " + " + GameManager.PlayerDef / 2;
        StatasText[3].text = PowText;
        StatasText[4].text = MPowText;
        StatasText[5].text = DefText;

        StatasText[6].text = "Level:  " + GameManager.PlayerLv.ToString();
        if(GameManager.PlayerLv < GameManager.MaxPlayerLevel)
        {
            StatasText[7].text = (GameManager.PlayerMaxExp - GameManager.PlayerExp).ToString() + " exp";
        }
        else
        {
            StatasText[7].text = "--- exp";
        }
        
         
        string StateText = "状態:    ";
        if (GameManager.eventCount[7] == 0) StateText += "健康";
        if (GameManager.eventCount[9] >= 1) StateText += "  攻1.5(" + (GameManager.eventCount[9] - 1) + ")";
        if (GameManager.eventCount[10] >= 1) StateText += "  魔1.5(" + (GameManager.eventCount[10] - 1) + ")";
        if (GameManager.eventCount[11] >= 1) StateText += "  防1.5(" + (GameManager.eventCount[11] - 1) + ")";

        StatasText[0].text = StateText;
    }

    /// <summary>
    /// 経験値獲得処理
    /// </summary>
    /// <param name="Eexp"></param>
    /// <param name="Ecoin"></param>
    public void GetExp(int Eexp, int Ecoin)
    {
        GameManager.PlayerExp += Eexp;
        GameManager.PlayerCoin += Ecoin;
        CoinUpdate();
        LevelUPCheack();
    }

    /// <summary>
    /// レベルアップの確認と処理
    /// </summary>
    private void LevelUPCheack()
    {
        //レベルアップ処理
      //  Time.timeScale = 0;
        if (GameManager.PlayerLv >= GameManager.MaxPlayerLevel) return;

        if (GameManager.PlayerExp >= GameManager.PlayerMaxExp)
        {
            GameManager.PlayerLv += 1;
            GameManager.PlayerExp -= GameManager.PlayerMaxExp;

            int[] nextStatas = GameManager.getPlayerStatas(GameManager.PlayerLv);
            GameManager.PlayerMaxExp = nextStatas[0];
            LevelUpText[5].SetActive(true);
            if (nextStatas[1] > GameManager.PlayerMaxHp)
            {
                GameManager.PlayerMaxHp = nextStatas[1];
                PlayerHP = PlayerHP;
                LevelUpText[0].SetActive(true);
            }
            if (nextStatas[2] > GameManager.PlayerMaxMp)
            {
                GameManager.PlayerMaxMp = nextStatas[2];
                PlayerMP = PlayerMP;
                LevelUpText[1].SetActive(true);
            }
            if (nextStatas[3] > GameManager.PlayerPow)
            {
                GameManager.PlayerPow = nextStatas[3];
                LevelUpText[2].SetActive(true);
            }
            if (nextStatas[4] > GameManager.PlayerMPow)
            {
                GameManager.PlayerMPow = nextStatas[4];
                LevelUpText[3].SetActive(true);
            }
            if (nextStatas[5] > GameManager.PlayerDef)
            {
                GameManager.PlayerDef = nextStatas[5];
                LevelUpText[4].SetActive(true);
            }
            GameManager.saveGameData();

            StatasExitButton.SetActive(false);
            UpdataStatasView();
            StatasWindow.gameObject.SetActive(true);
            ControllerWindow.gameObject.SetActive(false);
            Time.timeScale = 0;
            GameManager.playSystemSE(8);
        }
        else
        {
            GameManager.playStageSE(2);
            GameManager.saveGameData();
        }     
    }

    /// <summary>
    /// 敵のHPゲージの表示と更新
    /// </summary>
    /// <param name="EName"></param>
    /// <param name="EMaxHp"></param>
    /// <param name="EHp"></param>
    public void viewEnemyGauge(string EName, float EMaxHp, float EHp)
    {
        EnemyHpGaugeName.text = EName;
        EnemyHpGaugeMater.fillAmount = EHp / EMaxHp;
        EnemyGaugeDelta = 0;
        if (!EnemyGaugeViewFlag)
        {
            EnemyGaugeViewFlag = true;
            EnemyHpGauge.SetActive(true);
        }        
    }

    //ターゲットに関する関数およびプロパティ************************************************************************************
    public static bool EnemyTargetFlag = true;
    public static int MagicType = 0;      //0:なし　1:ファイア  2:サンダー
    private static List<GameObject> TargetObject = new List<GameObject>();
    public static void AddTarget(GameObject TargetObj) { TargetObject.Add(TargetObj); }
    public static void ChangeTarget(GameObject TargetObj)
    {
        if (TargetObject?.Count > 0)//オブジェクトが存在する場合
        {
            if(TargetObject.Count == 1)
            {
                if(TargetObject[0] != null)
                {
                    if (TargetObject[0].GetComponent<EnemyHitController>() != null) TargetObject[0].GetComponent<EnemyHitController>().StopTarget();
                    if (TargetObject[0].GetComponent<SwitchReceiver>() != null) TargetObject[0].GetComponent<SwitchReceiver>().StopTarget();
                    if (TargetObject[0].GetComponent<TorchSwitch>() != null) TargetObject[0].GetComponent<TorchSwitch>().StopTarget();
                    if (TargetObject[0].GetComponent<ChirdTorchSwitch>() != null) TargetObject[0].GetComponent<ChirdTorchSwitch>().StopTarget();
                }
            }
            TargetObject.Clear();
            TargetObject.Add(TargetObj);
        }
        else TargetObject.Add(TargetObj);
    }
    public static void RemoveTarget(GameObject TargetObj)
    {
        TargetObject.Remove(TargetObj);
    }
    public static void RemoveAllTarget() { if (TargetObject?.Count > 0) TargetObject.Clear(); }
    public static bool isTarget()
    {
        bool isObj = false;
        if (TargetObject?.Count > 0) isObj = true;
        return isObj;
    }
    public static GameObject GetTarget() { return TargetObject[0]; }
    public static GameObject[] GetTargets()
    {
        GameObject[] TargetObj = new GameObject[TargetObject.Count];
        for (int i = 0; i < TargetObject.Count; i++) { TargetObj[i] = TargetObject[i]; }
        return TargetObj;
    }
    //ターゲットに関する関数およびプロパティ****************************************************************************
  

    //コイン関係*****************************************************************************************************
    public void CoinUpdate()
    {
        CoinText.text = GameManager.PlayerCoin.ToString();
    }


    //コイン関係*****************************************************************************************************


    //画面の切り替え*******************************************************************************************************
    //ステータスを表示
    public void StatasButton()
    {
        GameManager.playSystemSE(0);
        UpdataStatasView();
        StatasWindow.gameObject.SetActive(true);
        ControllerWindow.gameObject.SetActive(false);
        Time.timeScale = 0;

        StatasExitButton.SetActive(true);
        for(int i = 0; i < LevelUpText.Length; i++)
        {
            LevelUpText[i].SetActive(false);
        }
       // LevelUPCheack();
    }
    //ステータスを閉じる
    public void StatasClose()
    {
        if (GameManager.PlayerExp >= GameManager.PlayerMaxExp)
        {
            LevelUPCheack();
        }
        else
        {
            GameManager.playSystemSE(3);
            StatasWindow.gameObject.SetActive(false);
            ControllerWindow.gameObject.SetActive(true);
            Time.timeScale = 1;
        }
    }

    //ExitViewのボタン処理*****************************************************
    /// <summary>
    /// ExitViewの表示
    /// </summary>
    public void OpenExitView()
    {
        GameManager.playSystemSE(2);
        StatasWindow.gameObject.SetActive(false);
        EventUI[6].SetActive(true);
    }

    /// <summary>
    /// ExitViewを閉じる
    /// </summary>
    public void CloseExitView()
    {
        GameManager.playSystemSE(3);
        StatasWindow.gameObject.SetActive(true);
        EventUI[6].SetActive(false);
    }

    /// <summary>
    /// SS画面に戻るかの確認をするビューを表示
    /// </summary>
    public void OpenExitStageView()
    {
        GameManager.playSystemSE(2);
        EventUI[6].SetActive(false);
        EventUI[4].SetActive(true);
    }

    /// <summary>
    /// SS画面に戻るかの確認をするビューを閉じる
    /// </summary>
    public void CloseExitStageView()
    {
        GameManager.playSystemSE(3);
        EventUI[4].SetActive(false);
        StatasWindow.gameObject.SetActive(true);
    }

    /// <summary>
    /// SS画面に戻る
    /// </summary>
    public void GoStageSelect()
    {
        EventUI[4].SetActive(false);
        GamePlay = false;
        Time.timeScale = 1;
        GameManager.GoStageSelect();
    }


    /// <summary>
    /// リスタート確認ビューを表示
    /// </summary>
    public void OpenReStartView()
    {
        GameManager.playSystemSE(2);
        EventUI[6].SetActive(false);
        EventUI[5].SetActive(true);
    }

    /// <summary>
    /// リスタート確認ビューを閉じる
    /// </summary>
    public void CloseReStartView()
    {
        GameManager.playSystemSE(3);
        EventUI[5].SetActive(false);
        StatasWindow.gameObject.SetActive(true);
    }

    public void GoReStart()
    {
        GameManager.playSystemSE(0);
        EventUI[5].SetActive(false);
        GamePlay = false;
        Time.timeScale = 1;
        GameManager.GoStage(GameManager.StageNum);
    }

    //**************未使用
    public void OpenExitFieldView()
    {
        GameManager.playSystemSE(2);
        EventUI[6].SetActive(false);
        EventUI[3].SetActive(true);
    }
    public void CloseExitFieldView()
    {
        GameManager.playSystemSE(3);
        EventUI[3].SetActive(false);
        StatasWindow.gameObject.SetActive(true);
    }
    //**************未使用




   /*
    public void SkillVideo()
    {
        GameManager.playSystemSE(2);
        EventUI[0].SetActive(false);
        SkillUI[2].SetActive(false);
        SkillUI[3].SetActive(false);

        StartDescription();
    }
    */

    public void StartDescription(bool soundFlag)
    {
        //Debug.Log("説明スタート");
        SoundFlag = soundFlag;
        Time.timeScale = 0;
        GameManager.playSystemSE(2);
        VideoPage = 1;
        VideoWindow.gameObject.SetActive(true);
        DescriptionChange();
    }
    public void EndDescription()
    {
        GameManager.playSystemSE(3);
        if(SoundFlag) GameManager.restartBGM();
        VButton[0].interactable = false;
        VButton[1].interactable = false;
        VButton[2].interactable = false;
        VideoWindow.gameObject.SetActive(false);
        Time.timeScale = 1;
    }
    public void NextDescription()
    {
        VideoPage += 1;
        GameManager.playSystemSE(1);
        DescriptionChange();
    }
    public void BackDescription()
    {
        VideoPage -= 1;
        GameManager.playSystemSE(1);
        DescriptionChange();
    }

    private void DescriptionChange()
    {
        //動画の読み込み
        if (SoundFlag)
        {
            GameManager.stopBGM();
            VPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        }
        else
        {
            VPlayer.audioOutputMode = VideoAudioOutputMode.None;
        }

        VPlayer.source = VideoSource.VideoClip;
        VPlayer.clip = Movie[VideoN[DNumber] + VideoPage - 1];
        
        

        VPlayer.Play();

        //タイトルテキスト
        string[] TitleText = new string[]
        {
             "移動（2Dモード）"
            ,"移動（3Dモード）"
            ,"ジャンプ"
            ,"通常攻撃"
            ,"ダッシュ攻撃"
            ,"ジャンプ攻撃"
            ,"回避"
            ,"ステータス画面"
            ,"ボール1"
            ,"ボール2"
            ,"ボール3"
            ,"炎魔法1"
            ,"炎魔法2"
            ,"炎魔法3"
            ,"ハイジャンプ1"
            ,"ハイジャンプ2"
            ,"ハイジャンプ3"
            ,"フロートボール1"
            ,"フロートボール2"
            ,"フロートボール3"
            ,"フロートボール4"
            ,"神速ダッシュ1"
            ,"神速ダッシュ2"
            ,"神速ダッシュ3"
            ,"サンダー1"
            ,"サンダー2"
        };
        VideoUI[1].GetComponent<Text>().text = TitleText[DNumber];

        //説明文
        string[] DText = new string[]
        {
             //2D移動(1/2)
            "プレイ準備\n" +
            "・端末を横に向ける.\n" +
            "・右図のように親指2本で操作する."
            ,//2D移動(2/2)
            "・画面左をタッチするとジョイスティックが表示される.\n" +                           
            "・2Dモード時は左右にのみ移動ができる.\n" +
            "・奥行方向や手前方向には移動ができない."
            ,//3D移動
            "・特定の物に接触すると3Dモードになる.\n" +                           
            "・3Dモードは全方向に動くことができる."
            ,//ジャンプ(1/3)
            "・画面右をタッチすると、アクションパレッドが表示される.\n" +                         
            "・アクションパレッドは指を離した場所のアクションが使用される.\n" +
            "・パレッド上入力がジャンプである."
            ,//ジャンプ(2/3)
            "・上にフリックすることで素早くジャンプすることができる."
            ,//ジャンプ(3/3)
            "・移動スティックと組み合わせることで様々な方向にジャンプができる."
            ,//通常攻撃
            "・停止状態で攻撃アクションを使用すると通常攻撃が使用できる.\n" +
            "・パレッドの真ん中が攻撃アクションである.\n" +
            "・タップすることで素早く発動ができる."
            ,//ダッシュ攻撃(1/2)
            "・ダッシュ中に攻撃アクションを使用するとダッシュ攻撃ができる."
            ,//ダッシュ攻撃(2/2)
            "・ダッシュ攻撃は連続で使用できない."
            ,//ジャンプ攻撃(1/2)
            "・ジャンプ中に攻撃アクションを使用するとジャンプ攻撃が使用できる."
            ,//ジャンプ攻撃(2/2)
            "・ジャンプ攻撃とダッシュ攻撃を組み合わせることで隙なく攻撃ができる."
            ,//回避
            "・回避アクションで炎や敵を通り抜けることができる.\n" +
            "・回避アクションはパレッド右入力である.\n" +
            "・右にフリックすることで素早く発動ができる."
            ,//ステータス画面
            "・画面左下の猫マークをタップするとステータス画面が開く."
            ,//ボール1(1/2)
            "・パレッド下入力でボール状態になることができる.\n" +
            "・ボール状態の解除もバレット下で行うことができる.\n" +
            "・ボール状態は狭いすき間でも通ることができる."
            ,//ボール1(2/2)
            "・下フリックで素早く切り替えることができる."
            ,//ボール2(1/2)
            "・ボール状態で宝石に触れることで光の道を通ることができる.\n" +
            "・移動スティック右：紫→青の宝石方向に移動.\n" +
            "・移動スティック左：青→紫の宝石方向に移動."
            ,//ボール2(2/2)
            "光の道の注意点\n" +
            "・徐々にしか減速や加速ができない.\n" +
            "・光の道を抜けると少しの時間再使用ができない."
            ,//ボール3
            "・ボール状態で特定のブロックに触れるとバウンドする.\n" +
            "・ボール状態でしかバウンドしないので気を付けよう."
            ,//ファイア1(1/2)
            "・パレッド左入力で詠唱状態になることができる.\n" +
            "・詠唱状態中は移動することができない.\n" +
            "・パレッド下で詠唱状態を解除することができる."
            ,//ファイア1(2/2)
            "・詠唱状態中にパレッド上で炎魔法が使用できる.\n" +
            "・魔法を使用するとMPが少し消費される.\n" +
            "・MPは敵を攻撃アクションで攻撃することで回復ができる.\n" +
            "・MPは少しずつなにもしなくても回復する."
            ,//ファイア2
            "・詠唱状態中にスイッチなどのオブジェクトをタップするとターゲティングができる.\n" +
            "・ターゲティング中に炎魔法を使用することで,そのオブジェクトに対して魔法を飛ばすことができる."
            ,//ファイア3
            "・敵に対しても詠唱中にターゲティングができる."
            ,//ハイジャンプ1(1/2)
            "・アクションパレッドは特定の方向に入力し続けることでチャージができる.\n" +
            "・上入力でチャージするとハイジャンプが使用できる."
            ,//ハイジャンプ1(2/2)
            "チャージの性質1\n" +
            "・チャージが完了するまでに移動スティックを使用するとチャージが中断される."
            ,//ハイジャンプ2(1/2)
            "チャージの性質2\n" +
            "・移動スティックを使わなけらば,落下中やジャンプ中でもチャージができる."
            ,//ハイジャンプ2(2/2)
            "チャージの性質3\n" +
            "・チャージが完了すれば移動してもチャージ状態がキープできる.\n" +
            "・チャージ完了後にパレッドを他の方向に入力するとチャージがキャンセルされる."
            ,//ハイジャンプ3
            "・チャージの性質を上手く使いこなそう."
            ,//フロートボール1
            "・パレッドのを下入力でチャージするとフロートボールが使用できる.\n" +
            "・フロートボールは空中で急な方向転換ができる.\n" +
            "・フロートボールは急な斜面を駆け上がることができる."
            ,//フロートボール2
            "・フロートボール中に垂直な壁方向に移動スティックを入力することで空中にとどまることができる."
            ,//フロートボール3(1/3)
            "・フロートボールで特別な曲面ブロックを駆け上がることで,オーバーフロート状態になることができる.\n" +
            "・オーバーフロート中は浮遊能力が上昇する.\n" +
            "・一定の高さを超えると強制的にオーバーフロートが解除される."
            ,//フロートボール3(2/3)
            "・オーバーフロート中に曲面ブロックを使うことで奥行方向に移動ができる."
            ,//フロートボール3(3/3)
            "・奥行方向の移動に失敗すると大ダメージを受けるので注意しよう." 
            ,//フロートボール4(1/2)
            "・曲面ブロックに様々な形がある." 
            ,//フロートボール4(2/2)
            "・オーバーフロート状態には,止まらずに一気に駆け上がらなけらばならない."
            ,//神速ダッシュ1
            "・パレッドを右入力でチャージすると神速ダッシュができる\n" +
            "・神速ダッシュは障害物に接触すると強制的に終了する."
            ,//神速ダッシュ2
            "・神速ダッシュ中は無敵状態である.\n" +
            "・神速ダッシュは炎や敵などを突っ切ることができる."
            ,//神速ダッシュ3(1/3)
            "・神速ダッシュ中にアクションパレッド下入力で解除することができる."
            ,//神速ダッシュ3(2/3)
            "・神速ダッシュは急停止できないので,行き過ぎて落下する危険性がある."
            ,//神速ダッシュ3(3/3)
            "・神速ダッシュでトゲなどの障害物に衝突するとダメージをくらう."
            ,//サンダー1
            "・パレッドの左入力でチャージすると雷魔法の詠唱状態になれる.\n" +
            "・ファイア魔法と同様に詠唱中に上入力で魔法使用,下入力で詠唱解除である.\n" +
            "・雷魔法は自分の周囲広範囲を攻撃する.\n" +
            "・雷魔法は敵の防御力貫通効果があるため,硬い敵に有効である\n" +
            "・MPの消費量が多いため,使用するタイミングに注意しよう." 
            ,//サンダー2
            "・雷魔法詠唱中は複数の敵やスイッチなどのオブジェクトをターゲティングできる.\n" +
            "・スワイプによりオブジェクトを触ることでもターゲティングができる.\n" +
            "・スワイプではターゲティングの解除はできない.\n" +
            "・複数の敵をターゲティングすることで一斉攻撃が可能である."
        };
        VideoUI[2].GetComponent<Text>().text = DText[VideoN[DNumber] + VideoPage - 1];

        //ボタンの表示
        if (VideoPage != VideoCount[DNumber] && VideoCount[DNumber] >= 2) VButton[0].interactable = true; else VButton[0].interactable = false;
        if (VideoPage >= 2) VButton[1].interactable = true; else VButton[1].interactable = false;
        if (VideoPage == VideoCount[DNumber] ) VButton[2].interactable = true; else VButton[2].interactable = false;

        //ページ番号の変更
        VideoUI[3].GetComponent<Text>().text = VideoPage.ToString() + "/" + VideoCount[DNumber].ToString();

    }

    public void SkipSkillVideo()
    {
        GameManager.playSystemSE(3);
        EventUI[0].SetActive(false);
        SkillUI[2].SetActive(false);
        SkillUI[3].SetActive(false);
        Time.timeScale = 1;
    }

    public void SkillUIBack()
    {
        GameManager.playSystemSE(3);
        EventUI[0].SetActive(false);
        SkillUI[1].SetActive(false);
        Time.timeScale = 1;
      //  LearnSkillUse();
        PlayerHP = PlayerHP;
        PlayerMP = PlayerMP;
    }

    public void OpenContinueUI()
    {
        GameManager.playSystemSE(2);
        EventUI[1].SetActive(true);
    }

    //続きから再開する
    /*
    public void ContinueYes1()
    {
        CmCount += 1;
        if(CmCount == CmN)
        {
            CmCount = 0;
            CmFlag = true;
        }
        Time.timeScale = 1;
        SceneCon.MoveStage();
    }
    */

    public void ContinueNo1()
    {
        GameManager.playSystemSE(2);
        EventUI[1].SetActive(false);
        EventUI[2].SetActive(true);
    }

    /*
    public void ContinueYes2()
    {
     //   GameStatas.StageStartNum[GameStatas.StageNum] = 0;
     //   GameStatas.SceneNum[GameStatas.StageNum] = 0;
        Time.timeScale = 1;
        SceneCon.MoveStage();
    }
    */

    public void ContinueNo2()
    {
        GameManager.playSystemSE(3);
        EventUI[2].SetActive(false);
        Time.timeScale = 1;
    }

    //ボードマップに関する記述
    public void OpenBoardMap()
    {
        Time.timeScale = 0;
        GameManager.playSystemSE(2);
        BoardImage.sprite = BoardSprites[BoardNumber];
        BoardView.SetActive(true);
    }

    public void CloseBoardMap()
    {
        BoardView.SetActive(false);
        Time.timeScale = 1;
        GameManager.playSystemSE(3);
    }


    //ショップボードに関する記述
    public void OpenShopBoard()
    {
        Time.timeScale = 0;
        GameManager.playSystemSE(2);
        if (GameManager.eventFlag[0]) CMCoinButton.interactable = false; else CMCoinButton.interactable = true;
        if (GameManager.eventFlag[1]) TweetButton.interactable = false; else TweetButton.interactable = true;
        EventUI[16].SetActive(true);
    }
    public void CloseShopBoard()
    {
        EventUI[16].SetActive(false);
        EventUI[17].SetActive(false);
        EventUI[18].SetActive(false);
        EventUI[19].SetActive(false);
        EventUI[20].SetActive(false);
        Time.timeScale = 1;
        GameManager.playSystemSE(3);
    }

    public void OpneCMCoinTextView()
    {
        EventUI[17].SetActive(true);
        GameManager.playSystemSE(2);

        CMCoinButton.interactable = false;
        TweetButton.interactable = false;

    }
    public void CMCoinPlus()
    {
        EventUI[17].SetActive(false);
        GameManager.playSystemSE(2);
        CmCoinFlag = true;
        CmGetCoin = 0;
     //   GameManager.cmShow();
    }
    public void OpenCmCoinGetView()
    {
        GameManager.playSystemSE(2);
        CMCoinText.text = CmGetCoin + "コイン獲得しました";
        CoinUpdate();
        GameManager.saveGameData();
        
        EventUI[18].SetActive(true);
    }
    public void CloseCmCoinGetView()
    {
        EventUI[18].SetActive(false);
        GameManager.playSystemSE(3);

        if (GameManager.eventFlag[0]) CMCoinButton.interactable = false; else CMCoinButton.interactable = true;
        if (GameManager.eventFlag[1]) TweetButton.interactable = false; else TweetButton.interactable = true;
    }
    public void OpneNoCm()
    {   
        OpenTextView("広告がありません");
    }

    private void OpenTextView(string text)
    {
        GameManager.playSystemSE(2);
        TextViewText.text = text;
        EventUI[19].SetActive(true);
    }

    public void CloseTextView()
    {
        EventUI[19].SetActive(false);
        GameManager.playSystemSE(3);
    }

    public void OpenTweetConView()
    {
        EventUI[20].SetActive(true);
        GameManager.playSystemSE(2);

        CMCoinButton.interactable = false;
        TweetButton.interactable = false;
    }

    public void CloseTweetConView()
    {
        EventUI[20].SetActive(false);
    }

    public void CloseTweetConView2()
    {
        EventUI[20].SetActive(false);
        GameManager.playSystemSE(3);

        if (GameManager.eventFlag[0]) CMCoinButton.interactable = false; else CMCoinButton.interactable = true;
        if (GameManager.eventFlag[1]) TweetButton.interactable = false; else TweetButton.interactable = true;
    }

    public void TweetSuc()
    {
        GameManager.eventFlag[1] = true;
        TweetButton.interactable = false;
        if (!GameManager.eventFlag[0]) CMCoinButton.interactable = true;
        int PlusCoins = 1000;
        if (GameManager.eventFlag[0]) PlusCoins *= 100;
        GameManager.eventCount[2] += PlusCoins;
        CoinUpdate();
        GameManager.saveGameData();

        OpenTextView("ツイートが完了しました\n" + PlusCoins + "コインゲット！");
    }

    public void TweetFaild()
    {
        OpenTextView("ツイートが失敗しました");
    }

    //ステージクリア
    public void StageClear()
    {
        GamePlay = false;
        Time.timeScale = 0;
        EventUI[9].SetActive(true);
        GameManager.stopBGM();
        GameManager.playStageSE(4);
  
        int SN = GameManager.StageNum;

        //レビュー誘導(簡易版)
        if(SN % 10 == 0)
        {
            //IOSの場合の処理
#if UNITY_IOS
            if (Device.RequestStoreReview())
            {

            }

            //Androidやそれ以外の場合の処理
#else

#endif
        }

    }

    public void Review()
    {
        EventUI[14].SetActive(false);
        ClearNext.interactable = true;
#if UNITY_IOS
        string url = "itms-apps://itunes.apple.com/jp/app/id1506407180?mt=8&action=write-review";
#else
		string url = "market://details?id=XXXXXXXX";
#endif
        Application.OpenURL(url);
    }

    public void NoReview()
    {
        GameManager.playSystemSE(3);
        EventUI[14].SetActive(false);
        ClearNext.interactable = true;
    }

    public void GoTitle()
    {
        GameManager.playSystemSE(0);
        Time.timeScale = 1;
        GameManager.GoStageSelect();
    }

    public void OpenTitleView()
    {
        GameManager.playSystemSE(2);
        EventUI[6].SetActive(false);
        EventUI[15].SetActive(true);
    }

    public void CloseTitleView()
    {
        GameManager.playSystemSE(3);
        EventUI[15].SetActive(false);
        StatasWindow.gameObject.SetActive(true);
    }

    /// <summary>
    /// ゲームオーバービューの表示
    /// </summary>
    private void GameOver()
    {
        Time.timeScale = 0;
        EventUI[8].SetActive(true);
        GameManager.stopBGM();
        GameManager.playStageSE(3);
    }

    //ショップの購入処理
    public void OpenShopUI(int shopN, int value,GameObject item)
    {
        Time.timeScale = 0;
        GameManager.playSystemSE(2);

        ShopNum = shopN;
        ShopPrice = value;
        ShopItem = item;

        string[] ItemName = new string[]  
        {"回復薬 小","回復薬 中","回復薬 大","力の薬","魔力の薬","防御の薬","魔吸の神薬","力の神薬"};
        string[] ItemDiscription = new string[]
            {"HPを10回復させる薬"
            ,"HPを50回復させる薬"
            ,"HPを100,MPを全回復させる薬"
            ,"攻撃力を1.5倍にする薬(5ターン分)"
            ,"魔力を1.5倍にする薬(5ターン分)"
            ,"防御力を1.5倍にする薬(5ターン分)"
            ,"魔力回復量を永続的に増加させる神薬"
            ,"攻撃力を永続的に増加させる神薬"};

        ShopUI[0].GetComponent<Text>().text = ItemName[ShopNum - 1];
        ShopUI[1].GetComponent<Text>().text = ShopPrice.ToString();
        ShopUI[2].GetComponent<Text>().text = ItemDiscription[ShopNum - 1];

        EventUI[10].SetActive(true);
    }

    public void CloseShopUI()
    {
        GameManager.playSystemSE(3);
        EventUI[10].SetActive(false);
        Time.timeScale = 1;
    }

    public void PurchaseItem()
    {
        GameManager.playSystemSE(2);
        EventUI[10].SetActive(false);
        if(ShopPrice > GameManager.eventCount[2])
        {
            EventUI[12].SetActive(true);
        }
        else
        {
            bool PFlag = true;
            if ( (ShopNum == 1 || ShopNum == 2) && PlayerHP == GameManager.PlayerMaxHp) PFlag = false;
            if (ShopNum == 3 && PlayerHP == GameManager.PlayerMaxHp && PlayerMP == GameManager.PlayerMaxMp) PFlag = false;
            if(ShopNum == 4 && GameManager.eventCount[9] == 6) PFlag = false;
            if (ShopNum == 5 && GameManager.eventCount[10] == 6) PFlag = false;
            if (ShopNum == 6 && GameManager.eventCount[11] == 6) PFlag = false;
            if (PFlag) EventUI[11].SetActive(true); else EventUI[13].SetActive(true);
        }
    }

    public void PurchaseYes()
    {
        EventUI[11].SetActive(false);
        GameManager.playSystemSE(5);
        GameManager.eventCount[2] -= ShopPrice;
        CoinUpdate();
        if (ShopNum == 1)
        {
            PlayerHP += 10;
        }
        if (ShopNum == 2)
        {
            PlayerHP += 50;
        }
        if (ShopNum == 3)
        {
            PlayerHP += 100;
            PlayerMP = GameManager.PlayerMaxMp;
            Destroy(ShopItem);
        }
        if (ShopNum == 4)
        {
            GameManager.eventCount[9] = 6;
            Destroy(ShopItem);       
        }
        if (ShopNum == 5)
        {
            GameManager.eventCount[10] = 6;
            Destroy(ShopItem);
        }
        if (ShopNum == 6)
        {
            GameManager.eventCount[11] = 6;
            Destroy(ShopItem);
        }
        Time.timeScale = 1;
        GameManager.saveGameData();
        CoinUpdate();
    }

    public void PurchaseNo()
    {
        GameManager.playSystemSE(3);
        EventUI[11].SetActive(false);
        Time.timeScale = 1;
    }

    public void NoMoneyReturn()
    {
        GameManager.playSystemSE(3);
        EventUI[12].SetActive(false);
        Time.timeScale = 1;
    }

    public void NoItemReturn()
    {
        GameManager.playSystemSE(3);
        EventUI[13].SetActive(false);
        Time.timeScale = 1;
    }

    public void RetryButton()
    {
        Time.timeScale = 1;
        GameManager.GoStage(GameManager.StageNum);
    }

    public void ExitButton()
    {
        Time.timeScale = 1;
        GameManager.GoStageSelect();
    }

    //コントローラのON,OFF
    public void ControllerActive(bool isEnable) { if(isEnable) ControllerWindow.gameObject.SetActive(true); else ControllerWindow.gameObject.SetActive(false); }
    //画面の切り替え*******************************************************************************************************



    //アクションパット、パレット関連***************************************************************************************
    //アクションパッドの変更

    private int[] setAction = new int[5] { 0, 1, 4, 2, 6};
    public int[] PaletteNumber
    {
        set
        {
            setAction = value;
            ChangeActionPad();
        }
        get { return setAction; }
    }
    
    //アクションアイコンを使用可能の色に変更
    public void UseIcon(int Pnum) => PadIcon[Pnum].color = new Color(1.0f, 1.0f, 1.0f, 0.8f); 
    //アクションアイコンを使用不可の色に変更
    public void NoUseIcon(int Pnum) => PadIcon[Pnum].color = new Color(0.1f, 0.1f, 0.1f, 0.8f); 
    //アクションパット、パレットの変更を反映する
    private void ChangeActionPad()
    {
        for (int n = 0; n < 5; n++)                                        
        {
            PadIcon[n].sprite = PaletteIconSprite[setAction[n]];            //アクションパットアイコンのスプライトを変更
            if (CanUseAction[setAction[n]]) UseIcon(n); else NoUseIcon(n);  //アイコンアクションの色を切り替える
        }
    }
    //アクションパッドの種類を切り替え(type:1 normal  type:2 Back  type:3 Go/Back)
    public void ChangePalette(int type)
    {
        if (type == 1)
        {
     //       Palette.sprite = PaletteSprite[0];
            Pad.sprite = PaletteSprite[0];

            for (int n = 0; n < 5; n++)
            {
                PadIcon[n].gameObject.SetActive(true);
            }
            ChangeActionPad();
        }
        else if (type == 2)
        {
         //   Palette.sprite = PaletteSprite[1];
            Pad.sprite = PaletteSprite[1];

            PadIcon[0].gameObject.SetActive(false);
            PadIcon[1].gameObject.SetActive(false);
            PadIcon[3].gameObject.SetActive(false);
            PadIcon[4].gameObject.SetActive(false);

            PadIcon[2].sprite = PaletteIconSprite[9];
        }
        else if (type == 3 || type == 4)
        {
          //  Palette.sprite = PaletteSprite[2];
            Pad.sprite = PaletteSprite[2];

            PadIcon[0].gameObject.SetActive(false);
            PadIcon[3].gameObject.SetActive(false);
            PadIcon[4].gameObject.SetActive(false);

            PadIcon[2].sprite = PaletteIconSprite[9];
            if(type == 3)
            {
                PadIcon[1].sprite = PaletteIconSprite[6];
            }
            else
            {
                PadIcon[1].sprite = PaletteIconSprite[7];
            }
        }
    }
    //アクションパット、パレット関連***************************************************************************************

}


/*
[System.Serializable]
public class PlayerDate
{
    public int pHP;
    public int pMP;
    public int[] PaletteNum;
    public bool[] ActionSkill;
    public bool[] StatasSkill;
    public bool[] pState;
}
*/

