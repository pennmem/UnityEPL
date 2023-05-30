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
        private Dictionary<int, bool> keyDownStates = new();
        private Dictionary<int, bool> mouseDownStates = new();

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
                    ReportKey((int)keyCode, true);
                }
                if (Input.GetKeyUp(keyCode)) {
                    ReportKey((int)keyCode, false);
                }
            }
        }

        private void ReportKey(int keyCode, bool pressed) {
            var key = (Enum.GetName(typeof(KeyCode), keyCode) ?? "none").ToLower();
            Dictionary<string, object> dataDict = new() {
                { "key code", key },
                { "is pressed", pressed },
            };
            var label = "key/mouse press/release";
            eventQueue.Enqueue(new DataPoint(label, dataDict));
        }

        private void CollectMousePosition() {
            Dictionary<string, object> dataDict = new() {
                { "position", Input.mousePosition },
            };
            eventQueue.Enqueue(new DataPoint("mouse position", dataDict));
            lastMousePositionReportFrame = Time.frameCount;
        }
    }

}