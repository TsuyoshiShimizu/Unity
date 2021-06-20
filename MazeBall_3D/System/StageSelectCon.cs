using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using UnityEngine.Purchasing;
using HtmlAgilityPack;

#if UNITY_ANDROID
using Firebase;
using Firebase.Unity.Editor;
using Firebase.Database;
#endif

public class StageSelectCon : MonoBehaviour,IStoreListener
{
    [SerializeField] private GameObject[] StageButton = null;
    [SerializeField] private Text[] StageButtonText = null;
    [SerializeField] private Image[] StageClearFlag = null;
    [SerializeField] private Sprite[] ClearFlagSprite = null;
    [SerializeField] private GameObject[] StageLock = null;
    [SerializeField] private Button NextPageButton = null;
    [SerializeField] private Button BackPageButton = null;
    [SerializeField] private Button ReturnButton = null;
    [SerializeField] private Button SettingButton = null;
    [SerializeField] private GameObject MenuView = null;
    [SerializeField] private Text[] cointext = null;
   
    [SerializeField] private GameObject[] UIView = null;
    [SerializeField] private Button UnlockB = null;

    [SerializeField] private Text[] UnlockViewText = null;
    [SerializeField] private Text[] StageTimeText = null;
    [SerializeField] private Color[] TextColor = null;

    [SerializeField] private Button MenuCloseButton = null;
    [SerializeField] private Text[] SettingText = null;
    [SerializeField] private Slider[] SettingSlider = null;

    [SerializeField] private Button FullPButton = null;
    [SerializeField] private GameObject[] PView = null;
    [SerializeField] private Button[] PButton = null;
    [SerializeField] private GameObject OtherAppButton = null;
    [SerializeField] private GameObject PurchasesButton = null;

    private int maxPage;
    private int LeftBNum = 0;
    private int RightBNum = 0;
    private int SpendCoins = 0;
    private int PlayStageNum = 0;

    private int coinN = 0;

    private string StoreURL;

    private List<int> ViewStageButtonNum = new List<int>();

    private IStoreController controller;
    private IExtensionProvider extensions;
    private string NonConsumable = "BallSpatialFull";
    private bool RestoreFlag = false;
    private float RestoreDelta = 0;

    private string AppVersion;
    private bool UpdateFlag = false;
#if UNITY_ANDROID
    private DatabaseReference VersionNum;
#endif
    private void Awake()
    {
        GameManager.playStageSelectBGM();

        // If we haven't set up the Unity Purchasing reference
        if (controller == null)
        {
            // Begin to configure our connection to Purchasing
            InitializePurchasing();
        }

        if (GameManager.eventFlag[0])
        {
            for (int i = 0; i <= GameManager.MaxStageNum; i++)
            {
                GameManager.stageUnLockFlag[i] = true;
            }
        }
    }
    private void Start()
    {
        //ページ切り替えボタンの有効の切り替え
        maxPage = (GameManager.MaxStageNum - 1) / 20 + 1;
        PageChangeButtonEnable();
        UpdateStageButton();
        

        AppVersion = Application.version;

        if (GameManager.FirstFlag)
        {
            GameManager.FirstFlag = false;
#if UNITY_ANDROID
            FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://spatialball.firebaseio.com/");
            VersionNum = FirebaseDatabase.DefaultInstance.GetReference("VersionNum");
#endif
            //バージョンチェックを開始
            CurrentVersionCheck();
            if (LoadInvalidVersionUpCheck()) return;

#if UNITY_IOS
            StartCoroutine(VersionCheckIOS());
#elif UNITY_ANDROID
            VersionCheckAndroid();
#endif
        }
    }

    private void FixedUpdate()
    {
        if(coinN != GameManager.coin)
        {
            cointext[0].text = GameManager.coin.ToString();
            cointext[1].text = GameManager.coin.ToString();
            coinN = GameManager.coin;
        }

        if (RestoreFlag)
        {
            RestoreDelta += 0.02f;
            if(RestoreDelta >= 6.0f)
            {
                RestoreFlag = false;
                RestoreDelta = 0;
                OpenFaildView3();
            }
        }

        if (UpdateFlag)
        {
            UpdateFlag = false;
            OpenUpdateView();
        }
    }

