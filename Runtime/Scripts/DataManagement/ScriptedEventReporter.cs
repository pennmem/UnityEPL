using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace UnityEPL {

    [AddComponentMenu("UnityEPL/Reporters/Scripted Event Reporter")]
    public class EventReporter : DataReporter<EventReporter> {

        // TODO: JPB: (needed) (bug) Make ReportScriptedEvent use a blittable type instead of Dictionary
        //            Or at least have it use Mutex.
        //            Even better, just make DataPoint a Native type and then use that
        public void LogTS(string type, Dictionary<string, object> data = null) {
            var time = Clock.UtcNow;
            LogTS(type, time, data);
        }
        public void LogTS(string type, DateTime time, Dictionary<string, object> data = null) {
            DoTS(() => {
                LogHelper(type.ToNativeText(), time, data);
            });
            //DoTS((type, time) => {
            //    ReportHelper(type, time, data);
            //}, type.ToNativeText(), time);
            //DoTS<NativeText, BlitDateTime, Dictionary<string, object>>(ReportScriptedEventHelper, type.ToNativeText(), time, data);
        }

        protected void LogHelper(NativeText type, BlitDateTime time, Dictionary<string, object> data = null) {
            eventQueue.Enqueue(new DataPoint(type.ToString(), time, data));
            type.Dispose();
        }
    }
}