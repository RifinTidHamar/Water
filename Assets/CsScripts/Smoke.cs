using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class Smoke : MonoBehaviour
{
    [SerializeField]
    Vector4 offsetVector;

    public ComputeShader smoke;

    public Camera firstCamera;
    public Camera secondCamera;
    public GameObject plane;// = new Plane(Vector3.up, transform.position);

    public Material mat;
    int smokeHandle;
    int pathHandle;
    int tableHandle;
    int blurHandle;
    RenderTexture outTex;
    int texRes = 64;

    //struct cell
    //{
    //    public int[] cPart;
    //    public int nextFreeSpace;
    //};

    struct smokeParticle
    {
        public Vector3 velocity;
        public Vector3 mVel;
        public float upSpeed;
        public Vector3Int position;
        public int id;
        public Vector3Int cInd;
    }

    //ComputeBuffer cellBuff;

    smokeParticle[] smokeArr;
    ComputeBuffer smokeBuff;

    //static int IdPerCell = 511;//when changing this value, also change argument for cell struct as well as the argument for calcNFree in Compute shader
    //static int cellSize = sizeof(int) * (IdPerCell + 1);
    //static int cellCount = 16;

    int smokeParticleCount = 512;
    int smokeParticleSize = (sizeof(float) * 3) * 2 + (sizeof(float) * 1) + (sizeof(int) * 3) * 2 + (sizeof(int) * 1);
    uint gSizeParticle = 0;
    uint gSizeText = 0;
    //int dtID;
    int timeID;
    int mouseID;
    int rDirID;
    //make grid somehow

    // Start is called before the first frame update
    void Start()
    {
        init();
    }

    //make a cone that feeds into the heating part
    void init()
    {
        outTex = new RenderTexture((int)texRes, (int)texRes, 0, GraphicsFormat.R8G8B8A8_UNorm);
        outTex.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        outTex.volumeDepth = (int)texRes;
        //outTex.wrapMode = 
        outTex.enableRandomWrite = true;
        outTex.filterMode = FilterMode.Point;
        outTex.Create();

        smokeArr = new smokeParticle[smokeParticleCount];
        int quartTexRes = texRes / 2;

        for (int i = 0; i < smokeParticleCount; i++)
        {
            smokeParticle smPart = new smokeParticle();

            int x = (int)UnityEngine.Random.Range(0, texRes);
            int y = (int)UnityEngine.Random.Range(0, texRes);
            int z = (int)UnityEngine.Random.Range(0, texRes);

            // Sample 3D Perlin noise at the random position.
            float3 position3D = new float3(x, y, z);
            float perlinValue3D = noise.snoise(position3D) * texRes;

            // Create a new point at the sampled position.
            Vector3 spawnPosition = new Vector3(x, y + perlinValue3D, z);

            int spawnX = (int)UnityEngine.Random.Range(0 + quartTexRes, texRes - quartTexRes);
            int spawnY = 0;// (int)UnityEngine.Random.Range(0, texRes);
            int spawnZ = (int)UnityEngine.Random.Range(0 + quartTexRes, texRes - quartTexRes);
            float2 noiseInp = new float2(spawnX, spawnZ);
            int spawn = (int)(noise.snoise(noiseInp) * (float)texRes);

            smPart.position = new Vector3Int(spawnX, spawn, spawnZ);
            smPart.velocity = Vector3.zero;
            smPart.id = i+1;
            smPart.mVel = new Vector3(0, 0, 0);
            smPart.cInd = new Vector3Int(0, 0, 0);
            smPart.upSpeed = UnityEngine.Random.Range(0.2f, 2f);
            smokeArr[i] = smPart;
        }

        smokeBuff = new ComputeBuffer(smokeParticleCount, smokeParticleSize);
        smokeBuff.SetData(smokeArr);

        //cellBuff = new ComputeBuffer(cellCount, cellSize);

        smoke.SetInt("texRes", texRes);
        smoke.SetInt("pCnt", smokeParticleCount);
       
        smoke.SetVector("wSpace", new Vector4(117.156998f, -0.238000005f, -0.0399999991f,1));
        //smoke.SetInt("cCnt", cellCount);
        //smoke.SetInt("idPCell", IdPerCell);

        smokeHandle = smoke.FindKernel("Smoke");
        smoke.SetBuffer(smokeHandle, "smp", smokeBuff);
        //smoke.SetBuffer(smokeHandle, "tbl", cellBuff);
        smoke.SetTexture(smokeHandle, "smokeText", outTex);

        pathHandle = smoke.FindKernel("SmoothPath");
        smoke.SetTexture(pathHandle, "smokeText", outTex);

        blurHandle = smoke.FindKernel("Blur");
        smoke.SetTexture(blurHandle, "smokeText", outTex);

        //tableHandle = smoke.FindKernel("CreateTable");
        //smoke.SetBuffer(tableHandle, "smp", smokeBuff);
        //smoke.SetBuffer(tableHandle, "tbl", cellBuff);
        //smoke.Dispatch(tableHandle, 1, 1, 1);

        //dtID = Shader.PropertyToID("dt");
        timeID = Shader.PropertyToID("time");
        mouseID = Shader.PropertyToID("mPos");
        rDirID = Shader.PropertyToID("rDir");
        smoke.GetKernelThreadGroupSizes(smokeHandle, out gSizeParticle, out _, out _);
        gSizeParticle = (uint) smokeParticleCount / gSizeParticle;

        smoke.GetKernelThreadGroupSizes(pathHandle, out gSizeText, out _, out _);
        gSizeText = (uint)texRes / gSizeText;

        mat.SetTexture("_MainTex", outTex);
    }

    // Update is called once per frame
    void Update()
    {
        CamToUv camToUv = new CamToUv();
        Vector2 mouseUvPos = camToUv.genMousePosOnImage(firstCamera, plane);
        Vector4 mObjV4 = new Vector4(mouseUvPos.x, mouseUvPos.y, 0, 0);
        Ray ray = secondCamera.ViewportPointToRay(new Vector3(mObjV4.x, mObjV4.y, 0.2f));
        //Debug.Log(mObjV4);
        //smoke.SetFloat(dtID, Time.deltaTime);
        if (Input.GetMouseButton(0))
        {
            smoke.SetVector(mouseID, ray.origin);
            smoke.SetVector(rDirID, ray.direction);
        }
        else
        {
            smoke.SetVector(mouseID, new Vector4(-1, -1, -1, 0));
            smoke.SetVector(rDirID, new Vector4(0, -1, 0, 0));
        }
        smoke.SetFloat(timeID, Time.time);
        smoke.Dispatch(smokeHandle, (int)gSizeParticle, 1, 1);
        smoke.Dispatch(blurHandle, (int)gSizeText, (int)gSizeText, (int)gSizeText);
        smoke.Dispatch(pathHandle, (int)gSizeText, (int)gSizeText, (int)gSizeText);
    }

    private void OnDestroy()
    {
        smokeBuff?.Dispose();
        outTex?.Release();
        //if (cellBuff != null)
        //{
        //    cellBuff.Dispose();
        //}
    }
}
