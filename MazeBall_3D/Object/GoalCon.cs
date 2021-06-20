using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalCon : MonoBehaviour
{
    private bool ContactFlag = true;
   // private float ContactDelta = 0;


    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player" && ContactFlag && EventManager.eventManager != null)
        {
            ContactFlag = false;
            EventManager.eventManager.StageClear();
        }
    }

}
