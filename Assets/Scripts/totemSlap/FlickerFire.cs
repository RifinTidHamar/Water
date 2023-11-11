using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickerFire : MonoBehaviour
{
    public Material mat;

    [SerializeField]
    float initVal;
    [SerializeField]
    float ranRange;

    // Start is called before the first frame update
    float time = 0;
    private void Update()
    {
        float timeWait = newRand();
        if (Time.time - time > timeWait)
        {
            float val = Random.Range(initVal - ranRange, initVal + ranRange);
            mat.SetColor("_Color", new Color(0,0, 0, val));
            time = Time.time;
            timeWait = newRand();
        }
    }

    private float newRand()
    {
        float rand = Random.Range(0.05f, 0.15f);
        return rand;
    }
}
