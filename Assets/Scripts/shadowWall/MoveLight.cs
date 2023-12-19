using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveLight : MonoBehaviour
{
    public GameObject light; //light parents


    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                transform.position = new Vector3(hit.point.x, hit.point.y, -2.27f);
            }
    }
}
