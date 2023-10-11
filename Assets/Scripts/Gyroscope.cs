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

    public TextMeshPro fx;
    public TextMeshPro fy;
    public TextMeshPro fz;

    public TextMeshPro ox;
    public TextMeshPro oy;
    public TextMeshPro oz;

    public TextMeshPro nx;
    public TextMeshPro ny;
    public TextMeshPro nz;

    [SerializeField]
    float x;
    [SerializeField]
    float y;
    [SerializeField]
    float z;
    [SerializeField]
    float w;
    public GameObject cube;
    Quaternion gyroOffset = Quaternion.identity;
    bool doGyro = false;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(startGyro());
    }

    IEnumerator startGyro()
    {
        gx.text = "here";
        yield return new WaitForSeconds(2.0f); // Wait for 2 seconds
        initGyro();
    }

    void initGyro()
    {
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            doGyro = true;

            Vector3 originalZAxis = Vector3.Scale(Input.gyro.attitude.eulerAngles, Vector3.up);
            Vector3 originalXAxis = Vector3.Scale(Input.gyro.attitude.eulerAngles, Vector3.left);
            Vector3 originalYAxis = Vector3.Scale(Input.gyro.attitude.eulerAngles, Vector3.forward);
            float yangle = Vector3.SignedAngle(originalYAxis, Vector3.forward, originalXAxis);
            float xangle = Vector3.SignedAngle(originalXAxis, Vector3.left, originalZAxis);
            float zangle = Vector3.SignedAngle(originalZAxis, Vector3.up, originalYAxis);
            Quaternion rotQuat = Quaternion.Euler(new Vector3(xangle, yangle, zangle));
            gyroOffset = Quaternion.Inverse(Input.gyro.attitude * rotQuat);

        }
        else
        {
            gx.text = "gyro not supported";
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (doGyro)
        {
            if (gyroOffset.x == 0)
            {
                initGyro();
            }

            //checkGyroOnce = false;s
            Quaternion gyro = GyroToUnity(Input.gyro.attitude * gyroOffset);
            gx.text = "X: " + Mathf.Round((gyro.x / Mathf.PI) * 1000f) / 1000f;
            gy.text = "Y: " + Mathf.Round((gyro.y / Mathf.PI) * 1000f) / 1000f;
            gz.text = "Z: " + Mathf.Round((gyro.z / Mathf.PI) * 1000f) / 1000f;
            gw.text = "W: " + Mathf.Round((gyro.w / Mathf.PI) * 1000f) / 1000f;
            //Quaternion newGyro = new Quaternion(gyro.y, 0, 0, gyro.w);
            Vector3 tilt = Quaternion.ToEulerAngles(gyro);//newGyro);
            //Vector3 zeroTilt = new Vector3(
            //    tilt.x - origV3.x,
            //    tilt.y - origV3.y,
            //    tilt.z - origV3.z);
            float tiltX = (tilt.x / Mathf.PI);
            float tiltY = (tilt.y / Mathf.PI);
            float tiltZ = (tilt.z / Mathf.PI);

            fx.text = "X: " + Mathf.Round(tiltX * 1000f) / 1000f;
            fy.text = "Y: " + Mathf.Round(tiltY * 1000f) / 1000f;
            fz.text = "Z: " + Mathf.Round(tiltZ * 1000f) / 1000f;

            //ox.text = "X: " + gyroOffset.x;
            //oy.text = "Y: " + gyroOffset.y;
            //oz.text = "Z: " + gyroOffset.z;

            //nx.text = "X: " + gyroOffset.w;
            //ny.text = "Y: " + Mathf.Round(tilt.y / 180 * 1000f) / 1000f;
            //nz.text = "Z: " + Mathf.Round(tilt.z / 180 * 1000f) / 1000f;
        }
    }

    private static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }
}
