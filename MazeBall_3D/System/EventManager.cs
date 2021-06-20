using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//using UnityEngine.EventSystems;

#if UNITY_IOS
using UnityEngine.iOS;
#else
using UnityEngine.Android;
#endif

/// <summary>
/// ステージプレイ中のイベントを管理
/// </summary>
public class EventManager : MonoBehaviour//, IPointerDownHandler
{
    [SerializeField] private GameObject[] EventView = null;
    [SerializeField] private Button PauseButton = null;
    [SerializeField] private TextMeshProUGUI[] StageText = null;
    [SerializeField] private GameObject[] EventUI = null;
    [SerializeField] private Color[] textColor = null;
    [SerializeField] private GameObject ViewObj = null;
    [SerializeField] private GameObject ViewCamera = null;
    [SerializeField] private GameObject[] ViewUI = null;
    [SerializeField] private GameObject[] StartViewUI = null; 
    
    private GameObject MainCamera = null;
   

    static public EventManager eventManager = null;

    private float GameOverDis = 50f;
    static public Vector3 StageMaxPos = 1000 * Vector3.one;
    static public Vector3 StageMinPos = -1000 * Vector3.one;
    static public bool SpringFlag = true;
    static public bool DamageFlag = false;
    static public bool WarpFlag = false;

    private float SpringReTime = 0.5f;
    private float WarpReTime = 1.0f;

    private Vector3 GameOverMinPos;
    private Vector3 GameOverMaxPos;

    private bool XFRotFlag = false;
    private bool YFRotFlag = false;
    private bool XRRotFlag = false;
    private bool YRRotFlag = false;
    private bool ZoomInFlag = false;
    private bool ZoomOutFlag = false;
    private float RotRange = 1.5f;
    private float ZoomRange = 5f;
    private bool ViewModeFlag = false;

    private List<GameObject> ActionObj = new List<GameObject>();
    private List<int> AObjSwitchNum = new List<int>();

    private float Timer = 0;
    private float SpringDelta = 0;
    private float WarpDelta = 0;

    private bool StartCountFlag = false;
    private int StartCount = 4;
    private float StartCountTimer = 0;
    private float StartCountInterval = 0.5f;
    private float AllowableErrorAngle = 20f;
    private bool FirstErrorAngle = true;

    /// <summary>
    /// 初期処理
    /// </summary>
    private void Awake()
    {
        int StageN = GameManager.StageNum;
        if(StageN <= 20)        GameManager.playStageBGM(0);
        else if (StageN <= 40)  GameManager.playStageBGM(1);
        else if (StageN <= 60) GameManager.playStageBGM(2);

        if (eventManager == null) eventManager = GetComponent<EventManager>();

        StageText[0].text = TimeConvert(0);
        StageText[5].text = GameManager.coin.ToString();

        WarpFlag = false;
        SpringFlag = false;
        DamageFlag = false;

        MainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }
    private void Start()
    {
        GameOverMaxPos = StageMaxPos + Vector3.one * GameOverDis;
        GameOverMinPos = StageMinPos - Vector3.one * GameOverDis;
        if(ViewObj != null)
        {
            ViewObj.transform.position = (StageMaxPos + StageMinPos) / 2f;
        }
        if(GameManager.StageNum == 1)
        {
            OpenHowView2();
        }
        else
        {
            // Invoke("StageStart", 1.0f);
            StartCountFlag = true;
        }
    }

    /// <summary>
    /// ステージをスタートさせる。
    /// </summary>
    private void StageStart()
    {
        GameManager.StageStartFlag = true;
        StartViewUI[5].SetActive(false);
        GameManager.playSystemSE(4);
        DamageFlag = true;
        WarpFlag = true;
        SpringFlag = true;
    }

