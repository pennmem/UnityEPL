using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEPL {

    [AddComponentMenu("UnityEPL/Reporters/UI Data Reporter")]
    public class UIDataReporter : DataReporter {
        /// <summary>
        /// This can be subscribed to Unity UI buttons, etc.
        /// </summary>
        /// <param name="name">Name of the event to log.</param>
        public void LogUIEventMB(string name) {
            DoMB<StackString>(LogUIEventHelper, name);
        }
        protected void LogUIEventHelper(StackString name) {
            eventQueue.Enqueue(new DataPoint(name, TimeStamp(), new Dictionary<string, object>()));
        }
    }
}