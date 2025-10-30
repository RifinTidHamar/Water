using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSSet : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        //QualitySettings.vSyncCount = 0;     // disable VSync
        Application.targetFrameRate = 60;
    }
}
