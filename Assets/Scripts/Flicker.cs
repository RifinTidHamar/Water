using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flicker : MonoBehaviour
{
    public Light fireLight;
    [SerializeField]
    float intense;
    [SerializeField]
    float intRange;
    [SerializeField]
    float range;
    [SerializeField]
    float ranRange;

    // Start is called before the first frame update
    float time = 0;
    private void Update()
    {
        time += Time.deltaTime;
        float timeWait = newRand();
        if(time > timeWait)
        {
            float val = Random.Range(range - intRange, range);
            fireLight.range = val;
            val = Random.Range(intense - ranRange, intense);
            fireLight.intensity = val;
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
