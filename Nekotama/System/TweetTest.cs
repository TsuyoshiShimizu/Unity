using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TweetTest : MonoBehaviour
{

    [SerializeField] private GameDirector Director = null;
    private const string FILENAME = "PV00.jpg";
    

    public void TweetMake()
    {
        string Durl = "https://apps.apple.com/jp/app/id1506407180";

        TwitterManager.DelFuncs delfunc = new TwitterManager.DelFuncs(CallbackTweetSuccsess, CallbackTweetFailed);
        string filename = Application.streamingAssetsPath + "/" + FILENAME;

        TwitterManager.TweetSet(delfunc, "にゃんこの本格アクションゲーム\n2Dと3Dが入り乱れた世界をにゃんこで駆け回ろう\n" +
            "IPhone、IPadで無料でプレイできます！\n\nダウンロードURL\n" + Durl + "\n#猫 #にゃんこ #ゲーム #猫珠", filename);

    }

    //コールバック用の関数
    private void CallbackTweetSuccsess()
    {
        Debug.Log("ツイート成功！");
        Director.TweetSuc();
    }

    private void CallbackTweetFailed(string errMsg)
    {
        Debug.Log("ツイート失敗… :" + errMsg);
        Director.TweetFaild();
    }

}
