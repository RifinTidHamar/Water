using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class barkTextureGeneration : MonoBehaviour
{
    public ComputeShader textureDraw;

    public Material mat;
    int barkDrawHandle;
    RenderTexture outputTexture;

    // Start is called before the first frame update
    void Start()
    {
        initBark();
    }

    void initBark()
    {
        outputTexture = new RenderTexture(1024, 1024, 0);
        outputTexture.enableRandomWrite = true;
        outputTexture.filterMode = FilterMode.Point;
        outputTexture.Create();

        barkDrawHandle = textureDraw.FindKernel("CreateTexture");
        textureDraw.SetTexture(barkDrawHandle, "treeText", outputTexture);

        mat.SetTexture("_MainTex", outputTexture);

        textureDraw.Dispatch(barkDrawHandle, 32, 32, 1);
    }

    // Update is called once per frame
    //void Update()
    //{
        
    //}
}
