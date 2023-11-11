using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotateInNextTotem : MonoBehaviour
{
    public Frame totem;
    public Button dodgeButt;

    // Update is called once per frame
    void Update()
    {
        if (GameVars.isClayDone && dodgeButt.interactable)
        {
            dodgeButt.interactable = false;
            this.transform.Rotate(Vector3.forward, 120 * Time.deltaTime);
        }

        if (this.transform.rotation.eulerAngles.z > 120 && this.transform.rotation.eulerAngles.z < 128f)
        {
            this.transform.Rotate(Vector3.forward, 114);
            GameVars.shapeInd++;
            totem.init();
        }
        else if(this.transform.rotation.eulerAngles.z > 357 && this.transform.rotation.eulerAngles.z < 364f)
        {
            dodgeButt.interactable = true;
            this.transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
            GameVars.isClayDone = false;
        }
    }
}
