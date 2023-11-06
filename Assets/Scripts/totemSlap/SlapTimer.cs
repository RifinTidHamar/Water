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
    bool anim = false;
    static bool dodge;
    float timeBetwFrames = 0.1f;

    public Sprite[] frames;
    public SpriteRenderer sprRend;
    public GameObject sand;
    public GameObject rayCatcher;
    public Button dodgeButt;

    // Start is called before the first frame update
    void Start()
    {
        lastSlapTime = Time.time;
        dodge = false;
        timeBetwSlaps = Random.Range(4, 7);
    }

    public void doDodge()
    {
        dodge = true;
        lastDodgeTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(dodge)
        {
            dodgeButt.interactable = false;
            lastButtonDisableTime = Time.time;
            rayCatcher.SetActive(false);
            sand.gameObject.transform.localPosition = new Vector3(0.268999994f, -0.164993092f, 10);
            if (Time.time - lastDodgeTime >= 0.7f)
            {
                sand.gameObject.transform.localPosition = new Vector3(-0.00499248505f, -0.164993092f, 10);
                dodge = false;
                rayCatcher.SetActive(true);
            }
        }
        if (dodgeButt.interactable == false)
        {
            if (Time.time - lastButtonDisableTime >= 2f)
            {
                dodgeButt.interactable = true;
            }
        }
        if (Time.time - lastSlapTime >= timeBetwSlaps)
        {
            timeBetwSlaps = Random.Range(4, 7);
            //Debug.Log("slap");
            anim = true;
            frameTime = Time.time;
        }
        if (anim)
        {
            lastSlapTime = Time.time; // in order that the timer doesn't interfere with the slap

            if (Time.time - frameTime >= timeBetwFrames * 8)
            {
                if(!dodge)
                {
                    sand.gameObject.transform.localPosition = new Vector3(-0.00499248505f, -0.164993092f, 10);
                }
                sprRend.sprite = frames[0];
                anim = false;
            }
            else if (Time.time - frameTime >= timeBetwFrames * 7)
            {
                if (!dodge)
                {
                    sand.gameObject.transform.localPosition = new Vector3(-0.00499248505f, -0.164993092f - 0.1f, 10);
                    TotemSlap totem = sand.gameObject.GetComponent<TotemSlap>();
                    totem.init();
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
