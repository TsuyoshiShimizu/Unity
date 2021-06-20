using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopController : MonoBehaviour
{
   // [SerializeField] GameObject[] ShopItem = null;

    private bool ContactFlag = true;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && ContactFlag)
        {
            ContactFlag = false;
            GameManager.GoStageSelect();
        }
    }
}
