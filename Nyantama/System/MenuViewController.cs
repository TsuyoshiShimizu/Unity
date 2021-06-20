using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using GoogleMobileAds.Api;

public class MenuViewController : MonoBehaviour
{
    //メインビュー
    [HideInInspector]
    [SerializeField] private GameObject MenuView = null;
    [HideInInspector]
    [SerializeField] private GameObject MoneyView = null;
    [HideInInspector]
    [SerializeField] private GameObject CatMovieView = null;
    [HideInInspector]
    [SerializeField] private GameObject OtherGameView = null;
    [HideInInspector]
    [SerializeField] private GameObject[] ConfirmationView = null;
    [HideInInspector]
    [SerializeField] private GameObject OtherAppButton = null;

    //メインUI
    [SerializeField] private Button MenuButton = null;

    //課金ビューのUI
    [SerializeField] private Button nocmButton = null;
    [SerializeField] private Button restoreButton = null;

    //他アプリ紹介のUI
    [SerializeField] private GameObject[] AppView = null;

    //ポン吉の動画
    [SerializeField] private Button[] UIButton = null;
    [SerializeField] private Button[] MovieButton = null;
    [SerializeField] private Sprite[] MovieSprite = null;
    [SerializeField] private Text PageText = null;
    [SerializeField] private RectTransform[] MovieView = null;

    [SerializeField] private GameObject[] RewardView = null;
    [SerializeField] private Button[] RewardButton = null;

    [SerializeField] private GameObject TwitterView = null;

    [SerializeField] private Text LifeNumText = null;
    [SerializeField] private Text LifeNumText2 = null;
    [SerializeField] private GameObject LifeNumText3 = null;
    [SerializeField] private GameObject LifeAddView = null;
    [SerializeField] private Button LifeAddButton = null;
    [SerializeField] private GameObject LoginBonusView = null;
    [SerializeField] private GameObject LifeTimeBoard = null;
    [SerializeField] private Text LifeRTime = null;

    [SerializeField] private GameObject GoOutView = null;
    [SerializeField] private GameObject MoveView = null;
    [SerializeField] private Text MoveViewText = null;

    [SerializeField] private HelpView Help = null;
    [SerializeField] private SettingView Setting = null;
    [SerializeField] private GameObject FirstLockViwe = null;
    [SerializeField] private GameObject[] FirstLockButton = null;

    private int MovieNum = 0;           //ムービーのナンバーを記憶
    private int AppNum = 0;             //アプリのナンバーを記憶
    private int MoviePage = 1;
    private int MaxPage;

    private string MoveText = "";

  //  private RewardedAd ExpRewardedAd;
  //  private RewardedAd HealthRewardedAd;
  //  private RewardedAd LifeRewardedAd;
    private int RewardType = 0;
    private bool RewardSuc = false;
   // private int[] RewardStage = new int[4];

    private InterstitialAd interstitial;
    private bool cmFinish = false;
    private float cmFinishDelta = 0;
    private float cmFinishLagTime = 0.5f;
    private bool ReloadShowADFlag = false;       //広告の読み込みが終わってない場合の再表示を試みるフラグ
    private int RetryShowADCount = 0;            //再読み込みを試みた回数
    private float RetryInterval = 1f;             //再読み込みを繰り返す間隔
    private int RetryMaxCount = 5;               //再読み込みを最大何回行うか
    private float RetryDelta = 0;
    private bool NoLoadingAD = false;

    private BannerView bannerView;

    private Animator animator;
    private DateTime LifeStartTime;

