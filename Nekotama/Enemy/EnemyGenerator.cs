using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    [SerializeField] private EnemyType enemyType = EnemyType.Slime;             //召喚する敵
    [SerializeField] private bool Viewflag = false;                     //召喚クリスタルが見えるか
    [SerializeField] private int SwitchNumber = 0;                      //スイッチでアクションを起動させる
    [SerializeField] private EnemyMode enemyMode = EnemyMode.Mode3D;    //敵のモード
    [SerializeField] private ActiveType activeType = ActiveType.negative;
    [SerializeField] private Direction direction = Direction.Right;     //敵の初期の向き（2D限定）
    [SerializeField] private int Level = 1;                             //敵のレベル
    [SerializeField] private int MaxEeneyNumber = 1;                    //敵の最大出現数
    [SerializeField] private int SameEnemyNumber = 1;                   //敵が同時に何体存在できるか
    [SerializeField] private float NextInterval = 10.0f;                //次の敵が出現するまでの時間
    [SerializeField] private float AddDis = 8.0f;                       //敵を出現させる距離
    [SerializeField] private float DeleteDis = 15.0f;                   //敵の活動限界距離
    [SerializeField] private GameObject[] EnemyPrefabs = null;
    [SerializeField] private GameObject[] EffectObj = null;

    private int EnemyCount = 0;
    private bool AddFlag = true;
    private float AddDelta = 0;
    private bool DeletaFlag = true;
   // private bool MetalFlag = false;
  //  private Effekseer.EffekseerEmitter EEmitter;


    //  private AudioSource aud;
    private enum EnemyMode { Mode3D, Mode2D }
    private enum Direction { Right, Left }
    private enum EnemyType { Slime, BlueSlime, Turtle, RedTurtle , RedWisp, BlueWisp, BossSlime, BossTurtle, BossWisp , BlueDragon ,MetalSlime, GoldSlime, MetalTurtle , GoldTurtle, MetalWisp, GoldWisp }
    private enum ActiveType { negative , normal, aggressive };

    private Shader TransShader;

    private void Awake()
    {
        if(Viewflag == false)
        {
            Material mat = GetComponent<Renderer>().material;
            mat.shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
            Color color = mat.color;
            color.a = 0;
            mat.color = color;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
   //     EEmitter = GetComponent<Effekseer.EffekseerEmitter>();
        TransShader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        inputAddEnemy();        //敵の出現
        recoverAddFlag();       //敵の出現フラグ処理
        
        //召喚器の消滅
        if (MaxEeneyNumber <= 0)
        {
            if (DeletaFlag)
            {
                DeletaFlag = false;
                if (Viewflag)
                {
                    // GetComponent<Effekseer.EffekseerEmitter>().Play(1);
                    EffectObj[1].SetActive(true);
                    GameManager.playStageSE(6);
                    Material mat = GetComponent<Renderer>().material;
                    mat.shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
                    Color color = mat.color;
                    color.a = 0;
                    mat.color = color;
                    Invoke("Delete", 2.0f);
                    GameDirector.PlayObjAction(SwitchNumber);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }      
    }

    /// <summary>
    /// 敵を追加する処理
    /// </summary>
    private void inputAddEnemy()
    {
        if (EnemyCount >= MaxEeneyNumber) return;   //敵の出現数が最大出現数以上の場合
        if (EnemyCount >= SameEnemyNumber) return;  //敵の出現数が同時出現数上限以上の場合
        if (!AddFlag) return;
        Vector3 PlayerDis = PlayerController.PlayerPos - transform.position;
        if (PlayerDis.magnitude > AddDis) return;
        AddFlag = false;
        //    EEmitter.Play(0);
        EffectObj[0].SetActive(true);
        GameManager.playStageSE(5);

        GameObject enemyPrefab = EnemyPrefabs[0];
        bool MetalFlag = false;
        if (enemyType == EnemyType.Slime) enemyPrefab = EnemyPrefabs[0];
        if (enemyType == EnemyType.BlueSlime) enemyPrefab = EnemyPrefabs[1];
        if (enemyType == EnemyType.Turtle) enemyPrefab = EnemyPrefabs[2];
        if (enemyType == EnemyType.RedTurtle) enemyPrefab = EnemyPrefabs[3];
        if (enemyType == EnemyType.RedWisp) enemyPrefab = EnemyPrefabs[4];
        if (enemyType == EnemyType.BlueWisp) enemyPrefab = EnemyPrefabs[5];
        if (enemyType == EnemyType.BossSlime) enemyPrefab = EnemyPrefabs[6];
        if (enemyType == EnemyType.BossTurtle) enemyPrefab = EnemyPrefabs[7];
        if (enemyType == EnemyType.BossWisp) enemyPrefab = EnemyPrefabs[8];
        if (enemyType == EnemyType.BlueDragon) enemyPrefab = EnemyPrefabs[9];
        if (enemyType == EnemyType.MetalSlime)
        {
            enemyPrefab = EnemyPrefabs[10];
            MetalFlag = true;
        }
        if (enemyType == EnemyType.MetalSlime)
        {
            enemyPrefab = EnemyPrefabs[10];
            MetalFlag = true;
        }
        if (enemyType == EnemyType.GoldSlime)
        {
            enemyPrefab = EnemyPrefabs[11];
            MetalFlag = true;
        }
        if (enemyType == EnemyType.MetalTurtle)
        {
            enemyPrefab = EnemyPrefabs[12];
            MetalFlag = true;
        }
        if (enemyType == EnemyType.GoldTurtle)
        {
            enemyPrefab = EnemyPrefabs[13];
            MetalFlag = true;
        }
        if (enemyType == EnemyType.MetalWisp)
        {
            enemyPrefab = EnemyPrefabs[14];
            MetalFlag = true;
        }
        if (enemyType == EnemyType.GoldWisp)
        {
            enemyPrefab = EnemyPrefabs[15];
            MetalFlag = true;
        }
        int ActiveInt = 0;
        if (activeType == ActiveType.normal) ActiveInt = 1;
        if (activeType == ActiveType.aggressive) ActiveInt = 2;

        GameObject enemyObj = Instantiate(enemyPrefab, transform.position, transform.rotation);
        bool Mode = true; if (enemyMode == EnemyMode.Mode2D) Mode = false;
        bool Dir = true; if (direction == Direction.Left) Dir = false;
        enemyObj.GetComponent<EnemyDamageController>().EnemySetup(gameObject, Level, enemyPrefab.name, Mode, Dir, DeleteDis, MetalFlag, ActiveInt);
        EnemyCount += 1;
    }

    /// <summary>
    /// 敵の出現インターバルの処理
    /// </summary>
    private void recoverAddFlag()
    {
        if (EnemyCount >= MaxEeneyNumber) return;   //敵の出現数が最大出現数以上の場合
        if (EnemyCount >= SameEnemyNumber) return;  //敵の出現数が同時出現数上限以上の場合
        if (AddFlag) return;
        AddDelta += 0.02f;
        if (AddDelta >= NextInterval)
        {
            AddDelta = 0;
            AddFlag = true;
        }
    }


    private void Delete()
    {
        Destroy(gameObject);
    }

    IEnumerator DelayDelete(float delay, GameObject Obj)
    {
        yield return new WaitForSeconds(delay);
        Destroy(Obj);
    }

    /// <summary>
    /// 敵の消失による出現数の変更
    /// </summary>
    /// <param name="kill"></param>
    public void EnemyDeleteCount(bool kill)
    {
        if (kill) MaxEeneyNumber -= 1;
        EnemyCount -= 1;
    }

    
}
