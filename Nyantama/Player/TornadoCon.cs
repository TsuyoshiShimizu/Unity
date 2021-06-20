using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TornadoCon : MonoBehaviour
{
    [SerializeField] private Vector3 SpeedVec = Vector3.zero;


    // Update is called once per frame
    void Update()
    {
        transform.localPosition += SpeedVec * Time.deltaTime;
    }
}
