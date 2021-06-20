using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialBlock : MonoBehaviour
{
    public int TutorialNum = 0;
    public int TutorialStage = 0;
    public bool TutorialFinsh = false;
    [SerializeField] private bool Hide = false;

    private void Start()
    {
        if(Hide) GetComponent<MeshRenderer>().enabled = false;
    }

}
