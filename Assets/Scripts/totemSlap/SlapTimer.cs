using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlapTimer : MonoBehaviour
{
    float timeBetwSlaps = 5f;
    float totalTime;
    static float lastSlapTime;
    float lastDodgeTime;
    float frameTime;
    float lastButtonDisableTime;
    //bool anim = false;
    //static bool dodge;
    float timeBetwFrames = 0.1f;

    public Sprite[] frames;
    public SpriteRenderer sprRend;
    public Frame sand;
    public GameObject sandFrame;
    public GameObject rayCatcher;
    public Button dodgeButt;

    public Transform defaultPos;
    public Transform dodgePos;
    public Transform smackPos;

    bool inSmackPos = false;

    // Start is called before the first frame update
    void Start()
    {
        lastSlapTime = Time.time;
        //Game = false;
        timeBetwSlaps = Random.Range(4, 7);
    }

    public void doDodge()
    {
        GameVars.isInDodge = true;
        lastDodgeTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(GameVars.isInDodge)
        {
            dodgeButt.interactable = false;
            lastButtonDisableTime = Time.time;
            sandFrame.gameObject.transform.position = dodgePos.position;
            if (Time.time - lastDodgeTime >= 0.7f)
            {
                GameVars.isInDodge = false;
                sandFrame.gameObject.transform.position = defaultPos.position;
                //rayCatcher.SetActive(true);
            }
        }
        if (dodgeButt.interactable == false)
        {
            if (Time.time - lastButtonDisableTime >= 2f)
            {
                dodgeButt.interactable = true;
            }
        }
        if(GameVars.isClayDone)
        {
            frameTime = Time.time;
        }
        if (Time.time - lastSlapTime >= timeBetwSlaps)
        {
            timeBetwSlaps = Random.Range(4, 7);
            //Debug.Log("slap");
            GameVars.isInAnim = true;
            frameTime = Time.time;
        }
        if (GameVars.isInAnim)
        {
            lastSlapTime = Time.time; // in order that the timer doesn't interfere with the slap

            if (Time.time - frameTime >= timeBetwFrames * 8)
            {
                if (!GameVars.isInDodge && !GameVars.isClayDone)
                {
                    sandFrame.gameObject.transform.position = defaultPos.position;
                    inSmackPos = false;
                    //sand.init();
                }
                sprRend.sprite = frames[0];
                GameVars.isInAnim = false;
            }
            else if (Time.time - frameTime >= timeBetwFrames * 7)
            {
                if (!GameVars.isInDodge && !GameVars.isClayDone && !inSmackPos)
                {
                    sandFrame.gameObject.transform.position = smackPos.position;
                    sand.init();
                    inSmackPos = true;
                }
                sprRend.sprite = frames[3];
            }
            else if (Time.time - frameTime >= timeBetwFrames * 6)
            {
                sprRend.sprite = frames[2];
                //whiteScreenColor += new Vector4(0, 0, 0, 0.25f); //0.25
            }
            else if (Time.time - frameTime >= 0)
            {
                sprRend.sprite = frames[1];
            }
        }
    }
}
