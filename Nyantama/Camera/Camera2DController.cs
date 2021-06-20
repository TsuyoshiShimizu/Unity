using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera2DController : MonoBehaviour
{
    [SerializeField] private int MinX = 150;
    [SerializeField] private int MaxX = 350;
    [SerializeField] private int MinY = 10;
    [SerializeField] private int MaxY = 100;
    [SerializeField] private int MinZ = 50;
    [SerializeField] private int MaxZ = 400;
   // [SerializeField] private bool SpotLightFlag = false;
    [SerializeField] private GameObject Wall = null;

   // private bool Camera3DFlag = false; 

    //カメラの距離
    float Cdx = 0.0f;
    float Cdy = 1.5f;
    float Cdz = -10.0f;
    //カメラの角度
    private new Camera camera;

    private void Awake()
    {
        GameObject miniWall = Instantiate(Wall) as GameObject;
        miniWall.transform.position = new Vector3(MinX, 50, 250);

        GameObject maxWall = Instantiate(Wall) as GameObject;
        maxWall.transform.position = new Vector3(MaxX, 50, 250);
        camera = GetComponent<Camera>();
        GameDirector.StageMaxPos = new Vector3(MaxX, MaxY, MaxZ) + Vector3.one * 10;
        GameDirector.StageMinPos = new Vector3(MinX, MinY, MinZ) - Vector3.one * 10;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (GameDirector.Player3DMode)
        {
            Cdy = 11f;
            Cdz = -10;
            transform.eulerAngles = new Vector3(45, 0, 0);
        }
        else
        {
            Cdy = 1.5f;
            Cdz = -10;
            transform.eulerAngles = Vector3.zero;
        }
        
        float WidthRange = 90;
        float distance = camera.farClipPlane;
        var frustumWidth = 2.0f * distance * Mathf.Tan(WidthRange * 0.5f * Mathf.Deg2Rad);
        var frustumHeight = frustumWidth / camera.aspect;
        camera.fieldOfView = 2.0f * Mathf.Atan(frustumHeight * 0.5f / distance) * Mathf.Rad2Deg;
        
    }

    void Update()
    {
        Vector3 PlayerP = PlayerController.PlayerPos;
        Vector3 CameraP = new Vector3(PlayerP.x + Cdx, PlayerP.y + Cdy, PlayerP.z + Cdz);
        if (MinX + 10 > PlayerP.x) CameraP.x = MinX + 10 + Cdx;
        if (PlayerP.x > MaxX - 10) CameraP.x = MaxX - 10 + Cdx;
        if (PlayerP.y <= MinY) CameraP.y = MinY + Cdy;
        if (PlayerP.y >= MaxY) CameraP.y = MaxY + Cdy;
        if (PlayerP.z <= MinZ) CameraP.z = MinZ + Cdz;
        if (PlayerP.z >= MaxZ) CameraP.z = MaxZ + Cdz;
        transform.position = CameraP;

        Transparent();
        TargetDrag();
    }
   

    public void Camera3DMode()
    {
     //   Camera3DFlag = true;
        Cdy = 11f;
        Cdz = -10;
        transform.eulerAngles = new Vector3(45,0,0);
    }

    public void Camera2DMode()
    {
     //   Camera3DFlag = false;
        Cdy = 1.5f;
        Cdz = -10;
        transform.eulerAngles = Vector3.zero;
    }

    public void ChangeCRange(float range)
    {
        C = range;
    }

    private float R = 0.01f; //Rayの半径
    private float C = 1f;   //ヒット距離の補正値
    //ステージオブジェクトがプレイヤーと重なった時の透過処理
    private void Transparent()
    {
        Ray ray = new Ray(transform.position, -(transform.position - PlayerController.PlayerPos));  //プレイヤーまでRayをとばす
        float Pdis = (PlayerController.PlayerPos - transform.position).magnitude;                   //カメラとプレイヤーの距離
        if (Physics.SphereCast(ray, R))
        {
            RaycastHit[] hits = Physics.SphereCastAll(ray, R);
            for (int i = 0; i < hits.Length; i++)
            {
                string GameTag = hits[i].collider.tag;
                if (GameTag == "2DObject" || GameTag == "3DObject" || GameTag == "DamageObject" || GameTag == "TransObj")
                {
                    if (hits[i].distance < Pdis - C)
                    {
                        ObjectTransparent TObj;
                        if (TObj = hits[i].collider.gameObject.GetComponent<ObjectTransparent>())
                        {
                            if (!TObj.TFlag) TObj.Transparent(); else TObj.TimerReset();
                        }
                    }
                }
            }
        }
    }
   
    public bool isTargetClick(Vector2 ClickPos)
    {
        if (GameDirector.TargetLock) return false;
        bool TargetClick = false;
        if (!GameDirector.EnemyTargetFlag) return false;
        Ray ray = Camera.main.ScreenPointToRay(ClickPos);
        RaycastHit[] hits = new RaycastHit[4];
        int hitNum = Physics.RaycastNonAlloc(ray, hits);
        for (int i = 0; i < hitNum; i++)
        {
            GameObject HitObj = hits[i].collider.gameObject;
            if (HitObj.tag == "EnemyHiter")
            {
                HitObj.GetComponent<EnemyHitController>().onEnemyClick();
                TargetClick = true;
                Debug.Log("enemyClick");
            }
            if (HitObj.tag == "Switch")
            {
                HitObj.GetComponent<SwitchReceiver>().onSwitchClick();
                TargetClick = true;
            }
            if (HitObj.tag == "Torch")
            {
                HitObj.GetComponent<TorchSwitch>().onClick();
                TargetClick = true;
            }
            if (HitObj.tag == "MTorch")
            {
                HitObj.GetComponent<ChirdTorchSwitch>().onClick();
                TargetClick = true;
            }
        }
        return TargetClick;
    }

    private void TargetDrag()
    {
        if (GameDirector.TargetLock) return;
        if (!GameDirector.EnemyTargetFlag) return;
        if (GameDirector.MagicType != 2) return;

        Touch touchinfo;
        if (Input.touchCount >= 1)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                touchinfo = Input.GetTouch(i);
                if(touchinfo.phase == TouchPhase.Moved)
                {
                    Ray ray = Camera.main.ScreenPointToRay(touchinfo.position);
                    RaycastHit[] hits = new RaycastHit[4];
                    int hitNum = Physics.RaycastNonAlloc(ray, hits);
                    for (int j = 0; j < hitNum; j++)
                    {
                        GameObject HitObj = hits[j].collider.gameObject;
                        if (HitObj.tag == "EnemyHiter") HitObj.GetComponent<EnemyHitController>().onEnemyDrag();
                        if (HitObj.tag == "Switch") HitObj.GetComponent<SwitchReceiver>().onSwitchDrag();
                        if (HitObj.tag == "Torch") HitObj.GetComponent<TorchSwitch>().onTorchDrag();
                        if (HitObj.tag == "MTorch") HitObj.GetComponent<ChirdTorchSwitch>().onMTorchDrag();
                    }
                }
            }
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit[] hits = new RaycastHit[4];
                int hitNum = Physics.RaycastNonAlloc(ray, hits);
                for (int j = 0; j < hitNum; j++)
                {
                    GameObject HitObj = hits[j].collider.gameObject;
                    if (HitObj.tag == "EnemyHiter") HitObj.GetComponent<EnemyHitController>().onEnemyDrag();
                    if (HitObj.tag == "Switch") HitObj.GetComponent<SwitchReceiver>().onSwitchDrag();
                    if (HitObj.tag == "Torch") HitObj.GetComponent<TorchSwitch>().onTorchDrag();
                    if (HitObj.tag == "MTorch") HitObj.GetComponent<ChirdTorchSwitch>().onMTorchDrag();
                }
            }
        }
    }
}
