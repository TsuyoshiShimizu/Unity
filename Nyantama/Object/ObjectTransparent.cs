using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Custom Object/TransparentObj")]
public class ObjectTransparent : MonoBehaviour
{
    private Shader DefShader;       //元のシェーダー
    private Shader TransShader;     //透過時のシェーダー
    private Color DefColor;         //元の色
    private Color TransColor;       //透過時の色
    private float Alpha = 0.2f;     //透過度
   // private float underAlpha = 0.1f;
   // private float uperAlpha = 0.7f;

    private Material Mat;           //マテリアル

    private float DTime = 0.1f;     
    private float DDelta = 0;       
    public bool TFlag { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        if (Mat == null) Mat = GetComponent<Renderer>().material;
        if (DefShader == null) DefShader = Mat.shader;
        if(TransShader == null) TransShader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
    }

    public void Transparent()
    {
        if (Mat == null) Mat = GetComponent<Renderer>().material;
        if (DefShader == null) DefShader = Mat.shader;
        if (TransShader == null) TransShader = Shader.Find("Legacy Shaders/Transparent/Diffuse");

        TFlag = true;
        ColorUpdate();
        Mat.shader = TransShader;
        Mat.color = TransColor;
    }

    public void TimerReset()
    {
        DDelta = 0;
    }

    private void FixedUpdate()
    {
        if (TFlag)
        {
            DDelta += 0.02f;
            if(DDelta >= DTime)
            {
                DDelta = 0;
                TFlag = false;
                Mat.shader = DefShader;
                Mat.color = DefColor;
            }
        }
    }

    private void ColorUpdate()
    {
        DefColor = Mat.color;
        TransColor = DefColor;
        TransColor.a = Alpha;
    }
}
