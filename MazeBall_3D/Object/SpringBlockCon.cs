using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpringBlockCon : MonoBehaviour
{
    [SerializeField] int SpringLv = 1;
    private Material Mat;

    public int GetSpringLv()
    {
        return SpringLv;
    }

    private void Awake()
    {
        SetUV(gameObject);

        Vector3 MaxPos = transform.position + transform.localScale / 2;
        Vector3 MiniPos = transform.position - transform.localScale / 2;

        if (EventManager.StageMaxPos.x == 1000 || EventManager.StageMaxPos.x < MaxPos.x) EventManager.StageMaxPos.x = MaxPos.x;
        if (EventManager.StageMaxPos.y == 1000 || EventManager.StageMaxPos.y < MaxPos.y) EventManager.StageMaxPos.y = MaxPos.y;
        if (EventManager.StageMaxPos.z == 1000 || EventManager.StageMaxPos.z < MaxPos.z) EventManager.StageMaxPos.z = MaxPos.z;
        if (EventManager.StageMinPos.x == -1000 || EventManager.StageMinPos.x > MiniPos.x) EventManager.StageMinPos.x = MiniPos.x;
        if (EventManager.StageMinPos.y == -1000 || EventManager.StageMinPos.y > MiniPos.y) EventManager.StageMinPos.y = MiniPos.y;
        if (EventManager.StageMinPos.z == -1000 || EventManager.StageMinPos.z > MiniPos.z) EventManager.StageMinPos.z = MiniPos.z;

        Mat = GetComponent<Renderer>().material;
        if(SpringLv == 2)
        {
            Mat.color = Color.blue;
        }
        else if (SpringLv == 3)
        {
            Mat.color = Color.red;
        }
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
}
