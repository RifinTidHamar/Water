using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class Frame : MonoBehaviour
{

    public ComputeShader clay;

    public Camera firstCamera;
    public GameObject plane;// = new Plane(Vector3.up, transform.position);

    public Material mat;
    int clayHandle;
    int checkClearHandle;

    RenderTexture outTex;
    int texRes = 128;
    //public bool finishedFrame;
    public int shapeIndex;

    struct clayParticle
    {
        public Vector2 mVel;
        public Vector2Int pos;
        public int safe;
        public int id; //1 is safe; 0 is not safe
    }

    //ComputeBuffer cellBuff;

    clayParticle[] clayArr;
    ComputeBuffer clayBuff;

    ComputeBuffer empC;

    int[] emptyArr;
    ComputeBuffer emptyBuff;

    //static int IdPerCell = 511;//when changing this value, also change argument for cell struct as well as the argument for calcNFree in Compute shader
    //static int cellSize = sizeof(int) * (IdPerCell + 1);
    //static int cellCount = 16;

    int clayParticleCount = 256;
    int clayParticleSize = (sizeof(float) * 2) + (sizeof(int) * 2) + (sizeof(int) * 2);
    uint gSizeParticle = 0;
    uint gSizeText = 0;
    int cellRes;
    int gridCellHW;

    //int dtID;
    //int timeID;
    int mouseID;
    //int rDirID;
    //make grid somehow

    // Start is called before the first frame update
    void Start()
    {
        GameVars.shapeInd = 0;
        GameVars.isClayDone = true;//for rotation in at start of game
        for(int i = 0; i < 500; i++)
            init();
    }

    //make a cone that feeds into the heating part
    public void init()
    {
        outTex = new RenderTexture(texRes, texRes, 0);
        outTex.enableRandomWrite = true;
        outTex.filterMode = FilterMode.Point;
        outTex.Create();

        clayArr = new clayParticle[clayParticleCount];

        for (int i = 0; i < clayParticleCount; i++)
        {
            clayParticle cPart = new clayParticle();

            int x = (int)UnityEngine.Random.Range(3, texRes - 3);
            int y = (int)UnityEngine.Random.Range(3, texRes - 3);
            Vector2Int spawnPosition = new Vector2Int(x, y);

            cPart.pos = spawnPosition;
            cPart.id = i + 1;
            cPart.mVel = new Vector3(0, 0, 0);
            cPart.safe = 1;
            clayArr[i] = cPart;
        }

        //int[] tmpCell = { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        //int[] tmpCell = { -1, 1, -1, -1, -1, -1, 1, -1, 1 };
        //int[] tmpCell = { -1, -1, -1, -1, -1, -1, -1, -1, 1 };
        int[,] tmpCell =   {{  -1, -1, -1, 1,/**/ -1, 1, -1, -1,/**/ -1, -1, 1, -1, /**/  1, -1, -1, -1},
                            { -1, -1, -1, -1,/**/ 1, -1, -1, 1,/**/ -1, -1, -1, -1, /**/ 1, -1, -1, 1,},
                            { -1, -1, -1, -1,/**/ -1, 1, 1, -1,/**/ -1, -1, -1, -1, /**/ 1, -1, -1, 1, },
                            { -1, 1, 1, -1,/**/ -1, -1, -1, -1,/**/ 1, -1, -1, 1, /**/ 1, -1, -1, 1,},
                            { -1, -1, -1, 1,/**/ -1, 1, -1, 1,/**/ -1, -1, -1, -1, /**/ 1, 1, -1, 1 } };

        // Assuming you want to copy the first row (tmpCell[0]) to emptiedCellsArr
        int[] emptiedCellsArr = new int[tmpCell.GetLength(1)];

        if(GameVars.shapeInd >= tmpCell.GetLength(0))
        {
            LoadBackToTrail.Load();
        }

        for (int i = 0; i < tmpCell.GetLength(1); i++)
        {
            emptiedCellsArr[i] = tmpCell[GameVars.shapeInd, i];
        }

        empC = new ComputeBuffer(emptiedCellsArr.Length, sizeof(int));
        empC.SetData(emptiedCellsArr);

        clayBuff = new ComputeBuffer(clayParticleCount, clayParticleSize);
        clayBuff.SetData(clayArr);

        emptyArr = new int[1];
        emptyArr[0] = 0;
        emptyBuff = new ComputeBuffer(1, sizeof(int));
        emptyBuff.SetData(emptyArr);


        gridCellHW = (int)Mathf.Sqrt(emptiedCellsArr.Length);
        cellRes = ((texRes /*- 4*/) / gridCellHW) + 1;

        clay.SetInt("gCellHW", gridCellHW);
        clay.SetInt("cellRes", cellRes);
        clay.SetInt("texRes", texRes);
        clay.SetInt("pCnt", clayParticleCount);

        clayHandle = clay.FindKernel("Clay");
        clay.SetBuffer(clayHandle, "cPart", clayBuff);
        clay.SetBuffer(clayHandle, "empC", empC);
        clay.SetTexture(clayHandle, "clayText", outTex);

        checkClearHandle = clay.FindKernel("CheckClearGrid");
        //clay.SetTexture(checkHandle, "clayText", outTex);
        clay.SetBuffer(checkClearHandle, "cPart", clayBuff);
        clay.SetBuffer(checkClearHandle, "emptied", emptyBuff);


        //dtID = Shader.PropertyToID("dt");
        mouseID = Shader.PropertyToID("mPos");

        clay.GetKernelThreadGroupSizes(clayHandle, out gSizeParticle, out _, out _);
        gSizeParticle = (uint)clayParticleCount / gSizeParticle;

        //clay.GetKernelThreadGroupSizes(colorHandle, out gSizeText, out _, out _);
        //gSizeText = (uint)texRes / gSizeText;

        mat.SetTexture("_MainTex", outTex);
    }

    // Update is called once per frame
    void Update()
    {
        CamToUv camToUv = new CamToUv();
        Vector2 mouseUvPos = camToUv.genMousePosOnImage(firstCamera, plane);
        Vector4 mObjV4 = new Vector4(mouseUvPos.x, mouseUvPos.y, 0, 0);

        if (Input.GetMouseButton(0) && !GameVars.isInDodge)
        {
            clay.SetVector(mouseID, mObjV4);
        }
        else
        {
            clay.SetVector(mouseID, new Vector4(-1,-1,0,0));
        }
        //more time you call the clayHandle, the better, but the more processing it takes
        for (int i = 0; i < 4; i++)
        {
            clay.Dispatch(clayHandle, (int)gSizeParticle, 1, 1);
        }
        clay.Dispatch(checkClearHandle, 1, 1, 1);


        //emptyBuff.GetData(emptyArr);
        //if (emptyArr[0] == 1)
        //{
        //    GameVars.isClayDone = true;
        //}

        AsyncGPUReadback.Request(emptyBuff, (request) =>
        {
            if (request.hasError)
            {
                Debug.Log("GPU readback error detected.");
            }
            else
            {
                emptyArr = request.GetData<int>().ToArray();
                if (emptyArr[0] == 1)
                {
                    GameVars.isClayDone = true;
                }
            }
        });
    }

    private void OnDisable()
    {
        clayBuff?.Dispose();

        empC?.Dispose();

        emptyBuff?.Dispose();

        outTex.Release();
    }
}
