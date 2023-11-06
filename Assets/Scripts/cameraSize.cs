using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraSize : MonoBehaviour
{
    public GameObject square; // Reference to your square GameObject
    public 

    void Start()
    {
        FitSquareToCamera();
    }

    void FitSquareToCamera()
    {
        Camera mainCamera = this.GetComponent<Camera>();
        float targetAspect = mainCamera.aspect;

        float squareWidth = square.GetComponent<Renderer>().bounds.size.x;
        float squareHeight = square.GetComponent<Renderer>().bounds.size.y;

        // Calculate the desired orthographic size based on the square's size and aspect ratio
        float orthographicSize = 0.5f * Mathf.Max(squareWidth / targetAspect, squareHeight);

        // Set the camera's orthographic size to fit the square perfectly
        mainCamera.orthographicSize = orthographicSize;
    }
}