    /// <summary>
    /// ステージボタン
    /// </summary>
    /// <param name="SNum">ステージボタンの位置番号</param>
    public void MoveStage(int SNum)
    {
        GameManager.playSystemSE(0);
        PlayStageNum = ViewStageButtonNum[SNum];

        StageTimeText[4].text = "Stage " + PlayStageNum;

        float[] Stime = GameManager.getStageTime(PlayStageNum);
        StageTimeText[1].text = "00:00:00～" + TimeConvert(Stime[1]);
        StageTimeText[2].text = TimeConvert(Stime[1]) + "～" + TimeConvert(Stime[0]);
        StageTimeText[3].text = TimeConvert(Stime[0]) + "～99:59:99";

        float BestTime = GameManager.clearTime[PlayStageNum];

        if (BestTime != 0)
        {
            StageTimeText[0].text = TimeConvert(BestTime);
            if (BestTime <= Stime[1]) StageTimeText[0].color = TextColor[0];
            else if (BestTime <= Stime[0]) StageTimeText[0].color = TextColor[1];
            else StageTimeText[0].color = TextColor[2];
        }
        else
        {
            StageTimeText[0].text = "--:--:--";
            StageTimeText[0].color = Color.black;
        }
            
        UIView[3].SetActive(true);
    }

    /// <summary>
    /// ロックボタン
    /// </summary>
    /// <param name="SNum">ステージボタンの位置番号</param>
    public void LockButton(int SNum)
    {
        int UnlockNum = ViewStageButtonNum[SNum];
        GameManager.playSystemSE(0);
        if (UnlockNum <= 8) UIView[0].SetActive(true);
        else
        {
            int LNum = UnlockSetup(UnlockNum);
            if (LNum <= 20) SpendCoins = (LNum - 1) / 2 * 5; else SpendCoins = 50;

            if (GameManager.coin < SpendCoins) UnlockB.interactable = false; else UnlockB.interactable = true;

            UnlockViewText[0].text = "Stage" + LeftBNum + "～" + RightBNum;
            string beforeY = "<color=#ffff00>";
            string afterY = "</color>";
            UnlockViewText[1].text = beforeY + SpendCoins + "コイン" + afterY + "消費する\nSpend " + beforeY + SpendCoins + " coins" + afterY;

            UIView[1].SetActive(true);
        }
    }

    /// <summary>
    /// タイトル画面に戻るボタン
    /// </summary>
    public void BackTitle()
    {
        GameManager.GoTitle();
        GameManager.playSystemSE(1);
    }

    /// <summary>
    /// 次のページの移動するボタン
    /// </summary>
    public void NextPageMove()
    {
        GameManager.StagePage += 1;
        PageChangeButtonEnable();
        UpdateStageButton();
        GameManager.playSystemSE(0);
    }

    /// <summary>
    /// 前のページの移動するボタン
    /// </summary>
    public void BackPageMove()
    {
        GameManager.StagePage -= 1;
        PageChangeButtonEnable();
        UpdateStageButton();
        GameManager.playSystemSE(0);
    }

    /// <summary>
    /// メニュービューを開くボタン
    /// </summary>
    public void OpenMenuButton()
    {
        if (!GameManager.IOSFlag)
        {
            OtherAppButton.SetActive(false);
        }

        GameManager.playSystemSE(0);   
        MenuView.SetActive(true);
        NextPageButton.interactable = false;
        BackPageButton.interactable = false;
        ReturnButton.interactable = false;
        SettingButton.interactable = false;
        if (GameManager.eventFlag[0]) FullPButton.interactable = false; else FullPButton.interactable = true;
    }

