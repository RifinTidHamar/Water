using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGeneration : MonoBehaviour
{
    public ComputeShader textureDraw;

    Material mat;
    //public GameObject treePrefab;

    int treeCount = 1;//probably eventually however many trees are in the scene at one time

    [SerializeField]
    int leafCount = 25;

    [SerializeField]
    int leafRange = 5;

    int leafDrawHandle;
    int barkDrawHandle;
    RenderTexture outputTexture;

    

    public struct tree
    {
        public Shape.polygon bark1;
        public Shape.polygon bark2;
    }

    public struct leaf
    {
        public Shape.trilygon rootTri;
        public Shape.trilygon topTri;
    }

    public struct barkPath
    {
        public Vector2 bot;
        public Vector2 top;
    }

    
    static int treeBuffSize = Shape.polygonSize * 2;
    static int leafBuffSize = Shape.triangleSize * 2;
    tree[] treeArr;
    leaf[] leafArr;
    ComputeBuffer treeBuff;
    ComputeBuffer leafBuff;

   

    barkPath getPath(Shape.polygon poly)
    {
        barkPath path = new barkPath();
        path.bot = new Vector2((poly.bl.x + poly.br.x) / 2, (poly.bl.y + poly.br.y) / 2);
        path.top = new Vector2((poly.tl.x + poly.tr.x) / 2, (poly.tl.y + poly.tr.y) / 2);

        return path;
    }

    Vector2 RotatePoint(Vector2 point, Vector2 pivot, float angleDegrees)
    {
        // Convert angle from degrees to radians
        float angleRadians = angleDegrees * Mathf.Deg2Rad;

        // Translate the point and rotation point to the origin
        Vector2 translatedPoint = point - pivot;

        // Perform the rotation
        Vector2 rotatedPoint = new Vector2(
            translatedPoint.x * Mathf.Cos(angleRadians) - translatedPoint.y * Mathf.Sin(angleRadians),
            translatedPoint.x * Mathf.Sin(angleRadians) + translatedPoint.y * Mathf.Cos(angleRadians)
        );

        // Translate the point back to its original position
        rotatedPoint += pivot;

        return rotatedPoint;
    }

    void setLeaves(ArrayList totalPath)
    {
        for(int i = 0; i < leafCount; i++)
        {
            //we need to have branches not start at the base of the bottom bark
            int numPaths = totalPath.Count;
            int selectedPathNum = (int)Random.Range(0, numPaths);
            barkPath selP = (barkPath)totalPath[selectedPathNum];

            //where on path base of leaf is
            float slope = (selP.bot.y - selP.top.y) / (selP.bot.x - selP.top.x);
            float yIntercept = selP.bot.y - slope * selP.bot.x;
            float leafBaseY = Random.Range(selP.bot.y, selP.top.y);
            float leafBaseX = (leafBaseY - yIntercept) / slope;
            Vector2 baseLoc = new Vector2(leafBaseX, leafBaseY);

            //where leaf ends
            float leafMin = 0.05f;
            float leafMax = 0.22f;
            float leafLength = Random.Range(leafMin, leafMax);

            // Calculate the x-component of the perpendicular vector
            float flip = (slope >= 0) ? 1f : - 1f;

            // Normalize the perpendicular vector
            Vector2 perpendicularVector = new Vector2(-slope * flip, 1 * flip).normalized;  // Swap components
            Vector2 leafEnd = baseLoc + (perpendicularVector * leafLength);

            float rotRange = 70;
            float rotation = Mathf.Lerp(-rotRange, rotRange, (leafLength - leafMin) / leafMax);
            rotation += 90;
            //float rotation   = Random.Range(0f, 180f);
            leafEnd = RotatePoint(leafEnd, baseLoc, rotation);

            //how thick leaf
            float leafThick = Random.Range(0.01f, 0.03f);

            float perpSlope = -1f / slope;
            perpendicularVector = new Vector2(-Mathf.Abs(slope), 1f).normalized;  // Swap components

            Vector2 leafBl = new Vector2(leafEnd.x - leafThick, leafEnd.y);
            Vector2 leafBr = new Vector2(leafEnd.x + leafThick, leafEnd.y);
            Vector2 leafTl = new Vector2(baseLoc.x - leafThick, baseLoc.y);
            Vector2 leafTr = new Vector2(baseLoc.x + leafThick, baseLoc.y);

            Shape.trilygon rootTri = new Shape.trilygon();
            rootTri.t = leafTl;
            rootTri.r = leafTr;
            rootTri.l = leafBl;

            Shape.trilygon topTri = new Shape.trilygon();
            topTri.t = leafTr;
            topTri.r = leafBr;
            topTri.l = leafBl;

            //put in leaf arr
            leafArr[i].rootTri = rootTri;
            leafArr[i].topTri = topTri;
        }
    }

    tree createTree()
    {
        ArrayList totalPath = new ArrayList();

        tree t = new tree();
        float xBaseBark1Loc = 0.5f;
        float xBaseBark1Width = Random.Range(0.05f, 0.1f);
        float yTopBark1Loc = Random.Range(0.1f, 0.45f);
        float xTopBark1Loc = 0.5f;
        float xTopBark1Width = Random.Range(0.05f, 0.08f);

        Vector2 bark1Base = new Vector2(xBaseBark1Loc, 0);
        Vector2 bark1Top = new Vector2(xTopBark1Loc, yTopBark1Loc);
        t.bark1 = Shape.createPolyGon(bark1Base,xBaseBark1Width, bark1Top, xTopBark1Width);
        //totalPath.Add(getPath(t.bark1));

        float xBaseBark2Width = xTopBark1Width;
        float rem = 1 - yTopBark1Loc;
        float yTopBark2Loc = yTopBark1Loc + Random.Range(0.2f, rem);
        float xTopBark2Loc = 0.51f;// Random.Range(0.3f, 0.6f);
        float xTopBark2Width = 0;// Random.Range(0.03f, 0.06f);

        Vector2 bark2Base = bark1Top;
        Vector2 bark2Top = new Vector2(xTopBark2Loc, yTopBark2Loc);
        t.bark2 = Shape.createPolyGon(bark2Base, xBaseBark2Width, bark2Top, xTopBark2Width);
        totalPath.Add(getPath(t.bark2));

        setLeaves(totalPath);

        return t;
    }

    public void Start()
    {
 
        initBark();
    }

    // Start is called before the first frame update
    public void initBark()
    {
        Renderer rend = GetComponent<Renderer>();
        mat = rend.material;

        //lets do a test first just to make sure we can draw the things
        treeArr = new tree[1];
        int adder = Random.Range(-leafRange, leafRange);
        leafCount += adder;
        leafArr = new leaf[leafCount];
        treeArr[0] = createTree();

        treeBuff = new ComputeBuffer(treeCount, treeBuffSize);
        treeBuff.SetData(treeArr);

        leafBuff = new ComputeBuffer(leafCount, leafBuffSize);
        leafBuff.SetData(leafArr);

        outputTexture = new RenderTexture(256, 256, 0);
        outputTexture.enableRandomWrite = true;
        outputTexture.filterMode = FilterMode.Point;
        outputTexture.Create();

        barkDrawHandle = textureDraw.FindKernel("barkDraw");
        textureDraw.SetBuffer(barkDrawHandle, "treeObj", treeBuff);
        textureDraw.SetTexture(barkDrawHandle, "treeText", outputTexture);

        leafDrawHandle = textureDraw.FindKernel("leafDraw");
        textureDraw.SetBuffer(leafDrawHandle, "leafObjs", leafBuff);
        textureDraw.SetTexture(leafDrawHandle, "treeText", outputTexture);
        textureDraw.SetInt("leafCount", leafCount);
        mat.SetTexture("_MainTex", outputTexture);

        textureDraw.Dispatch(barkDrawHandle, 8, 8, 1);
        textureDraw.Dispatch(leafDrawHandle, 8, 8, 1/*leafCount*/);
        leafCount -= adder;
    }

   
    private void OnDestroy()
    {
        if(treeBuff != null)
        {
            treeBuff.Dispose();
        }
        if(leafBuff != null)
        {
            leafBuff.Dispose();
        }
    }
}
