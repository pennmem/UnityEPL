using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityEPL {

    public class InputReporter : SingletonEventMonoBehaviour<InputReporter> {
        protected override void AwakeOverride() { }

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
        /// Collects the key events.  This includes mouse events, which are part of Unity's KeyCode enum.
        /// </summary>

        private void CollectKeyEvents() {
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode))) {
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
            manager.eventReporter.LogTS(label, dataDict);
        }

        private void CollectMousePosition() {
            Dictionary<string, object> dataDict = new() {
                { "position", Input.mousePosition },
            };
            manager.eventReporter.LogTS("mouse position", dataDict);
            lastMousePositionReportFrame = Time.frameCount;
        }
    }

}