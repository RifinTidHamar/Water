using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class crowd : MonoBehaviour
{
    //public GameObject[] corners;
    public ComputeShader comp;
    public Mesh fella;
    public Material fellaMat;
    [SerializeField]
    float minSpeed;
    [SerializeField]
    float maxSpeed;
    [SerializeField]
    int neabyDistMov;
    [SerializeField]
    int crowdCount;
    ComputeBuffer crowdBuff;
    ComputeBuffer argsBuff;
    ComputeBuffer standBuffer;
    ComputeBuffer wallBuffer;
    uint[] argsArr = new uint[5] { 0, 0, 0, 0, 0 };
    Person[] crowdArray;
    Stand[] standPos;
    Wall[] wallPos;
    int crowdHandle;
    Bounds bounds;
    GameObject[] walls;
    GameObject[] stands;
    GameObject[] spawnBounds;

    struct Person
    {
        public uint stand;
        public uint robes;
        public uint headwear;
        public Vector3 pos;
        public Vector3 direction;
        public float speed;
        public float speedSaved;
        public float height;
        public float timeAtShop;
        public float finishedTimeAtShop;
        public float timeSearching;

    }

    struct Stand
    {
        public Vector3 pos;
    }

    struct Wall
    {
        public Vector3 pos;
        public Vector2 halfScale;
        public Vector4 tblr; //top bottom left right 
    }

    int standSize = 3 * sizeof(float);
    int personSize = 12 * sizeof(float) + 3 * sizeof(uint);
    int wallSize = 9 * sizeof(float);
    int dtID;
    // Start is called before the first frame update
    void Start()
    {
        crowdHandle = comp.FindKernel("moveCrowd");

        bounds = new Bounds(Vector3.zero, Vector3.one * 1000);

        walls = GameObject.FindGameObjectsWithTag("walls");
        stands = GameObject.FindGameObjectsWithTag("stand");
        spawnBounds = GameObject.FindGameObjectsWithTag("spawn");

        init();
    }

    float axisPos(float pos, float scale)
    {
        float val = pos + scale / 2;
        return val;
    }

    void init()
    {
        dtID = Shader.PropertyToID("dt");
        crowdArray = new Person[crowdCount];
        standPos = new Stand[stands.Length];
        wallPos = new Wall[walls.Length];
        for(int i = 0; i < stands.Length; i++)
        {
            standPos[i].pos = new Vector3(stands[i].transform.position.x, 0, stands[i].transform.position.z);
        }
        for(int i = 0; i < crowdCount; i++)
        {
            uint stand = (uint)Random.Range(0, stands.Length);
            crowdArray[i].robes = (uint)Random.Range(0, 12);
            crowdArray[i].headwear = (uint)Random.Range(0, 16);
            crowdArray[i].stand = stand;
            crowdArray[i].speed = Random.Range(minSpeed, maxSpeed);
            crowdArray[i].speedSaved = crowdArray[i].speed;
            crowdArray[i].finishedTimeAtShop = Random.Range(6, 30);
            int boundNum = (int)(i % spawnBounds.Length);
            GameObject spawnBound = spawnBounds[boundNum];
            Vector3 boundPos = spawnBound.transform.position;
            Vector3 boundScale = spawnBound.transform.lossyScale;
            float b = axisPos(boundPos.x, -boundScale.x);
            float l = axisPos(boundPos.z, boundScale.z);
            float t = axisPos(boundPos.x, boundScale.x);
            float r = axisPos(boundPos.z, -boundScale.z);
            //Debug.Log(b + " " + l  + " " + t + " " + r);
            Vector3 bl = new Vector3(b, 0, l);
            Vector3 fl = new Vector3(t, 0, l);
            Vector3 br = new Vector3(b, 0, r);
            float z = Random.Range(br.z, bl.z);
            float x = Random.Range(bl.x, fl.x);
            //Debug.Log(z);
            crowdArray[i].pos = new Vector3(x, 0, z);
            crowdArray[i].direction = standPos[stand].pos - crowdArray[i].pos;
            crowdArray[i].direction.Normalize();

            crowdArray[i].height = Random.value * 0.5f + 0.5f;
            crowdArray[i].timeAtShop = 0;
        }
        for(int i = 0; i < walls.Length; i++)
        {
            wallPos[i].pos = new Vector3(walls[i].transform.position.x, 0, walls[i].transform.position.z);
            float hzScale = walls[i].transform.lossyScale.z / 2;
            float hxScale = walls[i].transform.lossyScale.x / 2;
            wallPos[i].halfScale = new Vector2(hxScale, hzScale);
            float l = wallPos[i].pos.z + hzScale;
            float r = wallPos[i].pos.z - hzScale;
            float t = wallPos[i].pos.x + hxScale;
            float b = wallPos[i].pos.x - hxScale;
            wallPos[i].tblr = new Vector4(t, b, l, r);
        }
        crowdBuff = new ComputeBuffer(crowdCount, personSize);
        crowdBuff.SetData(crowdArray);
        comp.SetBuffer(crowdHandle, "crowd", crowdBuff);
        standBuffer = new ComputeBuffer(stands.Length, standSize);
        standBuffer.SetData(standPos);
        comp.SetBuffer(crowdHandle, "stands", standBuffer);
        wallBuffer = new ComputeBuffer(walls.Length, wallSize);
        wallBuffer.SetData(wallPos);
        comp.SetBuffer(crowdHandle, "walls", wallBuffer);

        argsBuff = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsArr[0] = (uint)fella.GetIndexCount(0);
        argsArr[1] = (uint)crowdCount;
        argsBuff.SetData(argsArr);

        comp.SetInt("crowdCount", crowdCount);
        comp.SetInt("nearbyDistMov", neabyDistMov);
        comp.SetInt("standCount", stands.Length);
        comp.SetInt("wallCount", walls.Length);
        fellaMat.SetBuffer("crowd", crowdBuff);
    }

    // Update is called once per frame
    void Update()
    {
        comp.SetFloat(dtID, Time.deltaTime);
        comp.Dispatch(crowdHandle, 5, 1, 1);

        Graphics.DrawMeshInstancedIndirect(fella, 0, fellaMat, bounds, argsBuff);
    }

    private void OnDestroy()
    {
        if(crowdBuff != null)
        {
            crowdBuff.Dispose();
        }
        if(argsBuff != null)
        {
            argsBuff.Dispose();
        }
        if(standBuffer != null)
        {
            argsBuff.Dispose();
        }
    }
}
