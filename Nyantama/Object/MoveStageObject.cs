using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveStageObject : MonoBehaviour
{
    [SerializeField] private int MoveFieldNum = 0;
    [SerializeField] private int MoveStageNum = 0;
    [SerializeField] private int PlayerLvChange = 0;
    [SerializeField] private bool GoalFlag = false;
    [HideInInspector]
    [SerializeField] private GameObject ClearEffect = null;
    [HideInInspector]
    [SerializeField] private GameObject NoClearEffect = null;
    [HideInInspector]
    [SerializeField] private GameObject StartPoint = null;

    private bool contactFlag = true;
    private int EventStageNumber = 0;
    private Vector3 StartPos = Vector3.zero;
    private Quaternion StartRot = Quaternion.identity;

    // Start is called before the first frame update
    void Start()
    {
        StartPos = StartPoint.transform.position;
        StartRot = StartPoint.transform.rotation;
        StartPoint.SetActive(false);

        //ステージナンバーの記憶
        if(MoveStageNum != 0)
        {
            if (MoveFieldNum == 1) //トレーニングルーム
            {
                EventStageNumber = MoveStageNum;
            }
        }

        //エフェクトの変更
        if (GameManager.eventStageFlag[EventStageNumber] || (MoveFieldNum == 0 && MoveStageNum == 0) || GoalFlag)
        {
            ClearEffect.SetActive(true);
            NoClearEffect.SetActive(false);
        }
        else
        {
            ClearEffect.SetActive(false);
            NoClearEffect.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!contactFlag) return;
        if (other.gameObject.tag != "Player") return;

        GameDirector Director = GameManager.DirectorObj.GetComponent<GameDirector>();

        if(MoveFieldNum == 0 && MoveStageNum == 0)
        {
            Director.GoHome();
        }
        else
        {
            if(PlayerLvChange != 0)
            {
                GameManager.PlayerLv = PlayerLvChange;
                GameManager.setPlayerStatas(GameManager.getPlayerStatas(PlayerLvChange));
            }

            if(GoalFlag && MoveStageNum == 0)
            {
                GameManager.eventStageFlag[GameManager.PlayEventSrageNum] = true;
                GameManager.saveGameData();
            }
            else
            {
                GameManager.PlayEventSrageNum = EventStageNumber;
                GameManager.FieldPoint = StartPos;
                GameManager.FieldRot = StartRot;
            }
       
            GameManager.StageNum = MoveStageNum;
            GameManager.FieldNum = MoveFieldNum;
            Director.StageMove();            
        }
    } 

    // Update is called once per frame
    void Update()
    {
        
    }
}
