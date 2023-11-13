using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class treePop : MonoBehaviour
{
    [SerializeField]
    bool moving;

    [SerializeField]
    [Range(0, 3)]
    float darkStart;

    [SerializeField]
    [Range(0, 3)]
    float colPow;

    [SerializeField]
    int minS;

    [SerializeField]
    int maxS;

    [SerializeField]
    int treeCount = 1000;

    [SerializeField]
    float power = 2;

    public Transform leftBound;
    public Transform rightBound;
    public Transform backBound;
    // Update is called once per frame

    ArrayList treeArr;

    Renderer[] treeRendArr;


    Vector3 lb;
    Vector3 rb;
    Vector3 bb;

    public Color LightEffectCol;// = Color.white;
    public Color darkCol;// = Color.black;// = new Vector4(0.092889f, 0.2716448f, 0.464151f, 1);

    void initTreeLoc(GameObject curTree)
    {
        float zDisplacement = bb.z;
        int leftOrRight = Random.Range(0, 2);
        float xDisplacement;
        if (leftOrRight == 0)
        {
            xDisplacement = Random.Range(lb.x, ((rb.x - lb.x) * 0.46f) + lb.x);
        }
        else
        {
            xDisplacement = Random.Range(((rb.x - lb.x) * 0.54f) + lb.x, rb.x);
        }
        curTree.transform.position = new Vector3(xDisplacement, lb.y + 4f, zDisplacement);
        //GameObject curTree = GameObject.Instantiate(Resources.Load<GameObject>("prefab/treeOrigin"), , Quaternion.identity) as GameObject;
        //treeArr.Add(curTree);
        zDisplacement -= lb.z;
        zDisplacement /= bb.z;
        float zDispCurve = Mathf.Pow((1 - zDisplacement), power);
        curTree.transform.localScale = Vector3.one;
        curTree.transform.localScale *= Mathf.Lerp(minS, maxS, zDispCurve);
        curTree = curTree.transform.GetChild(0).gameObject;
        Renderer rend = curTree.GetComponent<Renderer>();
        //rend.material = new Material(Resources.Load<Shader>("Shaders/transparentColor"));

        Vector4 newCol = Vector4.Lerp(LightEffectCol, darkCol, darkStart - Mathf.Pow(zDispCurve, colPow));// 1 - Mathf.Pow(zDisplacement, 5));
        rend.material.SetColor("_Color", newCol);
        curTree.GetComponent<TreeGeneration>().initBark();
    }

    private void OnEnable()
    {
        treeArr = new ArrayList();

        lb = leftBound.position;
        rb = rightBound.position;
        bb = backBound.position;

        for (int i = 0; i < treeCount; i++)
        {
            float zDisplacement = Random.Range(lb.z, bb.z);
            int leftOrRight = Random.Range(0, 2);
            float xDisplacement;
            if (leftOrRight == 0)
            {
                xDisplacement = Random.Range(lb.x, ((rb.x - lb.x) * 0.46f) + lb.x);
            }
            else
            {
                xDisplacement = Random.Range(((rb.x - lb.x) * 0.54f) + lb.x, rb.x);
            }
            GameObject curTree = GameObject.Instantiate(Resources.Load<GameObject>("prefab/treeOrigin"), new Vector3(xDisplacement, lb.y + 4f, zDisplacement), Quaternion.identity) as GameObject;
            treeArr.Add(curTree);
            zDisplacement -= lb.z;
            zDisplacement /= bb.z;
            float zDispCurve = Mathf.Pow((1 - zDisplacement), power);
            curTree.transform.localScale *= Mathf.Lerp(minS, maxS, zDispCurve);
            curTree = curTree.transform.GetChild(0).gameObject;
            Renderer rend = curTree.GetComponent<Renderer>();
            rend.material = new Material(Resources.Load<Shader>("Shaders/transparentColor"));

            Vector4 newCol = Vector4.Lerp(LightEffectCol, darkCol, darkStart - Mathf.Pow(zDispCurve, colPow));// 1 - Mathf.Pow(zDisplacement, 5));
            rend.material.SetColor("_Color", newCol);
            curTree.GetComponent<TreeGeneration>().initBark();
            //Debug.Log("loaded");
        }
        //Time.timeScale = 4;
    }

    private void Update()
    {
        if(!moving)
        {
            return;
        }
        for (int i = 0; i < treeArr.Count; i++)
        {
            GameObject curTree = (GameObject)treeArr[i];
            float zDisplacement = curTree.transform.position.z;// + Time.deltaTime * 0.01f;
            float moveSpeed = Time.deltaTime * 0.5f;
            curTree.transform.position -= new Vector3(0, 0, moveSpeed * 0.05f);
            zDisplacement -= lb.z;
            zDisplacement /= bb.z;
            float zDispCurve = Mathf.Pow((1 - zDisplacement), power);
            float curScale = curTree.transform.localScale.x;
            curScale += Time.deltaTime * zDispCurve;
            curTree.transform.localScale = new Vector3(curScale, curScale, curScale);

            float zeroToOneBounds = (curTree.transform.position.x - lb.x)/(rb.x - lb.x);

            float horizEffector = Mathf.Lerp(-moveSpeed * 1.5f, moveSpeed * 1.5f, zeroToOneBounds);
            Vector3 horizontalMove = new Vector3(horizEffector * zDispCurve, 0, 0);
            curTree.transform.position += horizontalMove;

            if (curTree.transform.localScale.x > 38)
            {
                //treeArr.RemoveAt(i);
                //Destroy(curTree);
                initTreeLoc(curTree);
                continue;
            }

            curTree = curTree.transform.GetChild(0).gameObject;
            Renderer rend = curTree.GetComponent<Renderer>();

            Vector4 newCol = Vector4.Lerp(LightEffectCol, darkCol, darkStart - Mathf.Pow(zDispCurve, colPow));// 1 - Mathf.Pow(zDisplacement, 5));
            rend.material.SetColor("_Color", newCol);
            
        }
    }
}

