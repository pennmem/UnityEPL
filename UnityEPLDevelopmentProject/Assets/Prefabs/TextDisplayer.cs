using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextDisplayer : MonoBehaviour 
{
	public ScriptedEventReporter wordEventReporter;
	public UnityEngine.UI.Text textElement;

	public void DisplayText(string text)
	{
		textElement.text = text;
		Dictionary<string, string> dataDict = new Dictionary<string, string> ();
		dataDict.Add ("displayed text", text);
		wordEventReporter.ReportScriptedEvent ("word stimulus display", dataDict, 1);
	}

	public void ClearText()
	{
		textElement.text = "";
		wordEventReporter.ReportScriptedEvent ("world stimulus cleared", new Dictionary<string, string> (), 1);
	}
}