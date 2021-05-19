using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This makes the maximum text size smaller when display (presumably) long messages ending in a period.  It also makes the text take up the whole screen.
/// 
/// This is included in order to match the behavior of PyEPL (CML's previous experimental task software).
/// </summary>
public class TextResizer : MonoBehaviour
{
    public UnityEngine.UI.Text textElement;

    private Vector2 originalAnchorMin;
    private Vector2 originalAnchorMax;

    private const int SIZE_WHEN_SENTENCE = 60;
    private const int SIZE_WHEN_NOT = 300;

    void OnEnable()
    {
        TextDisplayer.OnText += OnText;
    }

    void OnDisable()
    {
        TextDisplayer.OnText -= OnText;
    }

    void OnText(string text)
    {
        if (text.Length > 0 && text[text.Length - 1].Equals('.'))
        {
            textElement.resizeTextMaxSize = SIZE_WHEN_SENTENCE;
            textElement.rectTransform.anchorMin = new Vector2(0, 0);
            textElement.rectTransform.anchorMax = new Vector2(1, 1);
        }
        else
        {
            textElement.resizeTextMaxSize = SIZE_WHEN_NOT;
            textElement.rectTransform.anchorMin = originalAnchorMin;
            textElement.rectTransform.anchorMax = originalAnchorMax;
        }
    }

    void Start()
    {
        originalAnchorMin = textElement.rectTransform.anchorMin;
        originalAnchorMax = textElement.rectTransform.anchorMax;
    }

}