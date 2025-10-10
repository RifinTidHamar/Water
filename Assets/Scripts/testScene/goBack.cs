using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class goBack : MonoBehaviour
{ 
    public void goToForest()
    {
        LoadBackToTrail.Load("Rifin");
    }
    public void goToCave()
    {
        LoadBackToTrail.Load("RifinCave");
    }
}
