using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Log : MonoBehaviour
{
    public ComputeShader comp;
    public int texRes = 256;
    public Material log;

    Renderer rend;
    RenderTexture outTex;
    RenderTexture logTex;

    int logHandle;
    int dtID;

    // Start is called before the first frame update
    void Start()
    {
        outTex = new RenderTexture(texRes, texRes, 4);
        outTex.enableRandomWrite = true;
        outTex.filterMode = FilterMode.Point;
        outTex.Create();

        logTex = new RenderTexture(texRes, texRes, 4);
        logTex.enableRandomWrite = true;
        logTex.filterMode = FilterMode.Point;
        logTex.Create();

        /*rend = GetComponent<Renderer>();
        rend.enabled = true;*/
        logHandle = comp.FindKernel("logTex");
        
        initShader();
    }

    

    void initShader()
    {
        comp.SetTexture(logHandle, "Result", logTex);
        comp.SetInt("texRes", texRes);
        //comp.SetVector("stretch", new Vector2(10, 4));
        log.SetTexture("_LogTex", logTex);
        comp.SetFloat(dtID, 0);
        comp.Dispatch(logHandle, texRes / 8, texRes / 8, 1);

        dtID = Shader.PropertyToID("dt");
        comp.SetTexture(logHandle, "Result", outTex);
        comp.SetVector("stretch", new Vector2(2, 2));
        comp.SetInt("texRes", texRes);
        log.SetTexture("_MainTex", outTex);
    }


    // Update is called once per frame
    void Update()
    {
        comp.SetFloat(dtID, Time.time);
        comp.Dispatch(logHandle, texRes / 8, texRes / 8, 1);
    }
}
