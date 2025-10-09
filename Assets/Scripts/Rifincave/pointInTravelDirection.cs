using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pointInTravelDirection : MonoBehaviour
{
    public Transform target;
    Vector3 oldPosition;
    float xAngle;
    // Update is called once per frame
    private void Start()
    {
        oldPosition = target.position;
        xAngle = target.eulerAngles.x;
    }
    private void Update()
    {
        Vector3 curPosition = transform.position;
        Vector3 travelDirection = (target.position - oldPosition).normalized;
        target.rotation = Quaternion.Lerp(target.rotation, Quaternion.LookRotation(travelDirection), Time.deltaTime * 1f);
        target.eulerAngles = new Vector3(xAngle, target.eulerAngles.y, 0);
        oldPosition = target.position;
    }
}
