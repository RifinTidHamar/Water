using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pointInTravelDirection : MonoBehaviour
{
    public Transform target;
    Vector3 oldPosition;
    // Update is called once per frame
    private void Start()
    {
        oldPosition = target.position;
    }
    private void Update()
    {
        Vector3 curPosition = transform.position;
        Vector3 travelDirection = (target.position - oldPosition).normalized;
        target.rotation = Quaternion.Lerp(target.rotation, Quaternion.LookRotation(travelDirection), Time.deltaTime * 1f);
        target.eulerAngles = new Vector3(12.36f, target.eulerAngles.y, 0);
        oldPosition = target.position;
    }
}
