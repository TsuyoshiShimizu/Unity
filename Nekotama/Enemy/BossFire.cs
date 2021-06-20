using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossFire : MonoBehaviour
{
    [SerializeField] private int FireType = 0;  //炎の種類　0:直線　1:追尾　2:カクカク

    private Rigidbody rigid;                    //物理性質
    private SphereCollider SCollider;           //物理ボディ

    private float WaitTime = 0;         //初期待機時間
    private float WaitDelta = 0;        //初期待機時間のカウント
    private float LifeTime = 5;         //生存時間
    private float LifeDelta = 0;        //生存時間のカウント
    private float FireSpeed = 0;        //炎のスピード

    private bool isActive = false;      //動作をさえるか
    private bool StartLock = true;      //初期動作のロック

   // private bool Irregular = true;
    private float IrregularTime = 1;
    private float IrregularDelta = 0;

    //初期値の設定関数
    public void Initialize(float fireSpeed, float waitTime ,float lifeTime)
    {
        FireSpeed = fireSpeed;
        WaitTime = waitTime;
        LifeTime = lifeTime;
        StartLock = false;
    }

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
        SCollider = GetComponent<SphereCollider>();
    }

    private void FireShot()
    {
        Vector3 FireVec = PlayerController.PlayerPos - transform.position;
        rigid.velocity = FireVec.normalized * FireSpeed;
    }

    private void FixedUpdate()
    {
        //初期動作処理
        if(!StartLock && !isActive)
        {
            WaitDelta += 0.02f;
            if(WaitDelta > WaitTime)
            {
                isActive = true;
                //アクティブ変更時に一回だけ動作する処理を記入
                SCollider.enabled = true;                       //当たり判定を有効
                FireShot();                                     //ファイアショット
            }
        }

        //アクティブ時に常時行う処理
        if (isActive)
        {
            if(FireType == 1)
            {
                Vector3 NowSpeedVec = rigid.velocity.normalized;
                Vector3 TargetVec = (PlayerController.PlayerPos - transform.position).normalized;
                Vector3 FixVec = TargetVec - NowSpeedVec;
                Vector3 FixSppedVec = (NowSpeedVec + FixVec * 0.05f) * FireSpeed;
                rigid.velocity = FixSppedVec;
            }

            if(FireType == 2)
            {
                IrregularDelta += 0.02f;
                if(IrregularDelta > IrregularTime)
                {
                    IrregularDelta = 0;
                    Vector3 PVec = PlayerController.PlayerPos - transform.position;
                    Vector3 SVec = PVec.normalized * FireSpeed;

                    int MaxN = 0;
                    if (SVec[MaxN] < SVec[1]) MaxN = 1;
                    if (SVec[MaxN] < SVec[2]) MaxN = 2;

                    float R0 = Random.Range(0.1f, 0.7f);
                    float R1 = Random.Range(0.25f, 2f);
                    float R2 = Random.Range(0.25f, 2f);

                    if (MaxN == 0) SVec = new Vector3(SVec[0] * R0, SVec[1] + SVec[0] * R1, SVec[2] + SVec[0] * R2);
                    if (MaxN == 1) SVec = new Vector3(SVec[0] + SVec[1] * R1, SVec[1] * R0, SVec[2] + SVec[1] * R2);
                    if (MaxN == 2) SVec = new Vector3(SVec[0] + SVec[2] * R1, SVec[1] + SVec[2] * R2, SVec[2] * R0);

                    rigid.velocity = SVec;
                }
            }
        }

        //生存時間の処理
        LifeDelta += 0.02f;
        if (LifeDelta > LifeTime) Destroy(gameObject);
    }
}
