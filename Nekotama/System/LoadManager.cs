using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadManager : MonoBehaviour
{
    [SerializeField] private GameObject LoadUI = null;
    [SerializeField] private Image LoadBG = null;
    [SerializeField] private Sprite[] BGSprite = null;

    // Start is called before the first frame update
    void Start()
    {
        ImageChange();
    }

    /// <summary>
    /// ロードビューの表示
    /// </summary>
    public void viewLoadImage() => LoadUI.SetActive(true);

    /// <summary>
    /// ロードビューの非表示
    /// </summary>
    public void nonviewLoadImage()
    {
        LoadUI.SetActive(false);
        ImageChange();
    }

    /// <summary>
    /// ロードビューのイメージを変更
    /// </summary>
    private void ImageChange()
    {
        int Rnum = Random.Range(0, BGSprite.Length);
        LoadBG.sprite = BGSprite[Rnum];
    }
}
