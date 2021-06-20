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
        }

        //3Dモードと2Dモードの切り替え
        if (!PlayerController.ModeLock)
        {
            if(other.gameObject.tag == "2DObject" || other.gameObject.tag == "2DMoveObject")
            {
                player.Pzp = other.gameObject.transform.position.z;
                if (GameDirector.Player3DMode)
                {
                    //   Debug.Log("チェンジ2Dカメラ");

                    PlayerController.ModeLock = true;
                    player.Change2DMode();
                }
            }
            else if (!GameDirector.Player3DMode && (other.gameObject.tag == "3DObject" || other.gameObject.tag == "3DMoveObject") )
            {
              //  Debug.Log("チェンジ3Dカメラ");
                PlayerController.ModeLock = true;
                player.Change3DMode();
            }
        }
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
