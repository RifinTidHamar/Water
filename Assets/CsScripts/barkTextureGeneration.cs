using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class barkTextureGeneration : MonoBehaviour
{
    public ComputeShader textureDraw;

    public Material mat;
    int barkDrawHandle;
    RenderTexture outTex;

    // Start is called before the first frame update
    void Start()
    {
        initBark();
    }

    void initBark()
    {
        outTex = new RenderTexture(1024, 1024, 0);
        outTex.enableRandomWrite = true;
        outTex.filterMode = FilterMode.Point;
        outTex.Create();

        barkDrawHandle = textureDraw.FindKernel("CreateTexture");
        textureDraw.SetTexture(barkDrawHandle, "treeText", outTex);

        mat.SetTexture("_MainTex", outTex);

        textureDraw.Dispatch(barkDrawHandle, 64, 64, 1);
    }

    private void OnDestroy()
    {
        outTex?.Release();
    }
    // Update is called once per frame
    //void Update()
    //{

    //}
}
