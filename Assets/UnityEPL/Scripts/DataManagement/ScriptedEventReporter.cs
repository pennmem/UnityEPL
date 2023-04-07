using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEPL {

    [AddComponentMenu("UnityEPL/Reporters/Scripted Event Reporter")]
    public class ScriptedEventReporter : DataReporter {
        public void ReportScriptedEvent(string type, Dictionary<string, object> data = null) {
            ReportScriptedEvent(type, TimeStamp(), data);
        }

        public void ReportScriptedEvent(string type, DateTime time, Dictionary<string, object> data = null) {
            var dataDict = data ?? new();
            eventQueue.Enqueue(new DataPoint(type, time, dataDict));
        }
    }
}