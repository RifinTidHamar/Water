using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shading : MonoBehaviour
{
    public ComputeShader comp;
    public int texRes = 512;
    public Material shadeMat;
    public Mesh mesh;
    public Transform floor;

    Renderer rend;
    RenderTexture outTex;

    int shadingHandel;
    int lightHandel;
    int dtID;

    struct MeshTriangle
    {
        public Vector3 p1WPos;
        public Vector2 p1Uv;
        public Vector3 p2WPos;
        public Vector2 p2Uv;
        public Vector3 p3WPos;
        public Vector2 p3Uv;
        public Vector3 normal;
    }

    struct CSLight
    {
        public Vector3 loc;
        public Vector4 color;
        public float intensity;
    }

    struct usedUV
    {
        public Vector2 uvPos;
        public Vector3 worldLoc;
        public int used;
    };

    ComputeBuffer triangleBuffer;
    ComputeBuffer lightBuffer;
    ComputeBuffer usedUVBuffer;
    MeshTriangle[] triangleArr;
    CSLight[] lightArr;
    usedUV[] usedUVsArr;
    int meshTriangleNum;
    int CSlightNum;
    int usedUVNum = 512 * 512;
    int meshTriangleSize = sizeof(float) * 12 + sizeof(float) * 6;
    int CSLightSize = sizeof(float) * 8;
    int usedUVSize = sizeof(float) * 5 + sizeof(int) * 1;
    GameObject[] lights;

    // Start is called before the first frame update
    void Start()
    {
        outTex = new RenderTexture(texRes, texRes, 4);
        outTex.enableRandomWrite = true;
        outTex.filterMode = FilterMode.Point;
        outTex.Create();
        /*rend = GetComponent<Renderer>();
        rend.enabled = true;*/

        rend = GetComponent<Renderer>();
        rend.enabled = true;
        shadingHandel = comp.FindKernel("CSMain");
        lightHandel = comp.FindKernel("DynamicLight");

        populateArray();
        initShader();
    }

    void populateArray()
    {
        lights = GameObject.FindGameObjectsWithTag("Light");
        CSlightNum = lights.Length;
        lightArr = new CSLight[CSlightNum];
        //TODO: make values changebale in editor

        for (int i = 0; i < CSlightNum; i++)
        {
            lightArr[i].loc = lights[i].gameObject.transform.position;
            lightArr[i].color = lights[i].gameObject.GetComponent<LightData>().color;
            lightArr[i].intensity = lights[i].gameObject.GetComponent<LightData>().intensity;
        }

        Vector3[] worldVerts = new Vector3[mesh.vertices.Length];
        for(int i = 0; i < mesh.vertices.Length; i++)
        {
            //worldVerts[i] = new Vector4(mesh.vertices[i].x, mesh.vertices[i].y, mesh.vertices[i].z, 1);
            worldVerts[i] = floor.localToWorldMatrix.MultiplyVector(mesh.vertices[i]);
        }
        meshTriangleNum = mesh.triangles.Length/3;
        //mesh.triangles[0];
        Debug.Log(meshTriangleNum);
        triangleArr = new MeshTriangle[meshTriangleNum];
        int[] vertIndices = mesh.triangles;
        /*for(int i = 0; i < vertIndices.Length; i++)
        {
            Debug.Log(vertIndices[i]);
        }*/
        //Debug.Log(vertIndices.Length / 3);
        //Debug.Log(mesh.uv.Length);
        for (int i = 0; i < meshTriangleNum; i++)
        {
            int vCount = i * 3;
            Vector3 v1 = worldVerts[vertIndices[vCount + 0]];
            Vector3 v2 = worldVerts[vertIndices[vCount + 1]];
            Vector3 v3 = worldVerts[vertIndices[vCount + 2]];
            triangleArr[i].p1WPos = v1;
            triangleArr[i].p2WPos = v2;
            triangleArr[i].p3WPos = v3;

            triangleArr[i].p1Uv = mesh.uv[vertIndices[vCount + 0]];
            triangleArr[i].p2Uv = mesh.uv[vertIndices[vCount + 1]];
            triangleArr[i].p3Uv = mesh.uv[vertIndices[vCount + 2]];

            triangleArr[i].normal = mesh.normals[vertIndices[vCount + 0]];
            //Debug.Log(triangleArr[i].p1Uv + " " + triangleArr[i].p2Uv + " " + triangleArr[i].p3Uv);
        }


        usedUVsArr = new usedUV[usedUVNum];
        for(int i = 0; i < usedUVNum; i++)
        {
            usedUVsArr[i].worldLoc = new Vector3(0, 0, 0);
            usedUVsArr[i].uvPos = new Vector2(0, 0);
            usedUVsArr[i].used = 0;
        }
    }

    /*Vector3 shaveOffEndPoint(Vector4 x)
    {
        Vector3 ret = new Vector3(x.x, x.y, x.z);
        return ret;
    }*/

    void initShader()
    {
        //comp.SetFloat(dtID, 0);
        triangleBuffer = new ComputeBuffer(meshTriangleNum, meshTriangleSize);
        lightBuffer = new ComputeBuffer(CSlightNum, CSLightSize);
        usedUVBuffer = new ComputeBuffer(usedUVNum, usedUVSize);
        triangleBuffer.SetData(triangleArr);
        lightBuffer.SetData(lightArr);
        usedUVBuffer.SetData(usedUVsArr);
        comp.SetTexture(shadingHandel, "Result", outTex);
        comp.SetBuffer(shadingHandel, "triangles", triangleBuffer);
        comp.SetBuffer(shadingHandel, "lights", lightBuffer);
        comp.SetBuffer(shadingHandel, "usedUVs", usedUVBuffer);

        comp.SetBuffer(lightHandel, "usedUVs", usedUVBuffer);

        comp.SetInt("numLights", CSlightNum);
        comp.SetInt("numTriangles", meshTriangleNum);

        comp.Dispatch(shadingHandel, texRes / 8, texRes / 8, 1);

        //dtID = Shader.PropertyToID("dt");
        //comp.SetVector("stretch", new Vector2(2, 2));
        //comp.SetInt("texRes", texRes);
        shadeMat.SetTexture("_ShadowTex", outTex);
    }


    // Update is called once per frame
    void Update()
    {

        for (int i = 0; i < CSlightNum; i++)
        {
            lightArr[i].loc = lights[i].gameObject.transform.position;
            lightArr[i].color = lights[i].gameObject.GetComponent<LightData>().color;
            lightArr[i].intensity = lights[i].gameObject.GetComponent<LightData>().intensity;
        }
        lightBuffer.SetData(lightArr);

        comp.SetTexture(lightHandel, "Result", outTex);
        comp.SetBuffer(lightHandel, "lights", lightBuffer);
        comp.Dispatch(lightHandel, texRes / 8, texRes / 8, 1);
    }

    private void OnApplicationQuit()
    {
        triangleBuffer.Release();
        lightBuffer.Release();
        usedUVBuffer.Release();
    }
}
