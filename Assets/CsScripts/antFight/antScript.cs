using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class antScript : MonoBehaviour
{
    public ComputeShader comp;
    public int texResX = 285;
    public int texResY = 135;

    public Material antMat;

    RenderTexture outTex;

    int clearHandle;
    int antHandle;
    int dtID;

    struct ant
    {
        public Vector3 col;
        public Vector2 pos;
        public Vector2 vel;
        public int team; //maybe more than two?
        public int health; //bewteen 0 and 10
        public int size; //0-small; 1-big
        public int alive;
    };

    int antCount = 2000;
    int antSize = ((sizeof(float) * 3) * 1) + ((sizeof(float) * 2) * 2) + (sizeof(int) * 4);
    ComputeBuffer antBuff;
    ant[] antArr; 
    // Start is called before the first frame update
    void Start()
    {
        outTex = new RenderTexture(texResX, texResY, 4);
        //outTex.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        outTex.enableRandomWrite = true;
        outTex.filterMode = FilterMode.Point;
        outTex.Create();

        initAnts();
        initShader();
    }

    void initAnts()
    {
        antArr = new ant[antCount];

        for (int i = 0; i < antCount; i++)
        {
            antArr[i] = new ant();
            antArr[i].size = UnityEngine.Random.Range(0, 2);
            antArr[i].alive = 1;
            if (i < antCount / 2)
            {
                antArr[i].team = 0;
                antArr[i].pos = new Vector2Int(UnityEngine.Random.Range(0, 35), UnityEngine.Random.Range(0,134));
                antArr[i].col = new Vector3(1, 0, 0);
                antArr[i].vel = new Vector2(UnityEngine.Random.Range(0.5f, 2), 0);//right moving
            }
            else
            {
                antArr[i].team = 1;
                antArr[i].pos = new Vector2Int(UnityEngine.Random.Range(250, 284), UnityEngine.Random.Range(0, 134));
                antArr[i].col = new Vector3(0.1f, 0.1f, 0.2f);
                antArr[i].vel = new Vector2(UnityEngine.Random.Range(0.5f, 2) * -1, 0);//left moving
            }
            if (antArr[i].size == 0)
            {
                antArr[i].health = 6;
            }
            else
            {
                antArr[i].health = 10;
            }
        }
    }
    void initShader()
    {
        antHandle = comp.FindKernel("antMovement");
        clearHandle = comp.FindKernel("clearTexture");
        antBuff = new ComputeBuffer(antCount, antSize);
        antBuff.SetData(antArr);

        dtID = Shader.PropertyToID("dt");
        comp.SetTexture(antHandle, "Result", outTex);
        comp.SetTexture(clearHandle, "Result", outTex);
        comp.SetBuffer(antHandle, "ants", antBuff);
        //comp.SetVector("stretch", new Vector2(2.5f, 2.5f));
        antMat.SetTexture("_MainTex", outTex);
    }


    // Update is called once per frame
    void Update()
    {
        comp.SetFloat(dtID, Time.time);
        comp.Dispatch(clearHandle, texResX / 15, texResY / 15, 1);
        comp.Dispatch(antHandle, antCount / 10, 1, 1);
    }
}
