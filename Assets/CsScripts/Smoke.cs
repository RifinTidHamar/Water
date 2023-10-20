using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class Smoke : MonoBehaviour
{
    public ComputeShader smoke;

    public Material mat;
    int smokeHandle;
    int pathHandle;
    int tableHandle;
    RenderTexture outputTexture;
    int texRes = 512;

    //struct cell
    //{
    //    public int[] cPart;
    //    public int nextFreeSpace;
    //};

    struct smokeParticle
    {
        public Vector2 velocity;
        public Vector2 mVel;
        public Vector2Int position;
        public int id;
        public Vector2Int cInd;
    }

    //ComputeBuffer cellBuff;

    smokeParticle[] smokeArr;
    ComputeBuffer smokeBuff;

    //static int IdPerCell = 511;//when changing this value, also change argument for cell struct as well as the argument for calcNFree in Compute shader
    //static int cellSize = sizeof(int) * (IdPerCell + 1);
    //static int cellCount = 16;

    int smokeParticleCount = 512;
    int smokeParticleSize = (sizeof(float) * 2) * 2 + (sizeof(int) * 2) * 2 + (sizeof(int) * 1);
    uint gSizeParticle = 0;
    uint gSizeText = 0;
    //int dtID;
    int timeID;
    int mouseID;
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
        int quartTexRes = texRes / 8;

        for (int i = 0; i < smokeParticleCount; i++)
        {
            smokeParticle smPart = new smokeParticle();
            int spawnX = (int)Random.Range(0 + quartTexRes, texRes - quartTexRes);
            int spawnY = (int)Random.Range(0, texRes);
            smPart.position = new Vector2Int(spawnX, spawnY);
            smPart.velocity = Vector2.zero;
            smPart.id = i+1;
            smPart.mVel = new Vector2(0, 0);
            smPart.cInd = new Vector2Int(0, 0);
            smokeArr[i] = smPart;
        }

        smokeBuff = new ComputeBuffer(smokeParticleCount, smokeParticleSize);
        smokeBuff.SetData(smokeArr);

        //cellBuff = new ComputeBuffer(cellCount, cellSize);

        smoke.SetInt("texRes", texRes);
        smoke.SetInt("pCnt", smokeParticleCount);
        //smoke.SetInt("cCnt", cellCount);
        //smoke.SetInt("idPCell", IdPerCell);

        smokeHandle = smoke.FindKernel("Smoke");
        smoke.SetBuffer(smokeHandle, "smp", smokeBuff);
        //smoke.SetBuffer(smokeHandle, "tbl", cellBuff);
        smoke.SetTexture(smokeHandle, "smokeText", outputTexture);

        pathHandle = smoke.FindKernel("SmoothPath");
        smoke.SetTexture(pathHandle, "smokeText", outputTexture);

        //tableHandle = smoke.FindKernel("CreateTable");
        //smoke.SetBuffer(tableHandle, "smp", smokeBuff);
        //smoke.SetBuffer(tableHandle, "tbl", cellBuff);
        //smoke.Dispatch(tableHandle, 1, 1, 1);

        //dtID = Shader.PropertyToID("dt");
        timeID = Shader.PropertyToID("time");
        mouseID = Shader.PropertyToID("mPos");
        smoke.GetKernelThreadGroupSizes(smokeHandle, out gSizeParticle, out _, out _);
        gSizeParticle = (uint) smokeParticleCount / gSizeParticle;

        smoke.GetKernelThreadGroupSizes(pathHandle, out gSizeText, out _, out _);
        gSizeText = (uint)texRes / gSizeText;

        mat.SetTexture("_MainTex", outputTexture);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 normScreen = new Vector3((float)1 / (float)Screen.width, (float)1 / (float)Screen.width, 1);

        Vector3 mPos = Vector3.Scale(Input.mousePosition, normScreen);

        Vector3 objPos = this.transform.position;
        float objScale = this.transform.lossyScale.x/2;

        Vector3 objRPos = objPos + new Vector3(objScale, -objScale, 0);
        objRPos = Camera.main.WorldToScreenPoint(objRPos);
        objRPos = Vector3.Scale(objRPos, normScreen);

        objPos += new Vector3(-objScale, -objScale, 0);
        objPos = Camera.main.WorldToScreenPoint(objPos);
        objPos = Vector3.Scale(objPos, normScreen);

        objScale = Vector3.Distance(objPos, objRPos);
        Vector3 normScale = new Vector3((float)1 / (float)objScale, (float)1 / (float)objScale, 1);


        Vector3 mObjPos = mPos - objPos;
        mObjPos = Vector3.Scale(mObjPos, normScale);
        Debug.Log(mObjPos);
        Vector4 mObjV4 = new Vector4(mObjPos.x, mObjPos.y, 0, 0);

        //smoke.SetFloat(dtID, Time.deltaTime);
        if(Input.GetMouseButton(0))
        {
            smoke.SetVector(mouseID, mObjV4);
        }
        else
        {
            smoke.SetVector(mouseID, new Vector4(-1, -1, 0, 0));
        }
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

        //if (cellBuff != null)
        //{
        //    cellBuff.Dispose();
        //}
    }
}
