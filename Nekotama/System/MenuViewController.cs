using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

public class MenuViewController : MonoBehaviour
{
    //メインビュー
    [SerializeField] private GameObject MenuView = null;
    [SerializeField] private GameObject MoneyView = null;
    [SerializeField] private GameObject CatMovieView = null;
    [SerializeField] private GameObject OtherGameView = null;
    [SerializeField] private GameObject[] ConfirmationView = null;

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

    //設定
    [SerializeField] private GameObject SettingView = null;
    [SerializeField] private Text[] SettingText = null;
    [SerializeField] private Slider[] SettingSlider = null;

    //バナー広告
  //  [SerializeField] private NendAdBanner nendADBanner = null;


    private int MovieNum = 0;           //ムービーのナンバーを記憶
    private int AppNum = 0;             //アプリのナンバーを記憶
    private int MoviePage = 1;
    private int MaxPage;

    private Animator animator;
    private void Awake()
    {
        /*
        if(nendADBanner != null && !GameManager.Editor)
        {
            GameManager.NABanner = nendADBanner;
        }
        */
    }

    private void Start()
    {
        animator = GetComponent<Animator>();

        int SpriteC = MovieSprite.Length;
        float PageF = (float)SpriteC / 2.0f;
        MaxPage = Mathf.CeilToInt(PageF);

        StartCoroutine(VersionCheckIOS());

        if (!GameManager.eventFlag[0] && !GameManager.Editor)
        {
         //   GameManager.requestInterstitial();
         //   GameManager.viewBanner();
        }
            
    }

    private string StoreURL;
   

    public void MoveShop()
    {
        GameManager.TitleSoundFlag = true;
        GameManager.playSystemSE(1);
        GameManager.playShopBGM();
        GameManager.GoShop();
    }

    //メニューを開く
    public void OpenMenuView()
    {
        GameManager.playSystemSE(1);
        MenuButton.interactable = false;
        MenuView.SetActive(true);
    }

    //メニューを閉じる
    public void CloseMenuView()
    {
        GameManager.playSystemSE(4);
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

#if UNITY_IOS
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
        GameManager.playSystemSE(1);
        SettingText[0].text = GameManager.eventCount[5].ToString();
        SettingText[1].text = GameManager.eventCount[6].ToString();
        SettingSlider[0].value = GameManager.eventCount[5];
        SettingSlider[1].value = GameManager.eventCount[6];
        SettingView.SetActive(true);
    }

    /// <summary>
    /// 設定ビューを閉じる
    /// </summary>
    public void CloseSettingView()
    {
        SettingView.SetActive(false);
        GameManager.seChangeValume();
        GameManager.playSystemSE(4);
        GameManager.saveGameData(); 
    }

    /// <summary>
    /// 設定画面のBGMスライダー処理
    /// </summary>
    public void BGMValumeChange()
    {
        GameManager.eventCount[5] = (int)SettingSlider[0].value;
        SettingText[0].text = GameManager.eventCount[5].ToString();
        GameManager.bgmChangeValume();
    }

    /// <summary>
    /// 設定画面のSEスライダー処理
    /// </summary>
    public void SEValumeChange()
    {
        GameManager.eventCount[6] = (int)SettingSlider[1].value;
        SettingText[1].text = GameManager.eventCount[6].ToString();
    }

}
