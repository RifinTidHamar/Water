using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PixelCrushers;
using TMPro;
public class ResetScrollBar : MonoBehaviour
{

    public TextMeshProUGUI dialogueText; // Reference to your TextMeshProUGUI element
    private string previousText;
    public Scrollbar scrolly;

    private void Start()
    {
        // Initialize the previous text to the current text.
        previousText = dialogueText.text;
    }

    private void Update()
    {
        // Check if the current text is different from the previous text.
        if (dialogueText.text != previousText)
        {
            // Custom logic to execute when the text changes.
            resetScrollBar();

            // Update the previous text to the current text.
            previousText = dialogueText.text;
        }
    }
    void resetScrollBar()
    {
        scrolly.value = 1;
    }
}
