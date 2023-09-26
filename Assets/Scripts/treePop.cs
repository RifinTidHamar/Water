using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class treePop : MonoBehaviour
{
    public GameObject treePrefab;
    [SerializeField]
    int treeCount = 1000;

    [SerializeField]
    float power = 2;

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
                //GameObject curTree = GameObject.Instantiate(Resources.Load<GameObject>("Assets/Resources/prefab/tree.prefab"), new Vector3(xDisplacement, 1.28f, (zDisplacement) + 1.5f), Quaternion.identity) as GameObject;//Instantiate(treePrefab, new Vector3(xDisplacement, 1.28f, (zDisplacement) + 1.5f), Quaternion.identity);
                Renderer rend = curTree.GetComponent<Renderer>();
                rend.material = new Material(Shader.Find("Unlit/transparentColor"));
                Vector4 noCol = Vector4.one;
                Vector4 darkCol = new Vector4(0.092889f, 0.2716448f, 0.464151f, 1);

                Vector4 newCol = Vector4.Lerp(noCol, darkCol, 1 - Mathf.Pow((1 - zDisplacement), power));// 1 - Mathf.Pow(zDisplacement, 5));
                rend.material.SetColor("_Color", newCol);
                //if (zDisplacement > percentage)
                //{
                //    rend.material.SetColor("_Color", ) ;
                //}
                curTree.GetComponent<TreeGeneration>().initBark();
            }
        }
    }
}

