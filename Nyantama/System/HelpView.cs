using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpView : MonoBehaviour
{
    [SerializeField] private GameObject[] View = null;

    public void OpenView(int num)
    {
        GameManager.playSystemSE(1);
        View[num].SetActive(true);
        View[num].transform.SetAsLastSibling();
    }

    public void CloseView(int num)
    {
        GameManager.playSystemSE(3);
        View[num].SetActive(false);

        //ビューが全て閉じられたらタイマースケールを通常に戻す処理
        bool TimeStartFlag = true;
        for(int i = 0; i < View.Length; i++)
        {
            if (View[i].activeSelf) TimeStartFlag = false;
        }
        if (TimeStartFlag) Time.timeScale = 1;
    }
}
