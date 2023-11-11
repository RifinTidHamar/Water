using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class unloadObjects : MonoBehaviour
{
    void Start()
    {
        Resources.UnloadUnusedAssets();
    }
}

