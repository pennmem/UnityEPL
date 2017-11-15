using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[AddComponentMenu("UnityEPL/Handlers/Debug Log Handler")]
public class DebugLogHandler : DataHandler
{
    protected override void HandleDataPoints(DataPoint[] dataPoints)
    {
        foreach (DataPoint dataPoint in dataPoints)
            Debug.Log(dataPoint.ToJSON());
    }
}
