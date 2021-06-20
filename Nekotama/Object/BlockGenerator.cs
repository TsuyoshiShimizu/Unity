using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BlockGenerator : MonoBehaviour
{
    [SerializeField] private int XN = 1;
    [SerializeField] private int YN = 1;
    [SerializeField] private int ZN = 1;
    [SerializeField] private BlockElement Element = BlockElement.Normal;
    [SerializeField] private int DamageValue = 10;
    [SerializeField] private float Alpha = 1;
    [SerializeField] private BlockType Type = BlockType.Grass;
    [SerializeField] private DamageBlockType DType = DamageBlockType.Fire;
    [SerializeField] private ViewBlockType VType = ViewBlockType.Attack;
    [SerializeField] private GameObject ViewBlock = null;
    [SerializeField] private PhysicsMat PMat = PhysicsMat.Normal;

    private enum BlockType { Grass, Wood , Brick , Stone ,Event, Evect2, Metal, Brick2, Brick3, Plaid , Wood2, Stone2, Float, Stone3, Stone4, Stone5, Stone6, Wood3, Pattern1, Float2, Pattern2 }
    private enum BlockElement { Normal, Damage,  View} 
    private enum DamageBlockType { Fire ,GreenFire }
    private enum ViewBlockType { Attack,Rolling,HighJump,Ball,FloatBall,Fire,Thunder,Jump,Vector1, Vector2, Vector3, Danger 
            ,Rush, Key2, Boss1, Boss2} 
    private enum PhysicsMat { Normal, WeakFriction ,ZeroFriction ,BigFriction, PowerfulBigFriction,Spring } 
    private Vector3 FPos;
    private Vector3 FRot;
    private Vector3 FScale;
    private int Count = 0;
    private bool Damage;
 
    private void Awake()
    {
        //メッシュ結合による表示バグ防止処理
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
        
        if (Element == BlockElement.Damage)
        {
            DamageObject DObj;
            if (GetComponent<DamageObject>() == null) DObj = gameObject.AddComponent<DamageObject>();
            else DObj = GetComponent<DamageObject>();
            DObj.Damage = DamageValue;
            gameObject.tag = "DamageObject";

            if(DType == DamageBlockType.GreenFire)
            {
                gameObject.layer = 14;
            }
        }

        if(Element == BlockElement.View)
        {
            GetComponent<BoxCollider>().isTrigger = true;
            if(Alpha != 1)
            {
                Material mat = GetComponent<Renderer>().material;
                mat.shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
                Color color = mat.color;
                color.a = Alpha;
                mat.color = color;
            }
            gameObject.tag = "Untagged";
        }

        if(Element == BlockElement.Normal)
        {
            if (Type == BlockType.Float)
            {
                gameObject.layer = 13;
                PMat = PhysicsMat.Normal;
            }

            if(Type == BlockType.Float2)
            {
                gameObject.layer = 13;
                PMat = PhysicsMat.ZeroFriction;
            }

            if(Type == BlockType.Pattern1)
            {
                gameObject.layer = 13;
                PMat = PhysicsMat.BigFriction;
            }
        }

        
        ViewBlock.SetActive(false);
        FPos = transform.position;
        FRot = transform.eulerAngles;
        FScale = transform.localScale;   
        transform.position = new Vector3();
        transform.eulerAngles = new Vector3();
        transform.localScale = Vector3.one;
        if(PMat == PhysicsMat.WeakFriction)
        {
            Collider collider = GetComponent<BoxCollider>();
            collider.material.dynamicFriction = 0;
            collider.material.staticFriction = 0;
        }
        if(PMat == PhysicsMat.ZeroFriction)
        {
            Collider collider = GetComponent<BoxCollider>();
            collider.material.dynamicFriction = 0;
            collider.material.staticFriction = 0;
            collider.material.frictionCombine = PhysicMaterialCombine.Minimum;
        }
        if(PMat == PhysicsMat.BigFriction)
        {
            Collider collider = GetComponent<BoxCollider>();
            collider.material.dynamicFriction = 1;
            collider.material.staticFriction = 1;
            collider.material.frictionCombine = PhysicMaterialCombine.Maximum;
        }

        if (PMat == PhysicsMat.PowerfulBigFriction)
        {
            Collider collider = GetComponent<BoxCollider>();
            collider.material.dynamicFriction = 2;
            collider.material.staticFriction = 1;
            collider.material.frictionCombine = PhysicMaterialCombine.Maximum;
        }

        if (PMat == PhysicsMat.Spring)
        {
            Collider collider = GetComponent<BoxCollider>();
            collider.material.bounciness = 1;
            collider.material.bounceCombine = PhysicMaterialCombine.Maximum;
            gameObject.tag = "Spring";
        }
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
                    if (yi == YN - 1 && 
                        (Type == BlockType.Grass || Type == BlockType.Stone) && Element == BlockElement.Normal )
                    {
                        SetTopUV(cube);
                    }
                    else
                    {
                        SetUV(cube);
                    }
                }
            }
        }
        GetComponent<MeshFilter>().mesh.Clear();
        Combine();
        transform.position = FPos;
        transform.eulerAngles = FRot;
        transform.localScale = FScale;
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.size = new Vector3(XN, YN, ZN);
        float px = (float)XN / 2.0f - 0.5f;
        float py = (float)YN / 2.0f - 0.5f;
        float pz = (float)ZN / 2.0f - 0.5f;
        boxCollider.center = new Vector3(px, py, pz);

        //透過ダメージブロックの透過処理(ビューブロックをダメージブロックに変更)
        if(Element == BlockElement.Damage && DType == DamageBlockType.GreenFire)
        {
            ViewBlock.SetActive(true);
            ViewBlock.GetComponent<Renderer>().enabled = false;
            BoxCollider BCllider = ViewBlock.AddComponent<BoxCollider>();
            BCllider.isTrigger = true;
            DamageObject damageObj = ViewBlock.AddComponent<DamageObject>();
            damageObj.Damage = DamageValue;
            ViewBlock.tag = "DamageObject";
        }

       // boxCollider.isTrigger = true;
    }

    private void OnValidate()
    {
        float px = XN / 2.0f - 0.5f;
        float py = YN / 2.0f - 0.5f;
        float pz = ZN / 2.0f - 0.5f;
        ViewBlock.transform.localPosition= new Vector3(px, py, pz);
        ViewBlock.transform.localScale = new Vector3(XN, YN, ZN);
    }

    private void SetUV(GameObject cube)
    {
        if (Element == BlockElement.Normal)
        {
            if (Type == BlockType.Grass) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(2, 15);
            if (Type == BlockType.Wood) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(6, 8);
            if (Type == BlockType.Brick) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(7, 15);
            if (Type == BlockType.Stone) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(13, 9);
            if (Type == BlockType.Event) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(9, 14);
            if (Type == BlockType.Evect2) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(7, 2);
            if (Type == BlockType.Metal) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(0, 0);
            if (Type == BlockType.Brick2) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(13, 2);
            if (Type == BlockType.Brick3) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(3, 4);
            if (Type == BlockType.Plaid) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(0, 2);
            if (Type == BlockType.Wood2) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(14, 2);
            if (Type == BlockType.Stone2) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(5, 2);
            if (Type == BlockType.Float) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(1, 2);
            if (Type == BlockType.Stone3) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(0, 1);
            if (Type == BlockType.Stone4) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(1, 1);
            if (Type == BlockType.Stone5) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(9, 1);
            if (Type == BlockType.Stone6) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(8, 1);
            if (Type == BlockType.Wood3) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(14, 3);
            if (Type == BlockType.Pattern1) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(15, 3);
            if (Type == BlockType.Float2) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(2, 3);
            if (Type == BlockType.Pattern2) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(6, 3);
        }
        if (Element == BlockElement.Damage)
        {
            if (DType == DamageBlockType.Fire) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(13, 1);
            if (DType == DamageBlockType.GreenFire) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(14, 1);
        }

        if (Element == BlockElement.View)
        {
            if (VType == ViewBlockType.Attack) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(10, 4);
            if (VType == ViewBlockType.Rolling) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(9, 4);
            if (VType == ViewBlockType.HighJump) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(8, 4);
            if (VType == ViewBlockType.Ball) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(11, 4);
            if (VType == ViewBlockType.FloatBall) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(8, 3);
            if (VType == ViewBlockType.Fire) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(9, 3);
            if (VType == ViewBlockType.Thunder) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(10, 3);
            if (VType == ViewBlockType.Jump) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(8, 5);
            if (VType == ViewBlockType.Vector1) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(9, 5);
            if (VType == ViewBlockType.Vector2) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(10, 5);
            if (VType == ViewBlockType.Vector3) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(11, 5);
            if (VType == ViewBlockType.Danger) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(12, 5);
            if (VType == ViewBlockType.Rush) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(13, 5);
            if (VType == ViewBlockType.Key2) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(10, 7);
            if (VType == ViewBlockType.Boss1) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(11, 6);
            if (VType == ViewBlockType.Boss2) cube.GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(11, 7);
        }
    }

    private void SetTopUV(GameObject cube)
    {
        if (Element == BlockElement.Normal)
        {
            if (Type == BlockType.Grass) cube.GetComponent<MeshFilter>().mesh.uv = GetTopBlockUVs(2, 15, 3, 15, 0, 15);
            if (Type == BlockType.Stone) cube.GetComponent<MeshFilter>().mesh.uv = GetTopBlockUVs(13, 9, 12, 9, 11, 9);
        }
    }

    private Vector2[] GetTopBlockUVs(float tileXUnder, float tileYUnder, float tileXSide, float tileYSide, float tileXTop, float tileYTop)
    {
        float pixelSize = 16;
        float tilePerc = 1 / pixelSize;

        float uminUnder = tilePerc * tileXUnder;
        float umaxUnder = tilePerc * (tileXUnder + 1);
        float vminUnder = tilePerc * tileYUnder;
        float vmaxUnder = tilePerc * (tileYUnder + 1);

        float uminSide = tilePerc * tileXSide;
        float umaxSide = tilePerc * (tileXSide + 1);
        float vminSide = tilePerc * tileYSide;
        float vmaxSide = tilePerc * (tileYSide + 1);

        float uminTop = tilePerc * tileXTop;
        float umaxTop = tilePerc * (tileXTop + 1);
        float vminTop = tilePerc * tileYTop;
        float vmaxTop = tilePerc * (tileYTop + 1);

        Vector2[] blockUVs = new Vector2[24];

        //-X
        blockUVs[2] = new Vector2(umaxSide, vmaxSide);
        blockUVs[3] = new Vector2(uminSide, vmaxSide);
        blockUVs[0] = new Vector2(umaxSide, vminSide);
        blockUVs[1] = new Vector2(uminSide, vminSide);

        //+Y
        blockUVs[4] = new Vector2(uminTop, vminTop);
        blockUVs[5] = new Vector2(umaxTop, vminTop);
        blockUVs[8] = new Vector2(uminTop, vmaxTop);
        blockUVs[9] = new Vector2(umaxTop, vmaxTop);

        //-Z
        blockUVs[23] = new Vector2(umaxSide, vminSide);
        blockUVs[21] = new Vector2(uminSide, vmaxSide);
        blockUVs[20] = new Vector2(uminSide, vminSide);
        blockUVs[22] = new Vector2(umaxSide, vmaxSide);

        //+Z
        blockUVs[19] = new Vector2(umaxSide, vminSide);
        blockUVs[17] = new Vector2(uminSide, vmaxSide);
        blockUVs[16] = new Vector2(uminSide, vminSide);
        blockUVs[18] = new Vector2(umaxSide, vmaxSide);

        //-Y
        blockUVs[15] = new Vector2(umaxUnder, vminUnder);
        blockUVs[13] = new Vector2(uminUnder, vmaxUnder);
        blockUVs[12] = new Vector2(uminUnder, vminUnder);
        blockUVs[14] = new Vector2(umaxUnder, vmaxUnder);

        //+X
        blockUVs[6] = new Vector2(uminSide, vminSide);
        blockUVs[7] = new Vector2(umaxSide, vminSide);
        blockUVs[10] = new Vector2(uminSide, vmaxSide);
        blockUVs[11] = new Vector2(umaxSide, vmaxSide);

        return blockUVs;
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
