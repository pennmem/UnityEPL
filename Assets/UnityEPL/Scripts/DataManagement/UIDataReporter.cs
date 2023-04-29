using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace UnityEPL {

    [AddComponentMenu("UnityEPL/Reporters/UI Data Reporter")]
    public class UIDataReporter : DataReporter {
        /// <summary>
        /// This can be subscribed to Unity UI buttons, etc.
        /// </summary>
        /// <param name="name">Name of the event to log.</param>
        public void LogUIEventMB(string name) {
            DoMB(LogUIEventHelper, name.ToNativeText());
        }
        protected void LogUIEventHelper(NativeText name) {
            eventQueue.Enqueue(new DataPoint(name.ToString(), TimeStamp(), new Dictionary<string, object>()));
            name.Dispose();
        }
    }
}