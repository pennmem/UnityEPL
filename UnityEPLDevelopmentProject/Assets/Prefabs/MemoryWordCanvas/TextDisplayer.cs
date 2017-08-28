using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextDisplayer : MonoBehaviour 
{
	public ScriptedEventReporter wordEventReporter;
	public UnityEngine.UI.Text textElement;

	public void DisplayText(string description, string text)
	{
		textElement.text = text;
		Dictionary<string, string> dataDict = new Dictionary<string, string> ();
		dataDict.Add ("displayed text", text);
		wordEventReporter.ReportScriptedEvent (description, dataDict, 1);
	}

	public void ClearText()
	{
		textElement.text = "";
		wordEventReporter.ReportScriptedEvent ("text display cleared", new Dictionary<string, string> (), 1);
	}

	public string CurrentText()
	{
		return textElement.text;
	}
}