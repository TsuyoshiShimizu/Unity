using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringCon : MonoBehaviour
{
    [SerializeField] private GameObject CentorObj = null;
   // private SpringBlockCon SCon;
    private float ForceValue = 600f;
    private Vector3 ForceVec;
    private Rigidbody rigid;

    // Start is called before the first frame update
    void Start()
    {
      //  SCon = transform.parent.gameObject.GetComponent<SpringBlockCon>();
        float springLV = transform.parent.gameObject.GetComponent<SpringBlockCon>().GetSpringLv();
        if (springLV == 0) ForceValue = 850f;
        if (springLV == 2) ForceValue = 800f; 
        else if (springLV == 3) ForceValue = 1000f;

        
        rigid = GameManager.PlayerObj.GetComponent<Rigidbody>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!GameManager.PlayerBallFlag) return;
        if (!GameManager.SpringFlag) return;
        if (other.gameObject.tag != "Player") return;
        GameManager.SpringFlag = false;
        ForceVec = (transform.position - CentorObj.transform.position).normalized;
        GameManager.playStageSE(19);

        rigid.Sleep();
        rigid.AddForce(ForceVec * ForceValue);
    }
}
