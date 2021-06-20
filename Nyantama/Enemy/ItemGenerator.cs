using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGenerator : MonoBehaviour
{
    [SerializeField] GameObject[] ItemPrefabs = null;
    private List<GameObject> ItemObj = new List<GameObject>();
    private bool Mode = false;
    private bool ItemFlag = false;
    private int ItemCount = 0;
    private float Iteminterval = 0.1f;
    private float TimeDelta = 0;
    private float randam3D;
    private bool itemdir = true;

    private void Start()
    {
        randam3D = Random.Range(0, 3.14f);
    }

    public void ItemCreate(int enemytype, int enemyLv,bool Enemy3DMode)
    {
        Mode = Enemy3DMode;
        if (enemytype == 1) SlimeItem(enemyLv);         //スライム
        if (enemytype == 2) BlueSlimeItem(enemyLv);     //ブルースライム 
        if (enemytype == 3) RedWispItem(enemyLv);       //レッドウィスプ
        if (enemytype == 4) BlueWispItem(enemyLv);      //ブルーウィスプ
        if (enemytype == 5) TurtleItem(enemyLv);        //タートル
        if (enemytype == 6) RedTurtleItem(enemyLv);     //レッドタートル
        ItemCount = ItemObj.Count;
        ItemFlag = true;
    }

    private void FixedUpdate()
    {
        if (ItemFlag)
        {
            TimeDelta += 0.02f;
            if(TimeDelta >= Iteminterval)
            {
                TimeDelta = 0;
                if (ItemCount == 0)
                {
                    Destroy(gameObject);
                }
                else
                {
                    GameObject item = Instantiate(ItemObj[ItemCount - 1]) as GameObject;
                    float itemx;
                    float itemy;
                    float itemz;
                    if (Mode)
                    {
                        itemx = transform.position.x + Mathf.Cos((float)ItemCount + randam3D) * 1;
                        itemy = transform.position.y + 1;
                        itemz = transform.position.z + Mathf.Sin((float)ItemCount + randam3D) * 1;
                        item.transform.position = new Vector3(itemx, itemy, itemz);
                    }
                    else
                    {
                        if (itemdir)
                        {
                            itemx = transform.position.x + 0.25f * (float)ItemCount;
                        }
                        else
                        {
                            itemx = transform.position.x - 0.25f * (float)ItemCount;
                        }
                        itemy = transform.position.y + 1;
                        itemz = transform.position.z;
                        item.transform.position = new Vector3(itemx, itemy, itemz);
                        itemdir = !itemdir;
                    }
                    ItemCount--;
                    item.GetComponent<ItemObject>().LateDelete();
                }
            }
        }
    }
    
    private void SlimeItem(int enemyLv)
    {
        int R1 = Random.Range(1, 100);
        if (enemyLv == 1)
        {
            ItemObj.Add(ItemPrefabs[0]);
            if (R1 >= 90) ItemObj.Add(ItemPrefabs[3]);
        }
        if (enemyLv == 2)
        {
            ItemObj.Add(ItemPrefabs[0]);
            ItemObj.Add(ItemPrefabs[0]);
            if (R1 >= 70) ItemObj.Add(ItemPrefabs[3]);
        }
        if (enemyLv == 3)
        {
            ItemObj.Add(ItemPrefabs[0]);
            ItemObj.Add(ItemPrefabs[0]);
            ItemObj.Add(ItemPrefabs[0]);
            if (R1 >= 50) ItemObj.Add(ItemPrefabs[3]);
        }
        if (enemyLv == 4)
        {
            ItemObj.Add(ItemPrefabs[1]);
            ItemObj.Add(ItemPrefabs[0]);
            if (R1 >= 50) ItemObj.Add(ItemPrefabs[4]);
        }
        if (enemyLv == 5)
        {
            ItemObj.Add(ItemPrefabs[2]);
            if (R1 >= 30) ItemObj.Add(ItemPrefabs[4]);
        }
    }

    private void BlueSlimeItem(int enemyLv)
    {
        int R1 = Random.Range(1, 100);
        if (enemyLv == 1)
        {
            ItemObj.Add(ItemPrefabs[0]);
            ItemObj.Add(ItemPrefabs[0]);
            ItemObj.Add(ItemPrefabs[0]);
            if (R1 >= 80) ItemObj.Add(ItemPrefabs[3]);
        }
        if (enemyLv == 2)
        {
            ItemObj.Add(ItemPrefabs[1]);
            if (R1 >= 60) ItemObj.Add(ItemPrefabs[3]);
        }
        if (enemyLv == 3)
        {
            ItemObj.Add(ItemPrefabs[1]);
            ItemObj.Add(ItemPrefabs[1]);
            ItemObj.Add(ItemPrefabs[1]);
            if (R1 >= 40) ItemObj.Add(ItemPrefabs[4]);
        }
        if (enemyLv == 4)
        {
            ItemObj.Add(ItemPrefabs[2]);
            ItemObj.Add(ItemPrefabs[2]);
            if (R1 >= 10) ItemObj.Add(ItemPrefabs[4]);
        }
        if (enemyLv == 5)
        {
            ItemObj.Add(ItemPrefabs[2]);
            ItemObj.Add(ItemPrefabs[2]);
            ItemObj.Add(ItemPrefabs[2]);
            ItemObj.Add(ItemPrefabs[2]);
            if (R1 >= 10) ItemObj.Add(ItemPrefabs[5]);
        }
    }

    private void RedWispItem(int enemyLv)
    {
        int R1 = Random.Range(1, 100);
        if (enemyLv == 1)
        {
            ItemObj.Add(ItemPrefabs[0]);
            ItemObj.Add(ItemPrefabs[0]);
            if (R1 >= 80) ItemObj.Add(ItemPrefabs[3]);
        }
        if (enemyLv == 2)
        {
            ItemObj.Add(ItemPrefabs[1]);
            if (R1 >= 70) ItemObj.Add(ItemPrefabs[3]);
        }
        if (enemyLv == 3)
        {
            ItemObj.Add(ItemPrefabs[1]);
            ItemObj.Add(ItemPrefabs[1]);
            if (R1 >= 70) ItemObj.Add(ItemPrefabs[4]);
        }
        if (enemyLv == 4)
        {
            ItemObj.Add(ItemPrefabs[2]);
            ItemObj.Add(ItemPrefabs[0]);
            ItemObj.Add(ItemPrefabs[0]);
            if (R1 >= 60) ItemObj.Add(ItemPrefabs[4]);
        }
        if (enemyLv == 5)
        {
            ItemObj.Add(ItemPrefabs[2]);
            ItemObj.Add(ItemPrefabs[1]);
            if (R1 >= 50) ItemObj.Add(ItemPrefabs[5]);
        }
    }

    private void BlueWispItem(int enemyLv)
    {
        int R1 = Random.Range(1, 100);
        if (enemyLv == 1)
        {
            ItemObj.Add(ItemPrefabs[0]);
            ItemObj.Add(ItemPrefabs[0]);
            ItemObj.Add(ItemPrefabs[0]);
            ItemObj.Add(ItemPrefabs[0]);
            ItemObj.Add(ItemPrefabs[3]);
            if (R1 >= 80) ItemObj.Add(ItemPrefabs[3]);
        }
        if (enemyLv == 2)
        {
            ItemObj.Add(ItemPrefabs[1]);
            ItemObj.Add(ItemPrefabs[3]);
            if (R1 >= 70) ItemObj.Add(ItemPrefabs[3]);
        }
        if (enemyLv == 3)
        {
            ItemObj.Add(ItemPrefabs[1]);
            ItemObj.Add(ItemPrefabs[1]);
            ItemObj.Add(ItemPrefabs[3]);
            if (R1 >= 70) ItemObj.Add(ItemPrefabs[4]);
        }
        if (enemyLv == 4)
        {
            ItemObj.Add(ItemPrefabs[2]);
            ItemObj.Add(ItemPrefabs[0]);
            ItemObj.Add(ItemPrefabs[0]);
            ItemObj.Add(ItemPrefabs[4]);
            if (R1 >= 50) ItemObj.Add(ItemPrefabs[4]);
        }
        if (enemyLv == 5)
        {
            ItemObj.Add(ItemPrefabs[2]);
            ItemObj.Add(ItemPrefabs[2]);
            ItemObj.Add(ItemPrefabs[2]);
            ItemObj.Add(ItemPrefabs[2]);
            ItemObj.Add(ItemPrefabs[4]);
            if (R1 >= 30) ItemObj.Add(ItemPrefabs[5]);
        }
    }

    private void TurtleItem(int enemyLv)
    {
        int R1 = Random.Range(1, 100);
        if (enemyLv == 1)
        {
            ItemObj.Add(ItemPrefabs[0]);
            ItemObj.Add(ItemPrefabs[0]);
            ItemObj.Add(ItemPrefabs[0]);
            if (R1 >= 70) ItemObj.Add(ItemPrefabs[3]);
        }
        if (enemyLv == 2)
        {
            ItemObj.Add(ItemPrefabs[1]);
            ItemObj.Add(ItemPrefabs[0]);
            if (R1 >= 70) ItemObj.Add(ItemPrefabs[3]);
        }
        if (enemyLv == 3)
        {
            ItemObj.Add(ItemPrefabs[1]);
            ItemObj.Add(ItemPrefabs[1]);
            if (R1 >= 60) ItemObj.Add(ItemPrefabs[4]);
        }
        if (enemyLv == 4)
        {
            ItemObj.Add(ItemPrefabs[1]);
            ItemObj.Add(ItemPrefabs[1]);
            ItemObj.Add(ItemPrefabs[1]);
            if (R1 >= 50) ItemObj.Add(ItemPrefabs[4]);
        }
        if (enemyLv == 5)
        {
            ItemObj.Add(ItemPrefabs[2]);
            ItemObj.Add(ItemPrefabs[1]);
            if (R1 >= 50) ItemObj.Add(ItemPrefabs[5]);
        }
    }

    private void RedTurtleItem(int enemyLv)
    {
        int R1 = Random.Range(1, 100);
        if (enemyLv == 1)
        {
            ItemObj.Add(ItemPrefabs[1]);
            if (R1 >= 60) ItemObj.Add(ItemPrefabs[3]);
        }
        if (enemyLv == 2)
        {
            ItemObj.Add(ItemPrefabs[1]);
            ItemObj.Add(ItemPrefabs[0]);
            ItemObj.Add(ItemPrefabs[0]);
            if (R1 >= 60) ItemObj.Add(ItemPrefabs[3]);
        }
        if (enemyLv == 3)
        {
            ItemObj.Add(ItemPrefabs[2]);
            if (R1 >= 50) ItemObj.Add(ItemPrefabs[4]);
        }
        if (enemyLv == 4)
        {
            ItemObj.Add(ItemPrefabs[1]);
            ItemObj.Add(ItemPrefabs[2]);
            ItemObj.Add(ItemPrefabs[3]);
            if (R1 >= 50) ItemObj.Add(ItemPrefabs[4]);
        }
        if (enemyLv == 5)
        {
            ItemObj.Add(ItemPrefabs[2]);
            ItemObj.Add(ItemPrefabs[2]);
            ItemObj.Add(ItemPrefabs[2]);
            ItemObj.Add(ItemPrefabs[3]);
            if (R1 >= 40) ItemObj.Add(ItemPrefabs[5]);
        }
    }

    private void EmptyItem(int enemyLv)
    {
        int R1 = Random.Range(1, 100);
        if (enemyLv == 1)
        {

        }
        if (enemyLv == 2)
        {

        }
        if (enemyLv == 3)
        {

        }
        if (enemyLv == 4)
        {

        }
        if (enemyLv == 5)
        {

        }
    }
}