    /// <summary>
    /// メニュービューを閉じるボタン
    /// </summary>
    public void CloseMenuButton()
    {
        GameManager.playSystemSE(1);
        MenuView.SetActive(false);
        ReturnButton.interactable = true;
        SettingButton.interactable = true;
        PageChangeButtonEnable();
    }

    /// <summary>
    /// ページ移動ボタンの更新
    /// </summary>
    private void PageChangeButtonEnable()
    {
        if (GameManager.StagePage == 1) BackPageButton.interactable = false; else BackPageButton.interactable = true;
        if (GameManager.StagePage == maxPage) NextPageButton.interactable = false; else NextPageButton.interactable = true;
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
        for (int i = 1; i <= 20; i++)
        {
            //ステージ番号の更新
            int StageButtonNum = i + 20 * (Page - 1);
            ViewStageButtonNum.Add(StageButtonNum);

            //ボタンのステージナンバーを更新
            StageButtonText[i - 1].text = StageButtonNum.ToString();

            //ステージボタンの有効化の切り替え
            if (StageButtonNum > GameManager.MaxStageNum)
            {
                StageButton[i - 1].SetActive(false);
                StageLock[i - 1].SetActive(false);
                StageClearFlag[i - 1].gameObject.SetActive(false);
            }
            else
            {
                StageButton[i - 1].SetActive(true);
                StageLock[i - 1].SetActive(true);
                StageClearFlag[i - 1].gameObject.SetActive(true);

                //ステージの攻略状況の更新
                int ClearStatas = GameManager.stageClearStatas[StageButtonNum];
                if (ClearStatas == 1)
                {
                    StageClearFlag[i - 1].gameObject.SetActive(true);
                    StageClearFlag[i - 1].sprite = ClearFlagSprite[0];
                }
                else if (ClearStatas == 2)
                {
                    StageClearFlag[i - 1].gameObject.SetActive(true);
                    StageClearFlag[i - 1].sprite = ClearFlagSprite[1];
                }
                else if (ClearStatas == 3)
                {
                    StageClearFlag[i - 1].gameObject.SetActive(true);
                    StageClearFlag[i - 1].sprite = ClearFlagSprite[2];
                }
                else
                {
                    StageClearFlag[i - 1].gameObject.SetActive(false);
                }

                //ステージのロック状況の更新
                bool UnLockFlag = GameManager.stageUnLockFlag[StageButtonNum];
                if (UnLockFlag) StageLock[i - 1].SetActive(false); else StageLock[i - 1].SetActive(true);
            }    
        }
    }

    /// <summary>
    /// ロックビューを閉じるボタン
    /// </summary>
    public void ClsoeLockView()
    {
        GameManager.playSystemSE(1);
        UIView[0].SetActive(false);
        UIView[1].SetActive(false);
    }

    /// <summary>
    /// ロック解除処理
    /// </summary>
    public void UnlockButton()
    {
        GameManager.playSystemSE(5);
        GameManager.coin -= SpendCoins;
        for(int i = LeftBNum; i <= RightBNum; i++)
        {
            GameManager.stageUnLockFlag[i] = true;
        }
        UIView[1].SetActive(false);
        UpdateStageButton();

        GameManager.saveGameData();
    }

    /// <summary>
    /// アンロックを行うための準備
    /// </summary>
    /// <param name="SNum"></param>
    /// <returns></returns>
    private int UnlockSetup(int SNum)
    {
        int LineNum = (SNum - 1) / 4 + 1;
        LeftBNum = 4 * LineNum - 3;
        RightBNum = 4 * LineNum;
        return LineNum;
    }

    /// <summary>
    ///コインゲット方法の説明ビューを開く
    /// </summary>
    public void OpenGetCoinHelpView()
    {
        GameManager.playSystemSE(0);
        UIView[2].SetActive(true);
    }

    /// <summary>
    /// コインゲット方法の説明ビューを閉じる
    /// </summary>
    public void CloseGetCoinHelpView()
    {
        GameManager.playSystemSE(1);
        UIView[2].SetActive(false);
    }

