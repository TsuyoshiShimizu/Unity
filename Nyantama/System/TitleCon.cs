using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using GoogleMobileAds.Api;

public class TitleCon : MonoBehaviour
{
    [SerializeField] GameObject Cat = null;
    [SerializeField] GameObject Alarm = null;
    [SerializeField] GameObject TV = null;
    [SerializeField] GameObject Sofa = null;
    [SerializeField] GameObject[] DeleteObj = null;
    [SerializeField] Sprite[] TVSprite = null;
    [SerializeField] Sprite[] OtherSprite = null;
    [SerializeField] GameObject BufferView = null;

   // [SerializeField] GameObject TestCat = null;
  //  LayoutElement testLayoutElement;

    private int CatPosNum = 0;
    private bool isPlayingCatAction = false;
    private float[,] StayCatPos = new float[,] { { 1000, 150 },{ 600, 120, }, { 380, 125, }, { 150, 400, }, { 180, 155, }, { 320, 330, }, { 950, 200, } };
    private float[,] MoveCatPos = new float[,] { { 1000, 150 }, { 600, 120, }, { 380, 125, }, { 270, 230, }, { 180, 155, }, { 380, 220, }, { 950, 200, } };

    private float DeltaDis;                 //一コマの移動距離
    private float WalkDis;                  //一秒の移動距離
    private float StandardWalkTime = 8.0f;  //画面端から端までの移動時間
    private Vector2 MoveVec;                //移動方向
    private float WalkTime;                 //移動時間
    private float WalkDelta = 0;

    private bool MoveFlag = false;
    private Vector2 StartPos;
    private Vector2 EndPos;
    private Vector2 StayPos;
    private Animator CatAnimator;

