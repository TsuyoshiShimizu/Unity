using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberBlock : MonoBehaviour
{
    [SerializeField] private int BlockNum = 0;
    [SerializeField] private int type = 0;
    private int UVy = 0;

    public void SetNumber(int num)
    {
        SetUV(num);
    }

    public void Awake()
    {
        if (type == 0) UVy = 6; else UVy = 7;
        SetUV(BlockNum);
    }

    private void SetUV(int Number)
    {
        if(0 <= Number && Number <= 9)
        {
            GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(Number, UVy);
        }
        else
        {
            GetComponent<MeshFilter>().mesh.uv = GetBlockUVs(0, UVy);
        }
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

        //-Z
        blockUVs[10] = new Vector2(umax, vmax);
        blockUVs[7] = new Vector2(umin, vmin);
        blockUVs[6] = new Vector2(umax, vmin);
        blockUVs[11] = new Vector2(umin, vmax);

        //+Z
        blockUVs[3] = new Vector2(umax, vmax);
        blockUVs[0] = new Vector2(umin, vmin);
        blockUVs[1] = new Vector2(umax, vmin);
        blockUVs[2] = new Vector2(umin, vmax);

        //-X
        blockUVs[18] = new Vector2(umax, vmax);
        blockUVs[16] = new Vector2(umin, vmin);
        blockUVs[19] = new Vector2(umax, vmin);
        blockUVs[17] = new Vector2(umin, vmax);

        //+X
        blockUVs[22] = new Vector2(umax, vmax);
        blockUVs[20] = new Vector2(umin, vmin);
        blockUVs[23] = new Vector2(umax, vmin);
        blockUVs[21] = new Vector2(umin, vmax);

        //+Y
        blockUVs[8] = new Vector2(umax, vmax);
        blockUVs[5] = new Vector2(umin, vmin);
        blockUVs[4] = new Vector2(umax, vmin);
        blockUVs[9] = new Vector2(umin, vmax);
        
        //-Y
        blockUVs[15] = new Vector2(umax, vmin);
        blockUVs[13] = new Vector2(umin, vmax);
        blockUVs[12] = new Vector2(umin, vmin);
        blockUVs[14] = new Vector2(umax, vmax);

        return blockUVs;
    }

}
