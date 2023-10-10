using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
    public struct trilygon
    {
        public Vector2 t;
        public Vector2 r;
        public Vector2 l;
    }
    public struct polygon
    {
        public Vector2 tl;
        public Vector2 tr;
        public Vector2 br;
        public Vector2 bl;
    }

    public static int polygonSize = (sizeof(float) * 2) * 4;
    public static int triangleSize = (sizeof(float) * 2) * 3;

    // Start is called before the first frame update
    public static Shape.polygon createPolyGon(Vector2 baseLoc, float baseWidth, Vector2 topLoc, float topWidth)
    {
        Shape.polygon poly = new Shape.polygon();
        poly.bl = new Vector2(baseLoc.x - (baseWidth / 2), baseLoc.y);
        poly.br = new Vector2(baseLoc.x + (baseWidth / 2), baseLoc.y);
        poly.tl = new Vector2(topLoc.x - (topWidth / 2), topLoc.y);
        poly.tr = new Vector2(topLoc.x + (topWidth / 2), topLoc.y);
        return poly;
    }

    public static Shape.trilygon createTrilygon(Vector2 baseLoc, float baseWidth, Vector2 topLoc)
    {
        Shape.trilygon tri = new Shape.trilygon();

        tri.t = topLoc;
        tri.r = new Vector2(baseLoc.x + (baseWidth / 2), baseLoc.y);
        tri.l = new Vector2(baseLoc.x - (baseWidth / 2), baseLoc.y);
        return tri;
    }
}
