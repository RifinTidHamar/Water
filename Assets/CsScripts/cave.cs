using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Jobs;
using UnityEngine;
using static Embers;

public class cave : MonoBehaviour
{
    //public GameObject[] pathPoints;

    public ComputeShader caveGenerate;
    public ComputeShader normMapGenerate;
    public ComputeShader bugTextureDraw;

    public GameObject circleVertPrefab;
    public Shading shadeScript;

    [SerializeField]
    int texRes = 256;

    int makeCircleHandle;
    int populateTriIndicesHandle;
    int createWallTexHandle;
    int createNormMapHandle;
    public struct PathPoint
    {
        public Vector3 pos;
        public Vector3 dir;
        public Vector3 norm;
        public Vector3 binorm;
    }

    public struct Vertex
    {
        public Vector3 pos;
        public Vector3 norm;
        public Vector2 uv;
    }

    int bugCount = 5;

    int bugTexHandle;
    int bugPosHandle;
    int dtID;

    public struct bugPos
    {
        public Vector2Int pos;
        public Vector2Int vel;
        public float life;
        public float lifeSave;
    }

    bugPos[] bugArr;
    ComputeBuffer bugBuff;
    int bugBuffSize = 2 * sizeof(float) + 4 * sizeof(int);

    PathPoint[] path;
    ComputeBuffer pathBuff;
    Vertex[] cirlces; //thought of as an 8 by 16 array
    ComputeBuffer vertexBuff;
    int[] triIndices;
    ComputeBuffer triIndiceBuff;

    int pathPointSize = (sizeof(float) * 3) * 4;
    int vertexSize = (sizeof(float) * 3) * 2 + (sizeof(float) * 2);
    int pathPointCount = 8;
    int vertexCount = 8 * 17;
    int indiceyCount = 3 * 16 * ((8 * 2) - 2);

    void initWallTexture()
    {
        createWallTexHandle = caveGenerate.FindKernel("CreateWallTexture");
        createNormMapHandle = normMapGenerate.FindKernel("genNormMap");
        RenderTexture wallTex = new RenderTexture(texRes, texRes, 4);
        wallTex.enableRandomWrite = true;
        wallTex.filterMode = FilterMode.Point;
        wallTex.Create();

        caveGenerate.SetTexture(createWallTexHandle, "wallTex", wallTex);
        caveGenerate.SetInt("texRes", texRes);
        //GetComponent<Renderer>().material.SetTexture("_MainTex", wallTex);

        RenderTexture normMap = new RenderTexture(texRes,texRes,4);
        normMap.enableRandomWrite = true;
        normMap.filterMode = FilterMode.Point;
        normMap.Create();

        normMapGenerate.SetTexture(createNormMapHandle, "nMap", normMap);
        normMapGenerate.SetTexture(createNormMapHandle, "gray", wallTex);
        normMapGenerate.SetInt("texRes", texRes);
        GetComponent<Shading>().normalMap = normMap;
    }

