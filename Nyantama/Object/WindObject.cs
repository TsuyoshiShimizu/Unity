using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindObject : MonoBehaviour
{
    [SerializeField] private float windSpeed = 1.0f;
    [SerializeField] private GameObject WindVecObj = null;
    public Vector3 WindVector { set; get; }

    // Start is called before the first frame update
    void Start()
    {
        WindVector = (WindVecObj.transform.position - transform.transform.position).normalized * windSpeed;
    }

    /*`
    private void Update()
    {
        WindVector = (WindVecObj.transform.position - transform.transform.position).normalized * windSpeed;
    }
    */


}
