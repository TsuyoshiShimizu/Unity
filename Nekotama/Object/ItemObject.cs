using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Custom Object/ItemObject")]
public class ItemObject : MonoBehaviour
{
    [SerializeField] private ItemType itemType = ItemType.HpRecover;

    [SerializeField] private int SkillNumber = 0;
    [SerializeField] private int RecoverAmount = 10;
    [SerializeField] private int SaveNumber = 0;
    [SerializeField] private int Price = 1;
    [SerializeField] private int ShopNumber = 1;

    private bool ContactFlag = true;
    private enum ItemType { HpRecover, MpRecover ,Goal ,ShopItem}
    private GameDirector Director;
    private PlayerController playerCon;
    private bool ContactStart = false;
    private float ContactDelta = 0;

    public int ITemPrice { get { return Price; } set { Price = value; }  }

    /// <summary>
    /// 接触判定
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player" && ContactFlag && ContactStart)
        {
            ContactFlag = false;

            if (itemType == ItemType.HpRecover)
            {
                GameManager.playStageSE(23);
                playerCon.PlayerEffect(9);             
                Director.PlayerHP += RecoverAmount;
                ItemDelete();
            }

            if (itemType == ItemType.MpRecover)
            {
                GameManager.playStageSE(28);
                playerCon.PlayerEffect(12);
                Director.PlayerMP += RecoverAmount;
                ItemDelete();
            }

            if (itemType == ItemType.Goal)
            {
                ItemDelete();
                int ClearStatas = GameManager.stageClearStatas[GameManager.StageNum];
                if(ClearStatas == 0)
                {
                    GameManager.stageClearStatas[GameManager.StageNum] = 1;
                    GameManager.UpdataLockStatas();
                }
                GameManager.saveGameData();
                Director.StageClear();
            }

            if(itemType == ItemType.ShopItem)
            {
                Director.OpenShopUI(ShopNumber, Price, gameObject);
            }
        }
    }

    /// <summary>
    /// アイテムを消去
    /// </summary>
    private void ItemDelete() { Destroy(gameObject); }


    private void Start()
    {
        Director = GameManager.DirectorObj.GetComponent<GameDirector>();
        playerCon = GameManager.PlayerObj.GetComponent<PlayerController>();
        Invoke("StartContact", 0.3f);
    }

    private void StartContact()
    {
        ContactStart = true;
    }

    public void LateDelete()
    {
        Invoke("ItemDelete", 10);
    }

    private void FixedUpdate()
    {
        if (!ContactFlag)
        {
            ContactDelta += 0.02f;
            if(ContactDelta >= 5.0f)
            {
                ContactDelta = 0;
                ContactFlag = true;
            }
        }
    }

}
