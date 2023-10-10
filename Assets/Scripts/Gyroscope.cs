using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Gyroscope : MonoBehaviour
{
    public TextMeshPro gx;
    public TextMeshPro gy;
    public TextMeshPro gz;
    public TextMeshPro gw;

    public TextMeshPro vx;
    public TextMeshPro vy;
    public TextMeshPro vz;

    [SerializeField]
    float x;
    [SerializeField]
    float y;
    [SerializeField]
    float z;
    [SerializeField]
    float w;

    public GameObject cube;
    void Start()
    {
        StartCoroutine(InitializeGyro());
    }

    IEnumerator InitializeGyro()
    {
        gx.text = "here";
        yield return new WaitForSeconds(2.0f); // Wait for 2 seconds
        if(SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = false;
            Input.gyro.enabled = true; 
        }
        else
        {
            gx.text = "gyro not supported";
        }
    }
    bool checkGyroOnce = true;
    Quaternion gyroOffset = Quaternion.identity;

    // Update is called once per frame
    void Update()
    {
        if (checkGyroOnce)
        {
            if (Input.gyro.enabled && SystemInfo.supportsGyroscope)
            {
                gyroOffset = Quaternion.Inverse(Input.gyro.attitude);
                checkGyroOnce = false;
                gx.text = "all good on the western front";
            }
        }

        //Quaternion gyro = GyroToUnity(Input.gyro.attitude * gyroOffset);
        //gx.text = "X: " + Mathf.Round((gyro.x / Mathf.PI) * 1000f) / 1000f;
        //gy.text = "Y: " + Mathf.Round((gyro.y / Mathf.PI) * 1000f) / 1000f;
        //gz.text = "Z: " + Mathf.Round((gyro.z / Mathf.PI) * 1000f) / 1000f;
        //gw.text = "W: " + Mathf.Round((gyro.w / Mathf.PI) * 1000f) / 1000f;
        //Quaternion newGyro = new Quaternion(gyro.z, 0, 0, gyro.w);
        //Vector3 tilt = Quaternion.ToEulerAngles(newGyro);
        //float tiltX = tilt.x / Mathf.PI;
        //float tiltY = tilt.y / Mathf.PI;
        //float tiltZ = tilt.z / Mathf.PI;

        //vx.text = "X: " + Mathf.Round(tiltX * 1000f) / 1000f;
        //vy.text = "Y: " + Mathf.Round(tiltY * 1000f) / 1000f;
        //vz.text = "Z: " + Mathf.Round(tiltZ * 1000f) / 1000f;
    }

    private static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }
}
