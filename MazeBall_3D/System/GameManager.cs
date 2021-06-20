using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using LitJson;
using NendUnityPlugin.AD.Video;

/// <summary>
/// ゲーム全般の共通処理を行う
/// シングルトンクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    static private GameManager gameManager = null;
    static public int StageNum = 0;                 //現在のステージを管理
    static public int StagePage = 1;                //現在のステージセレクトのページを管理
    static public int MaxStageNum = 56;             //現在の実装しているステージ数を管理
    static private int MaxEventNum = 10;            //使用するイベントフラグの数
    static public Vector3 StandardPosture = Vector3.back;

    static private AudioManager audioManager = null;
    static private GameObject AudioObj = null;

    //セーブ、ロードに使用する変数
    static public int[] stageClearStatas;
    static public bool[] stageUnLockFlag;
    static public int coin;
    static public bool[] eventFlag;
    static public float[] clearTime;
    static public int[] eventCount;
    static public bool FirstFlag = true;

    static public Rigidbody PlayerBody = null;

    static private int BGMNum = 0;

    static public bool StageStartFlag = false;

    static private JsonData StageTimeData = null;

    private int CMCount = 3;
    static public bool CMPlusCoin = false;

#if UNITY_EDITOR
    public static bool Editor = true;
#else
    public static bool Editor = false;
#endif

#if UNITY_IOS
    private string spotId = "1009652", apiKey = "6ecc54536ab538ab4509ade3199a19ea7f88297b";
    public static bool IOSFlag = true;
#else
    private string spotId = "1010796", apiKey = "c3c8d5f86141d325665a8df6695fafd0c1770b6d";
  //  private string spotId = "802559", apiKey = "e9527a2ac8d1f39a667dfe0f7c169513b090ad44";

    public static bool IOSFlag = false;
