using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleBall : MonoBehaviour
{
    private Rigidbody2D rigid;
    private float ForcePow = 5000;

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        AddF();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AddF()
    {
        float RSita = Random.Range(-Mathf.PI, Mathf.PI);

        rigid.AddForce(new Vector2(ForcePow * Mathf.Sin(RSita), ForcePow * Mathf.Cos(RSita)));
    }

    public void BallClick()
    {
        AddF();
    }
}
