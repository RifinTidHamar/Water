using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.XR;
using static Unity.Burst.Intrinsics.X86.Avx;

public class liquid : MonoBehaviour
{
    private struct SPHParticle
    {
        public Vector3 position;

        public Vector3 velocity;
        public Vector3 force;

        public float density;
        public float pressure;

        public SPHParticle(Vector3 pos)
        {
            position = pos;
            velocity = Vector3.zero;
            force = Vector3.zero;
            density = 0.0f;
            pressure = 0.0f;
        }
    }

    struct MeshTriangle
    {
        public Vector3 p1WPos;
        public Vector3 p2WPos;
        public Vector3 p3WPos;
        public Vector3 normal;
        public Vector3 tangent;
        public Vector3 binormal;
    };

    int SIZE_SPHPARTICLE = 11 * sizeof(float);
    int sizeTri = (sizeof(float) * 3) * 6;

    public float particleRadius = 1;
    public float smoothingRadius = 1;
    public float restDensity = 15;
    public float particleMass = 0.1f;
    public float particleViscosity = 1;
    public float particleDrag = 0.025f;
    public int particleCount = 8000;
    public int rowSize = 100;
    public Mesh mesh;
    public Transform meshTransform;

    int texRes = 64;
    RenderTexture outTex;

    public ComputeShader shader;
    public Material material;

    // Consts
    private Vector4 GRAVITY = new Vector4(0, 9.81f, 0.0f, 2000.0f);
    private const float DT = 0.00016f; //the lower the slower
    private const float BOUND_DAMPING = -0.5f;
    const float GAS = 2000.0f;

    private float smoothingRadiusSq;

    // Data
    SPHParticle[] particlesArray;
    ComputeBuffer particlesBuffer;

    MeshTriangle[] triangleArr;
    ComputeBuffer triangleBuffer;
    int meshTriangleNum;

    int kernelComputeDensityPressure;
    int kernelComputeForces;
    int kernelIntegrate;
    int kernelComputeColliders;
    int kernelPutIntoTexture;
    int kernelSmoothPath;
    int kernelBlur;

    int groupSize;
    int texGSize;

    int gravityID;
   
    private void Start()
    {
        initTriArr();
        InitSPH();
        InitShader();
    }

    void Update()
    {
        shader.Dispatch(kernelComputeDensityPressure, groupSize, 1, 1);
        shader.Dispatch(kernelComputeForces, groupSize, 1, 1);
        shader.Dispatch(kernelIntegrate, groupSize, 1, 1);
        shader.Dispatch(kernelComputeColliders, groupSize, 1, 1);
        shader.Dispatch(kernelPutIntoTexture, groupSize, 1, 1);
        shader.Dispatch(kernelBlur, texGSize, texGSize, texGSize);
        shader.Dispatch(kernelSmoothPath, texGSize, texGSize, texGSize);
        Vector3 g = Input.gyro.gravity;
        float gMul = -9.81f;
        GRAVITY = new Vector4(g.z * gMul*3, gMul, g.x * gMul, 2000);
        //GRAVITY = new Vector4( 0, gMul, 0, 2000);
        shader.SetVector(gravityID, GRAVITY);
    }

