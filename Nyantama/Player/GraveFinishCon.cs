using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraveFinishCon : MonoBehaviour
{
    [SerializeField] private GameObject PunchBody = null;
    [SerializeField] private GameObject[] GraveBody = null;
    [SerializeField] private GameObject[] GraveEffect = null;

    private float ChangeTime = 0.55f;
    private bool ChangeFlag = true;

    private float LiveTime = 2.0f;
    private float LagTime = 0.2f;
    private int GraveCount = 0;
    private int GraveEndCount = 0;

    private float EndTime = 2.55f;
    private bool EndFlag = true;



    private float DeltaTime = 0;
    private float SETime = 0.2f;
    private bool SEFlag = true;
    // Start is called before the first frame update


    // Update is called once per frame
    void Update()
    {
        DeltaTime += Time.deltaTime;
        if (DeltaTime >= SETime && SEFlag)
        {
            SEFlag = false;
            GameManager.playStageSE(50);
        }

        if (DeltaTime >= ChangeTime && ChangeFlag)
        {
            ChangeFlag = false;
            PunchBody.SetActive(false); 
            GraveEffect[0].SetActive(true);
            GraveEffect[1].SetActive(true);
            GraveEffect[2].SetActive(true);
        }

        for(int i = 0; i < GraveBody.Length; i++)
        {
            if(DeltaTime >= ChangeTime + LagTime * i && GraveCount == i)
            {
                GraveBody[i].SetActive(true);
                GraveCount++;
                //UnityEditor.EditorApplication.isPaused = true;
            }
            if (DeltaTime >= ChangeTime + LiveTime + LagTime * i && GraveEndCount == i)
            {
                GraveBody[i].SetActive(false);
                GraveEndCount++;
                //UnityEditor.EditorApplication.isPaused = true;
            }
        }
    }
}
