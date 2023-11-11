using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class backGroundNoise : MonoBehaviour
{
    public ComputeShader comp;
    [SerializeField]
    Vector2Int texRes;
    public Material noise;

    RenderTexture outTex;

    int noiseHandle;
    int dtID;

    // Start is called before the first frame update
    void Start()
    {
        outTex = new RenderTexture(texRes.x, texRes.y, 1);
        //outTex.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        outTex.enableRandomWrite = true;
        outTex.filterMode = FilterMode.Point;
        outTex.Create();

        noiseHandle = comp.FindKernel("NoiseOddRes");

        initShader();
    }
    void initShader()
    {
        dtID = Shader.PropertyToID("dt");
        comp.SetTexture(noiseHandle, "Result", outTex);
        comp.SetVector("stretch", new Vector2(10f, 10f));
        Vector4 texResV4 = new Vector4(texRes.x, texRes.y, 0, 0);
        comp.SetVector("texRes4", texResV4);
        noise.SetTexture("_MainTex", outTex);
    }


    // Update is called once per frame
    void Update()
    {
        comp.SetFloat(dtID, Time.time);
        comp.Dispatch(noiseHandle, texRes.x / 15, texRes.y / 27, 1);
    }
}
