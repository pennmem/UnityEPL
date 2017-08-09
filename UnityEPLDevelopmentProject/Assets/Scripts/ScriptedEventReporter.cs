using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("UnityEPL/Reporters/Scripted Event Reporter")]
public class ScriptedEventReporter : DataReporter
{

	//waits a specified number of frames and then reports the event as occuring at the beginning of the current frame
	public void ReportScriptedEvent(string type, Dictionary<string, string> dataDict, int frameDelay = 0)
	{
		StartCoroutine (ReportScriptedEventCoroutine(type, dataDict, frameDelay));
	}

	private IEnumerator ReportScriptedEventCoroutine(string type, Dictionary<string, string> dataDict, int frameDelay)
	{
		for (int i = 0; i < frameDelay; i++)
			yield return null;
		eventQueue.Enqueue(new DataPoint(type, RealWorldFrameDisplayTime(), dataDict));
	}
}