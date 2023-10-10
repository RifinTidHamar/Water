using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;

public class LoadBackToTrail : MonoBehaviour
{    public void Load()
    {
        SceneManager.LoadScene("Rifin", LoadSceneMode.Single);
        DialogueManager.SetDialoguePanel(true);

    }
}
