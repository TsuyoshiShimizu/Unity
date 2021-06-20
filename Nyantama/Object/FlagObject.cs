using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagObject : MonoBehaviour
{
    private bool ContactFlag = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!ContactFlag) return;
        if (other.gameObject.tag != "Player") return;
        if (transform.position != PlayerController.SavePoint)  GameManager.playStageSE(32);
        PlayerController.SavePoint = transform.position;
        GameDirector.RebornPos = transform.position;
        Destroy(gameObject);
    }
}
