using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;
using System;


public class StageButton : MonoBehaviour
{
    [SerializeField] private bool AlignmentLock = true;
    [SerializeField] private float StandardWidth = 1200;    //基準になる横スクリーンのサイズ
    [SerializeField] private int ColumnCount = 7;           //列数
    [SerializeField] private float LeftPos = 10;            //一番左のボタンの位置(百分率で記入)
    [SerializeField] private float UperPos = 90;            //一番上のボタンの位置(百分率で記入)
    [SerializeField] private float UnderPos = 10;           //一番下のボタンの位置(百分率で記入)
    [SerializeField] private float SizeScale = 1;           //ボタンサイズの倍率

    [SerializeField] private float CFPosShit = 5;
    [SerializeField] private float CFSizeScale = 1;

    [SerializeField] private RectTransform[] stageButton = null;
    [SerializeField] private RectTransform[] stageLock = null;
    [SerializeField] private RectTransform[] stageCF = null;
    [SerializeField] private Text[] stageButtonText = null;

    [SerializeField] private GameObject NextPageButton = null;
    [SerializeField] private GameObject BackPageButton = null;
    [SerializeField] private GameObject RetrunButton = null;
   
    [SerializeField] private GameObject[] StageUI = null;
    [SerializeField] private Text[] StageText = null;

    [SerializeField] private MenuViewController menuCon = null;


    private int maxPage;
   // private int PlayStageNum = 0;
    private List<int> ViewStageButtonNum = new List<int>();

    private GameObject[] stageBObj;
    private GameObject[] lockObj;
    private GameObject[] clearObj;

    /*
    private InterstitialAd interstitial;
    private bool cmFinish = false;
    private float cmFinishDelta = 0;
    private float cmFinishLagTime = 0.5f;

    private bool ReloadShowADFlag = false;       //広告の読み込みが終わってない場合の再表示を試みるフラグ
    private int RetryShowADCount = 0;            //再読み込みを試みた回数
    private float RetryInterval = 1f;             //再読み込みを繰り返す間隔
    private int RetryMaxCount = 5;               //再読み込みを最大何回行うか
    private float RetryDelta = 0;
    */

    //隠しコマンド
    private int[][] hiddenNum = new int[3][];
    private int[] hiddenStage = new int[3];
    private int hiddenRunNumber = 0;
    private bool LevelChangeFlag = false;

    private bool NoLoadingAD = false;

    private void OnValidate()
    {
        if (!AlignmentLock)
        {
            AlignmentButton();
        }     
    }

    private void Awake()
    {
        if (GameManager.TitleSoundFlag)
        {
            GameManager.TitleSoundFlag = false;
            GameManager.playTitleBGM();
        }
        AlignmentButton();
    }

    private void Start()
    {
        maxPage = (GameManager.MaxStageNum - 1) / stageButton.Length + 1;
        stageBObj = new GameObject[stageButton.Length];
        lockObj = new GameObject[stageButton.Length];
        clearObj = new GameObject[stageButton.Length];
        GameManager.UpdataLockStatas();

        for (int i = 0; i < stageButton.Length; i++)
        {
            stageBObj[i] = stageButton[i].gameObject;
            lockObj[i] = stageLock[i].gameObject;
            clearObj[i] = stageCF[i].gameObject;
        }
        PageChangeButtonEnable();
        UpdateStageButton();

        /*
        if (!GameManager.eventFlag[0])
        {
            // Initialize an InterstitialAd.
            interstitial = new InterstitialAd(GameManager.InterID);

            // Called when an ad request failed to load.
            interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
            // Called when the ad is closed.
            interstitial.OnAdClosed += HandleOnAdClosed;

            // Create an empty ad request.
            AdRequest request = new AdRequest.Builder().Build();
            // Load the interstitial with the request.
            interstitial.LoadAd(request);
        }
        */
        loadHiddenNum();
        // GameManager.resetGameData();
    }

    /*
    private void FixedUpdate()
    {
        ReloadInterAD();    //広告の再表示処理(読み込みが完了していない場合)
        CMFinish();         //広告表示が終了してからの処理
    }
    */

