using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TreePopSmoke : MonoBehaviour
{
    [SerializeField]
    int treeCount = 1000;

    [SerializeField]
    int grassCount = 1000;

    [SerializeField]
    float power = 4;

    float inBound = 3;
    float outBound = 40;

    public Transform p; //tree and grass spawn
    // Update is called once per frame

    public Color LightEffectCol;// = Color.white;
    public Color darkCol;// = Color.black;// = new Vector4(0.092889f, 0.2716448f, 0.464151f, 1);
    ArrayList treeArr;
    ArrayList grassArr;

    private void Start()
    {
        treeArr = new ArrayList();
        grassArr = new ArrayList();

        for (int i = 0; i < treeCount; i++)
        {

            // Get the bounds of the plane object.

            // Generate a random angle in radians.
            float angle = Random.Range(0, 2 * Mathf.PI);
            // Generate a random distance within the donut shape.
            float distance = Random.Range(inBound, outBound);

            // Calculate the position based on the angle and distance.
            Vector3 randomPoint = new Vector3(
                p.position.x + distance * Mathf.Cos(angle),
                p.position.y,
                p.position.z + distance * Mathf.Sin(angle)
            );

            GameObject curTree = GameObject.Instantiate(Resources.Load<GameObject>("prefab/treeOrigin"), randomPoint, Quaternion.identity) as GameObject;
            treeArr.Add(curTree);

            float dFSpawn = Vector3.Distance(new Vector2(randomPoint.x, randomPoint.z), new Vector2(p.position.x, p.position.z));
            float normDFSpawn = dFSpawn / outBound;
            float zDispCurve = Mathf.Pow(normDFSpawn, power);
            curTree.transform.localScale *= 8;
            curTree = curTree.transform.GetChild(0).gameObject;
            curTree.transform.localPosition = new Vector3(0, 0.5f, 0);
            curTree.layer = LayerMask.NameToLayer("smoke");
            Renderer rend = curTree.GetComponent<Renderer>();
            rend.material = new Material(Resources.Load<Shader>("Shaders/transparentColor"));

            Vector4 newCol = Vector4.Lerp(LightEffectCol, darkCol, zDispCurve);// 1 - Mathf.Pow(zDisplacement, 5));
            rend.material.SetColor("_Color", newCol);
            curTree.GetComponent<TreeGeneration>().initBark();
            // Calculate the direction from the current position to the target.
            // Assign the new rotation to your object.
            
            //curTree.transform.Rotate(new Vector3(0, 180, 0));
            //Debug.Log("loaded");
        }
        inBound = 2f;
        outBound = 7;
        for (int i = 0; i < grassCount; i++)
        {

            // Generate a random angle in radians.
            float angle = Random.Range(0, 2 * Mathf.PI);
            // Generate a random distance within the donut shape.
            float distance = Random.Range(inBound, outBound);

            // Calculate the position based on the angle and distance.
            Vector3 randomPoint = new Vector3(
                p.position.x + distance * Mathf.Cos(angle),
                p.position.y,
                p.position.z + distance * Mathf.Sin(angle)
            );

            GameObject curGrass = GameObject.Instantiate(Resources.Load<GameObject>("prefab/grass"), randomPoint, Quaternion.identity) as GameObject;
            grassArr.Add(curGrass);

            float dFSpawn = Vector3.Distance(new Vector2(randomPoint.x, randomPoint.z), new Vector2(p.position.x, p.position.z));
            float normDFSpawn = dFSpawn / 40;
            float zDispCurve = Mathf.Pow(normDFSpawn, power);
            //curTree.transform.localScale *= 8;
            //curTree = curTree.transform.GetChild(0).gameObject;
            curGrass.transform.localPosition += new Vector3(0, 0.25f, 0);
            curGrass.layer = LayerMask.NameToLayer("smoke");
            Renderer rend = curGrass.GetComponent<Renderer>();
            //rend.material = new Material(Resources.Load<Shader>("Shaders/transparentColor"));

            Vector4 newCol = Vector4.Lerp(LightEffectCol, darkCol, zDispCurve);// 1 - Mathf.Pow(zDisplacement, 5));
            rend.material.SetColor("_Color", newCol);
            // Calculate the direction from the current position to the target.
            // Assign the new rotation to your object.

            //curTree.transform.Rotate(new Vector3(0, 180, 0));
            //Debug.Log("loaded");
        }
    }

    private void Update()
    {
        Transform cam = Camera.main.transform;

        for (int i = 0; i < treeArr.Count; i++)
        {
            GameObject curTree = (GameObject)treeArr[i];
            curTree.transform.LookAt(new Vector3(cam.position.x, curTree.transform.position.y, cam.position.z));
            curTree.transform.Rotate(new Vector3(0, 180, 0));
        }
        for (int i = 0; i < grassArr.Count; i++)
        {
            GameObject curGrass = (GameObject)grassArr[i];
            curGrass.transform.LookAt(new Vector3(cam.position.x, curGrass.transform.position.y, cam.position.z));
            curGrass.transform.Rotate(new Vector3(0, 180, 0));
        }
    }
}
