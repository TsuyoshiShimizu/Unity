using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConboPunchCon : MonoBehaviour
{
    [SerializeField] private GameObject PunchBody = null;
    [SerializeField] private GameObject GraveBody = null;

    private float ChangeTime = 0.55f;
    private bool ChangeFlag = true;
    private float EndTime = 2.56f;
    private bool EndFlag = true;
    private float DTime = 0;
    private float SETime = 0.2f;
    private bool SEFlag = true;
    // Start is called before the first frame update


    // Update is called once per frame
    void Update()
    {
        DTime += Time.deltaTime;
        if(DTime >= SETime && SEFlag)
        {
            SEFlag = false;
            GameManager.playStageSE(44);
        }

        if(DTime >= ChangeTime && ChangeFlag)
        {
            ChangeFlag = false;
            PunchBody.SetActive(false);
            GraveBody.SetActive(true);
        }
        if(DTime >= EndTime && EndFlag)
        {
            EndFlag = false;
            GraveBody.SetActive(false);
           // UnityEditor.EditorApplication.isPaused = true;
        }

    }
}
