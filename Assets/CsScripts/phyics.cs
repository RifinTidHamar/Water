using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class phyics : MonoBehaviour
{
    public ComputeShader comp;
    public Material mat;

    int fireHandle;
    int groupCount;
    public int particleCount = 18;
    public float velocityMult = 1;
    int dtID;

    struct fireParticle
    {
        public float s;
        public float heat;
        public float lum;
        public float mass;
        public Vector3 pos;
        public Vector3 velocity;
    }

    ComputeBuffer vertexBuffer;
    ComputeBuffer fireBuffer;
    ComputeBuffer fireBufferSaved;
    fireParticle[] fireArray;
    int particleSize = (10) * sizeof(float);
    int vertSize = (3) * sizeof(float);
    // Start is called before the first frame update
    void Start()
    {
        fireHandle = comp.FindKernel("emitParticle");
        init();
    }

    float weridRand()
    {
        return (Random.value - 0.5f) * 2f * (Random.value - 0.5f) * 2f;
    }

    void init()
    {
        dtID = Shader.PropertyToID("dt");
        fireArray = new fireParticle[particleCount];

        for (int i = 0; i < particleCount; i++)
        {
            float rand = Random.value * 0.2f + 0.8f;
            fireArray[i].s = Random.value + 0.5f;
            fireArray[i].heat = rand;
            fireArray[i].lum = rand;
            fireArray[i].pos = this.transform.position + new Vector3(weridRand(), Random.value * 0.2f, weridRand()) * 0.25f;
            fireArray[i].velocity = new Vector3((Random.value - 0.5f), rand, (Random.value - 0.5f)) * 0.25f * velocityMult;
            fireArray[i].mass = 0.25f;
        }

        vertexBuffer = new ComputeBuffer(particleCount * 6, vertSize);
        fireBuffer = new ComputeBuffer(particleCount, particleSize);
        fireBufferSaved = new ComputeBuffer(particleCount, particleSize);
        fireBuffer.SetData(fireArray);
        fireBufferSaved.SetData(fireArray);

        //fireHandle = comp.FindKernel("emitParticle");

        uint threadGroupSizeX;
        comp.GetKernelThreadGroupSizes(fireHandle, out threadGroupSizeX, out _, out _);
        groupCount = particleCount / (int)threadGroupSizeX;
        if(groupCount < 1)
        {
            groupCount = 1;
        }
        comp.SetBuffer(fireHandle, "parti", fireBuffer);
        comp.SetBuffer(fireHandle, "partiSaved", fireBufferSaved);
        comp.SetBuffer(fireHandle, "vrt", vertexBuffer);

        mat.SetBuffer("parti", fireBuffer);
        mat.SetBuffer("verti", vertexBuffer);
    }

    void OnRenderObject()
    {
        mat.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, particleCount);
    }

    // Update is called once per frame
    void Update()
    {
        comp.SetFloat(dtID, Time.deltaTime);
        comp.Dispatch(fireHandle, groupCount, 1, 1);
    }


    private void OnDestroy()
    {
        if (fireBuffer != null)
        {
            fireBuffer.Release();
        }
        if(fireBufferSaved != null)
        {
            fireBufferSaved.Release();
        }
        if(vertexBuffer != null)
        {
            vertexBuffer.Release();
        }
    }
}
