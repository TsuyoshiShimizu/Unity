using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    [SerializeField] private float DestroyTime = 1.0f;
    private float TimeDelta = 0;

    // Update is called once per frame
    void Update()
    {
        TimeDelta += Time.deltaTime;
        if (TimeDelta < DestroyTime) return;

        Destroy(gameObject);
    }
}
