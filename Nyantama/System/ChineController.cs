using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChineController : MonoBehaviour
{
    [SerializeField] private Cinemachine.CinemachineSmoothPath CinemaPath = null;
    [SerializeField] private GameObject StartObj = null;
    [SerializeField] private GameObject EndObj = null;

    private void OnValidate()
    {
        StartObj.transform.localPosition = CinemaPath.m_Waypoints[0].position;
        EndObj.transform.localPosition = CinemaPath.m_Waypoints[CinemaPath.m_Waypoints.Length - 1].position;
    }
}
