using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    [SerializeField] GameObject[] ShopItem = null;

    private bool ContactFlag = true;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && ContactFlag)
        {
            ContactFlag = false;
            GameManager.GoStageSelect();
        }
    }

    private void timerChange(float Timer)
    {
        int s = (int)Timer;         //Timerは現在の時間
        int m = s / 60;             //分
        float msf = Timer - s;      
        int ms = (int)(100 * msf);  //10ミリ秒
        s %= 60;                    //秒

        int m1 = m % 10;            //分の一桁目
        int m2 = m / 10;            //分の二桁目
        int s1 = s % 10;            //秒の一桁目
        int s2 = s / 10;            //秒の二桁目
        int ms1 = ms % 10;          //10ミリ秒の一桁目
        int ms2 = ms / 10;          //10ミリ秒の二桁目
    }
}
