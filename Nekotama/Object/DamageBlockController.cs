using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageBlockController : MonoBehaviour
{
    private Effekseer.EffekseerEmitter Effect;

    [SerializeField] private int HP = 10;
    [SerializeField] private bool DestroyFlag = false;
    public int Damage = 10;
    private bool TargetFlag = false;
    public bool DamgaeFlag = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Attack")
        {
            if (DamgaeFlag && DestroyFlag)
            {
                DamgaeFlag = false;
                bool ThunderFlag = false;

                if (other.gameObject.GetComponent<MagicController>() != null)
                {
                    if (other.gameObject.GetComponent<MagicController>().MType == 1) ThunderFlag = true;
                }

                if (ThunderFlag)//雷魔法
                {
                    HP -= (GameManager.PlayerMPow * 2);
                }
                else
                {
                    HP -= GameManager.PlayerMPow;
                }

                if (HP > 0)
                {
                    Effect.Play(1);
                }
                else
                {
                    StopTarget();
                    GameDirector.RemoveTarget(this.gameObject);

                    Effect.Play(0);
                    GetComponent<Collider>().enabled = false;
                    Color color = gameObject.GetComponent<Renderer>().material.color;
                    color.a = 0;
                    gameObject.GetComponent<Renderer>().material.color = color;
                    Invoke("delete", 1.0f);
                }
                Invoke("DamageP", 0.5f);
            }
        }
    }

    private void DamageP()
    {
        DamgaeFlag = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        Effect = GetComponent<Effekseer.EffekseerEmitter>();
    }

    private void delete()
    {
        Destroy(gameObject);
    }

    public void onEnemyClick()
    {
        if (TargetFlag) //ターゲットを解除
        {
            StopTarget();
            GameDirector.RemoveTarget(this.gameObject);
        }
        else            //ターゲットする
        {
            StartTarget();
            if (GameDirector.MagicType == 1)
            {
                GameDirector.ChangeTarget(this.gameObject);
            }
            else if (GameDirector.MagicType == 2)
            {
                GameDirector.AddTarget(this.gameObject);
            }
        }
    }

    private void FixedUpdate()
    {
        if (GameDirector.EnemyTargetFlag == false && TargetFlag) StopTarget();
    }

    private void StartTarget()
    {
        TargetFlag = true;
        Effect.Play(2);
    }

    public void StopTarget()
    {
        TargetFlag = false;
        Effect.Stop(2);
    }
}
