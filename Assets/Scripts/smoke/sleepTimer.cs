using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class sleepTimer : MonoBehaviour
{
    int blinkCount;
    bool isBlinking;
    float timeSinceBlink;
    float timeBetwBlink;
    public Button blinkButton;
    public Button sleepText;

    public Button gameOverText;

    float timeSinceEnabledSleepText;
    bool enableSleepText;
    public GameObject blinkPlane;

    bool startedBlink = false;
    float timeInBlink;
    // Start is called before the first frame update
    void Start()
    {
        timeBetwBlink = 25;
        blinkButton.interactable = false;
        blinkPlane.SetActive(false);
        timeSinceBlink = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - timeSinceBlink >= timeBetwBlink && !isBlinking)
        {
            blinkButton.interactable = true;
        }
        else if(isBlinking)
        {
            //Debug.Log("in here");
            if(!startedBlink)
            {
                blinkPlane.SetActive(true);
                timeInBlink = Time.time;
            }
            startedBlink = true;
            if(Time.time - timeInBlink >= blinkCount && blinkCount < 3)
            {
                blinkPlane.SetActive(false);
                isBlinking = false;
                startedBlink = false;
            }
        }
        if (enableSleepText && Time.time - timeSinceEnabledSleepText >= 16)
        {
            LoadBackToTrail.Load();
        }
        else if (enableSleepText && Time.time - timeSinceEnabledSleepText >= 8)
        {
            sleepText.interactable = false;
            gameOverText.interactable = false;
        }
        else if (enableSleepText && Time.time - timeSinceEnabledSleepText >= 2)
        {
            sleepText.interactable = true;
            gameOverText.interactable = true;
        }
        
    }

    public void blink()
    {
        isBlinking = true;
        blinkButton.interactable = false;
        timeSinceBlink = Time.time;
        switch(blinkCount)
        {
            case 0:
                timeBetwBlink = 10;
                break;
            case 1:
                timeBetwBlink = 5;
                break;
            case 2:
            
                enableSleepText = true;
                timeSinceEnabledSleepText = Time.time;
                break;
            default: break;
        }
        blinkCount++;
    }
}
