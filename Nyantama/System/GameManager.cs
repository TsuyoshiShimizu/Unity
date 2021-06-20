using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using System.Linq;
using System;
using LitJson;
using GoogleMobileAds.Api;

/// <summary>
/// ゲーム全般の共通処理を行う
/// シングルトンクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    static private GameManager gameManager = null;
    static public int StageNum = 0;                 //現在のステージを管理
    static public int StagePage = 1;                //現在のステージセレクトのページを管理
    static public int MaxStageNum = 60;             //現在の実装しているステージ数を管理
    static public int MaxEventStageNum = 100;       //イベントステージの最大数
    static private int MaxEventNum = 20;            //使用するイベントフラグの数
    static public int MaxPlayerLevel = 50;          //プレイヤーの最大レベル
    static public int InfoNumber = 4;               //アップデート情報を表示させたい時に数字を一つ上げる  

    static public bool TitleSoundFlag = true;       //タイトルサウンドを再再生させる必要があるか

    //AudioManagerに関する変数
    static private AudioManager audioManager = null;

    //LoadManagerに関する変数
    static private LoadManager loadManager = null;

    //セーブ、ロードに使用する変数
    static public int[] stageClearStatas;
    static public bool[] stageUnLockFlag;
    static public bool[] eventFlag;
    static public int[] eventCount;
    static public string[] eventText;
    static public bool[] eventStageFlag;
    static public bool FirstFlag = true;

    //JSONデータ
    static public JsonData PlayerStatasData = null;
    static public JsonData EnemyStatasData = null;

    //プレイヤーのステータス
    static public int PlayerLv = 1;         //プレイヤーレベル
    static public int PlayerHp = 10;        //現在のプレイヤーHP
    static public int PlayerMaxHp = 10;     //プレイヤーの最大HP
    static public float PlayerMp = 10;        //現在のプレイヤーMP
    static public int PlayerMaxMp = 10;     //プレイヤーの最大MP
    static public int PlayerPow = 1;        //プレイヤーの攻撃力
    static public int PlayerMPow = 1;       //プレイヤーの魔力
    static public int PlayerDef = 1;        //プレイヤーの防御力
    static public int PlayerExp = 0;        //プレイヤーの現在の所持経験値 
  //  static public int PlayerNextExp = 100;  //プレイヤーの次のレベルになるまでの必要経験値
    static public int PlayerMaxExp = 100;   //プレイヤーのそのレベルの経験値テーブル
    static public int PlayerCoin = 0;       //プレイヤーの所持コイン
    static public int PlayerLife = 0;       //プレイヤーのライフ
    static public int PlayerLifeMaxR = 5;   //時間で自然回復する限界量
    static public int PlayerLifeMax = 10;   //ライフが増加する限界量

    static public bool SpringFlag = true;
    static public bool PlayerBallFlag = false;
    static public bool PlayerKingBuffer = false;        //王者のバフが有効か
    
    //バフによるステータスの掛け率
    static public float BufferPow = 1;
    static public float BufferMPow = 1;
    static public float BufferDef = 1;
    static public float BufferExp = 1;
    static public float BufferCoin = 1;

    //時間計測
    static public bool LoginBonusFlag = false;
    static public string LifeRecoverStartTimeText = "";
    static public int LifeSpanTime = 30;

    //ゲームオブジェクト
    static public GameObject DirectorObj = null;
    static public GameObject PlayerObj = null;

    static public Rigidbody PlayerBody = null;
  //  static private int BGMNum = 0;
    static public bool StageStartFlag = false;

    public static int CMCount = 3;

    public static RewardedAd ExpRewardedAd;
    public static RewardedAd HealthRewardedAd;
    public static RewardedAd LifeRewardedAd;
    public static RewardedAd LifeRewardedAd2;
    public static int[] RewardStage = new int[5];

    public static int FieldNum = 0;
    public static bool TrainigFlag = false;
    public static int SaveHp = 0;
    public static float SaveMp = 0;
    public static int SaveLv = 0;

    public static Vector3 FieldPoint = Vector3.zero;
    public static Quaternion FieldRot = Quaternion.identity;
    public static int PlayEventSrageNum = 0;

    //広告のIDを管理

    //テストID
    /*
#if UNITY_ANDROID
    public static string BannerID = "ca-app-pub-3940256099942544/6300978111";
    public static string InterID = "ca-app-pub-3940256099942544/1033173712";
    public static string RewardID = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
    public static string BannerID = "ca-app-pub-3940256099942544/2934735716";
    public static string InterID = "ca-app-pub-3940256099942544/4411468910";
    public static string RewardID = "ca-app-pub-3940256099942544/1712485313";
#else
    public static string BannerID = "unexpected_platform";
    public static string InterID = "unexpected_platform";
    public static string RewardID = "unexpected_platform";
#endif
*/

    //本番ID
    //  /*
