using PixelCrushers.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LoadBackToTrail
{    
    public static void Load(string scene)
    {
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
        DialogueManager.SetDialoguePanel(true);
    }
}
