using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pageScript : MonoBehaviour
{
    [Range(1, 10)]
    public float weight;

    [Range(0f, 3.15f)]
    public float flex;

    [Range(-20, 20)]
    public float gravity;

    public struct Vertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 uv;
    }

    Vertex[] vertexArray;
    int vertexSize = sizeof(float) * 8;

    public struct Line
    {
        public Vector3 curParentPos;
        public Vector3 prevPos;
        public Vector3 curPos;
        public Vector3 normal;
        public Vector3 parentNorm;
        public int v1Ind;
        public int v2Ind;
        public Vector3 vel;
        public Line(int v1i, int v2i)
        {
            curParentPos = new Vector3(0, 0, 0);
            prevPos = new Vector3(0, 0, 0);
            curPos = new Vector3(0, 0, 0);
            normal = new Vector3(0, 0, 0);
            parentNorm = new Vector3(0, 0, 0);
            v1Ind = v1i;
            v2Ind = v2i;
            vel = new Vector3(0, 0, 0);
        }
    }

    Line[] lineArray;
    int lineSize = sizeof(float) * 18 + sizeof(int) * 2;

    public Mesh mesh;
    public Transform meshTransform;
    public Transform rightHandTrans;
    public Transform leftHandTrans;

    Vector4 rightHandVel;

    public ComputeShader comp;
    ComputeBuffer vertBuff;
    ComputeBuffer lineBuff;
    public Material material;

    private Vector4 GRAVITY = new Vector4(0, -9.81f, 0.0f, 2000.0f);
    private const float DT = 0.0005f;
    private const float BOUND_DAMPING = -0.5f;

    int velKernelID;
    int mouseKernelID;
    int vertKernelID;

    int leftArrowID;
    int rightArrowID;
    int upArrowID;
    int downArrowID;

    int gravityID;
    int flexID;
    int weightID;

    int dtID;

    int rightHandID;
    int leftHandID;

    ComputeBuffer triangleBuffer;
    int meshTriangleNum;

    GraphicsBuffer indices;
    // Start is called before the first frame update
    void Start()
    {
       

        //remove
        Application.targetFrameRate = 120;
        //remove

        velKernelID = comp.FindKernel("calcVel");
        mouseKernelID = comp.FindKernel("mouseMove");
        vertKernelID = comp.FindKernel("vertFollow");
        initVertsAndLines();
        init();
    }

    Vertex setVert(int i)
    {
        Vertex v = new Vertex();
        v.position = mesh.vertices[i];
        v.normal = mesh.normals[i];
        v.uv = mesh.uv[i];
        vertexArray[i] = v;
        return v;
    }

    private void initVertsAndLines()
    {
        vertexArray = new Vertex[mesh.vertices.Length];
        lineArray = new Line[mesh.vertices.Length/4];

        ////these two lines are necessary for the way vertices are indexed by unity
        //vertexArray[0] = setVert(0);
        //vertexArray[1] = setVert(1);
        for (int i = 0; i < vertexArray.Length; i++)
        {
            vertexArray[i] = setVert(i);
        }

        for (int i = lineArray.Length - 1; i >= 0; i--)
        {
            Line l = new Line(i * 2, i * 2 + 1); // may need to be changed for first two indices due to vertex array order
            Vector3 v1 = mesh.vertices[l.v1Ind];
            Vector3 v2 = mesh.vertices[l.v2Ind];
            Vector3 mp = (v1 + v2) / 2;
            l.curPos = mp;
            l.normal = mesh.normals[l.v1Ind];
            if (i == lineArray.Length - 1) //first loop iteration which has no parent
            {
                l.curParentPos = mp;
            }
            else
            {
                l.curParentPos = lineArray[lineArray.Length - 1 - i - 1].curPos;
                l.parentNorm = lineArray[lineArray.Length - 1 - i - 1].normal;
            }
            lineArray[lineArray.Length - 1 - i] = l;
        }
    }

    private void init()
    {
        vertBuff = new ComputeBuffer(vertexArray.Length, vertexSize);
        lineBuff = new ComputeBuffer(lineArray.Length, lineSize);

        vertBuff.SetData(vertexArray);
        lineBuff.SetData(lineArray);

        comp.SetBuffer(velKernelID, "verts", vertBuff);
        comp.SetBuffer(velKernelID, "pLines", lineBuff);

        comp.SetBuffer(mouseKernelID, "pLines", lineBuff);

        comp.SetBuffer(vertKernelID, "pLines", lineBuff);


        leftArrowID = Shader.PropertyToID("la");
        rightArrowID = Shader.PropertyToID("ra");
        upArrowID = Shader.PropertyToID("ua");
        downArrowID = Shader.PropertyToID("da");

        gravityID = Shader.PropertyToID("grvty");
        weightID = Shader.PropertyToID("wght");
        flexID = Shader.PropertyToID("flex");

        dtID = Shader.PropertyToID("dt");

        rightHandID = Shader.PropertyToID("rHand");
        leftHandID = Shader.PropertyToID("lHand");

        material.SetBuffer("verts", vertBuff);

        comp.SetFloat(gravityID, gravity);
        comp.SetFloat(weightID, weight);
        comp.SetFloat(flexID, flex);
    }

    // Update is called once per frame
    void Update()
    {
        comp.SetInt(leftArrowID, Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
        comp.SetInt(rightArrowID, Input.GetKey(KeyCode.RightArrow) ? 1 : 0);
        comp.SetInt(upArrowID, Input.GetKey(KeyCode.UpArrow) ? 1 : 0);
        comp.SetInt(downArrowID, Input.GetKey(KeyCode.DownArrow) ? 1 : 0);

        float wTemp = weight;
        wTemp *= 2000;
        //wTemp += 5000;

        float gTemp = gravity;
        //gTemp *= -1;

        float fTemp = flex;
        fTemp /= 20;
        fTemp = Mathf.Lerp(0.5f,1f,fTemp);
        

        comp.SetFloat(flexID, fTemp);
        comp.SetFloat(weightID, wTemp);
        comp.SetFloat(gravityID, gTemp);

        comp.SetFloat(dtID, Time.deltaTime * 0.001f);

        Vector3 tempRHand = Vector3.zero;
        Vector3 tempLHand = Vector3.zero;
        //tempRHand = meshTransform.InverseTransformPoint(rightHandTrans.position);
        tempLHand = meshTransform.InverseTransformPoint(leftHandTrans.position);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitData;
        Physics.Raycast(ray, out hitData, 1000);
        tempRHand = meshTransform.InverseTransformPoint(hitData.point);

        comp.SetVector(rightHandID, tempRHand);
        comp.SetVector(leftHandID, tempLHand);

        for (int i = 0; i < 3; i++)
        {
            comp.Dispatch(vertKernelID, 1, 1, 1);

            comp.Dispatch(mouseKernelID, 1, 1, 1);

            comp.Dispatch(velKernelID, 1, 1, 1);
        }
        //vertBuff.GetData(vertexArray);
    }
}
