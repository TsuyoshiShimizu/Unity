using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FootSensor : MonoBehaviour
{
    public UnityEvent OnEnterGround;
    public UnityEvent OnExitGround;
    private PlayerController player;

    private void OnTriggerStay(Collider other)
    {
        //  Debug.Log("接触中");

        if (player.OnGround == false && player.SensorLock == false && other.gameObject.tag != "Attack") 
        {
            OnEnterGround.Invoke();
          //  Debug.Log("接触による着地");
          //  Debug.Log("tag " + other.gameObject.tag);
          //  Debug.Log("layer " + other.gameObject.layer);
        }

        //3Dモードと2Dモードの切り替え
        if (!PlayerController.ModeLock)
        {
            if(GameDirector.Player3DMode && (other.gameObject.tag  == "2DObject" || other.gameObject.tag == "2DMoveObject" ))
            {
             //   Debug.Log("チェンジ2Dカメラ");
                PlayerController.ModeLock = true;
                player.Pzp = other.gameObject.transform.localPosition.z;
                player.Change2DMode();
            }
            else if (!GameDirector.Player3DMode && (other.gameObject.tag == "3DObject" || other.gameObject.tag == "3DMoveObject") )
            {
              //  Debug.Log("チェンジ3Dカメラ");
                PlayerController.ModeLock = true;
                player.Change3DMode();
            }
        }
        /*
        //復活地点の記憶
        if (other.gameObject.tag == "2DObject")
        {
            Vector3 RPos = PlayerController.PlayerPos;
            if (PlayerController.PlayerPos.x > other.transform.position.x) RPos += Vector3.left;
            if (PlayerController.PlayerPos.x < other.transform.position.x) RPos += Vector3.right;
            PlayerController.RebornPos = RPos;
        }
        if (other.gameObject.tag == "3DObject")
        {
            Vector3 RPos = PlayerController.PlayerPos;
            if (PlayerController.PlayerPos.x > other.transform.position.x) RPos += Vector3.left;
            if (PlayerController.PlayerPos.x < other.transform.position.x) RPos += Vector3.right;
            if (PlayerController.PlayerPos.z > other.transform.position.z) RPos += Vector3.back;
            if (PlayerController.PlayerPos.z < other.transform.position.z) RPos += Vector3.forward;
            PlayerController.RebornPos = RPos;
        }
        */
    }

    private void OnTriggerExit(Collider other)
    {
        if (player.SensorLock == false)
        {
            OnExitGround.Invoke();
          //  Debug.Log("接触による空中");
          //  Debug.Log("tag " + other.gameObject.tag);
          //  Debug.Log("layer " + other.gameObject.layer);
        }
    }

    private void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }

}
