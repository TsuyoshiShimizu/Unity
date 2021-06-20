using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatAnimtor : MonoBehaviour
{
    [SerializeField] GameObject Alarm = null;
    Animator AlarmAnimator;

    // Start is called before the first frame update
    void Start()
    {
        AlarmAnimator = Alarm.GetComponent<Animator>();
    }

 
    public void playAttackSE() => GameManager.playSystemSE(9);
    public void AttackAlarm()
    {
        GameManager.playLoopSE(2);
        AlarmAnimator.SetTrigger("Shot");
    }
}
