using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class LightDat : MonoBehaviour
{
    public Color color;
    public float range;
    public float intensity;
    [HideInInspector]
    public Transform trans;

    public static List<LightDat> AllLights = new List<LightDat>();
   
    void Awake()
    {
        trans = this.transform;
        AllLights.Add(this);
    }
    void OnDisable() => AllLights.Remove(this);

    
}