    /// <summary>
    /// 常時処理（ゲームオーバーの判定、タイマーのカウント、その他フラグのリセット）
    /// </summary>
    private void FixedUpdate()
    {
        if (GameManager.StageStartFlag)
        {
            //ゲームオーバー処理
            Vector3 P = PlayerCon.PlayerPos;
            if( (P.x > GameOverMaxPos.x || P.y > GameOverMaxPos.y || P.z > GameOverMaxPos.z
                || P.x < GameOverMinPos.x || P.y < GameOverMinPos.y || P.z < GameOverMinPos.z )
                && GameManager.StageStartFlag)
            {
                GameOver();
            }

            //タイマーのカウント
            if (!ViewModeFlag)
            {
                Timer += 0.02f;
                StageText[0].text = TimeConvert(Timer);
            }
            
            if (!SpringFlag)
            {
                SpringDelta += 0.02f;
                if(SpringDelta >= SpringReTime)
                {
                    SpringDelta = 0;
                    SpringFlag = true;
                }
            }

            if (!WarpFlag)
            {
                WarpDelta += 0.02f;
                if (WarpDelta >= WarpReTime)
                {
                    WarpDelta = 0;
                    WarpFlag = true;
                }
            }

            //ビューのカメラ操作
            if (XFRotFlag)
            {
                Vector3 CRot = ViewObj.transform.eulerAngles;
                CRot += Vector3.up * RotRange;
                ViewObj.transform.eulerAngles = CRot;
            }
            if (YFRotFlag)
            {
                Vector3 CRot = ViewObj.transform.eulerAngles;
                CRot += Vector3.right * RotRange;
                ViewObj.transform.eulerAngles = CRot;
            }
            if (XRRotFlag)
            {
                Vector3 CRot = ViewObj.transform.eulerAngles;
                CRot += Vector3.down * RotRange;
                ViewObj.transform.eulerAngles = CRot;
            }
            if (YRRotFlag)
            {
                Vector3 CRot = ViewObj.transform.eulerAngles;
                CRot += Vector3.left * RotRange;
                ViewObj.transform.eulerAngles = CRot;
            }
            if (ZoomInFlag)
            {
                Vector3 CPos = ViewCamera.transform.localPosition;
                CPos += Vector3.forward * ZoomRange;
                ViewCamera.transform.localPosition = CPos;
            }
            if (ZoomOutFlag)
            {
                Vector3 CPos = ViewCamera.transform.localPosition;
                CPos += Vector3.back * ZoomRange;
                ViewCamera.transform.localPosition = CPos;
            }
        }
        else
        {
            //ステージスタート処理
            if (StartCountFlag)
            {
                Vector3 Posture = Input.acceleration;
                Vector3 SPosture = GameManager.StandardPosture;

                bool OverAngleFlag = false;
                for(int i = 0; i <= 2; i++)
                {
                    float ErrorAngle = Mathf.Abs( CDegree(SPosture[i]) - CDegree(Posture[i]) );
                    if (ErrorAngle > AllowableErrorAngle) OverAngleFlag = true;
                }

                if(Posture == Vector3.zero || !OverAngleFlag)
                {
                    StartCountTimer += 0.02f;
                    if (StartCountTimer >= StartCountInterval)
                    {
                        if (!FirstErrorAngle)
                        {
                            FirstErrorAngle = true;
                            StartViewUI[0].SetActive(false);
                            StartViewUI[4].SetActive(false);
                        }
                        StartCountTimer = 0;
                        StartCount--;
                        if (StartCount <= 0)
                        {
                            StartViewUI[1].SetActive(false);
                            StageStart();
                        }
                        else
                        {
                            GameManager.playSystemSE(6);
                            StartViewUI[StartCount].SetActive(true);
                            StartViewUI[StartCount + 1].SetActive(false);
                        }
                    }
                }
                else
                {
                    if (FirstErrorAngle)
                    {
                        FirstErrorAngle = false;
                        StartCount = 4;
                        StartCountTimer = 0;
                        StartViewUI[0].SetActive(true);
                        StartViewUI[1].SetActive(false);
                        StartViewUI[2].SetActive(false);
                        StartViewUI[3].SetActive(false);
                        StartViewUI[4].SetActive(true);
                    }
                }
            }
        }
    }

