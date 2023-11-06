using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeLightFlciker : MonoBehaviour
{
    public Material smoke;
    [SerializeField]
    float intense;
    [SerializeField]
    float intRange;

    // Start is called before the first frame update
    float time = 0;
    private void Update()
    {
        time += Time.deltaTime;
        float timeWait = newRand();
        if (time > timeWait)
        {
            float val = Random.Range(intense - intRange, intense);
            smoke.SetFloat("_Alpha", val);
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
