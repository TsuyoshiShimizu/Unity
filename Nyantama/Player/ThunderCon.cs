using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderCon : MonoBehaviour
{
    [SerializeField] private GameObject Lightning = null;
    [SerializeField] private DigitalRuby.LightningBolt.LightningBoltScript LightningCs = null;
    [SerializeField] private GameObject ThunderEffect = null;
    [SerializeField] private GameObject ThunderObj = null;
    private GameObject StartObj;
    private GameObject EndObj;
    private GameObject PlayerObj;
    private List<GameObject> EnemyObj = new List<GameObject>();
    private SphereCollider SCollider;

    private int ThunderStage = 0; //0;初期化まだ 1:初期化終了 2:スパーク可能 3:感電
    private float SparkStartTime = 0.4f;
    private float ShockStartTime = 0.8f;
    private float DeleteTime = 1.6f;
    private float timer = 0;

    private int CreateThunderCount = 0;

    private Vector3 TVec;

    public void init(GameObject SObj, Vector3 vec)
    {
        GameManager.playStageSE(15);
        StartObj = SObj;
        PlayerObj = SObj;
        ThunderStage = 1;
        TVec = vec;
        SCollider = GetComponent<SphereCollider>();
    }

    public void init(GameObject SObj, GameObject EObj)
    {
        GameManager.playStageSE(15);
        StartObj = SObj;
        PlayerObj = SObj;
        EndObj = EObj;
        EnemyObj.Add(EObj);
        ThunderStage = 1;

        LightningCs.StartObject = SObj;
        LightningCs.EndObject = EObj;
        Lightning.SetActive(true);
        SCollider = GetComponent<SphereCollider>();
    }

    public void init(GameObject[] EObjs,GameObject PObj)
    {
        GameManager.playStageSE(15);
        StartObj = EObjs[EObjs.Length - 2];
        EndObj = EObjs[EObjs.Length - 1];
        for(int i = 0; i < EObjs.Length; i++)
        {
            if (EObjs[i] != null) EnemyObj.Add(EObjs[i]);
        }
        LightningCs.StartObject = StartObj;
        LightningCs.EndObject = EndObj;
        PlayerObj = PObj;
        ThunderStage = 1;
        Lightning.SetActive(true);
        SCollider = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        if (ThunderStage == 0) return;
        timer += Time.deltaTime;
        if(timer >= SparkStartTime && ThunderStage == 1)
        {
            ThunderStage = 2;
            
        }
        else if (timer >= ShockStartTime && ThunderStage == 2)
        {
            SCollider.radius = 2;
            if (EndObj != null) Lightning.SetActive(false);
            ThunderStage = 3;
            ThunderEffect.SetActive(true);
            gameObject.tag = "Attack";
        }
        else if (timer >= DeleteTime && ThunderStage == 3)
        {
            ThunderStage = 4;
            Destroy(gameObject);
        }

        if(ThunderStage == 2)
        {
            if(EndObj == null)
            {
                transform.position = StartObj.transform.position + TVec;
            }
            else
            {
                transform.position = EndObj.transform.position;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (CreateThunderCount >= 1) return;
        if (ThunderStage != 2) return;
        GameObject HitObj = other.gameObject;
        if (HitObj.tag != "EnemyHiter") return;
        if (!EnemyObj.Contains(HitObj))
        {
            Debug.Log("ThunderCrate" + transform.position);
            CreateThunderCount += 1;
            EnemyObj.Add(HitObj);
            EnemyObj.RemoveAll(EnemyObj => EnemyObj == null);
            GameObject Thunder = Instantiate(ThunderObj, HitObj.transform.position, Quaternion.identity);
            
            if (EnemyObj.Count >= 2)
            {
                Thunder.GetComponent<ThunderCon>().init(EnemyObj.ToArray(),PlayerObj);
            }
            else
            {
                Thunder.GetComponent<ThunderCon>().init(PlayerObj, HitObj);
            }
        }
    }
}
