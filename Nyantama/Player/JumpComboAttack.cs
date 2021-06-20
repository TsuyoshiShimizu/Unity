using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpComboAttack : MonoBehaviour
{
    // Start is called before the first frame update
    private float StartSize = 0.2f;     //開始サイズ
    private float ExpansionSpeed = 3.5f;  //サイズの拡大スピード
    private float TimeDelta = 0;


    // Update is called once per frame
    void Update()
    {
        TimeDelta += Time.deltaTime;
        transform.localScale = Vector3.one * (StartSize + ExpansionSpeed * TimeDelta);
    }

    private void OnEnable()
    {
        TimeDelta = 0;
        transform.localScale = Vector3.one * StartSize;
    }
}
