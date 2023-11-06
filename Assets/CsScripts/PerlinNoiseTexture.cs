using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseTexture : MonoBehaviour
{
    public ComputeShader comp;
    public int texRes = 256;
    public Material noise;

    Renderer rend;
    RenderTexture outTex;
    RenderTexture noiseTex;

    int noiseHandle;
    int dtID;

    // Start is called before the first frame update
    void Start()
    {
        outTex = new RenderTexture(texRes, texRes, 4);
        //outTex.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        outTex.enableRandomWrite = true;
        outTex.filterMode = FilterMode.Point;
        outTex.Create();

        noiseHandle = comp.FindKernel("CSMain");

        initShader();
    }
    void initShader()
    {
        dtID = Shader.PropertyToID("dt");
        comp.SetTexture(noiseHandle, "Result", outTex);
        comp.SetVector("stretch", new Vector2(2.5f, 2.5f));
        comp.SetInt("texRes", texRes);
        noise.SetTexture("_FlameTex", outTex);
    }


    // Update is called once per frame
    void Update()
    {
        comp.SetFloat(dtID, Time.time);
        comp.Dispatch(noiseHandle, texRes / 8, texRes / 8, 1);
    }
}
