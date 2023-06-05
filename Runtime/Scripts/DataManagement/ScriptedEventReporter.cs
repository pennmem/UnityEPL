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
        //            And the NativeText as well...
        //            And Datetime....
        //            Even better, just make DataPoint a Native type and then use that
        public void ReportScriptedEvent(string type, Dictionary<string, object> data = null) {
            var time = TimeStamp();
            Do(() => {
                ReportScriptedEventHelper(type.ToNativeText(), time, data);
            });
            //Do((type, timestamp) => {
            //    ReportScriptedEventHelper(type, timestamp, data);
            //}, type.ToNativeText(), TimeStamp());
        }
        // TODO: JPB: (needed) Should ReportScriptedEventMB exist? Should everything be queued?
        public void ReportScriptedEventMB(string type, Dictionary<string, object> data = null) {
            DoMB(ReportScriptedEventHelper, type.ToNativeText(), TimeStamp(), data);
        }

        public void ReportScriptedEvent(string type, DateTime time, Dictionary<string, object> data = null) {
            Do(() => {
                ReportScriptedEventHelper(type.ToNativeText(), time, data);
            });
            //Do((type, time) => {
            //    ReportScriptedEventHelper(type, time, data);
            //}, type.ToNativeText(), time);
        }
        public void ReportScriptedEventMB(string type, DateTime time, Dictionary<string, object> data = null) {
            DoMB(ReportScriptedEventHelper, type.ToNativeText(), time, data);
        }

        protected void ReportScriptedEventHelper(NativeText type, DateTime time, Dictionary<string, object> data = null) {
            eventQueue.Enqueue(new DataPoint(type.ToString(), time, data));
            type.Dispose();
        }
    }
}