    /// <summary>
    /// ボタンを整列させる
    /// </summary>
    private void AlignmentButton()
    {
        //縦の相対スクリーンサイズ計算
        float HeightSize = (float)Screen.height / Screen.width * StandardWidth;
        int RowCount = (stageButton.Length - 1) / ColumnCount + 1;

        //位置に使用する係数の計算
        float ax = LeftPos / 100;
        float bx = (100 - 2 * LeftPos) / (ColumnCount - 1) / 100;
        float ay = UperPos / 100;
        float by = (UperPos - UnderPos) / (RowCount - 1) / 100;

        //ボタンサイズ(重ならないサイズの計算)
        float BSize = (100 - 2 * LeftPos) / 100 * StandardWidth / (ColumnCount - 1);
        float BSize2 = (UperPos - UnderPos) / 100 * HeightSize / (RowCount - 1);

        if (BSize > BSize2) BSize = BSize2;

        for (int i = 0; i < stageButton.Length; i++)
        {
            int xi = i % ColumnCount;
            int yi = i / ColumnCount;
            float xf = xi;
            float yf = yi;
            float fx = ax + bx * xf;
            float fy = ay - by * yf;
            stageButton[i].anchorMax = new Vector2(fx, fy);
            stageButton[i].anchorMin = new Vector2(fx, fy);
            stageButton[i].sizeDelta = Vector2.one * BSize * SizeScale;

            stageLock[i].anchorMax = new Vector2(fx, fy);
            stageLock[i].anchorMin = new Vector2(fx, fy);
            stageLock[i].sizeDelta = Vector2.one * BSize * SizeScale;

            stageCF[i].anchorMax = new Vector2(fx, fy - CFPosShit / 100);
            stageCF[i].anchorMin = new Vector2(fx, fy - CFPosShit / 100);
            stageCF[i].sizeDelta = Vector2.one * BSize * CFSizeScale;
        }
    }

    /// <summary>
    /// ページ移動ボタンの更新
    /// </summary>
    private void PageChangeButtonEnable()
    {
        if (GameManager.StagePage == 1) BackPageButton.SetActive(false); else BackPageButton.SetActive(true);
        if (GameManager.StagePage == maxPage) NextPageButton.SetActive(false); else NextPageButton.SetActive(true);
    }

    /// <summary>
    /// ステージボタンの更新
    /// </summary>
    private void UpdateStageButton()
    {
        int Page = GameManager.StagePage;       //現在のページ

        //ページ番号の更新
        ViewStageButtonNum.Clear();
        ViewStageButtonNum.Add(0);
        for (int i = 1; i <= stageButton.Length; i++)
        {
            //ステージ番号の更新
            int StageButtonNum = i + stageButton.Length * (Page - 1);
            ViewStageButtonNum.Add(StageButtonNum);

            //ボタンのステージナンバーを更新
            stageButtonText[i - 1].text = StageButtonNum.ToString();

            //ステージボタンの有効化の切り替え
            if (StageButtonNum > GameManager.MaxStageNum)
            {
                stageBObj[i - 1].SetActive(false);
                lockObj[i - 1].SetActive(false);
                clearObj[i - 1].SetActive(false);
            }
            else
            {
                stageBObj[i - 1].SetActive(true);
                lockObj[i - 1].SetActive(true);
                clearObj[i - 1].SetActive(true);

                //ステージの攻略状況の更新
                int ClearStatas = GameManager.stageClearStatas[StageButtonNum];
                if (ClearStatas == 1) clearObj[i - 1].SetActive(true); else clearObj[i - 1].SetActive(false);
                
                //ステージのロック状況の更新
                
                bool UnLockFlag = GameManager.stageUnLockFlag[StageButtonNum];
                if (UnLockFlag) lockObj[i - 1].SetActive(false); else lockObj[i - 1].SetActive(true);    
            }
        }
    }

