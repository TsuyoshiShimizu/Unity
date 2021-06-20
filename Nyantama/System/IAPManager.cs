using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class IAPManager : MonoBehaviour, IStoreListener
{

    [SerializeField] private Button NoCMButton = null;
    [SerializeField] private Button RestoreButton = null;
    [SerializeField] private Button CloseButton = null;

    [SerializeField] private GameObject[] UIView = null;
    [SerializeField] private MenuViewController menuView = null;

    private IStoreController controller;
    private IExtensionProvider extensions;
    private string NonConsumable = "NyantamaAD";
   // private SceneController SceneCon;

    private bool RestoreFlag = false;
  //  private bool FaildRestoreFlag = false;
    void Awake()
    {
        // If we haven't set up the Unity Purchasing reference
        if (controller == null)
        {
            // Begin to configure our connection to Purchasing
            InitializePurchasing();
        }
    }

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
            {"nyantamanoad", GooglePlay.Name},
            {"NyantamaNoAD", AppleAppStore.Name}
        });

        UnityPurchasing.Initialize(this, builder);
    }

    private void Start()
    {
  //      SceneCon = GameObject.FindGameObjectWithTag("SceneController").GetComponent <SceneController>() ;

     //   GameStatas.PurchaseFlag[0] = false;
      //  GameDirector.GameStatasSave();
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
        NoCMButton.interactable = false;
        CloseButton.interactable = false;
        RestoreButton.interactable = false;
        
        // Buy the non-consumable product using its general identifier. Expect a response either through ProcessPurchase or OnPurchaseFailed asynchronously.
        BuyProductID(NonConsumable);
    }

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

    // Restore purchases previously made by this customer. Some platforms automatically restore purchases. Apple currently requires explicit purchase restoration for IAP.
    public void RestorePurchases()
    {
        CloseButton.interactable = false;
        NoCMButton.interactable = false;
        RestoreButton.interactable = false;
        RestoreFlag = true;
        // If Purchasing has not yet been set up ...
        if (!IsInitialized())
        {
            // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            OpenFaildView();
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
                CloseButton.interactable = true;
                // The first phase of restoration. If no more responses are received on ProcessPurchase then no purchases are available to be restored.
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            });
            
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
       // OpenFaildView();
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
            if (!GameManager.eventFlag[0])
            {
                Debug.Log("購入完了");
                menuView.DestroyBanner();
                GameManager.eventFlag[0] = true;
                GameManager.saveGameData();
                CloseButton.interactable = true;

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

    private void OpenFaildView()
    {
        UIView[0].SetActive(true);
    }

    private void OpenFaildView2()
    {
        UIView[1].SetActive(true);
    }

    private void OpenFaildView3()
    {
        UIView[3].SetActive(true);
    }

    private void OpenSucView()
    {
        UIView[2].SetActive(true);
    }

    public void CloseFaildView()
    {
        if (GameManager.eventFlag[0])
        {
            GameManager.GoStageSelect();
        }
        else
        {
            for (int i = 0; i < UIView.Length; i++) { UIView[i].SetActive(false); }
            CloseButton.interactable = true;
            NoCMButton.interactable = true;
            RestoreButton.interactable = true;
        }
    }
}
