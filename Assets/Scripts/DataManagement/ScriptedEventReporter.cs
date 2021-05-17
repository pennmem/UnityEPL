using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("UnityEPL/Reporters/Scripted Event Reporter")]
public class ScriptedEventReporter : DataReporter
{
    public void ReportScriptedEvent(string type, Dictionary<string, object> dataDict)
    {
        eventQueue.Enqueue(new DataPoint(type, TimeStamp(), dataDict));
    }

    public void ReportScriptedEvent(string type, Dictionary<string, object> dataDict, System.DateTime time)
    {
        eventQueue.Enqueue(new DataPoint(type, time, dataDict));
    }
}