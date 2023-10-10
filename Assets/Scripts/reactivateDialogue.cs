using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class reactivateDialogue : MonoBehaviour
{
    public GameObject dialogueManager;// = GameObject.FindGameObjectWithTag("trailTexts");

    // Start is called before the first frame update
    void Start()
    {
        dialogueManager.SetActive(true);

    }
}