    void createMesh()
    {
        vertexBuff.GetData(cirlces);
        triIndiceBuff.GetData(triIndices);

        Vector3[] vertsForMesh = new Vector3[vertexCount];
        Vector2[] uvForMesh = new Vector2[vertexCount];
        //Vector3[] normForMesh = new Vector3[vertexCount];
        /*foreach (Vertex i in cirlces)
        {
            Instantiate(circleVertPrefab, i.pos, Quaternion.identity);
        }*/

        for (int i = 0; i < cirlces.Length; i++)
        {
            vertsForMesh[i] = cirlces[i].pos;
            uvForMesh[i] = cirlces[i].uv;
            //normForMesh[i] = cirlces[i].norm;
            //Vector3 v3 = Vector3.Cross(cirlces[i].norm, new Vector3(cirlces[i].norm.x, cirlces[i].norm.y, 0)).normalized;
            //tanForMesh[i] = new Vector4(v3.x, v3.y, v3.z, 1);
        }
        /*int max = 0;
        foreach (int i in triIndices)
        {
            if (i > max) max = i;
        }
        Debug.Log("max index: " + max);*/

        Mesh mesh = new Mesh
        {
            vertices = vertsForMesh,
            uv = uvForMesh,
            //normals = normForMesh,
            triangles = triIndices, 
        };
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    Vector3 getNextRandomPos(Vector3 curVec)
    {
        Vector3 distOfNextPathPoint = new Vector3(curVec.x + Random.Range(-0, 0f), curVec.y + Random.Range(-0f, 0f), curVec.z + 10);
        return distOfNextPathPoint;
        //return new Vector3(curVec.x + Random.Range(0f, 0f), curVec.y + Random.Range(0f, 0f), curVec.z + 10);
    }

    Vector3 getNextCirclePos(float i, Vector3 lastPos)
    {
        float r = 15;
        Vector3 posNextPoint = lastPos + new Vector3(Mathf.Sin(-i * 0.07f) * r, 0/*Random.Range(-4f, 4f)*/, Mathf.Cos(i* 0.07f) * r);
        return posNextPoint;
    }

    Vector3 getDir(Vector3 lastPos, Vector3 curPos)
    {
        return (lastPos - curPos).normalized;
    }

    Vector3 getNorm(Vector3 dir)
    {
        return Vector3.Cross(dir, Vector3.up).normalized;
    }

    Vector3 getBinorm(Vector3 dir, Vector3 norm)
    {
        return Vector3.Cross(dir, norm).normalized;
    }

    public Vector3[] getLastTwoPointsOnPath()
    {
        Vector3[] ret = new Vector3[2];
        ret[0] = path[pathPointCount - 1].pos;
        ret[1] = path[pathPointCount - 2].pos;
        return ret;
    }

    public PathPoint[] getPath()
    {
        return path;
    }

    void initPath(Vector3[] lastPoints, int pathI)
    {
        path = new PathPoint[pathPointCount];

        path[0].pos = lastPoints[0];
        path[0].dir = getDir(lastPoints[1], lastPoints[0]);
        path[0].norm = getNorm(path[0].dir);
        path[0].binorm = getBinorm(path[0].dir, path[0].norm);

        for (int i = 1; i < pathPointCount; i++)
        {
            path[i].pos = getNextCirclePos((float)(i + (pathI * pathPointCount)), path[i - 1].pos);
            path[i].dir = getDir(path[i - 1].pos, path[i].pos);
            path[i].norm = getNorm(path[i].dir);
            path[i].binorm = getBinorm(path[i].dir, path[i].norm);
        }

        pathBuff = new ComputeBuffer(pathPointCount, pathPointSize);
        pathBuff.SetData(path);

        caveGenerate.SetBuffer(makeCircleHandle, "path", pathBuff);
        caveGenerate.SetInt("pathI", pathI);
    }

    void initVertex()
    {
        cirlces = new Vertex[vertexCount];
        vertexBuff = new ComputeBuffer(vertexCount, vertexSize);
        caveGenerate.SetBuffer(makeCircleHandle, "circleVerts", vertexBuff);
        caveGenerate.SetBuffer(populateTriIndicesHandle, "circleVerts", vertexBuff);
    }

    void initIndice()
    {         
        triIndices = new int[indiceyCount];
        triIndiceBuff = new ComputeBuffer(indiceyCount, sizeof(int));
        caveGenerate.SetBuffer(populateTriIndicesHandle, "triIndices", triIndiceBuff);
    }

    void initBugs()
    {
        bugTextureDraw = Instantiate(bugTextureDraw);
        bugArr = new bugPos[bugCount];
        for (int i = 0; i < bugCount; i++)
        {
            bugArr[i].pos = new Vector2Int(Random.Range(0, texRes), Random.Range(0, texRes));
            bugArr[i].vel = new Vector2Int(0, 1);

            bugArr[i].life = Random.Range(1f, 5f);
            bugArr[i].lifeSave = bugArr[i].life;
        }
        bugBuff = new ComputeBuffer(bugCount, bugBuffSize);
        bugBuff.SetData(bugArr);

        RenderTexture bugTexture = new RenderTexture(texRes, texRes, 0);
        bugTexture.enableRandomWrite = true;
        bugTexture.filterMode = FilterMode.Point;
        bugTexture.Create();

        dtID = Shader.PropertyToID("dtb");

        bugTexHandle = bugTextureDraw.FindKernel("bugTextureDraw");
        bugPosHandle = bugTextureDraw.FindKernel("bugMov");
        bugTextureDraw.SetBuffer(bugTexHandle, "bugObj", bugBuff);
        bugTextureDraw.SetBuffer(bugPosHandle, "bugObj", bugBuff);
        bugTextureDraw.SetTexture(bugTexHandle, "bugs", bugTexture);
        bugTextureDraw.SetTexture(bugPosHandle, "bugs", bugTexture);
        bugTextureDraw.SetInt("bugCount", bugCount);
        bugTextureDraw.SetInt("texRes", texRes);
        bugTextureDraw.SetFloat(dtID, 0);
        GetComponent<Renderer>().material.SetTexture("_MainTex", bugTexture);
    }

    IEnumerator loop(float waitTime)
    {
        while (true)
        {
            bugTextureDraw.SetFloat(dtID, Time.time);
            bugTextureDraw.Dispatch(bugTexHandle, texRes/15, texRes/15, 1);
            bugTextureDraw.Dispatch(bugPosHandle, bugCount, 1, 1);
            yield return new WaitForSeconds(waitTime);
        }
    }

    public void makeCave(Vector3[] lastPoints, int pathI, int seed)
    {

        makeCircleHandle = caveGenerate.FindKernel("MakeCircles");
        populateTriIndicesHandle = caveGenerate.FindKernel("PopulateTriIndices");

        initPath(lastPoints, pathI);
        initVertex();
        initIndice();
        initWallTexture();
        initBugs();
        caveGenerate.SetInt("seed", seed);
        caveGenerate.Dispatch(makeCircleHandle, 1, 1, 1);
        caveGenerate.Dispatch(populateTriIndicesHandle, 1, 1, 1);
        caveGenerate.Dispatch(createWallTexHandle, texRes / 15, texRes / 15, 1);
        normMapGenerate.Dispatch(createNormMapHandle, texRes / 15, texRes / 15, 1);
        createMesh();
        IEnumerator coroutine = loop(0.04f);
        StartCoroutine(coroutine);
        shadeScript.enabled = true;
    }

private void OnDestroy()
    {
        pathBuff.Release();
        vertexBuff.Release();
        triIndiceBuff.Release();
        bugBuff.Release();
    }
}
