using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class rotateCameraAround : MonoBehaviour
{

    public Transform target; // The object or point you want to look at.
    public float rotationSpeed = 200.0f;

    private float horizontalInput;

    void Update()
    {
        // Get input from the left and right arrow keys.
        horizontalInput = Input.GetAxis("Horizontal");
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Rotate the camera around the target.
            if(SystemInfo.supportsGyroscope)
                transform.RotateAround(target.position, Vector3.up, rotationSpeed * Gyroscope.yRotation * Time.deltaTime);
           else
                transform.RotateAround(target.position, Vector3.up, horizontalInput * rotationSpeed * Time.deltaTime);

            // Make the camera look at the target.
            transform.LookAt(target);
        }
    }     
}
