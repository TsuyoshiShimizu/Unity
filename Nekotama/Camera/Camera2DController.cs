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

    private void Awake()
    {
        GameObject miniWall = Instantiate(Wall) as GameObject;
        miniWall.transform.position = new Vector3(MinX, 50, 250);

        GameObject maxWall = Instantiate(Wall) as GameObject;
        maxWall.transform.position = new Vector3(MaxX, 50, 250);

      //  GetComponent<Light>().enabled = SpotLightFlag;
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
    }

    void Update()
    {
        if (GameDirector.MagicType != 2) return;
        if (Input.GetMouseButton(0))
        {
          //  Debug.Log("てすと");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                GameObject HitObj = hit.collider.gameObject;
                if (HitObj.tag == "EnemyHiter") HitObj.GetComponent<EnemyHitController>().onEnemyDrag();
                if (HitObj.tag == "Switch") HitObj.GetComponent<SwitchReceiver>().onSwitchDrag();
                if (HitObj.tag == "Torch") HitObj.GetComponent<TorchSwitch>().onTorchDrag();
                if (HitObj.tag == "MTorch") HitObj.GetComponent<ChirdTorchSwitch>().onMTorchDrag();
            }
        }
    }

    private void FixedUpdate()
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
}
