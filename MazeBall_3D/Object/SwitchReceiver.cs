using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchReceiver : MonoBehaviour
{
    [SerializeField] GameObject EffectObj = null;
    private SwitchController Controller;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && Controller.SFlag)
        {
            EffectObj.SetActive(true);
            GameManager.playStageSE(1);
            Controller.SwitchChange();
            GetComponent<Collider>().enabled = false;
            GetComponent<MeshRenderer>().enabled = false;
            Invoke("OffSwitch", 1.0f);
        }
    }

    private void OffSwitch()
    {
        EffectObj.SetActive(false);
        gameObject.SetActive(false);
        GetComponent<Collider>().enabled = true;
        GetComponent<MeshRenderer>().enabled = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        Controller = GetComponentInParent<SwitchController>();
    }
}
