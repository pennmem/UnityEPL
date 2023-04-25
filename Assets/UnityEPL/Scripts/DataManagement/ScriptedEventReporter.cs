using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace UnityEPL {

    [AddComponentMenu("UnityEPL/Reporters/Scripted Event Reporter")]
    public class ScriptedEventReporter : DataReporter {

        // TODO: JPB: (needed) (bug) Make ReportScriptedEvent use a blittable type instead of Dictionary
        //            Or at least have it use ICloneable 
        public void ReportScriptedEvent(string type, Dictionary<string, object> data = null) {
            Do<StackString>((type) => {
                ReportScriptedEventHelper(type, TimeStamp(), data);
            }, type);
        }
        public void ReportScriptedEventMB(string type, Dictionary<string, object> data = null) {
            DoMB(ReportScriptedEventHelper, new StackString(type), TimeStamp(), data);
        }

        public void ReportScriptedEvent(StackString type, DateTime time, Dictionary<string, object> data = null) {
            Do<StackString, DateTime>((type, time) => {
                ReportScriptedEventHelper(type, time, data);
            }, type, time);
        }
        public void ReportScriptedEventMB(string type, DateTime time, Dictionary<string, object> data = null) {
            DoMB(ReportScriptedEventHelper, new StackString(type), time, data);
        }

        protected void ReportScriptedEventHelper(StackString type, DateTime time, Dictionary<string, object> data = null) {
            var dataDict = data ?? new();
            eventQueue.Enqueue(new DataPoint(type, time, dataDict));
        }
    }
}