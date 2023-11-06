using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class TotemSlap : MonoBehaviour
{
    [SerializeField]
    Vector4 offsetVector;

    public ComputeShader clay;

    public Camera firstCamera;
    public GameObject plane;// = new Plane(Vector3.up, transform.position);

    public Material mat;
    int clayHandle;
    int checkClearHandle;
    int checkHandle;

    RenderTexture outTex;
    RenderTexture gridTex;
    int texRes = 256;

    struct clayParticle
    {
        public Vector2 mVel;
        public Vector2Int position;
        public int safe;
        public int id; //1 is safe; 0 is not safe
    }

    //ComputeBuffer cellBuff;

    clayParticle[] clayArr;
    ComputeBuffer clayBuff;

    int[] emptiedCellsArr;
    ComputeBuffer emptiedCellsBuff;

    int[] emptyArr;
    ComputeBuffer emptyBuff;

    //static int IdPerCell = 511;//when changing this value, also change argument for cell struct as well as the argument for calcNFree in Compute shader
    //static int cellSize = sizeof(int) * (IdPerCell + 1);
    //static int cellCount = 16;

    int clayParticleCount = 256; //640 = 64* 10
    int clayParticleSize = (sizeof(float) * 2) + (sizeof(int) * 2) + (sizeof(int) * 2);
    uint gSizeParticle = 0;
    uint gSizeText = 0;

    //int dtID;
    //int timeID;
    int mouseID;
    //int rDirID;
    //make grid somehow

    // Start is called before the first frame update
    void Start()
    {
        init();
    }

    //make a cone that feeds into the heating part
    public void init()
    {
        outTex = new RenderTexture(texRes, texRes, 0);
        outTex.enableRandomWrite = true;
        outTex.filterMode = FilterMode.Point;
        outTex.Create();

        gridTex = new RenderTexture(texRes, texRes, 0);
        gridTex.enableRandomWrite = true;
        gridTex.filterMode = FilterMode.Point;
        gridTex.Create();

        clayArr = new clayParticle[clayParticleCount];

        for (int i = 0; i < clayParticleCount; i++)
        {
            clayParticle cPart = new clayParticle();

            int x = (int)UnityEngine.Random.Range(4, texRes);
            int y = (int)UnityEngine.Random.Range(4, texRes);
            Vector2Int spawnPosition = new Vector2Int(x, y);

            cPart.position = spawnPosition;
            cPart.id = i + 1;
            cPart.mVel = new Vector3(0, 0, 0);
            cPart.safe = 1;
            clayArr[i] = cPart;
        }

        int[] tmpCell = { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        //int[] tmpCell = { -1, 1, -1, -1, -1, -1, 1, -1, 1 };
        emptiedCellsArr = tmpCell;

        emptiedCellsBuff = new ComputeBuffer(emptiedCellsArr.Length, sizeof(float));
        emptiedCellsBuff.SetData(emptiedCellsArr);

        clayBuff = new ComputeBuffer(clayParticleCount, clayParticleSize);
        clayBuff.SetData(clayArr);

        emptyArr = new int[1];
        emptyArr[0] = 0;
        emptyBuff = new ComputeBuffer(1, sizeof(int));
        emptyBuff.SetData(emptyArr);


        int gridRes = ((texRes - 4) / 3)+1;
        int cellHW = (int)Mathf.Sqrt(emptiedCellsArr.Length);

        clay.SetInt("cellHW", cellHW);
        clay.SetInt("grdRes", gridRes);
        clay.SetInt("texRes", texRes);
        clay.SetInt("pCnt", clayParticleCount);

        clayHandle = clay.FindKernel("Clay");
        clay.SetBuffer(clayHandle, "cPart", clayBuff);
        clay.SetBuffer(clayHandle, "empC", emptiedCellsBuff);
        clay.SetTexture(clayHandle, "clayText", outTex);

        checkHandle = clay.FindKernel("CheckGrid");
        clay.SetBuffer(checkHandle, "cPart", clayBuff);
        clay.SetBuffer(checkHandle, "empC", emptiedCellsBuff);

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
        mat.SetTexture("_ColorTex", gridTex);
    }   

    // Update is called once per frame
    void Update()
    {
        CamToUv camToUv = new CamToUv();
        Vector2 mouseUvPos = camToUv.genMousePosOnImage(firstCamera, plane);
        Vector4 mObjV4 = new Vector4(mouseUvPos.x, mouseUvPos.y, 0, 0);

        if (Input.GetMouseButton(0))
        {
            clay.SetVector(mouseID, mObjV4);
        }
        //more time you call the clayHandle, the better, but the more processing it takes
        for(int i = 0; i < 2; i++)
        {
            clay.Dispatch(clayHandle, (int)gSizeParticle, 1, 1);
        }
        clay.Dispatch(checkHandle, (int)gSizeParticle, 1, 1);
        //clay.Dispatch(checkClearHandle, 1, 1, 1);

        clayBuff.GetData(clayArr);

        emptyBuff.GetData(emptyArr);
        if (emptyArr[0] == 1)
        {
            Debug.Log("all Empty");
        }
    }

    private void OnApplicationQuit()
    {
        if (clayBuff != null)
        {
            clayBuff.Dispose();
        }

        if(emptiedCellsBuff != null) 
        {
            emptiedCellsBuff.Dispose();
        }

        if(emptyBuff != null)
        {
            emptyBuff.Dispose();
        }
    }
}
