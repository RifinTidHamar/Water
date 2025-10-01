using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireComp : MonoBehaviour
{
    public ComputeShader comp;
    public int texRes;// = 256;
    //public Material noise;
    RenderTexture outTex;

    struct fireData
    {
        public Vector2 stretch;
    }
    Renderer[] fireRend;
    //new Vector2 s;

    int noiseHandle;
    int dtID;

    fireData[] fireArr;
    ComputeBuffer fireBuff;
    int fireDataSize = sizeof(float) * 2;
    int firePlaneCount;
    GameObject[] firePlanes;

    // Start is called before the first frame update

    private void Start()
    {
        firePlanes = GameObject.FindGameObjectsWithTag("Fire");
        firePlaneCount = firePlanes.Length;
        fireArr = new fireData[firePlaneCount];
        fireRend = new Renderer[firePlaneCount];
        noiseHandle = comp.FindKernel("CSMain");
        dtID = Shader.PropertyToID("dt");

        outTex = new RenderTexture((int)texRes, (int)texRes, UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat, UnityEngine.Experimental.Rendering.GraphicsFormat.None);
        outTex.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        outTex.volumeDepth = firePlaneCount;
        //outTex.wrapMode = 
        outTex.enableRandomWrite = true;
        outTex.filterMode = FilterMode.Point;
        outTex.Create();

        for (int i = 0; i < firePlaneCount; i++)
        {
            Transform curPlane = firePlanes[i].transform;
            Vector2 s = new Vector2(curPlane.localScale.x, curPlane.localScale.z);
            float maxValue = s.x > s.y? s.x : s.y;
            maxValue *= 1.5f;
            s = new Vector2(maxValue / s.x , maxValue/s.y);
            fireArr[i].stretch = new Vector2(s.y, s.x);

            fireRend[i] = firePlanes[i].GetComponent<Renderer>();
            //fireRend[i].material = new Material(Shader.Find("Unlit/Fire"));
            //fireRend[i].material.SetColor("_Color", new Color(0.9811321f, 0.4578615f, 0));
            //fireRend[i].material.SetFloat("_Test", 0.297f);
           
            fireBuff = new ComputeBuffer(firePlaneCount, fireDataSize);
            fireBuff.SetData(fireArr);

            comp.SetBuffer(noiseHandle, "fireStretch", fireBuff);
            comp.SetInt("texRes", texRes);
            comp.SetTexture(noiseHandle, "Result", outTex);
            comp.SetInt("planes", firePlaneCount);
            fireRend[i].material.SetTexture("_FlameTex", outTex);
            //Debug.Log(i);
            fireRend[i].material.SetInt("curCount", i);
            fireRend[i].material.SetInt("totCount", firePlaneCount - 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        comp.SetFloat(dtID, Time.time);
        comp.Dispatch(noiseHandle, texRes / 8, texRes / 8, firePlaneCount);
    }

    private void OnApplicationQuit()
    {
        fireBuff.Release();
    }
}
