using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using LitJson;
//using GoogleMobileAds.Api;
using System;

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
    static private int MaxEventNum = 20;            //使用するイベントフラグの数
    static public int MaxPlayerLevel = 50;           //プレイヤーの最大レベル

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

    static public int normalPlayerAttackPower = 10;     //プレイヤーの通常攻撃力係数
    static public int thnderMagicPower = 20;            //プレイヤーのサンダー魔法攻撃力係数
    static public bool SpringFlag = true;
    static public bool PlayerBallFlag = false;

    //ゲームオブジェクト
    static public GameObject DirectorObj = null;
    static public GameObject PlayerObj = null;

    static public Rigidbody PlayerBody = null;
  //  static private int BGMNum = 0;
    static public bool StageStartFlag = false;

    private int CMCount = 5;
    static public bool CMPlusCoin = false;
    private bool StageCMFlag = false;

#if UNITY_EDITOR
    public static bool Editor = true;
#else
    public static bool Editor = false;
#endif

 //   private InterstitialAd interstitial;
 //   private BannerView bannerView;

    /*
#if UNITY_IOS
   // private string spotId = "1009652", apiKey = "6ecc54536ab538ab4509ade3199a19ea7f88297b";

    private string spotId = "802557", apiKey = "b6a97b05dd088b67f68fe6f155fb3091f302b48b";

    public static bool IOSFlag = true;
#else
   // private string spotId = "1010796", apiKey = "c3c8d5f86141d325665a8df6695fafd0c1770b6d";
    private string spotId = "802559", apiKey = "e9527a2ac8d1f39a667dfe0f7c169513b090ad44";

    public static bool IOSFlag = false;
#endif

    private NendAdInterstitialVideo m_InterstitialVideoAd;
    public static NendAdBanner NABanner = null;
    */


    //インスタンス生成処理*************************************************************************
    /// <summary>
    /// インスタンス生成
    /// </summary>
    /// <returns>GameMAanagerのインスタンスを返す</returns>
    private static GameManager GetInstance()
    {
        if (gameManager == null) {
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

          //  MobileAds.Initialize(initStatus => { });

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
        userGameDate data = new userGameDate();
        data.StageClearStatas = stageClearStatas;
        data.StageUnLockFlag = stageUnLockFlag;
        data.EventFlag = eventFlag;
        data.EventCount = eventCount;

        data.EventCount[0] = PlayerLv;
        data.EventCount[1] = PlayerExp;
        data.EventCount[2] = PlayerCoin;
        data.EventCount[3] = PlayerHp;
        data.EventCount[4] = (int)PlayerMp;

        SaveGameDate(data);
    }

    /// <summary>
    /// ゲームデータのロード
    /// GameManagerのインスタンス生成で使用する
    /// </summary>
    private static void loadGameData()
    {
        userGameDate data = LoadGameData();
        stageClearStatas = data.StageClearStatas;
        stageUnLockFlag = data.StageUnLockFlag;
        eventFlag = data.EventFlag;
        eventCount = data.EventCount;

        PlayerLv = eventCount[0];
        PlayerExp = eventCount[1];
        PlayerCoin = eventCount[2];
        PlayerHp = eventCount[3];
        PlayerMp = eventCount[4];
    }

    /// <summary>
    /// ゲームデータのリセット
    /// </summary>
    public static void resetGameData()
    {
        userGameDate data = new userGameDate();
        data.StageClearStatas = new int[MaxStageNum + 1];
        data.StageUnLockFlag = new bool[MaxStageNum + 1];
        data.StageUnLockFlag[1] = true;
        data.EventFlag = new bool[MaxEventNum + 1];
        data.EventCount = new int[MaxEventNum + 1];
        data.EventCount[0] = 1;
        data.EventCount[3] = 10;
        data.EventCount[4] = 10;
        data.EventCount[5] = 100;
        data.EventCount[6] = 100;
        SaveGameDate(data);
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
            userGameDate data = JsonUtility.FromJson<userGameDate>(datastr);

            //個別のデータが欠落している場合の処理
            if(data.StageClearStatas == null) data.StageClearStatas = new int[MaxStageNum + 1];
            if (data.StageUnLockFlag == null)
            {
                data.StageUnLockFlag = new bool[MaxStageNum + 1];
                data.StageUnLockFlag[1] = true;
            }
            if (data.EventFlag == null) data.EventFlag = new bool[MaxEventNum + 1];
            if (data.EventCount == null)
            {
                data.EventCount = new int[MaxEventNum + 1];
                data.EventCount[0] = 1;
                data.EventCount[3] = 10;
                data.EventCount[4] = 10;
                data.EventCount[5] = 100;
                data.EventCount[6] = 100;
            }

            //個別のデータ数が足りていない場合の処理
            int ElementN1 = MaxStageNum + 2;
            int ElementN2 = MaxEventNum + 1;
            if (data.StageClearStatas.Length < ElementN1) data.StageClearStatas = data.StageClearStatas.Concat(new int[ElementN1 - data.StageClearStatas.Length]).ToArray();
            if (data.StageUnLockFlag.Length < ElementN1) data.StageUnLockFlag = data.StageUnLockFlag.Concat(new bool[ElementN1 - data.StageUnLockFlag.Length]).ToArray();
            if (data.EventFlag.Length < ElementN2) data.EventFlag = data.EventFlag.Concat(new bool[ElementN2 - data.EventFlag.Length]).ToArray();
            if (data.EventCount.Length < ElementN2) data.EventCount = data.EventCount.Concat(new int[ElementN2 - data.EventCount.Length]).ToArray();
            return data;
        }
        else
        {
            userGameDate data = new userGameDate();
            data.StageClearStatas = new int[MaxStageNum + 1];
            data.StageUnLockFlag = new bool[MaxStageNum + 1];
            data.StageUnLockFlag[1] = true;
            data.EventFlag = new bool[MaxEventNum + 1];
            data.EventCount = new int[MaxEventNum + 1];
            data.EventCount[0] = 1;
            data.EventCount[3] = 10;
            data.EventCount[4] = 10;
            data.EventCount[5] = 100;
            data.EventCount[6] = 100;
            return data;
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
        GetInstance().goStage();
    }
    private void goStage()
    {
        GameDirector.ResetActionObj();
      //  DestroyBanner();
        if (eventCount[9] >= 1) eventCount[9] -= 1;
        if (eventCount[10] >= 1) eventCount[10] -= 1;
        if (eventCount[11] >= 1) eventCount[11] -= 1;
        if (!eventFlag[0] && !Editor && !StageCMFlag)
        {
            /*
            eventCount[8] += 1;
            if (eventCount[8] >= CMCount && !eventFlag[0] && !Editor)
            {
                StageCMFlag = true;
            //    CMShow();
            }
            else
            {
                if (PlayerHp == 0)
                {
                    eventCount[8] += 2;
                    PlayerHp = PlayerMaxHp;
                    PlayerMp = PlayerMaxMp;
                }
                loadManager.viewLoadImage();
                SceneManager.LoadScene("Stage" + StageNum);
            }
            */

            if (PlayerHp == 0)
            {
                eventCount[8] += 2;
                PlayerHp = PlayerMaxHp;
                PlayerMp = PlayerMaxMp;
            }
            loadManager.viewLoadImage();
            SceneManager.LoadScene("Stage" + StageNum);
        }
        else
        {
            StageCMFlag = false;
            if(PlayerHp == 0)
            {
                PlayerHp = PlayerMaxHp;
                PlayerMp = PlayerMaxMp;
            }
            loadManager.viewLoadImage();
            SceneManager.LoadScene("Stage" + StageNum);
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
       // DestroyBanner();
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
        SceneManager.LoadScene("Shop");
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
        int BGMnumber = 1;
        if (StageNum == -1) return;
        if (0 <= StageNum && StageNum <= 8) BGMnumber = 1;
        if (9 <= StageNum && StageNum <= 18) BGMnumber = 2;
        if (19 <= StageNum && StageNum <= 23) BGMnumber = 3;
        if (25 <= StageNum && StageNum <= 29) BGMnumber = 3;
        if (StageNum == 24 || StageNum == 30) BGMnumber = 4;
        if (31 <= StageNum && StageNum <= 38) BGMnumber = 5;
        if (40 <= StageNum && StageNum <= 41) BGMnumber = 5;
        if (StageNum == 39 || StageNum == 42) BGMnumber = 6;
        if (43 <= StageNum && StageNum <= 60) BGMnumber = 7;
        audioManager.PlayStageBGM(BGMnumber);
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

    /*
public static void requestInterstitial()
{
    GetInstance().RequestInterstitial();
}

/// <summary>
/// 動画広告の読み込み
/// </summary>
private void RequestInterstitial()
{

    if (eventFlag[0] || Editor) return;       //有料版かエディターの場合


//テスト広告        
#if UNITY_ANDROID
    string adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
    string adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
    string adUnitId = "unexpected_platform";
#endif


//本番用        
#if UNITY_ANDROID
    string adUnitId = "ca-app-pub-3300095206848964/4633178724";
#elif UNITY_IPHONE
    string adUnitId = "ca-app-pub-3300095206848964/9135955540";
#else
    string adUnitId = "unexpected_platform";
#endif

    // Called when an ad request has successfully loaded.
    interstitial.OnAdLoaded += HandleOnAdLoaded;
    // Called when an ad request failed to load.
    interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
    // Called when an ad is shown.
    interstitial.OnAdOpening += HandleOnAdOpened;
    // Called when the ad is closed.
    interstitial.OnAdClosed += HandleOnAdClosed;
    // Called when the ad click caused the user to leave the application.
    interstitial.OnAdLeavingApplication += HandleOnAdLeavingApplication;

    // Initialize an InterstitialAd.
    interstitial = new InterstitialAd(adUnitId);
    // Create an empty ad request.
    AdRequest request = new AdRequest.Builder().Build();
    // Load the interstitial with the request.
    interstitial.LoadAd(request);
}
*/

    /*
//ロード成功時に実行
public void HandleOnAdLoaded(object sender, EventArgs args)
{
    MonoBehaviour.print("HandleAdLoaded event received");
}
//ロード失敗時に実行
public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
{
    MonoBehaviour.print("HandleFailedToReceiveAd event received with message: "
                        + args.Message);
}
//広告再生時に実行
public void HandleOnAdOpened(object sender, EventArgs args)
{
    MonoBehaviour.print("HandleAdOpened event received");
}
//広告終了時に実行
public void HandleOnAdClosed(object sender, EventArgs args)
{
    MonoBehaviour.print("HandleAdClosed event received");
    if (StageCMFlag)
    {
        StageCMFlag = false;
        goStage();
    }
    interstitial.Destroy();
    RequestInterstitial();
}
//広告クリック時
public void HandleOnAdLeavingApplication(object sender, EventArgs args)
{
    MonoBehaviour.print("HandleAdLeavingApplication event received");
}
/// <summary>
/// 動画広告の表示
/// </summary>
public static void cmShow()
{
    GetInstance().CMShow();
}
private void CMShow()
{
    if (!Editor)
    {
        if (interstitial.IsLoaded())
        {
            interstitial.Show();
        }
        else
        {
            if (StageCMFlag) goStage();
        }
    }
}
*/

    /*
/// <summary>
/// バナーの表示
/// </summary>
public static void viewBanner()
{
    GetInstance().ViewBanner();
}
private void ViewBanner()
{
#if UNITY_ANDROID
        string appId = "ca-app-pub-3300095206848964~2590496989";
#elif UNITY_IPHONE
    string appId = "ca-app-pub-3300095206848964~9623040703";
#else
        string appId = "unexpected_platform";
#endif
    // Initialize the Google Mobile Ads SDK.
    MobileAds.Initialize(appId);

    RequestBanner();
}
private void RequestBanner()
{
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
    string adUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
        string adUnitId = "unexpected_platform";
#endif

    // Create a 320x50 banner at the top of the screen.
    bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
}

/// <summary>
/// バナーの消去
/// </summary>
public static void destroyBanner()
{
    GetInstance().DestroyBanner();
}
private void DestroyBanner()
{
    if (bannerView != null) bannerView.Destroy();
}
*/


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
      //  PlayerNextExp = PlayerMaxExp - PlayerExp;
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
}



/// <summary>
/// ゲームデータを管理するクラス
/// </summary>
/// EventFlag :0 完全版かの判定
///           :1 Twtter
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
[System.Serializable]
public class userGameDate
{
    public int[] StageClearStatas;
    public bool[] StageUnLockFlag;
    public bool[] EventFlag;
    public int[] EventCount;
}