    void InitShader()
    {
        texGSize = texRes / 8;

        gravityID = Shader.PropertyToID("gravity");

        outTex = new RenderTexture((int)texRes, (int)texRes, 0, GraphicsFormat.R8G8B8A8_UNorm);
        outTex.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        outTex.volumeDepth = (int)texRes;
        //outTex.wrapMode = 
        outTex.enableRandomWrite = true;
        outTex.filterMode = FilterMode.Point;
        outTex.Create();

        kernelComputeForces = shader.FindKernel("ComputeForces");
        kernelIntegrate = shader.FindKernel("Integrate");
        kernelComputeColliders = shader.FindKernel("ComputeColliders");
        kernelPutIntoTexture = shader.FindKernel("putIntoTexture");
        kernelSmoothPath = shader.FindKernel("SmoothPath");
        kernelBlur = shader.FindKernel("Blur");

        float smoothingRadiusSq = smoothingRadius * smoothingRadius;

        particlesBuffer = new ComputeBuffer(particlesArray.Length, SIZE_SPHPARTICLE);
        particlesBuffer.SetData(particlesArray);
        triangleBuffer = new ComputeBuffer(meshTriangleNum, sizeTri);
        triangleBuffer.SetData(triangleArr);

        shader.SetInt("triCount", meshTriangleNum);
        shader.SetInt("particleCount", particlesArray.Length);
        shader.SetInt("texRes", texRes);
        shader.SetFloat("smoothingRadius", smoothingRadius);
        shader.SetFloat("smoothingRadiusSq", smoothingRadiusSq);
        shader.SetFloat("gas", GAS);
        shader.SetFloat("restDensity", restDensity);
        shader.SetFloat("radius", particleRadius);
        shader.SetFloat("mass", particleMass);
        shader.SetFloat("particleDrag", particleDrag);
        shader.SetFloat("particleViscosity", particleViscosity);
        shader.SetFloat("damping", BOUND_DAMPING);
        shader.SetFloat("deltaTime", DT);
        shader.SetVector("gravity", GRAVITY);

        shader.SetBuffer(kernelComputeColliders, "tris", triangleBuffer);
        shader.SetBuffer(kernelComputeDensityPressure, "particles", particlesBuffer);
        shader.SetBuffer(kernelComputeForces, "particles", particlesBuffer);
        shader.SetBuffer(kernelIntegrate, "particles", particlesBuffer);
        shader.SetBuffer(kernelComputeColliders, "particles", particlesBuffer);
        shader.SetBuffer(kernelPutIntoTexture, "particles", particlesBuffer);
        shader.SetTexture(kernelPutIntoTexture, "liqText", outTex);
        shader.SetTexture(kernelBlur, "liqText", outTex);
        shader.SetTexture(kernelSmoothPath, "liqText", outTex);

        material.SetTexture("_MainTex", outTex);
    }

    private void initTriArr()
    {
        Vector3[] worldVerts = new Vector3[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            //worldVerts[i] = new Vector4(mesh.vertices[i].x, mesh.vertices[i].y, mesh.vertices[i].z, 1);
            worldVerts[i] = meshTransform.localToWorldMatrix.MultiplyVector(mesh.vertices[i]);
        }
        meshTriangleNum = mesh.triangles.Length / 3;
        //mesh.triangles[0];
        //Debug.Log(meshTriangleNum);
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

            triangleArr[i].normal = -mesh.normals[vertIndices[vCount + 0]];

            triangleArr[i].tangent = mesh.tangents[vertIndices[vCount + 0]];

            triangleArr[i].binormal = Vector3.Cross(triangleArr[i].normal, triangleArr[i].tangent);

            //Debug.Log(triangleArr[i].p1Uv + " " + triangleArr[i].p2Uv + " " + triangleArr[i].p3Uv);
        }
    }

    private void InitSPH()
    {
        kernelComputeDensityPressure = shader.FindKernel("ComputeDensityPressure");

        uint numThreadsX;
        shader.GetKernelThreadGroupSizes(kernelComputeDensityPressure, out numThreadsX, out _, out _);
        groupSize = Mathf.CeilToInt((float)particleCount / (float)numThreadsX);
        int amount = (int)numThreadsX * groupSize;

        particlesArray = new SPHParticle[amount];
        float size = particleRadius * 1.1f;
        float center = rowSize * 0.5f;

        for (int i = 0; i < amount; i++)
        {
            Vector3 pos = new Vector3();
            pos.x = (i % rowSize) + UnityEngine.Random.Range(-0.1f, 0.1f) - center;
            pos.y = 2 + (float)((i / rowSize) / rowSize) * 1.1f;
            pos.z = ((i / rowSize) % rowSize) + UnityEngine.Random.Range(-0.1f, 0.1f) - center;
            pos *= particleRadius;

            particlesArray[i] = new SPHParticle(pos);
        }
    }

    private void OnDestroy()
    {
        particlesBuffer.Dispose();
        outTex?.Release();
    }
}