    private Animator AlarmAnimator;
    private Animator TVAnimator;
    private Image TVimage;
    private bool TVLock = false;

    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.TitleSoundFlag)
        {
            GameManager.TitleSoundFlag = false;
            GameManager.playTitleBGM();
        }
        CatPosNum = 0;
        isPlayingCatAction = false;

        //オブジェクト位置の画面サイズ基準への変換
        float[,] ConversionStayPos = StayCatPos;
        float[,] ConversionMovePos = MoveCatPos;
        float k = Screen.width / 1200f;
        for (int i = 0; i < StayCatPos.Length / 2; i++)
        {
            ConversionStayPos[i, 0] = StayCatPos[i, 0] * k;
            ConversionStayPos[i, 1] = StayCatPos[i, 1] * k;
            ConversionMovePos[i, 0] = MoveCatPos[i, 0] * k;
            ConversionMovePos[i, 1] = MoveCatPos[i, 1] * k;
        }
        StayCatPos = ConversionStayPos;
        MoveCatPos = ConversionMovePos;
        WalkDis = Screen.width / StandardWalkTime;
        DeltaDis = WalkDis / 50f;

        CatAnimator = Cat.GetComponent<Animator>();
        AlarmAnimator = Alarm.GetComponent<Animator>();
        TVimage = TV.GetComponent<Image>();
        TVAnimator = TV.GetComponent<Animator>();

        //ステージクリア数の違いによるアイテムオブジェクトの変化
        int StageClearCount = GameManager.GetClearCount();
        if (StageClearCount >= 10) DeleteObj[3].SetActive(true);
        if (StageClearCount >= 20)
        {
            DeleteObj[4].SetActive(true);
            DeleteObj[5].SetActive(true);
        }
        if (StageClearCount >= 30) Sofa.SetActive(true);
        if (StageClearCount >= 40) TV.SetActive(true);
        if (StageClearCount >= 50) DeleteObj[2].SetActive(true);
        if (GameManager.eventFlag[2]) Sofa.GetComponent<Image>().sprite = OtherSprite[0];

    }

    public void StartButton()
    {
        GameManager.stopLoopSE();
        GameManager.playSystemSE(1);
        GameManager.GoStageSelect();
    }

    private void FixedUpdate()
    {
        WalkMove();
    }
    
    private void Update()
    {
        if (isPlayingCatAction || TVLock) return;
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Input.mousePosition, Vector2.zero);
            if (hit)
            {
                string ClickItem = hit.collider.gameObject.name;
                int ItemNum = 0;
                if (ClickItem == "Carpet") ItemNum = 1;
                if (ClickItem == "Alarm") ItemNum = 2;
                if (ClickItem == "CatTowar1") ItemNum = 3;
                if (ClickItem == "Fan") ItemNum = 4;
                if (ClickItem == "Sofa") ItemNum = 5;
                if (ClickItem == "TV") ItemNum = 6;
                if (CatPosNum == 6 && ItemNum == 6) TVAction();
                if (CatPosNum == ItemNum) return;
                if (ItemNum != 0) CatAction(ItemNum);
            }
        }
    }
    

    private void CatAction(int PNum)
    {
        isPlayingCatAction = true;
        StartPos = new Vector2(MoveCatPos[CatPosNum, 0], MoveCatPos[CatPosNum, 1]);
        EndPos = new Vector2(MoveCatPos[PNum, 0], MoveCatPos[PNum, 1]);
        StayPos = new Vector2(StayCatPos[PNum, 0], StayCatPos[PNum, 1]);
        MoveVec = (EndPos - StartPos).normalized;

        int WCount = (int) ( (EndPos - StartPos).magnitude / DeltaDis );
        WalkTime = WCount * 0.02f;
        if(CatPosNum != 2 && CatPosNum != 4) CatAnimator.SetTrigger("Idel3");

        if (PNum == 2)
        {
            AlarmAnimator.SetTrigger("Play");
            GameManager.playOtherBGM(0);
        }
           
        CatPosNum = PNum;

        if(EndPos.x >= StartPos.x)
        {
            Cat.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            Cat.transform.localScale = new Vector3(1, 1, 1);
        }

        
        Cat.transform.position = StartPos;
        Invoke("MoveStart", 1.0f);
    }

    private void MoveStart()
    {
        CatAnimator.SetTrigger("Walk");
        MoveFlag = true;
    }

    private void WalkMove()
    {
        if (!MoveFlag) return;
        WalkDelta += 0.02f;
        if(WalkDelta < WalkTime)
        {
            Cat.transform.position = StartPos + WalkDis * MoveVec * WalkDelta;
        }
        else
        {
            MoveFlag = false;
            WalkDelta = 0;
            Cat.transform.position = EndPos;
            CatAnimator.SetTrigger("Idel3");
            Invoke("MoveEnd", 1.0f);
        }
    }
    
    private void MoveEnd()
    {
        Cat.transform.position = StayPos;
        if (CatPosNum == 1)     //カーペット
        {
            CatAnimator.SetTrigger("Idel2");
            isPlayingCatAction = false;
        }
        if (CatPosNum == 2)     //アラーム
        {
            Cat.transform.localScale = new Vector3(1, 1, 1);
            CatAnimator.SetTrigger("Attack");
            Invoke("ActionEnd", 1.0f);
        }
        if (CatPosNum == 3)     //猫タワー
        {
            CatAnimator.SetTrigger("Idel4");
            isPlayingCatAction = false;
        }
        if (CatPosNum == 4)     //扇風機
        {
            Cat.transform.localScale = new Vector3(1, 1, 1);
            isPlayingCatAction = false;
        }
        if (CatPosNum == 5)     //ソファー
        {
            CatAnimator.SetTrigger("Idel1");
            
            if (GameManager.eventFlag[2] && !GameManager.PlayerKingBuffer) OpenBufferView();
            else isPlayingCatAction = false;
        }
        if (CatPosNum == 6)     //テレビ
        {
            CatAnimator.SetTrigger("Idel5");
            Cat.transform.localScale = new Vector3(1, 1, 1);
            TVAction();
        }
    }

    private void ActionEnd()
    {
        isPlayingCatAction = false;
    }

    private void TVAction()
    {
        int R = Random.Range(0, 5);
        TVimage.sprite = TVSprite[R];
        GameManager.playOtherBGM(R + 1);
        if (R == 4)
        {
            DeleteObj[0].SetActive(false);
            DeleteObj[1].SetActive(false);
            TVLock = true;
            Invoke("TVAction2", 1.0f);
        }
        else
        {
            isPlayingCatAction = false;
        }   
    }

    private void TVAction2()
    {
        for (int i = 0; i < DeleteObj.Length; i++)
        {
            DeleteObj[i].SetActive(false);
        }
        TVAnimator.SetTrigger("Action");
        Invoke("TVAction3", 2.0f);
    }

    private void TVAction3()
    {
        GameManager.TitleSoundFlag = true;
        GameManager.GoStage(1000);
    }

    private void OpenBufferView()
    {
        GameManager.PlayerKingBuffer = true;
        GameManager.playSystemSE(1);
        BufferView.SetActive(true);
    }

    public void CloseBufferView()
    {
        BufferView.SetActive(false);
        GameManager.playSystemSE(4);
        isPlayingCatAction = false;
    }
}
