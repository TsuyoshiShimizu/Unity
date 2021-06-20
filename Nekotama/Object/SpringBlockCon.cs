using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpringBlockCon : MonoBehaviour
{
    [SerializeField] int SpringLv = 1;
    [SerializeField] private int XN = 1;
    [SerializeField] private int YN = 1;
    [SerializeField] private int ZN = 1;
    [SerializeField] private GameObject ViewBlock = null;
    [SerializeField] private GameObject[] SensorBlock = null;
    [SerializeField] private GameObject CentorObj = null;

    private Material Mat;
    private Vector3 FPos;
    private Vector3 FRot;
    private Vector3 FScale;
    private int Count = 0;

  //  public Vector3 CenterPos { set; get;}

    public int GetSpringLv()
    {
        return SpringLv;
    }

    private void Awake()
    {
        //ビューオブジェクトの非常時処理(メッシュ結合による表示バグを防ぐため)
        int ChirdCount = transform.childCount;
        {
            for (int i = 0; i < ChirdCount; i++)
            {
                GameObject ChirdObj = transform.GetChild(i).gameObject;
                if (ChirdObj.tag == "ViewObject")
                {
                    ChirdObj.SetActive(false);
                }
            }
        }

        //スプリングセンサを一旦無効化する
        for (int i = 0; i < SensorBlock.Length; i++)
        {
            SensorBlock[i].SetActive(false);
        }

        //初期処理
        FPos = transform.position;
        FRot = transform.eulerAngles;
        FScale = transform.localScale;
        transform.position = new Vector3();
        transform.eulerAngles = new Vector3();
        transform.localScale = Vector3.one;

        //ブロックのコピーおよび結合処理
        var material = GetComponent<MeshRenderer>().material;
        for (int xi = 0; xi < XN; xi++)
        {
            for (int zi = 0; zi < ZN; zi++)
            {
                for (int yi = 0; yi < YN; yi++)
                {
                    Count++;
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.SetParent(transform);
                    cube.transform.position = new Vector3(xi, yi, zi);
                    cube.GetComponent<MeshRenderer>().material = material;
                    SetUV(cube);
                }
            }
        }
        CentorObj.SetActive(false);
        GetComponent<MeshFilter>().mesh.Clear();
        Combine();
        transform.position = FPos;
        transform.eulerAngles = FRot;
        transform.localScale = FScale;
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.size = new Vector3(XN, YN, ZN);
        float px = XN / 2.0f - 0.5f;
        float py = YN / 2.0f - 0.5f;
        float pz = ZN / 2.0f - 0.5f;
        boxCollider.center = new Vector3(px, py, pz);

        //ブロックの色を変更
        Mat = GetComponent<Renderer>().material;
        if (SpringLv == 2)
        {
            Mat.color = Color.blue;
        }
        else if (SpringLv == 3)
        {
            Mat.color = Color.red;
        }

        //スプリングセンサを有効化
        for (int i = 0; i < SensorBlock.Length; i++)
        {
            SensorBlock[i].SetActive(true);
        }

        //ブロックの中心座標
        CentorObj.SetActive(true);
        Vector3 CPos;
        CPos.x = XN / 2.0f - 0.5f;
        CPos.y = YN / 2.0f - 0.5f;
        CPos.z = ZN / 2.0f - 0.5f;
        CentorObj.transform.localPosition = CPos;
        // CenterPos.x = transform.position.x + transform.localScale.x * (XN / 2.0f - 0.5f);


        //スプリングセンサの位置と大きさの調整
        //平面
        Vector3 SPos0;
        SPos0.x = XN / 2.0f - 0.5f;
        SPos0.y = YN - 0.5f;
        SPos0.z = ZN / 2.0f - 0.5f;
        SensorBlock[0].transform.localPosition = SPos0;

        Vector3 SScale0;
        SScale0.x = SensorBlock[0].transform.localScale.x * XN - 0.1f / transform.localScale.x;
        SScale0.y = 0.1f / transform.localScale.y;
        SScale0.z = SensorBlock[0].transform.localScale.z * ZN - 0.1f / transform.localScale.z;
        SensorBlock[0].transform.localScale = SScale0;

        //底面
        Vector3 SPos1;
        SPos1.x = XN / 2.0f - 0.5f;
        SPos1.y = - 0.5f;
        SPos1.z = ZN / 2.0f - 0.5f;
        SensorBlock[1].transform.localPosition = SPos1;

        Vector3 SScale1;
        SScale1.x = SensorBlock[1].transform.localScale.x * XN - 0.1f / transform.localScale.x;
        SScale1.y = 0.1f / transform.localScale.y;
        SScale1.z = SensorBlock[1].transform.localScale.z * ZN - 0.1f / transform.localScale.z;
        SensorBlock[1].transform.localScale = SScale1;

        //右側面
        Vector3 SPos2;
        SPos2.x = XN - 0.5f;
        SPos2.y = YN / 2.0f - 0.5f;
        SPos2.z = ZN / 2.0f - 0.5f;
        SensorBlock[2].transform.localPosition = SPos2;

        Vector3 SScale2;
        SScale2.x = SensorBlock[2].transform.localScale.x * YN - 0.1f / transform.localScale.x;
        SScale2.y = 0.1f / transform.localScale.y;
        SScale2.z = SensorBlock[2].transform.localScale.z * ZN - 0.1f / transform.localScale.z;
        SensorBlock[2].transform.localScale = SScale2;

        //左側面
        Vector3 SPos3;
        SPos3.x = -0.5f;
        SPos3.y = YN / 2.0f - 0.5f;
        SPos3.z = ZN / 2.0f - 0.5f;
        SensorBlock[3].transform.localPosition = SPos3;

        Vector3 SScale3;
        SScale3.x = SensorBlock[3].transform.localScale.x * YN - 0.1f / transform.localScale.x;
        SScale3.y = 0.1f / transform.localScale.y;
        SScale3.z = SensorBlock[3].transform.localScale.z * ZN - 0.1f / transform.localScale.z;
        SensorBlock[3].transform.localScale = SScale3;

        //正面
        Vector3 SPos4;
        SPos4.x = XN / 2.0f - 0.5f;
        SPos4.y = YN / 2.0f - 0.5f;
        SPos4.z = -0.5f;
        SensorBlock[4].transform.localPosition = SPos4;

        Vector3 SScale4;
        SScale4.x = SensorBlock[4].transform.localScale.x * XN - 0.1f / transform.localScale.x;
        SScale4.y = 0.1f / transform.localScale.y;
        SScale4.z = SensorBlock[4].transform.localScale.z * YN - 0.1f / transform.localScale.z;
        SensorBlock[4].transform.localScale = SScale4;

        //裏面
        Vector3 SPos5;
        SPos5.x = XN / 2.0f - 0.5f;
        SPos5.y = YN / 2.0f - 0.5f;
        SPos5.z = ZN - 0.5f;
        SensorBlock[5].transform.localPosition = SPos5;

        Vector3 SScale5;
        SScale5.x = SensorBlock[5].transform.localScale.x * XN - 0.1f / transform.localScale.x;
        SScale5.y = 0.1f / transform.localScale.y;
        SScale5.z = SensorBlock[5].transform.localScale.z * YN - 0.1f / transform.localScale.z;
        SensorBlock[5].transform.localScale = SScale5;
    }

    private void OnValidate()
    {
        float px = XN / 2.0f - 0.5f;
        float py = YN / 2.0f - 0.5f;
        float pz = ZN / 2.0f - 0.5f;
        ViewBlock.transform.localPosition = new Vector3(px, py, pz);
        ViewBlock.transform.localScale = new Vector3(XN, YN, ZN);
    }

    private void SetUV(GameObject cube)
    {
        cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(0, 2);
    }

    private Vector2[] GetBlockUVs(float tileX, float tileY)
    {
        float pixelSize = 16;
        float tilePerc = 1 / pixelSize;

        float umin = tilePerc * tileX;
        float umax = tilePerc * (tileX + 1);
        float vmin = tilePerc * tileY;
        float vmax = tilePerc * (tileY + 1);

        Vector2[] blockUVs = new Vector2[24];
        
        //-X
        blockUVs[2] = new Vector2(umax, vmax);
        blockUVs[3] = new Vector2(umin, vmax);
        blockUVs[0] = new Vector2(umax, vmin);
        blockUVs[1] = new Vector2(umin, vmin);
        
        //+Y
        blockUVs[4] = new Vector2(umin, vmin);
        blockUVs[5] = new Vector2(umax, vmin);
        blockUVs[8] = new Vector2(umin, vmax);
        blockUVs[9] = new Vector2(umax, vmax);
        

        //-Z
        blockUVs[23] = new Vector2(umax, vmin);
        blockUVs[21] = new Vector2(umin, vmax);
        blockUVs[20] = new Vector2(umin, vmin);
        blockUVs[22] = new Vector2(umax, vmax);
        

        //+Z
        blockUVs[19] = new Vector2(umax, vmin);
        blockUVs[17] = new Vector2(umin, vmax);
        blockUVs[16] = new Vector2(umin, vmin);
        blockUVs[18] = new Vector2(umax, vmax);
        
        //-Y
        blockUVs[15] = new Vector2(umax, vmin);
        blockUVs[13] = new Vector2(umin, vmax);
        blockUVs[12] = new Vector2(umin, vmin);
        blockUVs[14] = new Vector2(umax, vmax);
        

        //+X
        blockUVs[6] = new Vector2(umin, vmin);
        blockUVs[7] = new Vector2(umax, vmin);
        blockUVs[10] = new Vector2(umin, vmax);
        blockUVs[11] = new Vector2(umax, vmax);
       
        return blockUVs;
    }

    private void Combine()
    {
        //メッシュ統合
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }

        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true);
        transform.gameObject.SetActive(true);
    }

    public GameObject GetViewBlock()
    {
        return ViewBlock;
    }
}
