using Language.Lua;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LorR
{
    left,
    right
}

public class spotDetection : MonoBehaviour
{
    public LorR side; 
    void OnMouseEnter()
    {
        if (GlobVars.lOrR == (int)side || Time.time - GlobVars.timeSinceLast  > 3f)
        {
            //failed sequence
            GlobVars.reps = 0;
            GlobVars.lOrR = -1;

        }
        else
        {
            //successful step
            GlobVars.lOrR = (int)side;
            GlobVars.reps++;
            GlobVars.timeSinceLast = Time.time;
            Debug.Log(GlobVars.reps);
        }

        if (GlobVars.reps == 6)
        {
            //complete
            LoadBackToTrail.Load();
        }
    }
    void OnMouseExit()
    {
        //restart timer
        GlobVars.timeSinceLast = Time.time;
    }
}
