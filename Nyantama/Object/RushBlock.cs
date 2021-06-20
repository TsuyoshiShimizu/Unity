using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RushBlock : MonoBehaviour
{
    [SerializeField] GameObject FlontObj = null;
    [SerializeField] GameObject BackObj = null;
    [SerializeField] GameObject EffectObj = null;

    public Vector3 RushVec { set; get; }
    private float EffectTime = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RushVec = (FlontObj.transform.position - BackObj.transform.position).normalized;
    }

    public void PlayEffect()
    {
        EffectObj.SetActive(true);
        Invoke("EndEffect", EffectTime);
    }

    private void EndEffect()
    {
        EffectObj.SetActive(false);
    }
}