    /// <summary>
    /// ステージスタートビューを開く
    /// </summary>
    public void CloseStageStartView()
    {
        GameManager.playSystemSE(1);
        UIView[3].SetActive(false);
    }

    /// <summary>
    /// ゲームスタート
    /// </summary>
    public void StageStart()
    {
        GameManager.GoStage(PlayStageNum);
        GameManager.playSystemSE(0);
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
    /// 設定ビューを開く
    /// </summary>
    public void OpenSettingView()
    {
        GameManager.playSystemSE(0);
        MenuCloseButton.interactable = false;
        SettingText[0].text = GameManager.eventCount[0].ToString();
        SettingText[1].text = GameManager.eventCount[1].ToString();
        SettingText[2].text = GameManager.eventCount[2].ToString();
        SettingText[3].text = GameManager.eventCount[3].ToString();
        SettingText[4].text = GameManager.eventCount[4].ToString();
        SettingSlider[0].value = GameManager.eventCount[0];
        SettingSlider[1].value = GameManager.eventCount[1];
        SettingSlider[2].value = GameManager.eventCount[2];
        SettingSlider[3].value = GameManager.eventCount[3];
        SettingSlider[4].value = GameManager.eventCount[4];
        UIView[4].SetActive(true);
    }

    /// <summary>
    /// 設定ビューを閉じる
    /// </summary>
    public void CloseSettingView()
    {
        UIView[4].SetActive(false);
        GameManager.seChangeValume();
        GameManager.playSystemSE(1);
        MenuCloseButton.interactable = true;

        //セーブを入れる
        GameManager.saveGameData();
    }

    /// <summary>
    /// 設定画面のBGMスライダー処理
    /// </summary>
    public void BGMValumeChange()
    {
        GameManager.eventCount[0] = (int)SettingSlider[0].value;
        SettingText[0].text = GameManager.eventCount[0].ToString();
        GameManager.bgmChangeValume();
    }

    /// <summary>
    /// 設定画面のSEスライダー処理
    /// </summary>
    public void SEValumeChange()
    {
        GameManager.eventCount[1] = (int)SettingSlider[1].value;
        SettingText[1].text = GameManager.eventCount[1].ToString();
    }

    /// <summary>
    /// 設定画面のセンサ感度スライダー処理
    /// </summary>
    public void SensorValueChange()
    {
        GameManager.eventCount[2] = (int)SettingSlider[2].value;
        SettingText[2].text = GameManager.eventCount[2].ToString();
    }

    /// <summary>
    /// 設定画面の透過範囲スライダー
    /// </summary>
    public void TRangeChange()
    {
        GameManager.eventCount[3] = (int)SettingSlider[3].value;
        SettingText[3].text = GameManager.eventCount[3].ToString();
    }

    /// <summary>
    /// 設定画面の透過度スライダー
    /// </summary>
    public void TransparencyChange()
    {
        GameManager.eventCount[4] = (int)SettingSlider[4].value;
        SettingText[4].text = GameManager.eventCount[4].ToString();
    }

    /// <summary>
    /// ヘルプビューを開く
    /// </summary>
    public void OopnHelpView()
    {
        GameManager.playSystemSE(0);
        UIView[5].SetActive(true);
    }

    /// <summary>
    /// ヘルプビューを閉じる
    /// </summary>
    public void CloseHelpView()
    {
        GameManager.playSystemSE(1);
        UIView[5].SetActive(false);
    }

    /// <summary>
    /// ビューモードのヘルプ
    /// </summary>
    public void OpenHelpViewMode()
    {
        GameManager.playSystemSE(0);
        UIView[10].SetActive(true);
        if (!GameManager.eventFlag[1])
        {
            GameManager.eventFlag[1] = true;
            GameManager.saveGameData();
        }
    }
    public void CloseHelpViewMode()
    {
        GameManager.playSystemSE(1);
        UIView[10].SetActive(false);
    }

    /// <summary>
    /// 操作説明ビューを開く
    /// </summary>
    public void OpenHowView()
    {
        GameManager.playSystemSE(0);
        UIView[6].SetActive(true);
        UIView[9].SetActive(true);
    }

    public void NextHowView()
    {
        GameManager.playSystemSE(0);
        UIView[9].SetActive(false);
    }

    public void BackHowView()
    {
        GameManager.playSystemSE(1);
        UIView[9].SetActive(true);
    }

    /// <summary>
    /// 操作説明ビューを閉じる
    /// </summary>
    public void CloseHowView()
    {
        GameManager.playSystemSE(1);
        UIView[6].SetActive(false);
    }

    /// <summary>
    /// 他アプリ紹介ビューを開く
    /// </summary>
    public void OpenOtherAppView()
    {
        GameManager.playSystemSE(0);
        UIView[7].SetActive(true);
    }

    /// <summary>
    /// 他アプリ紹介ビューを閉じる
    /// </summary>
    public void CloseOtherAppView()
    {
        GameManager.playSystemSE(1);
        UIView[7].SetActive(false);
    }

    /// <summary>
    /// AppSoreを開く
    /// </summary>
    /// <param name="Num"></param>
    public void OpenAppDownload(int Num)
    {
#if UNITY_IOS
        string IOSNum = "0000000000";
        if (Num == 1)               //猫珠
        {
            IOSNum = "1506407180";
        }
        else if (Num == 2)          //ブラックサイズ
        {
            IOSNum = "1457372354";
        }
        else if (Num == 3)          //ポン吉の大冒険
        {
            IOSNum = "1474041840";
        }
        else
        {
            IOSNum = "1474041840";
        }
#endif

#if UNITY_IOS
        string url = "itms-apps://itunes.apple.com/jp/app/id" + IOSNum + "?mt=8";
#else
		string url = "market://details?id=XXXXXXXX";
#endif
        Application.OpenURL(url);
    }

    /// <summary>
    /// バージョンチェックコルーチン
    /// </summary>
    /// <returns></returns>
    const string CURRENT_VERSION_CHECK_KEY = "CurrentVersionCheck";

    void CurrentVersionCheck()
    {
        var version = PlayerPrefs.GetString(CURRENT_VERSION_CHECK_KEY, "");
        if (version != Application.version)
        {
            PlayerPrefs.SetString(CURRENT_VERSION_CHECK_KEY, Application.version);
            SaveInvalidVersionUpCheck(false);
        }
    }

    const string INVALID_VERSION_UP_CHECK_KEY = "InvalidVersionUpCheck";

    bool LoadInvalidVersionUpCheck()
    {
        return PlayerPrefs.GetInt(INVALID_VERSION_UP_CHECK_KEY, 0) != 0;
    }

    void SaveInvalidVersionUpCheck(bool invalid)
    {
        PlayerPrefs.SetInt(INVALID_VERSION_UP_CHECK_KEY, invalid ? 1 : 0);
    }

    IEnumerator VersionCheckIOS()
    {
        var url = string.Format("https://itunes.apple.com/lookup?bundleId={0}", Application.identifier);
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        //isNetworkErrorとisHttpErrorでエラー判定
        if (request.isHttpError || request.isNetworkError)
        {
            //エラー確認
            Debug.Log(request.error);
        }
        else
        {
            //結果確認
            // Debug.Log("ダウンロード成功" + request.downloadHandler.text);
            if (!string.IsNullOrEmpty(request.downloadHandler.text))
            {
                //JSONをデシリアライズしてストアにあるAPPのバージョンとストアURLを取得
                var lookupData = JsonUtility.FromJson<AppLookupData>(request.downloadHandler.text);

                if (lookupData.resultCount > 0 && lookupData.results.Length > 0)
                {
                    var result = lookupData.results[0];              
                    if (VersionComparative(result.version))
                    {
                        //ストアにある最新バージョンと現在のバージョンが異なる場合に実行される。
                        StoreURL = result.trackViewUrl;
                        GameManager.playStageBGM(0);
                        UIView[8].SetActive(true);
                    }
                }
            }
        }
    }

#if UNITY_ANDROID
    private void VersionCheckAndroid()
    {
        var url = string.Format("https://play.google.com/store/apps/details?id={0}", Application.identifier);

        VersionNum.GetValueAsync().ContinueWith(task => {
            if (task.IsFaulted)
            {
                // Handle the error...
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                string andVer = snapshot.Child("Android").GetValue(true).ToString();
                if (VersionComparative(andVer))
                {
                    StoreURL = url;
                    UpdateFlag = true;
                }
            }
        });
    }
#endif
    private void OpenUpdateView()
    {
        GameManager.playStageBGM(0);
        UIView[8].SetActive(true); ;
    }

    [Serializable] public class AppLookupData
    {
        public int resultCount;
        public AppLookupResult[] results;
    }
    [Serializable] public class AppLookupResult
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
            var currentVersion = new System.Version(AppVersion);

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
        GameManager.playSystemSE(0);
        UIView[8].SetActive(false);
        Application.OpenURL(StoreURL);
    }
    public void NoUpdate()
    {
        GameManager.playSystemSE(1);
        UIView[8].SetActive(false);
    }


