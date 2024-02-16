using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace UnityEPL {

    [AddComponentMenu("UnityEPL/Reporters/UI Data Reporter")]
    public class UIDataReporter : DataReporter<UIDataReporter> {
        /// <summary>
        /// This can be subscribed to Unity UI buttons, etc.
        /// </summary>
        /// <param name="name">Name of the event to log.</param>
        public void LogEventTS(string name) {
            DoTS(LogEventHelper, name.ToNativeText());
        }
        protected void LogEventHelper(NativeText name) {
            eventQueue.Enqueue(new DataPoint(name.ToString()));
            name.Dispose();
        }
    }
}