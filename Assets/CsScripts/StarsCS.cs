using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarsCS : MonoBehaviour
{
    public ComputeShader textureDraw;
    [SerializeField]
    int starCount;

    public Material shad;
    int texHandle;
    int posHandle;
    RenderTexture outputTexture;
    public struct starPos
    {
        public Vector2Int pos;
        public Vector2Int vel;
        public float life;
        public float lifeSave;
    }
    starPos[] starArr;
    ComputeBuffer starBuff;
    int buffSize = 2 * sizeof(float) + 4 * sizeof(int);

    void OnEnable()
    {
        starArr = new starPos[starCount];
        for(int i = 0; i < starCount; i++)
        {
            starArr[i].pos = new Vector2Int(Random.Range(0, 136), Random.Range(0, 113));
            int starVel = Random.Range(0, 8);
            switch (starVel)
            {
                case 0:
                    starArr[i].vel = new Vector2Int(1, 1);
                    break;
                case 1:
                    starArr[i].vel = new Vector2Int(-1, 1);
                    break;
                case 2:
                    starArr[i].vel = new Vector2Int(1, -1);
                    break;
                case 3:
                    starArr[i].vel = new Vector2Int(-1, -1);
                    break;
                case 4:
                    starArr[i].vel = new Vector2Int(0, 1);
                    break;
                case 5:
                    starArr[i].vel = new Vector2Int(0, -1);
                    break;
                case 6:
                    starArr[i].vel = new Vector2Int(1, 0);
                    break;
                case 7:
                    starArr[i].vel = new Vector2Int(-1, 0);
                    break;
            }
            /*starArr[i].vel = new Vector2Int(Random.Range(-1, 1), Random.Range(-1, 1));
            if (starArr[i].vel.x == 0 && starArr[i].vel.y == 0)
                starArr[i].vel.x++;*/
            starArr[i].life = Random.Range(0.1f, 10);
            starArr[i].lifeSave = starArr[i].life;
        }
        starBuff = new ComputeBuffer(starCount, buffSize);
        starBuff.SetData(starArr);

        outputTexture = new RenderTexture(135, 112, 0);
        outputTexture.enableRandomWrite = true;
        outputTexture.filterMode = FilterMode.Point;
        outputTexture.Create(); 

        texHandle = textureDraw.FindKernel("textureDraw");
        posHandle = textureDraw.FindKernel("starMov");
        textureDraw.SetBuffer(texHandle, "starObj", starBuff);
        textureDraw.SetBuffer(posHandle, "starObj", starBuff);
        textureDraw.SetTexture(texHandle, "stars", outputTexture);
        textureDraw.SetTexture(posHandle, "stars", outputTexture);

        //textureDraw.SetTexture(texHandle, "starsDoub", outputTexture);
        textureDraw.SetInt("starCount", starCount);
        shad.SetTexture("_MainTex", outputTexture);

        IEnumerator coroutine = loop(0.005f);
        StartCoroutine(coroutine);
    }

    IEnumerator loop(float waitTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            textureDraw.Dispatch(texHandle, 9, 8, 1);
            textureDraw.Dispatch(posHandle, starCount, 1, 1);
        }
    }

    void OnDestroy()
    {
        if(starBuff != null)
        {
            starBuff.Dispose();
        }
    }
}
