using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class follwParentPage : MonoBehaviour
{
    public MeshFilter parMesh;
    public MeshFilter curMesh;

    // Update is called once per frame
    void Update()
    {
        curMesh.mesh.vertices = parMesh.mesh.vertices;
        for (int i = 0; i < parMesh.mesh.vertices.Length; i++) 
        {
            curMesh.mesh.vertices[i] += new Vector3(0.01f, 0, 0);
        }
    }
}
