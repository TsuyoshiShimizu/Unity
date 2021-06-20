using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GameOverAction : MonoBehaviour
{
    [SerializeField] private GameObject ContinuButton = null;

    public void ContinuStartCheck()
    {

        if(PlayerController.SavePoint != Vector3.zero)
        {
            ContinuButton.SetActive(true);
        }
        else
        {
            ContinuButton.SetActive(false);
        }
    }
}
