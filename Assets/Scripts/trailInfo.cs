using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trailInfo : MonoBehaviour
{

    public enum trailStartIds
    {
        na,
        strt0crack,
        strt1star,
        strt2fire,
        
    }

    public enum trailMntIds
    {
        na,
        mnt0start,
    }

    public enum trailCtyIds
    {
        na,
        cty0start,
    }

    public enum trailPlnIds
    {
        na,
        pln0start,
    }

    public enum trailForIds
    {
        na,
        for0start,
    }

    public trailStartIds start;
    public trailForIds forest;
    public trailMntIds mountain;
    public trailPlnIds planes;
    public trailCtyIds city;
}
