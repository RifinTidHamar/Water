using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamToUv
{ 
    public Vector2 genMousePosOnImage(Camera cam, GameObject plane)
    {
        Vector3 normScreen = new Vector3((float)1 / (float)Screen.width, (float)1 / (float)Screen.width, 1);

        Vector3 mPos = Vector3.Scale(Input.mousePosition, normScreen);

        Vector3 objPos = plane.transform.position;
        Vector3 objScale = plane.transform.lossyScale / 2;

        Vector3 objRPos = objPos + new Vector3(objScale.x, -objScale.y, 0);
        objRPos = cam.WorldToScreenPoint(objRPos);
        objRPos = Vector3.Scale(objRPos, normScreen);

        objPos += new Vector3(-objScale.x, objScale.y, 0);
        objPos = cam.WorldToScreenPoint(objPos);
        objPos = Vector3.Scale(objPos, normScreen);

        float objXScale = Mathf.Abs(objPos.x - objRPos.x);
        float objYScale = Mathf.Abs(objPos.y - objRPos.y);

        Vector3 normScale = new Vector3((float)1 / (float)objXScale, (float)-1 / (float)objYScale, 1);


        Vector3 mObjPos = mPos - objPos;
        mObjPos = Vector3.Scale(mObjPos, normScale);

        return new Vector2(mObjPos.x, mObjPos.y);
    }

}
