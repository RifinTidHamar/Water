using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gyroPlayerMovement : MonoBehaviour
{
    public Sprite[] tiltSprites;
    public SpriteRenderer sprRend;
    int frameCount;
    float forwardSpeed = 0;

    [SerializeField]
    float rotationRate;

    [SerializeField]
    float horizontalMoveRate;
    private void Start()
    {
        frameCount = tiltSprites.Length;
    }
    // Update is called once per frame
    void Update()
    {
        this.transform.position += new Vector3(Gyroscope.yRotation * Time.deltaTime * horizontalMoveRate, 0, forwardSpeed* Time.deltaTime);
        float normRot = (Gyroscope.yRotation * rotationRate) + (rotationRate/2);
        int framSelect = (int)Mathf.Lerp(0, frameCount - 1, normRot);

        sprRend.sprite = tiltSprites[framSelect];
    }

    public void moveForward()
    {
        forwardSpeed = 2 ;
    }

}
