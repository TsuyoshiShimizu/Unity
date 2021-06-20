using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingView : MonoBehaviour
{
    [SerializeField] private GameObject settingView = null;
    [SerializeField] private Text[] SettingText = null;
    [SerializeField] private Slider[] SettingSlider = null;
    [SerializeField] private Toggle ActionPadToggle = null;
    [SerializeField] private GameObject ProtectView = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
        ActionPadToggle.isOn = GameManager.eventFlag[3];
        settingView.SetActive(true);
        ProtectView.SetActive(true);
    }

    /// <summary>
    /// 設定ビューを閉じる
    /// </summary>
    public void CloseSettingView()
    {
        settingView.SetActive(false);
        ProtectView.SetActive(false);
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

    /// <summary>
    /// アクションパッドを隠すかの取るぐ設定
    /// </summary>
    public void ChangeActionPadToggle()
    {
        if (ActionPadToggle.isOn)
        {
            GameManager.eventFlag[3] = true;
        }
        else
        {
            GameManager.eventFlag[3] = false;
        }
    }
}
