using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPoint : MonoBehaviour
{
    public int PointNum = 0;

    private void Awake()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
}