    /// <summary>
    /// 広告を再生してコインを獲得する
    /// </summary>
    public void CmPlay()
    {
        if(!GameManager.Editor) // && !GameManager.eventFlag[0])
        {
            GameManager.CMPlusCoin = true;
            GameManager.cmShow();
        }
        else
        {
            GameManager.playSystemSE(1);
        }
    }



    //購入処理に関する記述*******************************************]
    /// <summary>
    /// 購入処理の初期化
    /// </summary>
    public void InitializePurchasing()
    {
        // If we have already connected to Purchasing ...
        if (IsInitialized())
        {
            // ... we are done here.
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(NonConsumable, ProductType.NonConsumable, new IDs
        {
            {"ballspatialfull_android", GooglePlay.Name},
            {"BallSpatialFull", AppleAppStore.Name}
        });

        UnityPurchasing.Initialize(this, builder);
    }

    //初期化が終わっているか
    private bool IsInitialized()
    {
        // Only say we are initialized if both the Purchasing references are set.
        return controller != null && extensions != null;
    }

    //非消費型広告の購入
    public void BuyNonConsumable()
    {
        PButton[0].interactable = false;
        PButton[1].interactable = false;
        PButton[2].interactable = false;

        // Buy the non-consumable product using its general identifier. Expect a response either through ProcessPurchase or OnPurchaseFailed asynchronously.
        BuyProductID(NonConsumable);
    }

    /// <summary>
    /// 購入処理
    /// </summary>
    /// <param name="productId"></param>
    public void BuyProductID(string productId)
    {
        // If the stores throw an unexpected exception, use try..catch to protect my logic here.
        try
        {
            // If Purchasing has been initialized ...          
            if (IsInitialized())
            {
                // ... look up the Product reference with the general product identifier and the Purchasing system's products collection.
                Product product = controller.products.WithID(productId);

                // If the look up found a product for this device's store and that product is ready to be sold ... 
                if (product != null && product.availableToPurchase)
                {
                    Debug.Log(string.Format("Purchasing product asychronously: '{0}' - '{1}'", product.definition.id, product.definition.storeSpecificId));// ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed asynchronously.
                    controller.InitiatePurchase(product);
                }
                // Otherwise ...
                else
                {
                    // ... report the product look-up failure situation  
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                    OpenFaildView();
                }
            }
            // Otherwise ...
            else
            {
                // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or retrying initiailization.
                Debug.Log("BuyProductID FAIL. Not initialized.");
                OpenFaildView();
            }
        }
        // Complete the unexpected exception handling ...
        catch (Exception e)
        {
            // ... by reporting any unexpected exception for later diagnosis.
            Debug.Log("BuyProductID: FAIL. Exception during purchase. " + e);
            OpenFaildView();
        }
    }

    /// <summary>
    /// 復元処理
    /// </summary>
    // Restore purchases previously made by this customer. Some platforms automatically restore purchases. Apple currently requires explicit purchase restoration for IAP.
    public void RestorePurchases()
    {
        PButton[0].interactable = false;
        PButton[1].interactable = false;
        PButton[2].interactable = false;
        RestoreFlag = true;

        // If Purchasing has not yet been set up ...
        if (!IsInitialized())
        {
            // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            OpenFaildView2();
            return;
        }
        // If we are running on an Apple device ... 
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            // ... begin restoring purchases
            Debug.Log("RestorePurchases started ...");

            // Fetch the Apple store-specific subsystem.
            var apple = extensions.GetExtension<IAppleExtensions>();
            // Begin the asynchronous process of restoring purchases. Expect a confirmation response in the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            apple.RestoreTransactions(result => {

                // The first phase of restoration. If no more responses are received on ProcessPurchase then no purchases are available to be restored.
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            });

