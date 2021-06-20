using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureBlock : MonoBehaviour
{
    [SerializeField] private Texture texture = null;
    [SerializeField] private int XN = 1;
    [SerializeField] private int YN = 1;
    [SerializeField] private int ZN = 1;
    [SerializeField] private PhysicsMat PMat = PhysicsMat.Normal;
    [HideInInspector] [SerializeField] private GameObject ViewBlock = null;
    [HideInInspector] [SerializeField] new private MeshRenderer renderer = null;
    
    private Vector3 FPos;
    private Vector3 FRot;
    private Vector3 FScale;
    private int Count = 0;
    private enum PhysicsMat { Normal, WeakFriction, ZeroFriction, BigFriction, PowerfulBigFriction, Spring }

    // Start is called before the first frame update
    void Start()
    {
        var material = renderer.material;
        material.SetTexture("_MainTex", texture);
        
        ViewBlock.SetActive(false);
        FPos = transform.position;
        FRot = transform.eulerAngles;
        FScale = transform.localScale;
        transform.position = new Vector3();
        transform.eulerAngles = new Vector3();
        transform.localScale = Vector3.one;

        if (PMat == PhysicsMat.WeakFriction)
        {
            Collider collider = GetComponent<BoxCollider>();
            collider.material.dynamicFriction = 0;
            collider.material.staticFriction = 0;

            if (texture.name == "BlockTexture2")
            {
                material.color = Color.green;
            }
        }
        if (PMat == PhysicsMat.ZeroFriction)
        {
            Collider collider = GetComponent<BoxCollider>();
            collider.material.dynamicFriction = 0;
            collider.material.staticFriction = 0;
            collider.material.frictionCombine = PhysicMaterialCombine.Minimum;

            if(texture.name == "BlockTexture2")
            {
                material.color = Color.blue;
            }
        }
        if (PMat == PhysicsMat.BigFriction)
        {
            Collider collider = GetComponent<BoxCollider>();
            collider.material.dynamicFriction = 1;
            collider.material.staticFriction = 1;
            collider.material.frictionCombine = PhysicMaterialCombine.Maximum;
            if (texture.name == "BlockTexture2")
            {
                material.color = Color.magenta;
            }
        }

        if (PMat == PhysicsMat.PowerfulBigFriction)
        {
            Collider collider = GetComponent<BoxCollider>();
            collider.material.dynamicFriction = 2;
            collider.material.staticFriction = 1;
            collider.material.frictionCombine = PhysicMaterialCombine.Maximum;
            if (texture.name == "BlockTexture2")
            {
                material.color = Color.gray;
            }
        }

        if (PMat == PhysicsMat.Spring)
        {
            Collider collider = GetComponent<BoxCollider>();
            collider.material.bounciness = 1;
            collider.material.bounceCombine = PhysicMaterialCombine.Maximum;
            gameObject.tag = "Spring";
        }

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
        float px = XN / 2.0f - 0.5f;
        float py = YN / 2.0f - 0.5f;
        float pz = ZN / 2.0f - 0.5f;
        boxCollider.center = new Vector3(px, py, pz);
    }

    private void OnValidate()
    {
        float px = XN / 2.0f - 0.5f;
        float py = YN / 2.0f - 0.5f;
        float pz = ZN / 2.0f - 0.5f;
        ViewBlock.transform.localPosition = new Vector3(px, py, pz);
        ViewBlock.transform.localScale = new Vector3(XN, YN, ZN);

       // var material = renderer.sharedMaterial;
       // material.SetTexture("_MainTex", texture);
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
 }
