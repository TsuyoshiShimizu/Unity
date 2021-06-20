using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChineStart : MonoBehaviour
{
   // private Cinemachine.CinemachineSmoothPath ChinePath;
    private GameObject player;
    private PlayerController playerController;
    private GameDirector Director;
   // private Cinemachine.CinemachineDollyCart Chine;
    private bool ChineTouchFlag = true;
    [SerializeField] private bool StartFlag = true;
    [SerializeField] private Cinemachine.CinemachineSmoothPath ChinePath;
    private float ZPos = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!ChineTouchFlag) return;
        if (other.gameObject.tag != "Player") return;
        if (!playerController.BallFalg) return;


        ChineTouchFlag = false;
        if (playerController.BallChineFalg)  //解除時
        {
            Invoke("CanTouch", 4.0f);
    //        playerController.Pzp = ZPos;
            playerController.EndLightBall(ZPos);
        }
        else //起動時
        {
            Invoke("CanTouch", 2.0f);
            playerController.StartLightBall(ChinePath,StartFlag);
        }
    }

    private void CanTouch() => ChineTouchFlag = true;

    // Start is called before the first frame update
    void Start()
    {
        player = GameManager.PlayerObj;
        playerController = player.GetComponent<PlayerController>();
        // ZPos = transform.parent.transform.position.z + transform.localPosition.z;
        ZPos = transform.position.z;
    }
}
