using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEPL {

    [AddComponentMenu("UnityEPL/Handlers/Debug Log Handler")]
    public class DebugLogHandler : DataHandler {
        protected override void AwakeOverride() { }

        protected override void HandleDataPoints(DataPoint[] dataPoints) {
            foreach (DataPoint dataPoint in dataPoints)
                Debug.Log(dataPoint.ToJSON());
        }
    }

}