    public void ChangeStandard()
    {
        GameManager.StandardPosture = Input.acceleration;
        Vector3 SPosture = GameManager.StandardPosture.normalized;
        if (SPosture.z <= 0)
        {
            PlayerCon.RotPosture.x = Mathf.Asin(SPosture.y);
            PlayerCon.RotPosture.y = Mathf.Asin(SPosture.x / Mathf.Cos(PlayerCon.RotPosture.x));
        }
        else
        {
            if (Mathf.Abs(SPosture.y) >= Mathf.Abs(SPosture.x))
            {
                if (SPosture.y >= 0) PlayerCon.RotPosture.x = Mathf.PI - Mathf.Asin(SPosture.y);
                else PlayerCon.RotPosture.x = -Mathf.PI - Mathf.Asin(SPosture.y);
                PlayerCon.RotPosture.y = Mathf.Asin(SPosture.x / Mathf.Cos(PlayerCon.RotPosture.x));
            }
            else
            {
                PlayerCon.RotPosture.x = Mathf.Asin(SPosture.y);
                if (SPosture.x >= 0) PlayerCon.RotPosture.y = Mathf.PI - Mathf.Asin(SPosture.x / Mathf.Cos(PlayerCon.RotPosture.x));
                else PlayerCon.RotPosture.y = -Mathf.PI - Mathf.Asin(SPosture.x / Mathf.Cos(PlayerCon.RotPosture.x));
            }
        }
        PlayerCon.RotPosture = -PlayerCon.RotPosture;
        GameManager.playStageSE(1);
    }

    //端末姿勢(Input.acceleration)を成分単位で度表記に変換する関数
    private float CDegree(float NVec) { return Mathf.Asin(NVec) * 180 / Mathf.PI; }

    /// <summary>
    /// ゲームオーバー処理
    /// </summary>
    public void GameOver()
    {
        if (!GameManager.eventFlag[0])
        {
            GameManager.eventCount[5] += 2;
            GameManager.saveGameData();
        }

        GameManager.StageStartFlag = false;
        Time.timeScale = 0;
        GameManager.stopBGM();
        EventView[0].SetActive(true);
        GameManager.playSystemSE(2);
        GameManager.stopLoopSE();
    }

    /// <summary>
    /// ステージクリア処理
    /// </summary>
    public void StageClear()
    {
        if (!GameManager.eventFlag[0])
        {
            GameManager.eventCount[5] += 1;
            GameManager.saveGameData();
        }

        int sn = GameManager.StageNum;
        if (sn % 20 == 0)
        {
            Review2();
        }
        else if (sn % 10 == 0 && GameManager.IOSFlag)
        {
            Review1();
        }

        GameManager.StageStartFlag = false;
        Time.timeScale = 0;
        EventUI[0].SetActive(false);
        EventUI[1].SetActive(false);
        GameManager.stopBGM();
        GameManager.stopLoopSE();
        if (GameManager.StageNum == 1)
        {
            for (int i = 2; i <= 8; i++)
            {
                GameManager.stageUnLockFlag[i] = true;
            }
        }

        if (GameManager.stageUnLockFlag[GameManager.StageNum + 1]) EventView[1].SetActive(true); else EventView[2].SetActive(true);
        GameManager.playSystemSE(3);

        //クリアフラグの処理
        float[] Stime = GameManager.getStageTime(GameManager.StageNum);
        StageText[1].text = TimeConvert(Timer);
        StageText[3].text = TimeConvert(Timer);
        int getCoin = 0;
        int clearStatas = GameManager.stageClearStatas[GameManager.StageNum];

        string RankText = "";
       
        if(Timer <= Stime[1]) //金ランク
        {
            if (clearStatas == 0) getCoin += 3;
            if (clearStatas == 1) getCoin += 2;
            if (clearStatas == 2) getCoin += 1;
            GameManager.stageClearStatas[GameManager.StageNum] = 3;
            RankText = "Rank:Gold";
            StageText[2].color = textColor[0];
            StageText[4].color = textColor[0];
        }
        else if(Timer <= Stime[0])  //銀ランク
        {
            if (clearStatas == 0) getCoin += 2;
            if (clearStatas == 1) getCoin += 1;
            if(clearStatas <= 1) GameManager.stageClearStatas[GameManager.StageNum] = 2;
            RankText = "Rank:Silver";
            StageText[2].color = textColor[1];
            StageText[4].color = textColor[1];
        }
        else //銅ランク
        {
            if (clearStatas == 0) getCoin += 1;
            if (clearStatas == 0) GameManager.stageClearStatas[GameManager.StageNum] = 1;
            RankText = "Rank:Bronze";
            StageText[2].color = textColor[2];
            StageText[4].color = textColor[2];
        }

        if(getCoin == 0)
        {
            StageText[2].text = RankText;
            StageText[4].text = RankText;
        }
        else
        {
            StageText[2].text = RankText + "\nGet " + getCoin + " Coin";
            StageText[4].text = RankText + "\nGet " + getCoin + " Coin";
        }

        GameManager.coin += getCoin;
        StageText[5].text = GameManager.coin.ToString();
        GameManager.clearTime[GameManager.StageNum] = Timer;


        //データのセーブ処理
        GameManager.saveGameData();
    }

