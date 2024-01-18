using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

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
    int SIZE_SPHPARTICLE = 11 * sizeof(float);

    public float particleRadius = 1;
    public float smoothingRadius = 1;
    public float restDensity = 15;
    public float particleMass = 0.1f;
    public float particleViscosity = 1;
    public float particleDrag = 0.025f;
    public int particleCount = 8000;
    public int rowSize = 100;

    int texRes = 64;
    RenderTexture outTex;

    public ComputeShader shader;
    public Material material;

    // Consts
    private Vector4 GRAVITY = new Vector4(0, -9.81f, 0.0f, 2000.0f);
    private const float DT = 0.0008f;
    private const float BOUND_DAMPING = -0.5f;
    const float GAS = 2000.0f;

    private float smoothingRadiusSq;

    // Data
    SPHParticle[] particlesArray;
    ComputeBuffer particlesBuffer;

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
        float gMul = 2;
        GRAVITY = new Vector4(g.z * -gMul, g.y * gMul, g.x * -gMul, 2000);
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
            pos.x = (i % rowSize) + Random.Range(-0.1f, 0.1f) - center;
            pos.y = 2 + (float)((i / rowSize) / rowSize) * 1.1f;
            pos.z = ((i / rowSize) % rowSize) + Random.Range(-0.1f, 0.1f) - center;
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