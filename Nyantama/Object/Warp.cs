using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warp : MonoBehaviour
{
    [SerializeField] private GameObject WarpMoveObject;
    public Vector3 WarpMovePos { private set; get; }

    // Start is called before the first frame update
    void Start()
    {
        WarpMovePos = WarpMoveObject.transform.position;
        WarpMoveObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
