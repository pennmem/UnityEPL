using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("UnityEPL/Reporters/UI Data Reporter")]
public class UIDataReporter : DataReporter
{
    /// <summary>
    /// This can be subscribed to Unity UI buttons, etc.
    /// </summary>
    /// <param name="name">Name of the event to log.</param>
    public void LogUIEvent(string name)
    {
        eventQueue.Enqueue(new DataPoint(name, RealWorldFrameDisplayTime(), new Dictionary<string, object>()));
    }
}