    private void Start()
    {
        animator = GetComponent<Animator>();

        int SpriteC = MovieSprite.Length;
        float PageF = (float)SpriteC / 2.0f;
        MaxPage = Mathf.CeilToInt(PageF);

        if (!GameManager.AndroidFlag)
        {
            StartCoroutine(VersionCheckIOS());
        }

        GameManager.stopLoopSE();
        if (GameManager.AndroidFlag) OtherAppButton.SetActive(false);

        OpenInfoView();
        UpdateLife();

        //セーブポイントの初期化
        PlayerController.SavePoint = Vector3.zero;
        
        if(GameManager.LoginBonusFlag)
        {
            GameManager.LoginBonusFlag = false;
            OpneLoginView();
        }
        if (!GameManager.eventFlag[0])
        {
            GameManager.LifeRecoverTimeUpdate();
            if (GameManager.LifeRecoverStartTimeText != "")
            {
                LifeStartTime = DateTime.FromBinary(Convert.ToInt64(GameManager.LifeRecoverStartTimeText));
            }
        }

        //インタースティシャル広告の読み込み
        if (!GameManager.eventFlag[0] && (GameManager.eventCount[8] >= GameManager.CMCount - 1 || GameManager.PlayerHp <= 0) ) 
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

        //リワード広告の読み込み
        if (GameManager.RewardStage[1] == 0)
        {
            GameManager.RewardStage[1] = 1;
            GameManager.HealthRewardedAd = CreateAndLoadRewardedAd(GameManager.RewardID);
        }
        if (GameManager.RewardStage[2] == 0)
        {
            GameManager.RewardStage[2] = 1;
            GameManager.ExpRewardedAd = CreateAndLoadRewardedAd(GameManager.RewardID);
        }
        if (GameManager.RewardStage[3] == 0)
        {
            GameManager.RewardStage[3] = 1;
            GameManager.LifeRewardedAd = CreateAndLoadRewardedAd(GameManager.RewardID);
        }

        if (GameManager.stageClearStatas[1] != 0)
        {
            for(int i = 0; i < FirstLockButton.Length; i++)
            {
                FirstLockButton[i].SetActive(false);
            }
        }

        //バナー広告の読み込みと表示
        ViewBanner();
    }

    private void Update()
    {
        float dtime = Time.deltaTime;
        LifeTimeUpdate();
        ReloadInterAD(dtime);    //広告の再表示処理(読み込みが完了していない場合)
        CMFinish(dtime);         //広告表示が終了してからの処理
    }

    private string StoreURL;
   
    

    /// <summary>
    /// メニューを開く
    /// </summary>
    public void OpenMenuView()
    {
        GameManager.playSystemSE(1);
        HideBanner();
        MenuButton.interactable = false;
        MenuView.SetActive(true);
    }

    /// <summary>
    /// メニューを閉じるs
    /// </summary>
    public void CloseMenuView()
    {
        GameManager.playSystemSE(4);
        ShowBanner();
        MenuButton.interactable = true;
        MenuView.SetActive(false);
    }

    //********************アプリの最新バージョンがあるかの確認
    
    IEnumerator VersionCheckIOS()
    {

        var url = string.Format("https://itunes.apple.com/lookup?bundleId={0}", Application.identifier);
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        //3.isNetworkErrorとisHttpErrorでエラー判定
        if (request.isHttpError || request.isNetworkError)
        {
            //4.エラー確認
            Debug.Log(request.error);
        }
        else
        {
            //4.結果確認
            Debug.Log("ダウンロード成功" + request.downloadHandler.text);
            if (!string.IsNullOrEmpty(request.downloadHandler.text))
            {
                //JSONをデシリアライズしてストアにあるAPPのバージョンとストアURLを取得
                var lookupData = JsonUtility.FromJson<AppLookupData>(request.downloadHandler.text);

                if (lookupData.resultCount > 0 && lookupData.results.Length > 0)
                {
                    var result = lookupData.results[0];
                    Debug.Log(result.version);
                    if (VersionComparative(result.version))
                    {
                        //ストアにある最新バージョンと現在のバージョンが異なる場合に実行される。
                        StoreURL = result.trackViewUrl;
                        GameManager.playSystemSE(1);
                        ConfirmationView[2].SetActive(true);
                    }
                }
            }
        }
    }

    [Serializable]
    public class AppLookupData
    {
        public int resultCount;
        public AppLookupResult[] results;
    }

    [Serializable]
    public class AppLookupResult
    {
        public string version;
        public string trackViewUrl;
    }