    /// <summary>
    /// ステージボタン
    /// </summary>
    /// <param name="SNum">ステージボタンの位置番号</param>
    public void MoveStage(int SNum)
    {
        GameManager.playSystemSE(1);
        GameManager.StageNum = ViewStageButtonNum[SNum];
        StageText[0].text = "Stage" + GameManager.StageNum;
        RetrunButton.SetActive(false);
        NextPageButton.SetActive(false);
        BackPageButton.SetActive(false);
        StageUI[0].SetActive(true);

        //隠しコマンド
        if (LevelChangeFlag)
        {
            int CLv = ViewStageButtonNum[SNum];
            if (CLv > GameManager.MaxPlayerLevel) CLv = GameManager.MaxPlayerLevel;
            GameManager.PlayerLv = CLv;
            GameManager.PlayerExp = 0;
            GameManager.setPlayerStatas(GameManager.getPlayerStatas(GameManager.PlayerLv));
            GameManager.saveGameData();

            LevelChangeFlag = false;
        }

        for(int i = 0; i < hiddenStage.Length; i++)
        {
            if(hiddenNum[i][hiddenStage[i]] == SNum)
            {
                hiddenStage[i] += 1;
            }
            else
            {
                hiddenStage[i] = 0;
            }
            if(hiddenStage[i] >= hiddenNum[i].Length)
            {
                hiddenRunNumber = i + 1;
                StageUI[2].SetActive(true);
            }
        }
    }

    /// <summary>
    /// ゲームスタート
    /// </summary>
    public void StageStart()
    {
        menuCon.stageStart();
        /*
        if(!cmFinish) GameManager.eventCount[8] += 1;
        if (GameManager.PlayerHp == 0) GameManager.eventCount[8] = GameManager.CMCount;
        if (GameManager.eventCount[8] >= GameManager.CMCount && !cmFinish && !GameManager.Editor && !GameManager.eventFlag[0] && !NoLoadingAD)
        {
            GameManager.LoadViewOpen();
            if (interstitial.IsLoaded())
            {
                GameManager.stopBGM();
                interstitial.Show();
            }
            else
            {
                ReloadShowADFlag = true;
            }
        }
        else
        {
            GameManager.TitleSoundFlag = true;
            GameManager.GoStage(GameManager.StageNum);
            GameManager.playSystemSE(1);

            if (GameManager.eventFlag[0]) return;
            interstitial.Destroy();
        }
        */
    }

    /// <summary>
    /// ゲームをスタートさせない
    /// </summary>
    public void NoStageStart()
    {
        RetrunButton.SetActive(true);
        PageChangeButtonEnable();
        StageUI[0].SetActive(false);
        GameManager.playSystemSE(4);
    }

    /// <summary>
    /// ロックボタン
    /// </summary>
    /// <param name="SNum">ステージボタンの位置番号</param>
    public void LockButton(int SNum)
    {
        int UnlockNum = ViewStageButtonNum[SNum];
        GameManager.playSystemSE(0);

        StageText[1].text = "Stage" + UnlockNum;

        if (UnlockNum <= 2)
        {
            StageText[2].text = "1個のステージをクリア";
            StageText[3].text = "1個";
        }
        else if (UnlockNum <= 8)
        {
            StageText[2].text = "2個のステージをクリア";
            StageText[3].text = 2 - GameManager.GetClearCount() + "個";
        }
        
        else if (UnlockNum <= 12)
        {
            StageText[2].text = "8個のステージをクリア";
            StageText[3].text = 8 - GameManager.GetClearCount() + "個";
        }
        else
        {
            int a = (UnlockNum - 1) / 6;
            StageText[2].text = a * 5 + "個のステージをクリア";
            StageText[3].text =  a * 5 - GameManager.GetClearCount() + "個";
        }

        RetrunButton.SetActive(false);
        NextPageButton.SetActive(false);
        BackPageButton.SetActive(false);
        StageUI[1].SetActive(true);
    }

    /// <summary>
    /// ロックビューを閉じるボタン
    /// </summary>
    public void ClsoeLockView()
    {
        RetrunButton.SetActive(true);
        PageChangeButtonEnable();
        StageUI[1].SetActive(false);
        GameManager.playSystemSE(4);
    }