    /// <summary>
    /// ステージをリトライする処理
    /// </summary>
    public void StageRetry()
    {
        if (!GameManager.eventFlag[0])
        {
            GameManager.eventCount[5] += 1;
            GameManager.saveGameData();
        }

        ResetStage();
        GameManager.playSystemSE(0);
        GameManager.GoStage(GameManager.StageNum);
    }

    /// <summary>
    /// 次のステージをプレイする処理
    /// </summary>
    public void StageNext()
    {
        ResetStage();
        GameManager.StageNum += 1;
        GameManager.playSystemSE(0);
        GameManager.GoStage(GameManager.StageNum);
    }

    /// <summary>
    /// ステージセレクト画面に戻る処理
    /// </summary>
    public void RetrunStageSelect()
    {
        ResetStage();
        GameManager.playSystemSE(1);
        GameManager.GoStageSelect();
    }

    /// <summary>
    /// 次のシーンに移動する時の初期化処理
    /// </summary>
    private void ResetStage()
    {
        StageMaxPos = 1000 * Vector3.one;
        StageMinPos = -1000 * Vector3.one;
        GameManager.StageStartFlag = false;
        StageCon.StageSurface = 1;
        eventManager = null;
        Time.timeScale = 1;
        GameManager.stopLoopSE();
    }

    /// <summary>
    /// スイッチを使用するオブジェクトを登録する
    /// </summary>
    /// <param name="obj">スイッチを使用するオブジェクト</param>
    /// <param name="SwitchNum">スイッチ起動ナンバー</param>
    public void InputActionObj(GameObject obj, int SwitchNum)
    {
        ActionObj.Add(obj);
        AObjSwitchNum.Add(SwitchNum);
    }

    /// <summary>
    /// スイッチアクションを起動する処理
    /// </summary>
    /// <param name="SwitchNum">スイッチ起動ナンバー</param>
    public void PlayObjAction(int SwitchNum)
    {
        for (int i = 0; i < ActionObj.Count; i++)
        {
            if (AObjSwitchNum[i] == SwitchNum)
            {
                ActionObj[i].GetComponent<MoveObject>().ChangeSwitch();
            }
        }
    }

    /// <summary>
    /// 時間を表示用テキストに変換する
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private string TimeConvert(float time)
    {
        int intTime = (int)time;
        float floatdecimalTime = time - intTime;

        int minuteTime = intTime / 60;
        int secondTime = intTime % 60;
        int decimalTime = (int)(floatdecimalTime * 100.0f);

        string minuteText = "0";
        if (minuteTime < 10) minuteText += minuteTime; else minuteText = minuteTime.ToString();
        string secondText = "0";
        if (secondTime < 10) secondText += secondTime; else secondText = secondTime.ToString();
        string decimalText = "0";
        if (decimalTime < 10) decimalText += decimalTime; else decimalText = decimalTime.ToString();

        return minuteText + ":" + secondText + ":" + decimalText;
    }

    /// <summary>
    /// ポーズボタンを開く
    /// </summary>
    public void OpenPauseView()
    {
        Time.timeScale = 0;
        GameManager.playSystemSE(0);
        EventView[3].SetActive(true);
        PauseButton.interactable = false;
    }

    /// <summary>
    /// ポーズボタンを閉じる
    /// </summary>
    public void ClosePauseView()
    {
        Time.timeScale = 1;
        GameManager.playSystemSE(1);
        EventView[3].SetActive(false);
        PauseButton.interactable = true;
    }

    /// <summary>
    /// 操作説明ビューを開く
    /// </summary>
    private void OpenHowView2()
    {
        GameManager.playSystemSE(0);
        EventView[5].SetActive(true);
        EventView[6].SetActive(true);
    }

