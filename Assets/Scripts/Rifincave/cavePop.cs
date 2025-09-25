using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Splines;
public class cavePop : MonoBehaviour
{
    [SerializeField]
    int caveCount = 5;
    // Start is called before the first frame update

    ArrayList caveArr;
    public SplineContainer sCont;
    public SplineAnimate sAnim;

    void makeCaveSpline(cave.PathPoint[] cavePoints)
    {
        BezierKnot[] knots = new BezierKnot[cavePoints.Length];
        
        Vector3 knotScale = new Vector3(6, 3, 6);

        for (int i = 0; i < cavePoints.Length - 1; i++) // Im not sure why this is "-1" but changing it would require a lot of reworking
        {
            knots[i] = new BezierKnot();
            knots[i].Position = cavePoints[i].pos;
            Vector3 tangent = Vector3.Scale(cavePoints[i].dir.normalized, knotScale);
            if (i != 0)
                knots[i].TangentIn = tangent;
            if(i != cavePoints.Length - 2)           
                knots[i].TangentOut = -tangent;
            sCont.Spline.Add(knots[i]);
        }
    }
    Vector3[] lastPoints = new Vector3[2];

    void OnEnable()
    {
        caveArr = new ArrayList();
        GameObject curCave = GameObject.Instantiate(Resources.Load<GameObject>("prefab/caveOrigin"), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        caveArr.Add(curCave);

        Renderer rend = curCave.GetComponent<Renderer>();
        rend.material = new Material(Resources.Load<Shader>("Shaders/DoubleTex"));

        lastPoints[0] = new Vector3(300, 0, 0);
        lastPoints[1] = new Vector3(300, 0, -1);

        curCave.GetComponent<cave>().makeCave(lastPoints, 0);
        lastPoints = curCave.GetComponent<cave>().getLastTwoPointsOnPath();
        for (int i = 1; i < caveCount; i++)
        {
            curCave = GameObject.Instantiate(Resources.Load<GameObject>("prefab/caveOrigin"), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            caveArr.Add(curCave);

            rend = curCave.GetComponent<Renderer>();
            rend.material = new Material(Resources.Load<Shader>("Shaders/DoubleTex"));
            curCave.GetComponent<cave>().makeCave(lastPoints, i);
            lastPoints = curCave.GetComponent<cave>().getLastTwoPointsOnPath();
        }

        curCave = (GameObject)caveArr[caveI];
        cave.PathPoint[] targetPos = curCave.GetComponent<cave>().getPath();

        makeCaveSpline(targetPos);
    }
    int caveI = 0;
    int totalCavesMade = 0;
    private void Start()
    {
        sAnim.Play();
        StartCoroutine(splineUpdate());
        StartCoroutine(caveUpdate());
        totalCavesMade = caveCount;
    }

    void makeNewCave()
    {
        GameObject lastCave = (GameObject)caveArr[caveI];
        GameObject curCave = (GameObject)caveArr[caveI + 1];

        GameObject destroyCave = (GameObject)caveArr[caveI];
        Destroy(destroyCave);
        caveArr.RemoveAt(0);
        curCave = GameObject.Instantiate(Resources.Load<GameObject>("prefab/caveOrigin"), new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        caveArr.Add(curCave);

        Renderer rend = curCave.GetComponent<Renderer>();
        rend.material = new Material(Resources.Load<Shader>("Shaders/DoubleTex"));

        curCave.GetComponent<cave>().makeCave(lastPoints, totalCavesMade);
        totalCavesMade++;
        //caveI--;
        lastPoints = curCave.GetComponent<cave>().getLastTwoPointsOnPath();
    }

    private IEnumerator caveUpdate()
    {
        float dur = 0;
        float oldDur = 0;

        sAnim.Update();
        dur = sAnim.Duration;
        dur -= oldDur;
        oldDur += dur;
        yield return new WaitForSeconds(dur + (dur/2.0f));
        makeNewCave();

        while (true)
        {
            sAnim.Update();
            dur = sAnim.Duration;
            dur -= oldDur;
            oldDur += dur;

            yield return new WaitForSeconds(dur - 0.5f);
            makeNewCave();
        }
    }
    private IEnumerator splineUpdate()
    {
        float dur = 0;
        float oldDur = 0;
        while (true)
        {
            sAnim.Update();
            dur = sAnim.Duration;
            dur -= oldDur;
            oldDur += dur;
            yield return new WaitForSeconds(dur - 0.05f);

            GameObject lastCave = (GameObject)caveArr[caveI];
            cave.PathPoint[] lastPath = lastCave.GetComponent<cave>().getPath();

            GameObject curCave = (GameObject)caveArr[caveI + 1];
            cave.PathPoint[] curPoints = curCave.GetComponent<cave>().getPath();
            cave.PathPoint[] curPointsPlusOne = new cave.PathPoint[curPoints.Length + 1];

            curPointsPlusOne[0] = lastPath[lastPath.Length - 1];
            for (int i = 1; i < curPoints.Length; i++)
            {
                curPointsPlusOne[i] = curPoints[i];
            }
            makeCaveSpline(curPointsPlusOne);

            
        }
    }
}
