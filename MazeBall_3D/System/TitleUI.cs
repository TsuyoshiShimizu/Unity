using UnityEngine;


public class TitleUI : MonoBehaviour
{
    public void StartButton()
    {
        GameManager.GoStageSelect();
        GameManager.playSystemSE(0);
    }

    private void Start()
    {
        GameManager.playTitleBGN();
    }
}
