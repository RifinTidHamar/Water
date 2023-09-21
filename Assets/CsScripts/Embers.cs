using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Embers : MonoBehaviour
{
    public ComputeShader textureDraw;
    [SerializeField]
    int emberCount;

    public Material shad;
    int texHandle;
    int posHandle;
    int dtID;
    RenderTexture outputTexture;
    public struct emberPos
    {
        public Vector2Int pos;
        public Vector2Int vel;
        public float life;
        public float lifeSave;
    }

    emberPos[] emberArr;
    ComputeBuffer emberBuff;
    int buffSize = 2 * sizeof(float) + 4 * sizeof(int);
    void OnEnable()
    {
        emberArr = new emberPos[emberCount];
        for (int i = 0; i < emberCount; i++)
        {
            emberArr[i].pos = new Vector2Int(Random.Range(136 / 3, 136 - 136 / 3), Random.Range(113 / 3, 113 - 113 / 3));
            emberArr[i].vel = new Vector2Int(0, 1);
  
            emberArr[i].life = Random.Range(1f, 5);
            emberArr[i].lifeSave = emberArr[i].life;
        }
        emberBuff = new ComputeBuffer(emberCount, buffSize);
        emberBuff.SetData(emberArr);

        outputTexture = new RenderTexture(135, 112, 0);
        outputTexture.enableRandomWrite = true;
        outputTexture.filterMode = FilterMode.Point;
        outputTexture.Create();

        texHandle = textureDraw.FindKernel("textureDraw");
        posHandle = textureDraw.FindKernel("emberMov");
        textureDraw.SetBuffer(texHandle, "emberObj", emberBuff);
        textureDraw.SetBuffer(posHandle, "emberObj", emberBuff);
        textureDraw.SetTexture(texHandle, "embers", outputTexture);
        textureDraw.SetTexture(posHandle, "embers", outputTexture);
        textureDraw.SetInt("emberCount", emberCount);
        textureDraw.SetFloat(dtID, 0);
        shad.SetTexture("_MainTex", outputTexture);

        dtID = Shader.PropertyToID("dt");

        IEnumerator coroutine = loop(0.02f);
        StartCoroutine(coroutine);
    }

    IEnumerator loop(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            textureDraw.Dispatch(texHandle, 9, 8, 1);
            textureDraw.Dispatch(posHandle, emberCount, 1, 1);
            textureDraw.SetFloat(dtID, Time.time);
        }
    }

    void OnDestroy()
    {
        if (emberBuff != null)
        {
            emberBuff.Dispose();
        }
    }
}
