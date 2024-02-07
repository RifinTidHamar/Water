using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class IntroSequence : MonoBehaviour
{
    [SerializeField]
    Color plCol;

    [SerializeField]
    bool doCrack = true;
    float timeSinceStart;
    float timeUntilCrack = 3;
    public GameObject bluePlane;
    public GameObject crackPicture;
    public GraphicRaycaster UIText;

    float timeSinceCrack;

    bool crackIsVisiable = false;
    float timeUntilFadedBlue = 3;
    public Material bPlaneMat;
    float bPlTrans = 1;
    // Start is called before the first frame update
    private void OnEnable()
    {
        if(Time.time > 5)//to make sure that the blue plane only plays at the start of the game; might need to be changed if we put in a main menu
        {
            bluePlane.SetActive(false);
        }
    }
    void Start()
    {
        timeSinceStart = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - timeSinceStart >= timeUntilCrack && !crackIsVisiable)
        {
            if(doCrack)
                crackPicture.SetActive(true);
            timeSinceCrack = Time.time;
            crackIsVisiable = true;
        }
        else if (Time.time - timeSinceCrack >= timeUntilFadedBlue && crackIsVisiable)
        {
            bPlTrans -= 0.5f * Time.deltaTime;
            plCol = new Color(plCol.r, plCol.g, plCol.b, bPlTrans);
            bPlaneMat.SetColor("_Color", plCol);
            if(bPlTrans < 0)
            {
                plCol = new Color(plCol.r, plCol.g, plCol.b, 1);
                bPlaneMat.SetColor("_Color", plCol);
                UIText.enabled = true;
                bluePlane.SetActive(false);
            }
        }
    }
}
