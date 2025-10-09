using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tunnel : MonoBehaviour
{
    public int columns;
    public int loops;
    struct Vertex
    {
        public Vector3 position;
        public Vector2 uv;
        public Vector3 normal;
        public Vector3 tangent;
        public Vector3 binormal;
    }
    int vertSize = ((3 * sizeof(float)) * 4) + ((2 * sizeof(float)) * 1);
    int numVerts = 0;
    ComputeBuffer vertexBuffer;
    // Start is called before the first frame update
    void Start()
    {
        numVerts = columns * loops;
        if (columns * loops % 8 != 0) 
        {
            Debug.LogError("columsn * loops must be a factor of 8"); 
        }
        vertexBuffer = new ComputeBuffer(numVerts, vertSize);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
