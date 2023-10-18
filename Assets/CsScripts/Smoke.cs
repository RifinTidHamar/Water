using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoke : MonoBehaviour
{
    public ComputeShader smoke;

    public Material mat;
    int smokeHandle;
    int pathHandle;
    RenderTexture outputTexture;
    int texRes = 256;
    struct smokeParticle
    {
        public Vector2 velocity;
        public Vector2Int position;
        public float temp;
        public int id;
    }

    smokeParticle[] smokeArr;
    ComputeBuffer smokeBuff;

    int smokeParticleCount = 512;
    int smokeParticleSize = (sizeof(float) * 2) + (sizeof(int) * 2) + (sizeof(float) * 1) + (sizeof(int) * 1);
    uint gSizeParticle = 0;
    uint gSizeText = 0;
    int dtID;
    int timeID;
    //make grid somehow

    // Start is called before the first frame update
    void Start()
    {
        init();
    }

    //make a cone that feeds into the heating part
    void init()
    {
        outputTexture = new RenderTexture(texRes, texRes, 0);
        outputTexture.enableRandomWrite = true;
        outputTexture.filterMode = FilterMode.Point;
        outputTexture.Create();

        smokeArr = new smokeParticle[smokeParticleCount];

        for (int i = 0; i < smokeParticleCount; i++)
        {
            smokeParticle smPart = new smokeParticle();
            smPart.temp = Random.Range(-1, 1);
            int spawnX = (int)Random.Range(0, texRes);
            int spawnY = (int)Random.Range(0, texRes);
            smPart.position = new Vector2Int(spawnX, spawnY);
            smPart.velocity = Vector2.zero;
            smPart.id = i;
            smokeArr[i] = smPart;
        }

        smokeBuff = new ComputeBuffer(smokeParticleCount, smokeParticleSize);
        smokeBuff.SetData(smokeArr);
        smokeHandle = smoke.FindKernel("Smoke");
        smoke.SetInt("texRes", texRes);
        smoke.SetInt("pCnt", smokeParticleCount);
        smoke.SetBuffer(smokeHandle, "smp", smokeBuff);
        smoke.SetTexture(smokeHandle, "smokeText", outputTexture);

        pathHandle = smoke.FindKernel("SmoothPath");
        smoke.SetTexture(pathHandle, "smokeText", outputTexture);

        dtID = Shader.PropertyToID("dt");
        timeID = Shader.PropertyToID("time");

        smoke.GetKernelThreadGroupSizes(smokeHandle, out gSizeParticle, out _, out _);
        gSizeParticle = (uint) smokeParticleCount / gSizeParticle;

        smoke.GetKernelThreadGroupSizes(pathHandle, out gSizeText, out _, out _);
        gSizeParticle = (uint)texRes / gSizeText;

        mat.SetTexture("_MainTex", outputTexture);
    }

    // Update is called once per frame
    void Update()
    {
        smoke.SetFloat(dtID, Time.deltaTime);
        smoke.SetFloat(timeID, Time.time);
        smoke.Dispatch(smokeHandle, (int)gSizeParticle, 1, 1);
        smoke.Dispatch(pathHandle, (int)gSizeText, (int)gSizeText, 1);
    }

    private void OnApplicationQuit()
    {
        if(smokeBuff != null)
        {
            smokeBuff.Dispose();
        }
    }
}