    /// <summary>
    /// 操作説明ビューを閉じる
    /// </summary>
    public void CloseHowView2()
    {
        GameManager.playSystemSE(1);
        EventView[5].SetActive(false);
        StartCountFlag = true;
       // Invoke("StageStart", 1.0f);
    }

    /// <summary>
    /// 操作説明ビューのページを送る
    /// </summary>
    public void NextHowView2()
    {
        GameManager.playSystemSE(0);
        EventView[6].SetActive(false);
    }

    /// <summary>
    /// 操作説明ビューのページを戻る
    /// </summary>
    public void BackHowView2()
    {
        GameManager.playSystemSE(1);
        EventView[6].SetActive(true);
    }

    public void StartCameraView()
    {
        // Time.timeScale = 0;
        GameManager.PlayerBody.isKinematic = true;
        ViewModeFlag = true;
        GameManager.playSystemSE(0);
        ViewCamera.SetActive(true);
        PauseButton.interactable = false;
        ViewUI[0].SetActive(false);
        ViewUI[1].SetActive(true);
        ViewUI[2].SetActive(true);
        ViewUI[3].SetActive(true);
        ViewUI[4].SetActive(true);
        ViewUI[5].SetActive(true);
        ViewUI[6].SetActive(true);
        ViewUI[7].SetActive(true);

        if (!GameManager.eventFlag[1])
        {
            GameManager.eventFlag[1] = true;
            GameManager.saveGameData();
            ViewUI[8].SetActive(true);
        }
    }

    
    public void StopCameraView()
    {
        // Time.timeScale = 1;
        ViewModeFlag = false;
        GameManager.PlayerBody.isKinematic = false;
        GameManager.playSystemSE(1);
        ViewCamera.SetActive(false);
        PauseButton.interactable = true;
        ViewUI[1].SetActive(false);
        ViewUI[2].SetActive(false);
        ViewUI[3].SetActive(false);
        ViewUI[4].SetActive(false);
        ViewUI[5].SetActive(false);
        ViewUI[6].SetActive(false);
        ViewUI[7].SetActive(false);
    }

    public void CloseViewMode()
    {
        ViewUI[8].SetActive(false);
        GameManager.playSystemSE(1);
    }

    /// <summary>
    /// Buttonのフラグを変更する関数
    /// </summary>
    public void XFRotButttonUP()
    {
        XFRotFlag = false;
    }
    public void XFRotButttonDown()
    {
        XFRotFlag = true;
    }
    public void YFRotButttonUP()
    {
        YFRotFlag = false;
    }
    public void YFRotButttonDown()
    {
        YFRotFlag = true;
    }
    public void XRRotButttonUP()
    {
        XRRotFlag = false;
    }
    public void XRRotButttonDown()
    {
        XRRotFlag = true;
    }
    public void YRRotButttonUP()
    {
        YRRotFlag = false;
    }
    public void YRRotButttonDown()
    {
        YRRotFlag = true;
    }
    public void ZoomInButttonUP()
    {
        ZoomInFlag = false;
    }
    public void ZoomInButttonDown()
    {
        ZoomInFlag = true;
    }
    public void ZoomOutButttonUp()
    {
        ZoomOutFlag = false;
    }
    public void ZoomOutButttonDown()
    {
        ZoomOutFlag = true;
    }

   

    /// <summary>
    /// レビュー誘導　簡易版
    /// </summary>
    private void Review1()
    {
#if UNITY_IOS
    if (Device.RequestStoreReview()) {  }
#else

#endif
    }

    /// <summary>
    /// レビュー誘導　詳細版
    /// </summary>
    private void Review2()
    {
        GameManager.playSystemSE(0);
        EventView[4].SetActive(true);
    }

    /// <summary>
    /// ストアのレビューに移動する
    /// </summary>
    public void MoveReview()
    {
        EventView[4].SetActive(false);
#if UNITY_IOS
        string url = "itms-apps://itunes.apple.com/jp/app/id1525326497?mt=8&action=write-review";
#else
		string url = "market://details?id=ball.action.com";
#endif
        Application.OpenURL(url);
    }

    public void NoReview()
    {
        EventView[4].SetActive(false);
        GameManager.playSystemSE(1);
    }

  
}
