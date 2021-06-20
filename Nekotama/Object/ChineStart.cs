using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChineStart : MonoBehaviour
{
    private Cinemachine.CinemachineSmoothPath ChinePath;
    private GameObject player;
    private PlayerController playerController;
    private GameDirector Director;
    private Cinemachine.CinemachineDollyCart Chine;
    private bool ChineTouchFlag = true;
    [SerializeField] private bool StartFlag = true;
    private float ZPos = 0;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            if (playerController.BallFalg && ChineTouchFlag)
            {
                ChineTouchFlag = false;         
                if (playerController.BallChineFalg)  //解除時
                {
                    Invoke("CanTouch", 4.0f);
                    playerController.Pzp = ZPos;
                    player.transform.localPosition = new Vector3
                        ( player.gameObject.transform.localPosition.x
                        , player.gameObject.transform.localPosition.y
                        , ZPos);
                    Chine.enabled = false;
                    player.gameObject.GetComponent<Rigidbody>().isKinematic = false;
                    Director.UseIcon(2);
                }
                else　//起動時
                {
                    Invoke("CanTouch", 2.0f);
                    Chine.m_Path = ChinePath;
                    if (StartFlag)
                    {
                        Chine.m_Position = 0;
                        Chine.m_Speed = 3;
                    }
                    else
                    {
                        Chine.m_Position = 10000;
                        Chine.m_Speed = -3;
                    }
                    Chine.enabled = true;
                    player.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                    Director.NoUseIcon(2);
                }
                playerController.BallChineFalg = !playerController.BallChineFalg;
            }
        }
    }

    private void CanTouch()
    {
        ChineTouchFlag = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        ChinePath = GetComponentInParent<Cinemachine.CinemachineSmoothPath>();
        player = GameManager.PlayerObj;
        playerController = player.GetComponent<PlayerController>();
        Director = GameManager.DirectorObj.GetComponent<GameDirector>();
        Chine = player.GetComponent<Cinemachine.CinemachineDollyCart>();
        ZPos = transform.parent.transform.localPosition.z + transform.localPosition.z;
    }
}
