using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    [Tooltip("Typically leave unticked so temporary Dialogue Managers don't unregister your functions.")]
    public bool unregisterOnDisable = false;

    public GameObject dialogueManager;// = GameObject.FindGameObjectWithTag("trailTexts");

    void OnEnable()
    {
        // Make the functions available to Lua: (Replace these lines with your own.)
        Lua.RegisterFunction(nameof(DebugLog), this, SymbolExtensions.GetMethodInfo(() => DebugLog(string.Empty)));
        Lua.RegisterFunction(nameof(Load), this, SymbolExtensions.GetMethodInfo(() => Load(string.Empty)));
    }

    void OnDisable()
    {
        if (unregisterOnDisable)
        {
            // Remove the functions from Lua: (Replace these lines with your own.)
            Lua.UnregisterFunction(nameof(DebugLog));
            Lua.UnregisterFunction(nameof(Load));
        }
    }

    public void DebugLog(string message)
    {
        Debug.Log(message);
    }

    public void Load(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        DialogueManager.SetDialoguePanel(false);
    }
}
