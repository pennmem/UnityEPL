using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("UnityEPL/Reporters/Scripted Event Reporter")]
public class ScriptedEventReporter : DataReporter
{

    public void ReportScriptedEvent(string type, Dictionary<string, object> dataDict, int frameDelay = 0)
    {
        StartCoroutine(ReportScriptedEventCoroutine(type, dataDict, frameDelay));
    }

    private IEnumerator ReportScriptedEventCoroutine(string type, Dictionary<string, object> dataDict, int frameDelay)
    {
        for (int i = 0; i < frameDelay; i++)
            yield return null;
        eventQueue.Enqueue(new DataPoint(type, RealWorldFrameDisplayTime(), dataDict));
    }
}