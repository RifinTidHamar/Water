using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LoadBackToTrail
{    public static void Load()
    {
        SceneManager.LoadScene("Rifin", LoadSceneMode.Single);
        DialogueManager.SetDialoguePanel(true);

    }
}