#if UNITY_ANDROID
    public static string BannerID = "ca-app-pub-3300095206848964/1438386780";
    public static string InterID = "ca-app-pub-3300095206848964/4633178724";
    public static string RewardID = "ca-app-pub-3300095206848964/6457459929";
    public static string DeviceID = "ca-app-pub-3300095206848964~2590496989";
    public static bool AndroidFlag = true;
#elif UNITY_IPHONE
    public static string BannerID = "ca-app-pub-3300095206848964/1629958475";
    public static string InterID = "ca-app-pub-3300095206848964/9135955540";
    public static string RewardID = "ca-app-pub-3300095206848964/1396704935";
    public static string DeviceID = "ca-app-pub-3300095206848964~9623040703";
    public static bool AndroidFlag = false;
#else
    public static string BannerID = "unexpected_platform";
    public static string InterID = "unexpected_platform";
    public static string RewardID = "unexpected_platform";
    public static string DeviceID = "unexpected_platform";
    public static bool AndroidFlag = false;
#endif
    //     */

    //private BannerView bannerView;

#if UNITY_EDITOR
    public static bool Editor = true;
#else
    public static bool Editor = false;
#endif

  //  private InterstitialAd interstitial;
    


    //インスタンス生成処理*************************************************************************
    /// <summary>
    /// インスタンス生成
    /// </summary>
    /// <returns>GameMAanagerのインスタンスを返す</returns>
    private static GameManager GetInstance()
    {
        if (gameManager == null)
        {
            loadGameData();
            GameObject Obj = new GameObject("GameManager");
            gameManager = Obj.AddComponent<GameManager>();
            gameManager.initAudioManager();
            gameManager.initLoadManager();
            DontDestroyOnLoad(gameManager);

            TextAsset PlayerJson = Resources.Load("JSON/PlayerStatasData") as TextAsset;
            PlayerStatasData = JsonMapper.ToObject(PlayerJson.text);
            setPlayerStatas(getPlayerStatas(PlayerLv));

            TextAsset EnemyJson = Resources.Load("JSON/EnemyStatasData") as TextAsset;
            EnemyStatasData = JsonMapper.ToObject(EnemyJson.text);

            MobileAds.Initialize(initStatus => { });
            // gameManager.RequestInterstitial();
        }
        return gameManager;
    }

    /// <summary>
    /// オーディオマネージャーを生成
    /// </summary>
    private void initAudioManager()
    {
        if (audioManager == null)
        {
            GameObject audioPrefab = (GameObject)Resources.Load("AudioManager");
            GameObject AudioObj = Instantiate(audioPrefab, Vector3.zero, Quaternion.identity);
            DontDestroyOnLoad(AudioObj);
            audioManager = AudioObj.GetComponent<AudioManager>();
            audioManager.BGMValumeChange();
            audioManager.SEValumeChange();
        }
    }

    private void initLoadManager()
    {
        if (loadManager == null)
        {
            GameObject loadPrefab = (GameObject)Resources.Load("LoadManager");
            GameObject LoadObj = Instantiate(loadPrefab, Vector3.zero, Quaternion.identity);
            DontDestroyOnLoad(LoadObj);
            loadManager = LoadObj.GetComponent<LoadManager>();
        }
    }

    //ゲームのセーブ、ロード処理*******************************************************************
    /// <summary>
    /// ゲームデータのセーブ
    /// </summary>
    public static void saveGameData()
    {
        userGameDate date = new userGameDate();
        date.StageClearStatas = stageClearStatas;
        date.StageUnLockFlag = stageUnLockFlag;
        date.EventFlag = eventFlag;
        date.EventCount = eventCount;
        date.EventText = eventText;
        
        date.EventCount[1] = PlayerExp;
        date.EventCount[2] = PlayerCoin;
        date.EventCount[14] = PlayerLife;
        date.EventText[2] = LifeRecoverStartTimeText;
        date.EventText[0] = DateTime.Now.ToBinary().ToString();
        date.EventStageFlag = eventStageFlag;
        if (FieldNum != 1) //トレーニング中以外
        {                
            date.EventCount[3] = PlayerHp;
            date.EventCount[4] = (int)PlayerMp;
            date.EventCount[0] = PlayerLv;
        }
        else //トレーニング中
        {         
            date.EventCount[3] = SaveHp;
            date.EventCount[4] = (int)SaveMp;
            date.EventCount[0] = SaveLv;
        }
        SaveGameDate(date);
    }

    /// <summary>
    /// ゲームデータのロード
    /// GameManagerのインスタンス生成で使用する
    /// </summary>
    private static void loadGameData()
    {
        userGameDate date = LoadGameData();
        stageClearStatas = date.StageClearStatas;
        stageUnLockFlag = date.StageUnLockFlag;
        eventFlag = date.EventFlag;
        eventCount = date.EventCount;
        eventText = date.EventText;

        PlayerLv = eventCount[0];
        PlayerExp = eventCount[1];
        PlayerCoin = eventCount[2];
        PlayerHp = eventCount[3];
        PlayerMp = eventCount[4];
        PlayerLife = eventCount[14];
        LifeRecoverStartTimeText = eventText[2];

        //デバッグ用
        SaveLv = PlayerLv;
        SaveHp = PlayerHp;
        SaveMp = PlayerMp;

        DateTime NowTime = DateTime.Now;

        if(eventText[1] == "")
        {
            LoginBonusFlag = true;
        }
        else
        {
            DateTime LastLoginTime = DateTime.FromBinary(Convert.ToInt64(eventText[1]));
            if (NowTime.Year != LastLoginTime.Year) LoginBonusFlag = true;
            if (NowTime.Month != LastLoginTime.Month) LoginBonusFlag = true;
            if (NowTime.Day != LastLoginTime.Day) LoginBonusFlag = true;
        }
        eventText[1] = DateTime.Now.ToBinary().ToString();      //ログイン時間の更新

        eventStageFlag = date.EventStageFlag;
    }

    /// <summary>
    /// ゲームデータのリセット
    /// </summary>
    public static void resetGameData()
    {
        userGameDate date = new userGameDate();
        date.StageClearStatas = new int[MaxStageNum + 1];
        date.StageUnLockFlag = new bool[MaxStageNum + 1];
        date.StageUnLockFlag[1] = true;
        date.EventFlag = new bool[MaxEventNum + 1];
        date.EventCount = new int[MaxEventNum + 1];
        date.EventCount[0] = 1;
        date.EventCount[3] = 10;
        date.EventCount[4] = 10;
        date.EventCount[5] = 100;
        date.EventCount[6] = 100;
        date.EventText = new string[MaxEventNum + 1];
        date.EventStageFlag = new bool[MaxEventStageNum + 1];
        SaveGameDate(date);
    }

    /// <summary>
    /// Jsonファイルからゲームデータを読み取る
    /// </summary>
    /// <returns>ゲームデータ</returns>
    private static userGameDate LoadGameData()
    {
        if (File.Exists(Application.persistentDataPath + "/NekotamaGameData.json"))
        { 
            string datastr = "";
            StreamReader reader;
            reader = new StreamReader(Application.persistentDataPath + "/NekotamaGameData.json");
            datastr = reader.ReadToEnd();
            reader.Close();
            userGameDate date = JsonUtility.FromJson<userGameDate>(datastr);

            //個別のデータが欠落している場合の処理
            if(date.StageClearStatas == null) date.StageClearStatas = new int[MaxStageNum + 1];
            if (date.StageUnLockFlag == null)
            {
                date.StageUnLockFlag = new bool[MaxStageNum + 1];
                date.StageUnLockFlag[1] = true;
            }
            if (date.EventFlag == null) date.EventFlag = new bool[MaxEventNum + 1];
            if (date.EventCount == null)
            {
                date.EventCount = new int[MaxEventNum + 1];
                date.EventCount[0] = 1;
                date.EventCount[3] = 10;
                date.EventCount[4] = 10;
                date.EventCount[5] = 100;
                date.EventCount[6] = 100;
            }
            if (date.EventText == null) date.EventText = new string[MaxEventNum + 1];
            if (date.EventStageFlag == null) date.EventStageFlag = new bool[MaxEventStageNum + 1];

            //個別のデータ数が足りていない場合の処理
            int ElementN1 = MaxStageNum + 2;
            int ElementN2 = MaxEventNum + 1;
            int ElementN3 = MaxEventStageNum + 1;
            if (date.StageClearStatas.Length < ElementN1) date.StageClearStatas = date.StageClearStatas.Concat(new int[ElementN1 - date.StageClearStatas.Length]).ToArray();
            if (date.StageUnLockFlag.Length < ElementN1) date.StageUnLockFlag = date.StageUnLockFlag.Concat(new bool[ElementN1 - date.StageUnLockFlag.Length]).ToArray();
            if (date.EventFlag.Length < ElementN2) date.EventFlag = date.EventFlag.Concat(new bool[ElementN2 - date.EventFlag.Length]).ToArray();
            if (date.EventCount.Length < ElementN2) date.EventCount = date.EventCount.Concat(new int[ElementN2 - date.EventCount.Length]).ToArray();
            if (date.EventText.Length < ElementN2) date.EventText = date.EventText.Concat(new string[ElementN2 - date.EventText.Length]).ToArray();
            if (date.EventStageFlag.Length < ElementN3) date.EventStageFlag = date.EventStageFlag.Concat(new bool[ElementN3 - date.EventStageFlag.Length]).ToArray();
            return date;
        }
        else
        {
            userGameDate date = new userGameDate();
            date.StageClearStatas = new int[MaxStageNum + 1];
            date.StageUnLockFlag = new bool[MaxStageNum + 1];
            date.StageUnLockFlag[1] = true;
            date.EventFlag = new bool[MaxEventNum + 1];
            date.EventCount = new int[MaxEventNum + 1];
            date.EventText = new string[MaxEventNum + 1];
            date.EventCount[0] = 1;
            date.EventCount[3] = 10;
            date.EventCount[4] = 10;
            date.EventCount[5] = 100;
            date.EventCount[6] = 100;
            date.EventStageFlag = new bool[MaxEventStageNum + 1];
            return date;
        }
    }

    /// <summary>
    /// ゲームデータをJsonファイルから読み取る
    /// </summary>
    /// <param name="gamedate"></param>
    private static void SaveGameDate(userGameDate gamedate)
    {
        StreamWriter writer;
        string jsonstr = JsonUtility.ToJson(gamedate);
        writer = new StreamWriter(Application.persistentDataPath + "/NekotamaGameData.json", false);
        writer.Write(jsonstr);
        writer.Flush();
        writer.Close();
    }


    //シーン移動処理*******************************************************************************
   
    /// <summary>
    /// ステージのシーンに移動する
    /// </summary>
    /// <param name="SNum"></param>
    public static void GoStage(int SNum)
    {
        StageNum = SNum;
        GetInstance().goStage();
    }
    private void goStage()
    {
        GameDirector.ResetActionObj();

        //FieldNum 0:通常ステージ 1:トレーニングステージ

        
        if (FieldNum != 1)
        {
            if (eventCount[9] >= 1) eventCount[9] -= 1;
            if (eventCount[10] >= 1) eventCount[10] -= 1;
            if (eventCount[11] >= 1) eventCount[11] -= 1;
            if (eventCount[12] >= 1) eventCount[12] -= 1;
            if (eventCount[13] >= 1) eventCount[13] -= 1;    
        }
        BufferUpdate();

        if (PlayerHp <= 0 || eventCount[12] >= 1 || FieldNum == 1)
        {
            PlayerHp = PlayerMaxHp;
            PlayerMp = PlayerMaxMp;
        }
        
        loadViewOpen();
        if(FieldNum == 0)
        {
            SceneManager.LoadScene("Stage" + StageNum);
        }
        if(FieldNum == 1)
        {
            SceneManager.LoadScene("Training" + StageNum);
        }
        
    }
    
    /// <summary>
    /// タイトル画面に移動する
    /// </summary>
    public static void GoTitle()
    {
        StageNum = 0;    
        GetInstance().goTitle();
    }
    private void goTitle()
    {
    //    DestroyBanner();
        SceneManager.LoadScene("TitleScene");
    }

    /// <summary>
    /// ステージセレクト画面に移動する
    /// </summary>
    public static void GoStageSelect()
    {
        StageNum = -1;
        GetInstance().goStageSelect();
    }
    private void goStageSelect()
    {
        SceneManager.LoadScene("StageSelect");
    }

    public static void GoShop()
    {
        GetInstance().goShop();
    }
    private void goShop()
    {
       // DestroyBanner();
        if (PlayerHp <= 0)
        {
            PlayerHp = PlayerMaxHp;
            PlayerMp = PlayerMaxMp;
        }
        loadViewOpen();
        SceneManager.LoadScene("Shop");
    }

    public static void LoadViewOpen()
    {
        GetInstance().loadViewOpen();
    }
    private void loadViewOpen()
    {
        loadManager.viewLoadImage();
    }

    public static void LoadViewClose()
    {
        GetInstance().loadViewClose();
    }
    private void loadViewClose()
    {
        loadManager.nonviewLoadImage();
    }



    //SE、BGMの処理********************************************************************************
    /// <summary>
    /// タイトルのBGMの再生
    /// </summary>
    public static void playTitleBGM()
    {
        StageNum = 0;
        GetInstance().PlayBGM();
    }

    /// <summary>
    /// その他BGMの再生
    /// </summary>
    /// <param name="Num"></param>
    public static void playOtherBGM(int Num)
    {
        GetInstance().PlayOtherBGM(Num);
    }

    /// <summary>
    /// ステージセレクトのBGMの再生
    /// </summary>
    public static void playShopBGM()
    {
        StageNum = -1;
        GetInstance().ShopBGM();
    }
    /// <summary>
    /// ステージのBGMを再生
    /// </summary>
    /// <param name="num">ステージBGMのトラックナンバー</param>
    public static void playStageBGM()
    {
        GetInstance().SelectBGM();
    }

    /// <summary>
    /// BGMの停止
    /// </summary>
    public static void stopBGM()
    {
        GetInstance().StopBGM();
    }
    public static void restartBGM()
    {
        GetInstance().ReStartBGM();
    }


    /// <summary>
    /// BGMの再生処理
    /// </summary>
    private void PlayBGM()
    {
        if (StageNum <= 0) audioManager.PlayTitleBGM(); else SelectBGM();
    }

    private void PlayOtherBGM(int Num)
    {
        audioManager.PlayOtherBGM(Num);
    }

    private void ShopBGM()
    {
        audioManager.PlayShopBGM();
    }
    /// <summary>
    /// BGMの停止処理
    /// </summary>
    private void StopBGM()
    {
        audioManager.BGMStop();
    }

    private void ReStartBGM()
    {
        audioManager.BGMReStart();
    }

    /// <summary>
    /// ステージのBGM設定
    /// </summary>
    private void SelectBGM()
    {
        if (Editor)
        {
            string SceneName = SceneManager.GetActiveScene().name;
            if (SceneName.Contains("Stage"))
            {
                FieldNum = 0;
                string StageNumberText = SceneName.Replace("Stage","");
                int sn;
                if (int.TryParse(StageNumberText, out sn)) StageNum = sn;
            }

            if (SceneName.Contains("Training"))
            {
                FieldNum = 1;
                string StageNumberText = SceneName.Replace("Training", "");
                int sn;
                if (int.TryParse(StageNumberText, out sn)) StageNum = sn;
            }
        }

        if(FieldNum == 0)
        {
            int BGMnumber = 1;
            if (StageNum == -1) return;
            if (0 <= StageNum && StageNum <= 8) BGMnumber = 1;
            if (9 <= StageNum && StageNum <= 18) BGMnumber = 2;

            if (StageNum == 24 || StageNum == 30) BGMnumber = 4;
            else if (19 <= StageNum && StageNum <= 30) BGMnumber = 3;

            if (StageNum == 39 || StageNum == 42) BGMnumber = 6;
            else if (31 <= StageNum && StageNum <= 42) BGMnumber = 5;

            if (StageNum == 49 || StageNum == 52) BGMnumber = 6;
            else if (StageNum == 54) BGMnumber = 4;
            else if (43 <= StageNum && StageNum <= 60) BGMnumber = 7;
            if (StageNum == 1000) BGMnumber = 8;
            audioManager.PlayStageBGM(BGMnumber);
        }

        if(FieldNum == 1)
        {
            if (StageNum == 0) audioManager.PlayOtherBGM(6);
            if (StageNum != 0) audioManager.PlayOtherBGM(7);
        }
        
    }

    /// <summary>
    /// システムSEの再生
    /// </summary>
    /// <param name="num">システムSEのトラックナンバー</param>
    public static void playSystemSE(int num)
    {
        GetInstance().PlaySystemSE(num);
    }
    /// <summary>
    /// システムSEの再生処理
    /// </summary>
    /// <param name="num"></param>
    private void PlaySystemSE(int num)
    {
        audioManager.PlaySystemSE(num);
    }

    /// <summary>
    /// ステージSEの再生
    /// </summary>
    /// <param name="num"></param>
    public static void playStageSE(int num)
    {
        GetInstance().PlayStageSE(num);
    }
    /// <summary>
    /// ステージSEの再生処理
    /// </summary>
    /// <param name="num"></param>
    private void PlayStageSE(int num)
    {
        audioManager.PlayStageSE(num);
    }

    /// <summary>
    /// ループSEの再生
    /// </summary>
    /// <param name="num"></param>
    public static void playLoopSE(int num)
    {
        GetInstance().PlayLoopSE(num);
    }
    /// <summary>
    /// ループSEの再生処理
    /// </summary>
    /// <param name="num"></param>
    private void PlayLoopSE(int num)
    {
        audioManager.PlayLoopSE(num);
    }
    /// <summary>
    /// ループSEの停止
    /// </summary>
    public static void stopLoopSE()
    {
        GetInstance().StopLoopSE();
    }
    /// <summary>
    ///ループSEの停止処理
    /// </summary>
    private void StopLoopSE()
    {
        audioManager.StopLoopSE();
    }

    /// <summary>
    /// BGMの音量変更
    /// </summary>
    public static void bgmChangeValume()
    {
        GetInstance().BGMChangeValime();
    }
    /// <summary>
    /// BGMの音量変更処理
    /// </summary>
    private void BGMChangeValime()
    {
        audioManager.BGMValumeChange();
    }

    /// <summary>
    /// SEの音量変更
    /// </summary>
    public static void seChangeValume()
    {
        GetInstance().SEChangeValime();
    }
    /// <summary>
    /// SEの音量変更処理
    /// </summary>
    private void SEChangeValime()
    {
        audioManager.SEValumeChange();
    }

    public static AudioSource getBGMAudioSoruce()
    {
        return GetInstance().getAudioSorce();
    }

    private AudioSource getAudioSorce()
    {
        return audioManager.GetBGMAudioSource();
    }
    //広告に関する関数*****************************************************************************

    //その他
    /// <summary>
    /// クリアしているステージ数を取得
    /// </summary>
    /// <returns></returns>
    public static int GetClearCount()
    {
        int ClearN = 0;
        for(int i = 1; i < stageClearStatas.Length; i++)
        {
            if (stageClearStatas[i] == 1) ClearN++ ;
        }
        return ClearN;
    }

    /// <summary>
    /// ステージロックフラグの更新
    /// </summary>
    public static void UpdataLockStatas()
    {
        int clearCount = GetClearCount();
        int LockRNum = 1;       //ステージ解放ナンバー
        if      (clearCount <= 0) LockRNum = 1;
        else if (clearCount <= 1) LockRNum = 2;
        else if (clearCount <= 7) LockRNum = 8;
        else if (clearCount <= 9) LockRNum = 12;
        else                      LockRNum = (clearCount / 5 + 1) * 6;
        for (int i = 1; i < stageUnLockFlag.Length; i++)
        {
            if(i <= LockRNum) stageUnLockFlag[i] = true; else stageUnLockFlag[i] = false;
        }
    }

    /// <summary>
    /// プレイヤーのステータスデータを取得
    /// </summary>
    /// <param name="PLv"></param>
    /// <returns></returns>
    public static int[] getPlayerStatas(int PLv)
    {
        string Exp = PlayerStatasData["Lv" + PLv][0].ToString();
        string Hp = PlayerStatasData["Lv" + PLv][1].ToString();
        string Mp = PlayerStatasData["Lv" + PLv][2].ToString();
        string Pow = PlayerStatasData["Lv" + PLv][3].ToString();  
        string MPow = PlayerStatasData["Lv" + PLv][4].ToString();
        string Def = PlayerStatasData["Lv" + PLv][5].ToString();
        int exp = int.Parse(Exp);
        int hp = int.Parse(Hp);
        int mp = int.Parse(Mp);
        int pow = int.Parse(Pow);
        int mpow = int.Parse(MPow);
        int def = int.Parse(Def);
        return new int[] { exp, hp, mp, pow, mpow, def };
    }

    /// <summary>
    /// プレイヤーのステータスをセットする
    /// </summary>
    /// <param name="Pstatas"></param>
    public static void setPlayerStatas(int[] Pstatas)
    {
        PlayerMaxHp = Pstatas[1];
        PlayerMaxMp = Pstatas[2];
        PlayerPow = Pstatas[3];
        PlayerMPow = Pstatas[4];
        PlayerDef = Pstatas[5];
        PlayerMaxExp = Pstatas[0];
    }

    public static int[] getEnemyStatas(int ELv , string EnemyName)
    {
        string Exp = EnemyStatasData[EnemyName + "Lv" + ELv][0].ToString();
        string Coin = EnemyStatasData[EnemyName + "Lv" + ELv][1].ToString();
        string Hp = EnemyStatasData[EnemyName + "Lv" + ELv][2].ToString();
        string Pow = EnemyStatasData[EnemyName + "Lv" + ELv][3].ToString();
        string Def = EnemyStatasData[EnemyName + "Lv" + ELv][4].ToString();

        int exp = int.Parse(Exp);
        int coin = int.Parse(Coin);
        int hp = int.Parse(Hp);   
        int pow = int.Parse(Pow);
        int def = int.Parse(Def);
        return new int[] { exp, coin, hp, pow, def };
    }


    /// <summary>
    /// バフ値の更新
    /// </summary>
    private static void BufferUpdate()
    {
        float BHp = 1;
        float BMp = 1;
        float BPow = 1;
        float BMPow = 1;
        float BDef = 1;
        float BExp = 1;
        float BCoin = 1;

        if(FieldNum != 1)   //トレーニングルーム以外の時
        {
            if (eventCount[9] >= 1) BPow *= 1.5f;
            if (eventCount[10] >= 1) BMPow *= 1.5f;
            if (eventCount[11] >= 1) BDef *= 1.5f;
            if (eventCount[12] >= 1)
            {
                BHp *= 1.5f;
                BMp *= 1.5f;
            }
            if (eventCount[13] >= 1) BExp *= 2f;
            if (PlayerKingBuffer)
            {
                BPow *= 1.2f;
                BMPow *= 1.2f;
                BDef *= 1.2f;
                BHp *= 1.2f;
                BMp *= 1.2f;
                BExp *= 1.2f;
                BCoin *= 1.2f;
            }
        }

        string Hp = PlayerStatasData["Lv" + PlayerLv][1].ToString();
        string Mp = PlayerStatasData["Lv" + PlayerLv][2].ToString();
        int hp = int.Parse(Hp);
        int mp = int.Parse(Mp);

        PlayerMaxHp = (int)(hp * BHp);
        PlayerMaxMp = (int)(mp * BMp);
        BufferPow = BPow;
        BufferMPow = BMPow;
        BufferDef = BDef;
        BufferExp = BExp;
        BufferCoin = BCoin;
    }
    
    /// <summary>
    /// 時間経過によるライフ回復処理
    /// </summary>
    public static void LifeRecoverTimeUpdate()
    {
        if(PlayerLife >= PlayerLifeMaxR)
        {
            LifeRecoverStartTimeText = "";
        }
        else
        {
            DateTime NowTime = DateTime.Now;
            if (LifeRecoverStartTimeText == "")
            {
                LifeRecoverStartTimeText = NowTime.ToBinary().ToString();
            }
            else
            {
                DateTime StartTime = DateTime.FromBinary(Convert.ToInt64(LifeRecoverStartTimeText));
                TimeSpan LifeSpan;
                bool Loop = true;
                int CC = 0;
                while (Loop)
                {
                    LifeSpan = NowTime - StartTime;
                    double Minute = LifeSpan.TotalMinutes;
                    if(Minute >= LifeSpanTime)
                    {
                        PlayerLife += 1;
                        StartTime += new TimeSpan(0, LifeSpanTime, 0);
                        LifeRecoverStartTimeText = StartTime.ToBinary().ToString();
                        if(PlayerLife >= PlayerLifeMaxR)
                        {
                            Loop = false;
                            LifeRecoverStartTimeText = "";
                        }
                    }
                    else
                    {
                        Loop = false;
                    }
                    CC += 1;
                    if (CC >= 10) Loop = false;
                }
                saveGameData();
            }
        }
    }
    
    
}



