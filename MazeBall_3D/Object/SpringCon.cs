using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringCon : MonoBehaviour
{
    [SerializeField] private float Scale_i = 1;
    private GameObject ParentBlock = null;
    private float ForceValue = 500f;
    private Vector3 ForceVec;
    private Rigidbody rigid;

    // Start is called before the first frame update
    void Start()
    {
        ParentBlock = transform.parent.gameObject;
        Vector3 ScaleVec = transform.localScale;
        if(Scale_i == 1)
        {
            ScaleVec.y = 0.1f / ParentBlock.transform.localScale.x;
            ScaleVec.x -= 0.1f / ParentBlock.transform.localScale.y;
            ScaleVec.z -= 0.1f / ParentBlock.transform.localScale.z;
        }
        else if(Scale_i == 2)
        {
            ScaleVec.y = 0.1f / ParentBlock.transform.localScale.y;
            ScaleVec.x -= 0.1f / ParentBlock.transform.localScale.x;
            ScaleVec.z -= 0.1f / ParentBlock.transform.localScale.z;
        }
        else if(Scale_i == 3)
        {
            ScaleVec.y = 0.1f / ParentBlock.transform.localScale.z;
            ScaleVec.x -= 0.1f / ParentBlock.transform.localScale.x;
            ScaleVec.z -= 0.1f / ParentBlock.transform.localScale.y;
        }

        float springLV = ParentBlock.GetComponent<SpringBlockCon>().GetSpringLv();
        if (springLV == 2) ForceValue *= 1.5f; 
        else if (springLV == 3) ForceValue *= 2.0f;

        transform.localScale = ScaleVec;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player" && EventManager.SpringFlag)
        {
            EventManager.SpringFlag = false;
            ForceVec = (transform.position - ParentBlock.transform.position).normalized;
            if(rigid == null) rigid = other.gameObject.GetComponent<Rigidbody>();
            GameManager.playStageSE(2);
            // rigid.isKinematic = true;
            rigid.constraints = RigidbodyConstraints.FreezePosition;
            Invoke("Force1", 0.05f);
        }
    }

    private void Force1()
    {
        //rigid.isKinematic = false;
        rigid.constraints = RigidbodyConstraints.None;
        rigid.AddForce(ForceVec * ForceValue);
      //  Invoke("Force2", 0.05f);
    }
    /*
    private void Force2()
    {
        rigid.AddForce(ForceVec * ForceValue);
    }
    */
}
