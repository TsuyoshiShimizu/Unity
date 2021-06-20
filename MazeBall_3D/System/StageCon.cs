using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ステージの物理法則などの制御を行う
/// カメラ、重力、光源の向きなど
/// </summary>
public class StageCon : MonoBehaviour
{
    [SerializeField] GameObject UperPoint = null;
    [SerializeField] GameObject BackPoint = null;
    [SerializeField] GameObject RightPoint = null;
    
    static public bool StageRotLock = false;
    //1:下面　2;上面　3:正面　4:奥面　5:右側面　6:左側面
    static public int StageSurface = 1;
    static public GameObject RotEffect = null;

    static private int RotDirectoin = 0;
    static private float RotTime = 0;
    static private float RotD = 0;
    static private bool RotateFinishFlag = false;
    
    private float RotLockTime = 1.0f;
    private float RotLockDelta = 0;

   // static private AudioSource[] SEAudio;

    private void Start()
    {
        StageRotLock = false;
        StageSurface = 1;
        RotDirectoin = 0;
        RotTime = 0;
        RotD = 0;
        RotateFinishFlag = false;
    }

    private void FixedUpdate()
    {
        if( RotTime > 0)
        {
            //Debug.Log("回転実行");
            RotTime -= 0.02f;
            if (RotDirectoin == 1)
            {
                transform.Rotate(new Vector3(RotD, 0, 0), Space.World);
            }
            if (RotDirectoin == 2)
            {
                transform.Rotate(new Vector3(-RotD, 0, 0), Space.World);
            }
            if (RotDirectoin == 3)
            {
                transform.Rotate(new Vector3(0, RotD, 0), Space.World);
            }
            if (RotDirectoin == 4)
            {
                transform.Rotate(new Vector3(0, -RotD, 0), Space.World);
            }
            if (RotDirectoin == 5)
            {
                transform.Rotate(new Vector3(0, 0, RotD), Space.World);
            }
            if (RotDirectoin == 6)
            {
                transform.Rotate(new Vector3(0, 0, -RotD), Space.World);
            }
        }
        else
        {
            if (RotateFinishFlag)
            {
                RotateFinishFlag = false;
                //ステージの回転が終わったときに一回実行される
                transform.eulerAngles = MultipleVector(transform.eulerAngles, 90f);
                
                Vector3 GV = PlayerCon.GravityVector;
                GV = MultipleVector(GV, 1.0f);
                if (GV.y == -1.0f) StageSurface = 1;
                if (GV.y == 1.0f) StageSurface = 2;
                if (GV.z == -1.0f) StageSurface = 3;
                if (GV.z == 1.0f) StageSurface = 4;
                if (GV.x == 1.0f) StageSurface = 5;
                if (GV.x == -1.0f) StageSurface = 6;

                RotEffect.SetActive(false);
                GameManager.stopLoopSE();
                GameManager.playStageSE(1);
            }

            //ステージの回転ロック解除処理
            if (StageRotLock)
            {
                RotLockDelta += 0.02f;
                if(RotLockDelta >= RotLockTime)
                { 
                    RotLockDelta = 0;
                    StageRotLock = false;
                }
            }
        }
    }

    static public void RotStage(int SurfaceNum ,float time, float sita)
    {
        GameManager.playStageSE(0);
        //回転方向の判定
        if(StageSurface == 1) //下面
        {
            if (SurfaceNum == 3) RotDirectoin = 1; //x 正転 
            if (SurfaceNum == 4) RotDirectoin = 2; //x 逆転
            if (SurfaceNum == 5) RotDirectoin = 5; //z 正転
            if (SurfaceNum == 6) RotDirectoin = 6; //z 逆転
        }
        if (StageSurface == 2) //上面
        {
            if (SurfaceNum == 4) RotDirectoin = 1; //x 正転 
            if (SurfaceNum == 3) RotDirectoin = 2; //x 逆転
            if (SurfaceNum == 6) RotDirectoin = 5; //z 正転
            if (SurfaceNum == 5) RotDirectoin = 6; //z 逆転
        }
        if (StageSurface == 3) //正面
        {
            if (SurfaceNum == 2) RotDirectoin = 1; //x 正転 
            if (SurfaceNum == 1) RotDirectoin = 2; //x 逆転
            if (SurfaceNum == 6) RotDirectoin = 3; //z 正転
            if (SurfaceNum == 5) RotDirectoin = 4; //z 逆転
        }
        if (StageSurface == 4) //奥面
        {
            if (SurfaceNum == 1) RotDirectoin = 1; //x 正転 
            if (SurfaceNum == 2) RotDirectoin = 2; //x 逆転
            if (SurfaceNum == 5) RotDirectoin = 3; //y 正転
            if (SurfaceNum == 6) RotDirectoin = 4; //y 逆転
        }
        if (StageSurface == 5) //右側面
        {
            if (SurfaceNum == 3) RotDirectoin = 3; //y 正転 
            if (SurfaceNum == 4) RotDirectoin = 4; //y 逆転
            if (SurfaceNum == 2) RotDirectoin = 5; //z 正転
            if (SurfaceNum == 1) RotDirectoin = 6; //z 逆転
        }
        if (StageSurface == 6) //左側面
        {
            if (SurfaceNum == 4) RotDirectoin = 3; //y 正転 
            if (SurfaceNum == 3) RotDirectoin = 4; //y 逆転
            if (SurfaceNum == 1) RotDirectoin = 5; //z 正転
            if (SurfaceNum == 2) RotDirectoin = 6; //z 逆転
        }
        RotD = sita * 0.02f / time;
        RotTime = time;
        RotateFinishFlag = true;
        GameManager.playLoopSE(0);
    }

    private void LateUpdate()
    {
        transform.position = PlayerCon.PlayerPos;
        PlayerCon.GravityVector = (transform.position - UperPoint.transform.position).normalized;
        PlayerCon.BackVector = (BackPoint.transform.position - transform.position).normalized;
        PlayerCon.RightVector = (RightPoint.transform.position - transform.position).normalized;
    }

    private float MultipleFloor(float value, float multiple)
    {
        return Mathf.Floor(value / multiple) * multiple;
    }

    private float MultipleRound(float value, float multiple)
    {
        return MultipleFloor(value + multiple * 0.5f, multiple);
    }

    private Vector3 MultipleVector(Vector3 value, float multiple)
    {
        float mx = MultipleRound(value.x, multiple);
        float my = MultipleRound(value.y, multiple);
        float mz = MultipleRound(value.z, multiple);
        return new Vector3(mx, my, mz);
    }
}
