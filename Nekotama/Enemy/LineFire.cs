using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineFire : MonoBehaviour
{
    public float FireSpeed { get; set; }
    private Rigidbody rigid;
    private float Delta = 0;

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
        GameManager.playStageSE(7);
    }

    public void FireShot()
    {
        Vector3 FireVec = PlayerController.PlayerPos - transform.position;
        rigid.velocity = FireVec.normalized * FireSpeed;
        GameManager.playStageSE(8);
    }

    private void FixedUpdate()
    {
        Delta += 0.02f;
        if(Delta > 20)
        {
            Destroy(gameObject);
        }
    }
}
