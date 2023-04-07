using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityEPL {

    [AddComponentMenu("UnityEPL/Reporters/Input Reporter")]
    public class InputReporter : DataReporter {
        public bool reportKeyStrokes = true;
        public bool reportMouseClicks = false;
        public bool reportMousePosition = false;
        public int framesPerMousePositionReport = 60;
        private Dictionary<int, bool> keyDownStates = new Dictionary<int, bool>();
        private Dictionary<int, bool> mouseDownStates = new Dictionary<int, bool>();

        private int lastMousePositionReportFrame;

        void Update() {
            if (reportKeyStrokes)
                CollectKeyEvents();
            if (reportMousePosition && Time.frameCount - lastMousePositionReportFrame > framesPerMousePositionReport)
                CollectMousePosition();
        }

        /// <summary>
        /// Collects the key events.  Except in MacOS, this includes mouse events, which are part of Unity's KeyCode enum.
        /// 
        /// On MacOS, UnityEPL uses a native plugin to achieve higher accuracy timestamping.
        /// </summary>

        private void CollectKeyEvents() {
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode))) {
                if (Input.GetKeyDown(keyCode)) {
                    ReportKey((int)keyCode, true, DataReporter.TimeStamp());
                }
                if (Input.GetKeyUp(keyCode)) {
                    ReportKey((int)keyCode, false, DataReporter.TimeStamp());
                }
            }
        }

        private void ReportKey(int keyCode, bool pressed, System.DateTime timestamp) {
            Dictionary<string, object> dataDict = new Dictionary<string, object>();
            var key = (Enum.GetName(typeof(KeyCode), keyCode) ?? "none").ToLower();
            dataDict.Add("key code", key);
            dataDict.Add("is pressed", pressed);
            var label = "key/mouse press/release";
            eventQueue.Enqueue(new DataPoint(label, timestamp, dataDict));
        }

        private void CollectMousePosition() {
            Dictionary<string, object> dataDict = new Dictionary<string, object>();
            dataDict.Add("position", Input.mousePosition);
            eventQueue.Enqueue(new DataPoint("mouse position", DataReporter.TimeStamp(), dataDict));
            lastMousePositionReportFrame = Time.frameCount;
        }
    }

}