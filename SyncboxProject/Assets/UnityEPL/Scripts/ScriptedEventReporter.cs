using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("UnityEPL/Reporters/Scripted Event Reporter")]
public class ScriptedEventReporter : DataReporter
{
    /// <summary>
    /// Use this to report events from your own scripts.  The data will then be handled according to any handlers handling this reporter.
    /// </summary>
    /// <param name="type">Name of event.</param>
    /// <param name="dataDict">The data about the event.  String key is description of data, object value is the data to report.</param>
    public void ReportScriptedEvent(string type, Dictionary<string, object> dataDict, bool mainThread = true)
    {
        if (mainThread)
            eventQueue.Enqueue(new DataPoint(type, RealWorldFrameDisplayTime(), dataDict));
        else
            eventQueue.Enqueue(new DataPoint(type, ThreadsafeTime(), dataDict));
    }

    /// <summary>
    /// As ReportScriptedEvent, but uses a .NET clock class to avoid threading restrictions on Unity timing functionality.
    /// </summary>
    /// <param name="type">Name of event.</param>
    /// <param name="dataDict">The data about the event.  String key is description of data, object value is the data to report.</param>
    public void ReportOutOfThreadScriptedEvent(string type, Dictionary<string, object> dataDict)
    {
        eventQueue.Enqueue(new DataPoint(type, ThreadsafeTime(), dataDict));
    }
}