          //  PButton[0].interactable = true;
        }
        // Otherwise ...
        else
        {
            // We are not running on an Apple device. No work is necessary to restore purchases.
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            OpenFaildView2();
        }

        if (GameManager.eventFlag[0]) { OpenFaildView3(); }
    }

    /// <summary>
    /// Unity IAP が購入処理を行える場合に呼び出されます
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        this.controller = controller;
        this.extensions = extensions;
    }

    /// <summary>
    ///  Unity IAP 回復不可能な初期エラーに遭遇したときに呼び出されます。
    ///
    /// これは、インターネットが使用できない場合は呼び出されず、
    /// インターネットが使用可能になるまで初期化を試みます。
    /// </summary>
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
     //   OpenFaildView();
    }

    /// <summary>
    /// 購入が終了したときに呼び出されます。
    ///
    ///  OnInitialized() 後、いつでも呼び出される場合があります。
    /// </summary>
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        if (String.Equals(e.purchasedProduct.definition.id, NonConsumable, StringComparison.Ordinal))
        {
            // ここに非消費アイテムを買った時の処理を入れる
            if (!GameManager.eventFlag[0])      //購入履歴がな場合
            {
                Debug.Log("購入完了");

                GameManager.eventFlag[0] = true;
                for (int i = 0; i <= GameManager.MaxStageNum; i++)
                {
                    GameManager.stageUnLockFlag[i] = true;
                }
                GameManager.saveGameData();
                PButton[0].interactable = true;
                UpdateStageButton();

                if (RestoreFlag)
                {
                    RestoreFlag = false;
                    OpenSucView();
                }
            }
        }
        return PurchaseProcessingResult.Complete;
    }

    /// <summary>
    /// 購入が失敗したときに呼び出されます。
    /// </summary>
    public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
    {
        OpenFaildView();
    }

    //購入失敗ビューの表示
    private void OpenFaildView()
    {
        PView[1].SetActive(true);
    }

    //復元失敗ビューの表示
    private void OpenFaildView2()
    {
        PView[3].SetActive(true);
    }

    //復元なしビューの表示
    private void OpenFaildView3()
    {
        PView[4].SetActive(true);
    }

    //復元成功ビューの表示
    private void OpenSucView()
    {
        PView[2].SetActive(true);
        GameManager.playSystemSE(5);
    }

    //購入、復元結果ビューをとじる
    public void CloseFaildView()
    {
        if (GameManager.eventFlag[0])
        {
            FullPButton.interactable = false;
            PButton[1].interactable = false;
            PButton[2].interactable = false;
        }
        else
        {
            FullPButton.interactable = true;
            PButton[1].interactable = true;
            PButton[2].interactable = true;
        }
        for (int i = 1; i < PView.Length; i++) { PView[i].SetActive(false); }
        PButton[0].interactable = true;
        GameManager.playSystemSE(1);
    }

    public void OpenPurchaView()
    {
        GameManager.playSystemSE(0);
        PView[0].SetActive(true);
    }

    public void ClosePurchaView()
    {
        GameManager.playSystemSE(1);
        PView[0].SetActive(false);
    }
}
