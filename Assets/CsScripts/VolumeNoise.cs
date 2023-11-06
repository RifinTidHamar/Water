using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeNoise : MonoBehaviour
{
    public ComputeShader comp;
    public Material noise;

    [SerializeField]
    Vector3 texRes;

    [SerializeField]
    string shaderTextureName;

    [SerializeField]
    Vector3 compress;

    //Renderer rend;
    RenderTexture outTex;
    //RenderTexture noiseTex;

    int noiseHandle;
    int dtID;

    // Start is called before the first frame update
    void Start()
    {
        outTex = new RenderTexture((int)texRes.x, (int)texRes.y, UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat, UnityEngine.Experimental.Rendering.GraphicsFormat.None);
        outTex.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        outTex.volumeDepth = (int)texRes.z;
        //outTex.wrapMode = 
        outTex.enableRandomWrite = true;
        outTex.filterMode = FilterMode.Trilinear;
        outTex.Create();

        noiseHandle = comp.FindKernel("CSMain");

        initShader();
    }
    void initShader()
    {
        dtID = Shader.PropertyToID("dt");
        comp.SetTexture(noiseHandle, "Result", outTex);
        comp.SetVector("compress", compress);
        comp.SetVector("texRes", texRes);
        noise.SetTexture(shaderTextureName, outTex);
    }


    // Update is called once per frame
    void Update()
    {
        //this.gameObject.transform.LookAt(Camera.main.transform);
        //this.gameObject.transform.Rotate (new Vector3(180, 0, 0));
        comp.SetFloat(dtID, Time.time);
        comp.Dispatch(noiseHandle, (int)texRes.x / 8, (int)texRes.y / 8, (int)texRes.z / 8);
    }
}
