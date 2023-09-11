using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResetScrollBar : MonoBehaviour
{
    public static Scrollbar scrolly;
    public static void resetScrollBar()
    {
        scrolly.value = 1;
    }
}
