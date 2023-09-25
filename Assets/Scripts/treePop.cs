using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class treePop : MonoBehaviour
{
    public GameObject treePrefab;
    [SerializeField]
    int treeCount = 1000;

    [SerializeField]
    float percentage = 0;

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            GameObject[] allTrees = GameObject.FindGameObjectsWithTag("tree");
            foreach(GameObject tree in allTrees)
            {
                Destroy(tree);
            }
            for(int i = 0; i < treeCount; i++)
            {
                float zDisplacement = Random.Range(0f, 1f);
                float xDisplacement = Random.Range(-2.5f, 8.5f);
                GameObject curTree = Instantiate(treePrefab, new Vector3(xDisplacement, 1.28f, (zDisplacement) + 1.5f), Quaternion.identity);
                Renderer rend = curTree.GetComponent<Renderer>();
                rend.material = new Material(Shader.Find("Unlit/transparentColor"));
                if (zDisplacement > percentage)
                {
                    rend.material.SetColor("_Color", new Vector4(0.392889f, 0.5716448f, 0.764151f, 1));
                }
                curTree.GetComponent<TreeGeneration>().initBark();
            }
        }
    }
}

