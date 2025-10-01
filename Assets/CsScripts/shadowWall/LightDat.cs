using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class LightDat : MonoBehaviour
{
    public Color color = new Color(1,1,1,1);
    public float range = 20;
    public float intensity = 1;
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
