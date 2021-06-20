using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlarmAnime : MonoBehaviour
{
    public void AlarmSEStop()
    {
        GameManager.stopLoopSE();
        GameManager.playSystemSE(10);
    }

    public void ReplayBGM()
    {
        GameManager.playTitleBGM();
    }
}
