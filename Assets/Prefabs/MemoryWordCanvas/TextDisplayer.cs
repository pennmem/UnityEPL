using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextDisplayer : MonoBehaviour
{
    /// <summary>
    /// Subscribe to this event to be notified of changes in the displayed text.
    /// 
    /// Single string argument is the new text which is being displayed.
    /// </summary>
    InterfaceManager manager;
    public delegate void TextDisplayed(string text);
    public static TextDisplayed OnText;

    /// <summary>
    /// Drag a scripted event reporter here to have this monobehavior automatically report when text is displayed or cleared.
    /// </summary>
    public ScriptedEventReporter wordEventReporter = null;

    /// <summary>
    /// These text elements will all be updated when this monobehaviors public methods are used.
    /// </summary>
    public UnityEngine.UI.Text textElement;
    public UnityEngine.UI.Text titleElement;

    private Color[] originalColors;

    void Start()
    {
        originalColors = new Color[2];
        originalColors[0] = textElement.color;
        originalColors[1] = titleElement.color;
    }

    /// <summary>
    /// Returns the color of the assigned text elements to whatever they were when this monobehavior initialized (usually scene load).
    /// </summary>
    public void OriginalColor()
    {
        textElement.color = originalColors[0];
        titleElement.color = originalColors[1];
        if (wordEventReporter != null)
            wordEventReporter.ReportScriptedEvent("restore original text color", new Dictionary<string, object>());
    }

    /// <summary>
    /// First argument is a description of the text to be displayed.  This is logged if the wordEventReporter field is populated in the editor.
    /// 
    /// Second argument is the text to be displayed.  All elements in the textElements field will be updated.  This is logged in the "data" field under "displayed text" if the wordEventReporter field is populated in the editor.
    /// </summary>
    /// <param name="description">Description.</param>
    /// <param name="text">Text.</param>
    public void DisplayText(string description, string text)
    {
        if (OnText != null)
            OnText(text);

        textElement.text = text;
        Dictionary<string, object> dataDict = new Dictionary<string, object>();
        dataDict.Add("displayed text", text);
        if (wordEventReporter != null)
            wordEventReporter.ReportScriptedEvent(description, dataDict);
    }

    public void DisplayTitle(string description, string text)
    {
        if (OnText != null)
            OnText(text);

        if(titleElement == null) {
            return;
        }

        titleElement.text = text;
        Dictionary<string, object> dataDict = new Dictionary<string, object>();
        dataDict.Add("displayed title", text);
        if (wordEventReporter != null)
            wordEventReporter.ReportScriptedEvent(description, dataDict);
    }

    /// <summary>
    /// Clears the text of all textElements.  This is logged if the wordEventReporter field is populated in the editor.
    /// </summary>
    public void ClearText()
    {
        if(titleElement == null) {
            return;
        }

        textElement.text = "";
        if (wordEventReporter != null)
            wordEventReporter.ReportScriptedEvent("text display cleared", new Dictionary<string, object>());
    }

    public void ClearTitle()
    {
        titleElement.text = "";
        if (wordEventReporter != null)
            wordEventReporter.ReportScriptedEvent("title display cleared", new Dictionary<string, object>());
    }

    /// <summary>
    /// Changes the color of all textElements.  This is logged if the wordEventReporter field is populated in the editor.
    /// </summary>
    /// <param name="newColor">New color.</param>
    public void ChangeColor(Color newColor)
    {
        textElement.color = newColor;
        Dictionary<string, object> dataDict = new Dictionary<string, object>();
        dataDict.Add("new color", newColor.ToString());
        if (wordEventReporter != null)
            wordEventReporter.ReportScriptedEvent("text color changed", dataDict);
    }

    /// <summary>
    /// Returns the current text being displayed on the first textElement.  Throws an error if there are no textElements.
    /// </summary>
    public string CurrentText()
    {
        if (textElement == null)
            throw new UnityException("There aren't any text elements assigned to this TextDisplayer.");
        return textElement.text;
    }
}