#endif

    private NendAdInterstitialVideo m_InterstitialVideoAd;


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
            gameManager.initAudio();

            DontDestroyOnLoad(gameManager);

            TextAsset STimeJson = Resources.Load("Json/StageTime") as TextAsset;
            StageTimeData = JsonMapper.ToObject(STimeJson.text);
            gameManager.CMVideoLoad();
        }
        return gameManager;
    }

    /// <summary>
    /// オーディオマネージャーを生成
    /// </summary>
    private void initAudio()
    {
        if(AudioObj == null)
        {
            GameObject audioObj = (GameObject)Resources.Load("AudioManager");
            AudioObj = Instantiate(audioObj, Vector3.zero, Quaternion.identity);
            DontDestroyOnLoad(AudioObj);
            audioManager = AudioObj.GetComponent<AudioManager>();
            audioManager.BGMValumeChange();
            audioManager.SEValumeChange();
        }
    }


    //ゲームのセーブ、ロード処理*******************************************************************
    /// <summary>
    /// ゲームデータのセーブ
    /// </summary>
    public static void saveGameData()
    {
        UserGameDate data = new UserGameDate();
        data.StageClearStatas = stageClearStatas;
        data.StageUnLockFlag = stageUnLockFlag;
        data.Coin = coin;
        data.EventFlag = eventFlag;
        data.ClearTime = clearTime;
        data.EventCount = eventCount;
        SaveGameDate(data);
    }

    /// <summary>
    /// ゲームデータのロード
    /// GameManagerのインスタンス生成で使用する
    /// </summary>
    private static void loadGameData()
    {
        UserGameDate data = LoadGameData();
        stageClearStatas = data.StageClearStatas;
        stageUnLockFlag = data.StageUnLockFlag;
        coin = data.Coin;
        eventFlag = data.EventFlag;
        clearTime = data.ClearTime;
        eventCount = data.EventCount;
    }

    /// <summary>
    /// ゲームデータのリセット
    /// </summary>
    public static void resetGameData()
    {
        UserGameDate data = new UserGameDate();
        data.StageClearStatas = new int[MaxStageNum + 1];
        data.StageUnLockFlag = new bool[MaxStageNum + 1];
        data.StageUnLockFlag[1] = true;
        data.Coin = 0;
        data.EventFlag = new bool[MaxEventNum + 1];
        data.ClearTime = new float[MaxStageNum + 1];
        data.EventCount = new int[MaxEventNum + 1];
        SaveGameDate(data);
    }

    /// <summary>
    /// Jsonファイルからゲームデータを読み取る
    /// </summary>
    /// <returns>ゲームデータ</returns>
    private static UserGameDate LoadGameData()
    {
        if (File.Exists(Application.persistentDataPath + "/BallGameData.json"))
        { 
            string datastr = "";
            StreamReader reader;
            reader = new StreamReader(Application.persistentDataPath + "/BallGameData.json");
            datastr = reader.ReadToEnd();
            reader.Close();
            UserGameDate data = JsonUtility.FromJson<UserGameDate>(datastr);

            //個別のデータが欠落している場合の処理
            if(data.StageClearStatas == null) data.StageClearStatas = new int[MaxStageNum + 1];
            if (data.StageUnLockFlag == null)
            {
                data.StageUnLockFlag = new bool[MaxStageNum + 1];
                data.StageUnLockFlag[1] = true;
            }
            if (data.EventFlag == null) data.EventFlag = new bool[MaxEventNum + 1];
            if (data.ClearTime == null) data.ClearTime = new float[MaxStageNum + 1];
            if (data.EventCount == null)
            {
                data.EventCount = new int[MaxEventNum + 1];
                data.EventCount[0] = 100;
                data.EventCount[1] = 100;
                data.EventCount[2] = 50;
                data.EventCount[3] = 50;
                data.EventCount[4] = 50;
            }

            //個別のデータ数が足りていない場合の処理
            int ElementN1 = MaxStageNum + 2;
            int ElementN2 = MaxEventNum + 1;

            if (data.StageClearStatas.Length < ElementN1) data.StageClearStatas = data.StageClearStatas.Concat(new int[ElementN1 - data.StageClearStatas.Length]).ToArray();
            if (data.StageUnLockFlag.Length < ElementN1) data.StageUnLockFlag = data.StageUnLockFlag.Concat(new bool[ElementN1 - data.StageUnLockFlag.Length]).ToArray();
            if (data.ClearTime.Length < ElementN1) data.ClearTime = data.ClearTime.Concat(new float[ElementN1 - data.ClearTime.Length]).ToArray();
            if (data.EventFlag.Length < ElementN2) data.EventFlag = data.EventFlag.Concat(new bool[ElementN2 - data.EventFlag.Length]).ToArray();
            if (data.EventCount.Length < ElementN2) data.EventCount = data.EventCount.Concat(new int[ElementN2 - data.EventCount.Length]).ToArray();

            return data;
        }
        else
        {
            UserGameDate data = new UserGameDate();
            data.StageClearStatas = new int[MaxStageNum + 1];
            data.StageUnLockFlag = new bool[MaxStageNum + 1];
            data.StageUnLockFlag[1] = true;
            data.Coin = 0;
            data.EventFlag = new bool[MaxEventNum + 1];
            data.ClearTime = new float[MaxStageNum + 1];
            data.EventCount = new int[MaxEventNum + 1];
            data.EventCount[0] = 100;
            data.EventCount[1] = 100;
            data.EventCount[2] = 50;
            data.EventCount[3] = 50;
            data.EventCount[4] = 50;
            return data;
        }
    }

    /// <summary>
    /// ゲームデータをJsonファイルから読み取る
    /// </summary>
    /// <param name="gamedate"></param>
    private static void SaveGameDate(UserGameDate gamedate)
    {
        StreamWriter writer;
        string jsonstr = JsonUtility.ToJson(gamedate);
        writer = new StreamWriter(Application.persistentDataPath + "/BallGameData.json", false);
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
        if(eventCount[5] >= CMCount && !eventFlag[0] && !Editor)
        {
            CMShow();
        }
        else
        {
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


    //SE、BGMの処理********************************************************************************
    /// <summary>
    /// タイトルのBGMの再生
    /// </summary>
    public static void playTitleBGN()
    {
        StageNum = 0;
        GetInstance().PlayBGM();
    }
    /// <summary>
    /// ステージセレクトのBGMの再生
    /// </summary>
    public static void playStageSelectBGM()
    {
        StageNum = -1;
        GetInstance().PlayBGM();
    }
    /// <summary>
    /// ステージのBGMを再生
    /// </summary>
    /// <param name="num">ステージBGMのトラックナンバー</param>
    public static void playStageBGM(int num)
    {
        BGMNum = num;
        GetInstance().PlayBGM();
    }
    /// <summary>
    /// BGMの停止
    /// </summary>
    public static void stopBGM()
    {
        GetInstance().StopBGM();
    }
    /// <summary>
    /// BGMの再生処理
    /// </summary>
    private void PlayBGM()
    {
        if (StageNum == 0) audioManager.PlayTitleBGM();
        if (StageNum == -1) audioManager.PlayStageSelectBGM();
        if (StageNum >= 1) audioManager.PlayStageBGM(BGMNum);
    }
    /// <summary>
    /// BGMの停止処理
    /// </summary>
    private void StopBGM()
    {
        audioManager.BGMStop();
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

    //広告に関する関数*****************************************************************************
    public static void cmShow()
    {
        GetInstance().CMShow();
    }


    /// <summary>
    /// 動画広告の読み込み
    /// </summary>
    private void CMVideoLoad()
    {
        if (!Editor)  // && !eventFlag[0])
        {
            m_InterstitialVideoAd =
            NendAdInterstitialVideo.NewVideoAd(spotId, apiKey);

            m_InterstitialVideoAd.AdLoaded += (instance) => {
                // 広告ロード成功のコールバック
            };
            m_InterstitialVideoAd.AdFailedToLoad += (instance, errorCode) => {
                // 広告ロード失敗のコールバック
                // GameDirector.CmCoinFlag = false;
                CMPlusCoin = false;
            };
            m_InterstitialVideoAd.AdFailedToPlay += (instance) => {
                // 再生失敗のコールバック
                if (CMPlusCoin)
                {
                    CMPlusCoin = false;
                }
                else
                {
                    goStage();
                }

                /*
                if (PlayerStatas.HP <= 0) PlayerStatas.HP = PlayerStatas.MaxHP;
                if (GameDirector.CmCoinFlag)
                {
                    GameDirector.CmCoinFlag = false;
                    if (Director != null) Director.OpneNoCm();
                }
                */
            };
            m_InterstitialVideoAd.AdShown += (instance) => {
                // 広告表示のコールバック
            };
            m_InterstitialVideoAd.AdStarted += (instance) => {
                // 再生開始のコールバック
                /*
                if (GameDirector.CmCoinFlag)
                {
                    GameStatas.Coin += 100;
                    GameDirector.CmGetCoin = 100;
                }
                */
            };
            m_InterstitialVideoAd.AdStopped += (instance) => {
                // 再生中断のコールバック
               // if (PlayerStatas.HP <= 0) PlayerStatas.HP = PlayerStatas.MaxHP;
            };
            m_InterstitialVideoAd.AdCompleted += (instance) => {
                // 再生完了のコールバック
                if (CMPlusCoin)
                {
                    coin += 2;
                    saveGameData();
                }
            };
            m_InterstitialVideoAd.AdClicked += (instance) => {
                // 広告クリックのコールバック

                if (CMPlusCoin)
                {
                    coin += 2;
                    saveGameData();
                }
            };
            m_InterstitialVideoAd.InformationClicked += (instance) => {
                // オプトアウトクリックのコールバック
            };
            m_InterstitialVideoAd.AdClosed += (instance) => {
                // 広告クローズのコールバック
                CMDestroy();
                gameManager.CMVideoLoad();
                if (CMPlusCoin)
                {
                    CMPlusCoin = false;
                    //コインをプラスする処理
                    coin += 1;
                    saveGameData();
                }
                else
                {
                    eventCount[5] = 0;
                    goStage();
                }
                /*
                if (!GameDirector.CmCoinFlag)
                {
                    if (GameDirector.FieldFlag) MoveField(); else MoveStage();
                }
                else
                {
                    GameDirector.CmCoinFlag = false;
                    if (Director != null) Director.OpenCmCoinGetView();
                }
                */
            };
            m_InterstitialVideoAd.Load();
        }
    }

    /// <summary>
    /// 動画広告の表示
    /// </summary>
    private void CMShow()
    {
        if (!Editor)
        {
            if (m_InterstitialVideoAd.IsLoaded())
            {
                m_InterstitialVideoAd.Show();
            }
        }
    }

    /// <summary>
    /// 動画広告の破棄
    /// </summary>
    private void CMDestroy()
    {
        if (!Editor)
        {
            m_InterstitialVideoAd.Release();
        }
    }


    //その他*****************************************************************************************
    /// <summary>
    /// ステージの目標タイムを取得する処理
    /// </summary>
    /// <param name="StageNum">ステージの番号</param>
    /// <returns></returns>
    public static float[] getStageTime(int StageNum)
    {
        string S1 = StageTimeData["Stage" + StageNum][0].ToString();
        string S2 = StageTimeData["Stage" + StageNum][1].ToString();
        float F1 = float.Parse(S1);
        float F2 = float.Parse(S2);
        return new float[] {F1, F2};
    }


    //*******デバッグ用
    /*
    private void Update()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            //デバッグ用 回転角度の計算
            Vector3 SPosture = Input.acceleration.normalized;
            Vector2 RotPostrue = Vector2.zero;
            if      (SPosture.z <= 0)
            {
                RotPostrue.x = Mathf.Asin(SPosture.y);
                RotPostrue.y = Mathf.Asin(SPosture.x / Mathf.Cos(RotPostrue.x));
            } 
            else
            {
                if(Mathf.Abs(SPosture.y) >= Mathf.Abs(SPosture.x))
                {
                    if (SPosture.y >= 0) RotPostrue.x = Mathf.PI - Mathf.Asin(SPosture.y);
                    else RotPostrue.x = -Mathf.PI - Mathf.Asin(SPosture.y);
                    RotPostrue.y = Mathf.Asin(SPosture.x / Mathf.Cos(RotPostrue.x));
                }
                else
                {
                    RotPostrue.x = Mathf.Asin(SPosture.y);
                    if(SPosture.x >= 0) RotPostrue.y = Mathf.PI - Mathf.Asin(SPosture.x / Mathf.Cos(RotPostrue.x));
                    else RotPostrue.y = -Mathf.PI - Mathf.Asin(SPosture.x / Mathf.Cos(RotPostrue.x));
                }
            }
            
            
           

          //  RotPostrue.x = -RotPostrue.x;

       //     Debug.Log("方向:" + SPosture.x  + "  大きさ:" + SPosture.magnitude);
            Debug.Log(RotPostrue * 180 / Mathf.PI);
        }
        
    }
    */

}



/// <summary>
/// ゲームデータを管理するクラス
/// </summary>
/// EventFlag :0 完全版かの判定
///           :1 ビューモードの説明を見ているか
///           
/// 
/// EventCount:0 BGMの音量
///            1 SEの音量
///            2 センサ感度の強度
///            3 オブジェクトの透過範囲
///            4 オブジェクトの透過度
///            5 広告を表示するかのカウント
[System.Serializable]
public class UserGameDate
{
    public int[] StageClearStatas;
    public bool[] StageUnLockFlag;
    public int Coin;
    public bool[] EventFlag;
    public float[] ClearTime;
    public int[] EventCount;
}
