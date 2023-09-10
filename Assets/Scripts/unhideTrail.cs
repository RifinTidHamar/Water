using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class unhideTrail : MonoBehaviour
{
    public Scrollbar textScrollBar;

    public GameObject[,] trailScenes;
    public GameObject[,] trailTexts; 

    public void showTrailAndText(GameObject trail)
    {
        GameObject[] trails = GameObject.FindGameObjectsWithTag("trail");

        foreach(GameObject t  in trails)
        {
            t.SetActive(false);
        }

        trail.SetActive(true);
    }

    void showTrailText(int trailId)
    {
        GameObject[] trailTexts = GameObject.FindGameObjectsWithTag("trailTexts");

        foreach (GameObject t in trailTexts)
        {
            t.SetActive(false);
            //if ()
        }

        //trailText.SetActive(true);
        textScrollBar.value = 1;
    }
}
