using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Panishment : MonoBehaviour
{
    [SerializeField] GameObject PanishEffect = null;
    [SerializeField] GameObject FinishObj = null;
    private float FinishAttackTime = 4.0f;
    private float FinishAttackEndTime = 4.5f;
    private float DestroyTime = 5.0f;
    private bool FinishFlag = true;
    private bool FinishAttackEndFlag = true;
    private bool DestroyFlag = true;
    private float TimeDelta = 0;

 //   private Vector3 VelocityVec = Vector3.right;
    private float Speed = 2f;

    private Rigidbody rigid;

    private void Start()
    {
        GameManager.playStageSE(48);
    }

    // Update is called once per frame
    void Update()
    {
        TimeDelta += Time.deltaTime;
        if(FinishFlag && TimeDelta >= FinishAttackTime)
        {
            FinishFlag = false;
            PanishEffect.SetActive(false);
            FinishObj.SetActive(true);
            GameManager.playStageSE(49);
        }
        if(FinishAttackEndFlag && TimeDelta >= FinishAttackEndTime)
        {
            FinishAttackEndFlag = false;
            FinishObj.GetComponent<Collider>().enabled = false;
        }
        if(DestroyFlag && TimeDelta >= DestroyTime)
        {
            DestroyFlag = false;
            Destroy(gameObject);
        }
    }

    public void ChangeVelocity(Vector3 velocity)
    {
        rigid = GetComponent<Rigidbody>();
        rigid.velocity = velocity * Speed;

    }

}
