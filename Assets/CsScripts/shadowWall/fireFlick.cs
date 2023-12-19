using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fireFlick : MonoBehaviour
{
    public LightDat fireLight;
    /*[SerializeField]
    float intense;*/
    [SerializeField]
    float intRange;
    [SerializeField]
    float ranRange;

    float range;
    float intensity;
    private void Start()
    {
        range = fireLight.range;
        intensity = fireLight.intensity;
    }

    // Start is called before the first frame update
    float time = 0;
    private void Update()
    {
        time += Time.deltaTime;
        float timeWait = newRand();
        if(time > timeWait)
        {
            float val = Random.Range(range - ranRange, range);
            fireLight.range = val;
            val = Random.Range(intensity - intRange, intensity);
            fireLight.intensity = val;
            //val = Random.Range(intense - ranRange, intense);
            //fireLight.intensity = val;
            time = 0;
            timeWait = newRand();
        }
    }

    private float newRand()
    {
        float rand = Random.Range(0.05f, 0.15f);
        return rand;
    }
}