    bool VersionComparative(string storeVersionText)
    {
        if (string.IsNullOrEmpty(storeVersionText))
        {
            return false;
        }
        try
        {
            var storeVersion = new System.Version(storeVersionText);
            var currentVersion = new System.Version(Application.version);

            if (storeVersion.CompareTo(currentVersion) > 0)
            {
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("{0} VersionComparative Exception caught.", e);
        }
        return false;
    }

    public void YesUpdate()
    {
        GameManager.playSystemSE(1);
        ConfirmationView[2].SetActive(false);
        Application.OpenURL(StoreURL);
    }

    public void NoUpdate()
    {
        GameManager.playSystemSE(4);
        ConfirmationView[2].SetActive(false);
    }

    //********************アプリの最新バージョンがあるかの確認

    //**********************課金に関する関数****************************************
    //課金ビューを開く
    public void OpenPurchaseView()
    {
        GameManager.playSystemSE(1);
        if (GameManager.eventFlag[0]) nocmButton.interactable = false; else nocmButton.interactable = true;
        restoreButton.interactable = true;
        MoneyView.SetActive(true);
    }

    //課金ビューを閉じる
    public void ClosePurchaseView()
    {
        GameManager.playSystemSE(4);
        MoneyView.SetActive(false);
        UpdateLife();
    }
    //**********************課金に関する関数****************************************



    //**********************猫の動画に関する関数****************************************
    public void OpenCatMovieView()
    {
        //ビューの初期化
        for(int i = 0; i < MovieButton.Length; i++)
        {
            MovieButton[i].image.sprite = MovieSprite[i];
        }
        PageText.text = "1/" + MaxPage;

        GameManager.playSystemSE(1);
        CatMovieView.SetActive(true);
    }

    //アプリ紹介ビューを閉じる
    public void CloseCatMovieView()
    {
        GameManager.playSystemSE(4);
        CatMovieView.SetActive(false);
    }

    public void OpenMovieConView2(int Num)
    {
        MovieNum = Num + MoviePage * 2;
        GameManager.playSystemSE(1);
        ConfirmationView[0].SetActive(true);
    }


    public void RightMove()
    {
        GameManager.playSystemSE(1);
        int BfoPage = MoviePage;
        MoviePage += 1;
        if (MoviePage > MaxPage) MoviePage = 1;
        MovieButton[0].image.sprite = MovieSprite[(MoviePage - 1) * 2];
        if ((MoviePage - 1) * 2 + 1 == MovieSprite.Length)
        {
            MovieButton[1].image.sprite = MovieSprite[1];
        }
        else
        {
            MovieButton[1].image.sprite = MovieSprite[(MoviePage - 1) * 2 + 1];
        }
        MovieButton[4].image.sprite = MovieSprite[(BfoPage - 1) * 2];
        if ((BfoPage - 1) * 2 + 1 == MovieSprite.Length)
        {
            MovieButton[5].image.sprite = MovieSprite[1];
        }
        else
        {
            MovieButton[5].image.sprite = MovieSprite[(BfoPage - 1) * 2 + 1];
        }
        MovieButton[0].interactable = false;
        MovieButton[1].interactable = false;
        MovieButton[4].interactable = false;
        MovieButton[5].interactable = false;
        UIButtonOff();
        animator.SetTrigger("RightMove");
    }
    public void RightMoveFinish()
    {
        PageText.text = MoviePage + "/" + MaxPage;
        MovieButton[0].interactable = true;
        MovieButton[1].interactable = true;
        MovieButton[4].interactable = true;
        MovieButton[5].interactable = true;
        UIButtonOn();
    }

    public void LeftMove()
    {
        GameManager.playSystemSE(1);
        int BfoPage = MoviePage;
        MoviePage -= 1;
        if (MoviePage < 1) MoviePage = MaxPage;
        MovieButton[0].image.sprite = MovieSprite[(MoviePage - 1) * 2];
        if((MoviePage - 1) * 2 + 1 == MovieSprite.Length)
        {
            MovieButton[1].image.sprite = MovieSprite[1];
        }
        else
        {
            MovieButton[1].image.sprite = MovieSprite[(MoviePage - 1) * 2 + 1];
        }
        MovieButton[2].image.sprite = MovieSprite[(BfoPage - 1) * 2];
        if ((BfoPage - 1) * 2 + 1 == MovieSprite.Length)
        {
            MovieButton[3].image.sprite = MovieSprite[1];
        }
        else
        {
            MovieButton[3].image.sprite = MovieSprite[(BfoPage - 1) * 2 + 1];
        }
        MovieButton[0].interactable = false;
        MovieButton[1].interactable = false;
        MovieButton[2].interactable = false;
        MovieButton[3].interactable = false;
        UIButtonOff();
        animator.SetTrigger("LeftMove");
    }

    public void LeftMoveFinish()
    {      
        PageText.text = MoviePage + "/" + MaxPage;
        MovieButton[0].interactable = true;
        MovieButton[1].interactable = true;
        MovieButton[2].interactable = true;
        MovieButton[3].interactable = true;
        UIButtonOn();
    }

    private void UIButtonOn()
    {
        for(int i = 0; i < UIButton.Length; i++)
        {
            UIButton[i].interactable = true;
        }
    }
    private void UIButtonOff()
    {
        for (int i = 0; i < UIButton.Length; i++)
        {
            UIButton[i].interactable = false;
        }
    }

    //**********************猫の動画に関する関数****************************************


    //**********************アプリ紹介に関する関数****************************************
    //アプリ紹介ビューを開く
    public void OpenOtherAppView()
    {
        GameManager.playSystemSE(1);
        OtherGameView.SetActive(true);
    }

    //アプリ紹介ビューを閉じる
    public void CloseOtherAppView()
    {
        GameManager.playSystemSE(4);
        OtherGameView.SetActive(false);
    }

    public void OpenAppView(int Num)
    {
        GameManager.playSystemSE(1);
        AppView[Num].SetActive(true);
    }

    public void CloseAppView(int Num)
    {
        GameManager.playSystemSE(4);
        AppView[Num].SetActive(false);
    }

    public void OpenAppConView(int Num)
    {
        AppNum = Num;
        GameManager.playSystemSE(1);
        ConfirmationView[1].SetActive(true);
    }

    public void OpenMovieConView(int Num)
    {
        MovieNum = Num;
        GameManager.playSystemSE(1);
        ConfirmationView[0].SetActive(true);
    }

    public void CloseAppConView()
    {
        GameManager.playSystemSE(4);
        ConfirmationView[1].SetActive(false);
    }

    public void CloseMovieConView()
    {
        GameManager.playSystemSE(4);
        ConfirmationView[0].SetActive(false);
    }

    public void AppDownload()
    {
        int Num = AppNum;
        Debug.Log("ダウンロード  " + Num);
#if UNITY_IOS
        string IOSNum = "0000000000"; 
        if(Num == 0)
        {
            IOSNum = "1436678056";
        }
        else if (Num == 1)
        {
            IOSNum = "1457372354";
        }
        else if (Num == 2)
        {
            IOSNum = "1474041840";
        }
        else
        {
            IOSNum = "1474041840";
        }


        string url = "itms-apps://itunes.apple.com/jp/app/id" + IOSNum +"?mt=8";
#else
        string url = "market://details?id=XXXXXXXX";
#endif
        Application.OpenURL(url);
    }

    public void YouTubeMovie()
    {
        int Num = MovieNum;
        Debug.Log("動画  " + Num);
        string url = "";
        if (Num == 0)
        {
            url = "https://www.youtube.com/watch?v=DklZskxPDNY";
        }
        else if (Num == 1)
        {
            url = "https://www.youtube.com/watch?v=rYItYYsi3";
        }
        else if (Num == 2)
        {
            url = "https://www.youtube.com/watch?v=xnMzzSbgpQw";
        }
        else if (Num == 3)
        {
            url = "https://www.youtube.com/watch?v=iEsZQxuGKXM";
        }
        else if (Num == 4)
        {
            url = "https://www.youtube.com/watch?v=_A28Qqa6IYU";
        }
        else if (Num == 5)
        {
            url = "https://www.youtube.com/watch?v=j9NhETh-ke4";
        }
        else if (Num == 6)
        {
            url = "https://www.youtube.com/watch?v=t7pTxBYJqqE";
        }
        else if (Num == 7)
        {
            url = "https://www.youtube.com/watch?v=ejtbUNUDMaY";
        }
        else if (Num == 8)
        {
            url = "https://www.youtube.com/watch?v=xnMzzSbgpQw";
        }
        else
        {
            url = "https://www.youtube.com/watch?v=iEsZQxuGKXM";
        }
        Application.OpenURL(url);
    }
    //**********************アプリ紹介に関する関数****************************************

    /// <summary>
    /// 設定ビューを開く
    /// </summary>
    public void OpenSettingView()
    {
        Setting.OpenSettingView();
    }

    public void OpenTwitterView()
    {
        TwitterView.SetActive(true);
        GameManager.playSystemSE(1);
    }

    public void CloseTwitterView()
    {
        TwitterView.SetActive(false);
        GameManager.playSystemSE(3);
    }

    public void OpenTwitter()
    {
        string url = "http://twitter.com/ponkitigames";
        Application.OpenURL(url);
    }

    /// <summary>
    /// ライフを加算するビューを表示
    /// </summary>
    public void OpenLifeAddView()
    {
        LifeNumText2.text = GameManager.PlayerLife.ToString();
        LifeAddView.SetActive(true);
        GameManager.playSystemSE(1);
        HideBanner();
    }

    public void CloseLifeAddView()
    {
        LifeAddView.SetActive(false);
        GameManager.playSystemSE(3);
        ShowBanner();
    }

    private void OpneLoginView()
    {
        LoginBonusView.SetActive(true);
        GameManager.playSystemSE(11);
        GameManager.PlayerLife += 3;
        if (GameManager.PlayerLife > GameManager.PlayerLifeMax) GameManager.PlayerLife = GameManager.PlayerLifeMax;
        UpdateLife();
    }

    public void CloseLoginView()
    {
        LoginBonusView.SetActive(false);
        GameManager.playSystemSE(1);
    }

    private void LifeTimeUpdate()
    {
        if (GameManager.eventFlag[0]) return;
        if (GameManager.PlayerLife >= GameManager.PlayerLifeMaxR) return;
        TimeSpan Span = DateTime.Now - LifeStartTime;
        if (Span.TotalMinutes >= GameManager.LifeSpanTime)
        {
            GameManager.LifeRecoverTimeUpdate();
            if (GameManager.PlayerLife >= GameManager.PlayerLifeMaxR)
            {
                LifeTimeBoard.SetActive(false);
                return;
            }
            else
            {
                LifeStartTime = DateTime.FromBinary(Convert.ToInt64(GameManager.LifeRecoverStartTimeText));
                Span = DateTime.Now - LifeStartTime;
            }
        }
        //時間のテキスト更新
        TimeSpan NextTime = new TimeSpan(0, GameManager.LifeSpanTime, 0) - Span;
        LifeRTime.text = NextTime.Minutes.ToString() + "分" + NextTime.Seconds.ToString() + "秒";
    }

    /// <summary>
    /// ステージ開始処理
    /// </summary>
    public void stageStart()
    {
        DestroyBanner();
        if (!cmFinish) GameManager.eventCount[8] += 1;
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
    }

    /// <summary>
    /// タイトルシーンに戻る
    /// </summary>
    public void backTitle()
    {
        DestroyBanner();
        if (interstitial != null) interstitial.Destroy();
        GameManager.GoTitle();
        GameManager.playSystemSE(1);
    }

    /// <summary>
    /// ヘルプビューを開く
    /// </summary>
    /// <param name="num"></param>
    public void OpenHelpView(int num)
    {
        if (num == 0) HideBanner();
        Help.OpenView(num);
    }

    /// <summary>
    /// ヘルプビューを閉じる
    /// </summary>
    /// <param name="num"></param>
    public void CloseHelpView(int num)
    {
        if (num == 0) ShowBanner();
        Help.CloseView(num);
    }

    /// <summary>
    /// アップデート時の情報表示
    /// </summary>
    private void OpenInfoView()
    {
        if (GameManager.eventCount[15] < GameManager.InfoNumber)
        {
            GameManager.eventCount[15] = GameManager.InfoNumber;
            OpenHelpView(1);
            GameManager.saveGameData();
        }
    }

    /// <summary>
    /// ライフの表示をアップデートする。
    /// </summary>
    private void UpdateLife()
    {
        if (GameManager.eventFlag[0])
        {
            LifeNumText.gameObject.SetActive(false);
            LifeNumText3.SetActive(true);
            LifeAddButton.gameObject.SetActive(false);
        }
        else
        {
            if (GameManager.PlayerLife >= GameManager.PlayerLifeMax) LifeAddButton.interactable = false;
            LifeNumText.text = GameManager.PlayerLife.ToString();
            if (GameManager.PlayerLife >= GameManager.PlayerLifeMaxR) LifeTimeBoard.SetActive(false);
            else
            {
                LifeTimeUpdate();
                LifeTimeBoard.SetActive(true);
            }
        }
        GameManager.saveGameData();
    }

    /// <summary>
    /// パワーアップビューを開く
    /// </summary>
    public void OpneRewardView()
    {
        GameManager.playSystemSE(1);
        if (GameManager.eventCount[12] == 4) RewardButton[0].interactable = false;
        if (GameManager.eventCount[13] == 4) RewardButton[1].interactable = false;
        RewardView[0].SetActive(true);
    }

    /// <summary>
    /// パワーアップビューを閉じる
    /// </summary>
    public void CloseRewardView()
    {
        GameManager.playSystemSE(4);
        RewardView[0].SetActive(false);
    }

    /// <summary>
    /// 外出ボタンを表示する
    /// </summary>
    public void OpenGoOutView()
    {
        GameManager.playSystemSE(1);
        GoOutView.SetActive(true);
        HideBanner();
    }

    /// <summary>
    /// 外出ボタンを閉じる
    /// </summary>
    public void CloseGoOutView()
    {
        GameManager.playSystemSE(4);
        GoOutView.SetActive(false);
        ShowBanner();
    }

    public void OpenMoveView(string moveText)
    {
        MoveText = moveText;
        MoveViewText.text = moveText;
        GameManager.playSystemSE(1);
        MoveView.SetActive(true);
    }

    public void CloseMoveView()
    {
        GameManager.playSystemSE(4);
        MoveView.SetActive(false);
    }

    public void GoMove()
    {
        GameManager.playSystemSE(1);
        if (MoveText == "ショップ")
        {
            MoveShop();
        }
        if (MoveText == "トレーニングルーム")
        {
            MoveTraining();
        }
        if (MoveText == "練習部屋")
        {
            MoveParctice();
        }

    }

    /// <summary>
    /// 初期ロックボタンの確認ビューを開く
    /// </summary>
    public void OpenFirstLockView()
    {
        GameManager.playSystemSE(1);
        FirstLockViwe.gameObject.SetActive(true);
    }

    /// <summary>
    /// 初期ロックボタンの確認ビューを閉じる
    /// </summary>
    public void CloseFirstLockView()
    {
        GameManager.playSystemSE(3);
        FirstLockViwe.gameObject.SetActive(false);
    }

    /// <summary>
    /// ショップに移動する
    /// </summary>
    private void MoveShop()
    {
        DestroyBanner();
        if (interstitial != null) interstitial.Destroy();
        GameManager.TitleSoundFlag = true;
        GameManager.playSystemSE(1);
        GameManager.playShopBGM();
        GameManager.GoShop();
    }

    /// <summary>
    /// トレーニングルームに移動する
    /// </summary>
    private void MoveTraining()
    {
        DestroyBanner();
        if (interstitial != null) interstitial.Destroy();
        GameManager.TitleSoundFlag = true;
        GameManager.playSystemSE(1);
        GameManager.FieldNum = 1;
        GameManager.FieldPoint = Vector3.zero;

        GameManager.SaveHp = GameManager.PlayerHp;
        GameManager.SaveMp = GameManager.PlayerMp;
        GameManager.SaveLv = GameManager.PlayerLv;

        GameManager.GoStage(0);
    }

    /// <summary>
    /// 練習部屋に移動
    /// </summary>
    private void MoveParctice()
    {
        DestroyBanner();
        if (interstitial != null) interstitial.Destroy();
        GameManager.TitleSoundFlag = true;
        GameManager.playSystemSE(1);
        GameManager.FieldNum = 1;
        GameManager.FieldPoint = Vector3.zero;

        GameManager.SaveHp = GameManager.PlayerHp;
        GameManager.SaveMp = GameManager.PlayerMp;
        GameManager.SaveLv = GameManager.PlayerLv;

        GameManager.GoStage(100);
    }

    //ここから広告関連の関数********************************************************************************
    //**********************リワード広告の設定**************************************************************
    /// <summary>
    /// 超健康状態の成功時に呼ばれるビューを閉じる
    /// </summary>
    public void CloseHealthRewardView()
    {
        GameManager.playSystemSE(4);
        RewardView[1].SetActive(false);
    }

    /// <summary>
    /// 経験値2倍状態の成功時に呼ばれるビューを閉じる
    /// </summary>
    public void CloseExpRewardView()
    {
        GameManager.playSystemSE(4);
        RewardView[2].SetActive(false);
    }

    /// <summary>
    /// ライフ加算成功時に呼ばれるビューを閉じる
    /// </summary>
    public void CloseLifeRewardView()
    {
        GameManager.playSystemSE(4);
        RewardView[5].SetActive(false);
    }

    /// <summary>
    /// 超健康状態になるためのリワード広告の表示
    /// </summary>
    public void ShowHealthAd()
    {
        RewardSuc = false;
        RewardType = 1;
        if (GameManager.HealthRewardedAd.IsLoaded())
        {
            RewardButton[0].interactable = false;
            GameManager.stopBGM();
            GameManager.HealthRewardedAd.Show();
        }
        else
        {
            GameManager.playSystemSE(3);
            RewardView[3].SetActive(true);
        }
    }

    /// <summary>
    /// 経験値2倍状態になるためのリワード広告の表示
    /// </summary>
    public void ShowExpAd()
    {
        RewardSuc = false;
        RewardType = 2;
        
        if (GameManager.ExpRewardedAd.IsLoaded())
        {
            RewardButton[1].interactable = false;
            GameManager.stopBGM();
            GameManager.ExpRewardedAd.Show();
        }
        else
        {
            GameManager.playSystemSE(3);
            RewardView[3].SetActive(true);
        }
    }

    /// <summary>
    /// ライフを増やすリワード広告を表示
    /// </summary>
    public void ShowLifeAd()
    {
        RewardSuc = false;
        RewardType = 3;
        LifeAddView.SetActive(false);

        //RewardButton[1].interactable = false;
        if (GameManager.LifeRewardedAd.IsLoaded())
        {
            GameManager.stopBGM();
            GameManager.LifeRewardedAd.Show();
        }
        else
        {
            GameManager.playSystemSE(3);
            RewardView[3].SetActive(true);
        }
    }

    /// <summary>
    /// 再度リワード広告の表示を試みる
    /// </summary>
    public void RetryShow()
    {
        if(RewardType == 1)
        {
            if (GameManager.HealthRewardedAd.IsLoaded())
            {
                RewardView[3].SetActive(false);
                GameManager.stopBGM();
                GameManager.HealthRewardedAd.Show();
            }
            else
            {
                GameManager.playSystemSE(3);
            }
        }
        else if (RewardType == 2)
        {
            if (GameManager.ExpRewardedAd.IsLoaded())
            {
                RewardButton[1].interactable = false;
                RewardView[3].SetActive(false);
                GameManager.ExpRewardedAd.Show();
            }
            else
            {
                GameManager.playSystemSE(3);
            }
        }
        else if (RewardType == 3)
        {
            if (GameManager.LifeRewardedAd.IsLoaded())
            {
                RewardView[3].SetActive(false);
                GameManager.LifeRewardedAd.Show();
            }
            else
            {
                GameManager.playSystemSE(3);
            }
        }
    }

    /// <summary>
    /// 再度広告表示を試みるかを確認するビューを閉じる
    /// </summary>
    public void CloseReloadView()
    {
        GameManager.playSystemSE(3);
        RewardView[3].SetActive(false);
    }

    /// <summary>
    /// 広告表示失敗を知らせるビューを閉じる
    /// </summary>
    public void CloseFailureView()
    {
        GameManager.playSystemSE(4);
        RewardView[3].SetActive(false);
    }

    /// <summary>
    /// リワード広告の作成と読み込み
    /// </summary>
    /// <param name="adUnitId"></param>
    /// <returns></returns>
    public RewardedAd CreateAndLoadRewardedAd(string adUnitId)
    {
        RewardedAd rewardedAd = new RewardedAd(adUnitId);

        // rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when an ad request failed to show.
        rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;

        rewardedAd.OnAdClosed += HandleRewardedAdClosed;


        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        rewardedAd.LoadAd(request);

        return rewardedAd;
    }

    //広告イベント
    /// <summary>
    /// 広告の表示が失敗した時の処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        MonoBehaviour.print(
            "HandleRewardedAdFailedToShow event received with message: "
                             + args.Message);
        GameManager.playSystemSE(3);
        RewardView[4].SetActive(true);
    }

    /// <summary>
    /// リワード広告の報酬を受け取る処理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void HandleUserEarnedReward(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        MonoBehaviour.print(
            "HandleRewardedAdRewarded event received for "
                        + amount.ToString() + " " + type);
        RewardSuc = true;
        if (RewardType == 1)
        {
            GameManager.eventCount[12] = 4;
            GameManager.saveGameData();
            RewardView[1].SetActive(true);
        }

        if (RewardType == 2)
        {
            GameManager.eventCount[13] = 4;
            GameManager.saveGameData();
            RewardView[2].SetActive(true);
        }

        if (RewardType == 3)
        {
            GameManager.PlayerLife += 2;
            if (GameManager.PlayerLife > GameManager.PlayerLifeMax) GameManager.PlayerLife = GameManager.PlayerLifeMax;
            RewardView[5].SetActive(true);
            UpdateLife();
        }
        
    }

    /// <summary>
    /// リワード広告を閉じる
    /// /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        MonoBehaviour.print("HandleRewardedAdClosed event received");
        GameManager.playTitleBGM();
        GameManager.RewardStage[RewardType] = 0;
        /*
        if (RewardSuc)
        {
           // GameManager.eve
            if (RewardType == 1)
            {
                //             RewardView[1].SetActive(true);
               // GameManager.HealthRewardedAd = CreateAndLoadRewardedAd(GameManager.RewardID);
            }
            if (RewardType == 2)
            {
                //          RewardView[2].SetActive(true);
               // GameManager.ExpRewardedAd = CreateAndLoadRewardedAd(GameManager.RewardID);
            }
            if (RewardType == 3)
            {
        //        RewardView[5].SetActive(true);
              //  GameManager.LifeRewardedAd = CreateAndLoadRewardedAd(GameManager.RewardID);
            }
           // LifeRewardedAd = CreateAndLoadRewardedAd(GameManager.RewardID);
           
        }
        */
    }

    private void DestroyRewardAD()
    {
    //   if(ExpRewardedAd != null) Destroy(ExpRewardedAd)
    }

    //********************:***インタースティシャル広告の関数
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
    private void CMFinish(float dTime)
    {
        if (!cmFinish) return;
        cmFinishDelta += dTime;
        if (cmFinishDelta >= cmFinishLagTime)
        {
            cmFinishDelta = 0;
            stageStart();
        }
    }

    /// <summary>
    /// 広告読み込み途中に表示しようとした時の処理
    /// </summary>
    private void ReloadInterAD(float dTime)
    {
        if (!ReloadShowADFlag) return;
        RetryDelta += dTime;
        if (RetryDelta >= RetryInterval)
        {
            RetryDelta = 0;
            RetryShowADCount += 1;
            if (RetryShowADCount <= RetryMaxCount)
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

    //************************バナー広告の関数
    private void ViewBanner()
    {
        if (!GameManager.eventFlag[0])
        {
            // Create a 320x50 banner at the top of the screen.
            bannerView = new BannerView(GameManager.BannerID, AdSize.Banner, AdPosition.Bottom);

            bannerView.OnAdFailedToLoad += HandleOnAdFailedToLoadBanner;

            // Create an empty ad request.
            AdRequest request = new AdRequest.Builder().Build();

            // Load the banner with the request.
            bannerView.LoadAd(request);
        }
    }

    /// <summary>
    /// バナーの消去
    /// </summary>
    public void DestroyBanner()
    {
        if (bannerView != null)
        {
            bannerView.Hide();
            bannerView.Destroy();
        }
    }

    /// <summary>
    /// バナーを隠す
    /// </summary>
    private void HideBanner()
    {
        if (bannerView != null)
        {
            bannerView.Hide();
        }
    }

    /// <summary>
    /// バナーを表示する
    /// </summary>
    private void ShowBanner()
    {
        if (bannerView != null)
        {
            bannerView.Show();
        }
    }

    /// <summary>
    /// バナー広告のロード失敗時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public void HandleOnAdFailedToLoadBanner(object sender, AdFailedToLoadEventArgs args)
    {
        MonoBehaviour.print("HandleFailedToReceiveAd event received with message: "
                            + args.Message);
    }
}
