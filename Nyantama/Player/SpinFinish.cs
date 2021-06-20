using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinFinish : MonoBehaviour
{
    [SerializeField] private float RotationSpeed = 1.0f;
    [SerializeField] private float LifeTime = 6f;

    private bool LifeFlag = true;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        LifeTime -= Time.deltaTime;
        transform.Rotate(Vector3.up * RotationSpeed * Time.deltaTime);

        if(LifeTime <= 0 && LifeFlag)
        {
            LifeFlag = false;
            Destroy(gameObject);
        }
    }
}