/// <summary>
/// ゲームデータを管理するクラス
/// </summary>
/// EventFlag :0 完全版かの判定
///           :1 Twtter
///           :2 隠しステージ1クリアかの有無
///           :3 アクションパッドを隠すか
///           
/// 
/// EventCount:0 プレイヤーのレベル
///            1 現在の所持経験値
///            2 現在の所持コイン
///            3 プレイヤーの現在のHP
///            4 プレイヤーの現在のMP
///            5 BGMの音量
///            6 SEの音量
///            7 プレイヤーの状態
///            8 CMをみるカウント
///            9 攻撃の薬のカウント
///            10 魔力の薬のカウント
///            11 防御力の薬のカウント
///            12 超健康状態のカウント
///            13 獲得経験値2倍のカウント
///            14 プレイヤーのライフ
///            15 インフォナンバー
///            
/// 
/// EventText :0 最終セーブ時間のDataTimeのバイナリ
///            1 ゲーム開始時の時間
///            2 ライフの回復開始時間のDataTimeのバイナリ
[System.Serializable]
public class userGameDate
{
    public int[] StageClearStatas;
    public bool[] StageUnLockFlag;
    public bool[] EventFlag;
    public int[] EventCount;
    public string[] EventText;
    public bool[] EventStageFlag;
}