    /// <summary>
    /// タイトル画面に戻るボタン
    /// </summary>
    public void BackTitle()
    {
        menuCon.backTitle();
    //    GameManager.GoTitle();
       // GameManager.playSystemSE(1);
    }

    /// <summary>
    /// 次のページの移動するボタン
    /// </summary>
    public void NextPageMove()
    {
        GameManager.StagePage += 1;
        PageChangeButtonEnable();
        UpdateStageButton();
        GameManager.playSystemSE(1);
    }

    /// <summary>
    /// 前のページの移動するボタン
    /// </summary>
    public void BackPageMove()
    {
        GameManager.StagePage -= 1;
        PageChangeButtonEnable();
        UpdateStageButton();
        GameManager.playSystemSE(1);
    }

    /*
    //広告のイベント
    /// <summary>
    /// ロード失敗時に実行する処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        MonoBehaviour.print("HandleFailedToReceiveAd event received with message: "
                        + args.Message);
        // cmFinish = true;
        NoLoadingAD = true;
    }

    /// <summary>
    /// 広告終了時に実行
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleAdClosed event received");
        GameManager.eventCount[8] = 0;
        cmFinish = true;
    }

    /// <summary>
    /// 広告表示が終了した後の処理
    /// </summary>
    private void CMFinish()
    {
        if (!cmFinish) return;
        cmFinishDelta += 0.02f;
        if (cmFinishDelta >= cmFinishLagTime)
        {
            cmFinishDelta = 0;
            StageStart();
        }
    }

    /// <summary>
    /// 広告読み込み途中に表示しようとした時の処理
    /// </summary>
    private void ReloadInterAD()
    {
        if (!ReloadShowADFlag) return;
        RetryDelta += 0.02f;
        if(RetryDelta >= RetryInterval)
        {
            RetryDelta = 0;
            RetryShowADCount += 1;
            if(RetryShowADCount <= RetryMaxCount)
            {
                if (interstitial.IsLoaded())
                {
                    ReloadShowADFlag = false;
                    GameManager.stopBGM();
                    interstitial.Show();
                }
            }
            else
            {
                GameManager.TitleSoundFlag = true;
                GameManager.GoStage(GameManager.StageNum);
                GameManager.playSystemSE(1);
                interstitial.Destroy();
            }
        }
    }

    */
    private void loadHiddenNum()
    {
        hiddenNum[0] = new int[] { 1, 9, 9, 2, 1, 1, 2, 7 };
        hiddenNum[1] = new int[] { 2, 4, 9, 2, 4, 9, 2, 2 };
        hiddenNum[2] = new int[] { 2, 4, 1, 2, 4, 1, 2, 2 };
    }

    public void YesHiddenRun()
    {
        StageUI[2].SetActive(false);

        //隠しコマンドを実行
        if(hiddenRunNumber == 1)
        {
            for(int i = 1; i < GameManager.stageClearStatas.Length; i++)
            {
                GameManager.stageClearStatas[i] = 1;
            }
            GameManager.UpdataLockStatas();
            GameManager.saveGameData();
        }
        if(hiddenRunNumber == 2)
        {
            LevelChangeFlag = true;
        }

        if (hiddenRunNumber == 3)
        {
            GameManager.PlayerKingBuffer = true;
            GameManager.eventCount[9] = 9;
            GameManager.eventCount[10] = 9;
            GameManager.eventCount[11] = 9;
            GameManager.eventCount[12] = 9;
            GameManager.eventCount[13] = 9;
        }

        for (int i = 0; i < hiddenStage.Length; i++)
        {
            hiddenStage[i] = 0;
        }
        hiddenRunNumber = 0;
    }

    public void NoHiddenRun()
    {
        StageUI[2].SetActive(false);

        for (int i = 0; i < hiddenStage.Length; i++)
        {
            hiddenStage[i] = 0;
        }
        hiddenRunNumber = 0;
    }
}
