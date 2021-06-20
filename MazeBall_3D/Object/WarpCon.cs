using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpCon : MonoBehaviour
{
    [SerializeField] GameObject MoveObj = null;

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Player" && EventManager.WarpFlag)
        {
            EventManager.WarpFlag = false;
            GameManager.playStageSE(4);
            other.transform.position = MoveObj.transform.position;
        }
    }


}
