using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cave : MonoBehaviour
{
    //public GameObject[] pathPoints;

    public ComputeShader caveGenerate;

    public GameObject circleVertPrefab;

    [SerializeField]
    int texRes = 256;

    int makeCircleHandle;
    int populateTriIndicesHandle;
    int createWallTexHandle;
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
        RenderTexture wallTex = new RenderTexture(texRes, texRes, 4);
        wallTex.enableRandomWrite = true;
        wallTex.filterMode = FilterMode.Point;
        wallTex.Create();
        caveGenerate.SetTexture(createWallTexHandle, "wallTex", wallTex);
        caveGenerate.SetInt("texRes", texRes);
        GetComponent<Renderer>().material.SetTexture("_MainTex", wallTex);
    }

    void createMesh()
    {
        vertexBuff.GetData(cirlces);
        triIndiceBuff.GetData(triIndices);

        Vector3[] vertsForMesh = new Vector3[vertexCount];
        Vector2[] uvForMesh = new Vector2[vertexCount];
        /*foreach (Vertex i in cirlces)
        {
            Instantiate(circleVertPrefab, i.pos, Quaternion.identity);
        }*/

        for (int i = 0; i < cirlces.Length; i++)
        {
            vertsForMesh[i] = cirlces[i].pos;
            uvForMesh[i] = cirlces[i].uv;
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
            triangles = triIndices
        };
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    Vector3 getNextRandomPos(Vector3 curVec)
    {
        return new Vector3(curVec.x + Random.Range(-4f, 4f), curVec.y + Random.Range(-7f, 7f), curVec.z + 10);
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
        //for (int i = 0; i < pathPoints.Length; i++)
        //{
        //    path[i].pos = pathPoints[i].transform.position;
        //    path[i].dir = pathPoints[i].transform.forward;
        //    path[i].norm = pathPoints[i].transform.up;
        //    path[i].binorm = pathPoints[i].transform.right;
        //}

        path[0].pos = lastPoints[0];
        path[0].dir = getDir(lastPoints[1], lastPoints[0]);
        path[0].norm = getNorm(path[0].dir);
        path[0].binorm = getBinorm(path[0].dir, path[0].norm);

        for (int i = 1; i < pathPointCount; i++)
        {
            path[i].pos = getNextRandomPos(path[i - 1].pos);
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
    // Update is called once per frame
    public void makeCave(Vector3[] lastPoints, int pathI)
    {

        makeCircleHandle = caveGenerate.FindKernel("MakeCircles");
        populateTriIndicesHandle = caveGenerate.FindKernel("PopulateTriIndices");

        initPath(lastPoints, pathI);
        initVertex();
        initIndice();
        initWallTexture();

        caveGenerate.Dispatch(makeCircleHandle, 1, 1, 1);
        caveGenerate.Dispatch(populateTriIndicesHandle, 1, 1, 1);
        caveGenerate.Dispatch(createWallTexHandle, texRes / 16, texRes / 16, 1);
        createMesh();
    }

    private void OnDestroy()
    {
      

        pathBuff.Release();
        vertexBuff.Release();
        triIndiceBuff.Release();
    }
}
