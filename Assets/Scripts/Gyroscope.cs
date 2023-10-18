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
    Quaternion rotQuat;
    Vector3 origRot;
    public static float yRotation;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(startGyro());
    }

    IEnumerator startGyro()
    {
        //gx.text = "here";
        yield return new WaitForSeconds(0.5f); // Wait for 2 seconds
        initGyro();
    }

    void initGyro()
    {
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            doGyro = true;
            Vector3 up = new Vector3(0, 0, 1);
            Vector3 left = new Vector3(1, 0, 0);
            Vector3 fore = new Vector3(0, 1, 0);
            origRot = Input.gyro.attitude.eulerAngles;
            //Vector3 originalZAxis = Vector3.Scale(Input.gyro.attitude.eulerAngles, up);
            //Vector3 originalXAxis = Vector3.Scale(Input.gyro.attitude.eulerAngles, left);
            //Vector3 originalYAxis = Vector3.Scale(Input.gyro.attitude.eulerAngles, fore);
            Vector3 crossForeGyro = Vector3.Cross(fore, origRot);
            Vector3 crossLeftGyro = Vector3.Cross(left, origRot);
            Vector3 crossUpGyro = Vector3.Cross(up, origRot);

            float yangle = Vector3.SignedAngle(fore, origRot, crossForeGyro);
            float xangle = Vector3.SignedAngle(left, origRot, crossLeftGyro);
            float zangle = Vector3.SignedAngle(up, origRot, crossUpGyro);

             rotQuat = Quaternion.Euler(fore);

            //ox.text = "y angle: " +  yangle;

            //rotQuat = Quaternion.Euler(new Vector3(0, -yangle, 0));
            gyroOffset = Quaternion.Inverse(Input.gyro.attitude) /** Quaternion.Inverse(rotQuat)*/;
        }
        else
        {
            //gx.text = "gyro not supported";
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
            //rotQuat = Quaternion.FromToRotation(origRot, Input.gyro.attitude.eulerAngles);

            Quaternion gyro = GyroToUnity(Input.gyro.attitude * gyroOffset/* * Quaternion.Inverse(rotQuat)*/);
            //gx.text = "X: " + Mathf.Round((gyro.x / Mathf.PI) * 1000f) / 1000f;
            //gy.text = "Y: " + Mathf.Round((gyro.y / Mathf.PI) * 1000f) / 1000f;
            //gz.text = "Z: " + Mathf.Round((gyro.z / Mathf.PI) * 1000f) / 1000f;
            //gw.text = "W: " + Mathf.Round((gyro.w / Mathf.PI) * 1000f) / 1000f;
            Vector3 tilt = Quaternion.ToEulerAngles(gyro);//newGyro);

            float tiltX = (tilt.x / Mathf.PI);
            float tiltY = (tilt.y / Mathf.PI);
            float tiltZ = (tilt.z / Mathf.PI);

            yRotation = tiltY;

            //fx.text = "X: " + Mathf.Round(tiltX * 1000f) / 1000f;
            //fy.text = "Y: " + Mathf.Round(tiltY * 1000f) / 1000f;
            //fz.text = "Z: " + Mathf.Round(tiltZ * 1000f) / 1000f;

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
