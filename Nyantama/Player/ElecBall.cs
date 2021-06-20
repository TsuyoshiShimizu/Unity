using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElecBall : MonoBehaviour
{
    // Start is called before the first frame update
    private float StartLiveTime = 4.0f;     //開始時の生存時間
    private float LiveTime = 0.0f;          //残り生存時間時間
    private float StartSize = 100f;         //開始時のサイズ
    private float EndSize = 10f;            //終了時のサイズ

    // Update is called once per frame
    void Update()
    {
        LiveTime -= Time.deltaTime;
        transform.localScale = Vector3.one * ( (StartSize - EndSize) * (LiveTime / StartLiveTime) + EndSize );

        if(LiveTime <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        LiveTime = StartLiveTime;
        transform.localScale = Vector3.one * StartSize;
    }
}
