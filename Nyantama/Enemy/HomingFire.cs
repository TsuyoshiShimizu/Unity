using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingFire : MonoBehaviour
{
    public float FireSpeed { get; set; }
    private Rigidbody rigid;
    private float Delta = 0;
    private bool FireMove = false;

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
        FireMove = true;
    }

    private void FixedUpdate()
    {
        Delta += 0.02f;
        if(Delta > 10)
        {
            Destroy(gameObject);
        }

        if (FireMove)
        {
            Vector3 NowSpeedVec = rigid.velocity.normalized;
            Vector3 TargetVec = (PlayerController.PlayerPos - transform.position).normalized;
            Vector3 FixVec = TargetVec - NowSpeedVec;
            Vector3 FixSppedVec = (NowSpeedVec + FixVec * 0.05f) * FireSpeed;
            rigid.velocity = FixSppedVec;
        }
    }
}
