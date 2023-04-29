using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace UnityEPL {

    [AddComponentMenu("UnityEPL/Reporters/Scripted Event Reporter")]
    public class ScriptedEventReporter : DataReporter {

        // TODO: JPB: (needed) (bug) Make ReportScriptedEvent use a blittable type instead of Dictionary
        //            Or at least have it use Mutex
        public void ReportScriptedEvent(string type, Dictionary<string, object> data = null) {
            Do((type) => {
                ReportScriptedEventHelper(type, TimeStamp(), data);
            }, type.ToNativeText());
        }
        public void ReportScriptedEventMB(string type, Dictionary<string, object> data = null) {
            DoMB(ReportScriptedEventHelper, type.ToNativeText(), TimeStamp(), data);
        }

        public void ReportScriptedEvent(string type, DateTime time, Dictionary<string, object> data = null) {
            Do((type, time) => {
                ReportScriptedEventHelper(type, time, data);
            }, type.ToNativeText(), time);
        }
        public void ReportScriptedEventMB(string type, DateTime time, Dictionary<string, object> data = null) {
            DoMB(ReportScriptedEventHelper, type.ToNativeText(), time, data);
        }

        protected void ReportScriptedEventHelper(NativeText type, DateTime time, Dictionary<string, object> data = null) {
            var dataDict = data ?? new();
            eventQueue.Enqueue(new DataPoint(type.ToString(), time, dataDict));
            type.Dispose();
        }
    }
}