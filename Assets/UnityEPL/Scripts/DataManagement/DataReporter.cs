using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace UnityEPL {

    //this superclass implements an interface for retrieving behavioral events from a queue
    // TODO: JPB: (needed) (bug) Refactor DataReporter to use Clock class for timing accuracy
    public abstract class DataReporter : EventMonoBehaviour {
        public string reportingID = "Object ID not set.";
        private static DateTime realWorldStartTime;
        private static Stopwatch stopwatch;

        protected volatile static bool nativePluginRunning = false;
        private static bool startTimeInitialized = false;

        protected Queue<DataPoint> eventQueue = new();

        public DataHandler reportTo;

        protected override void AwakeOverride() {
            // this can be set in the editor, change won't appear in code search
            if (reportingID == "Object ID not set.") {
                GenerateDefaultName();
            }

            if (!startTimeInitialized) {
                realWorldStartTime = DateTime.UtcNow;
                stopwatch = new();
                stopwatch.Start();
                startTimeInitialized = true;
            }

            if (QualitySettings.vSyncCount == 0) {
                UnityEngine.Debug.LogWarning("vSync is off!  This will cause tearing, which will prevent meaningful reporting of frame-based time data.");
            }
        }

        protected void OnEnable() {
            if (!reportTo) {
                GameObject data = GameObject.Find("DataManager");
                if (data != null) {
                    reportTo = (DataHandler)data.GetComponent("DataHandler");
                }
            }

            if (reportTo) {
                // TODO: JPB: (needed) (bug) Figure out why DataReporter::OnEnable crashes when I call AddReporterMB
                //reportTo.AddReporterMB(this);
            }
        }

        protected void OnDisable() {
            if (reportTo) {
                eventQueue.Enqueue(new DataPoint(reportingID + "Disabled", TimeStamp(), new Dictionary<string, object>()));
                reportTo.RemoveReporterMB(this);
            }
        }

        /// <summary>
        /// The number of data points currently queued in this object.
        /// 
        /// Datapoints are dequeued when read. (Usually when  handled by a DataHandler.)
        /// </summary>
        /// <returns>The data point count.</returns>
        public int UnreadDataPointCountMB() {
            return DoGetMB(UnreadDataPointCountHelper);
        }
        protected int UnreadDataPointCountHelper() {
            return eventQueue.Count;
        }

        /// <summary>
        /// If you want to be responsible yourself for handling data points, instead of letting DataHandlers handle them, you can call this.
        /// 
        /// Read datapoints will be dequeueud and not read by other users of this object.
        /// </summary>
        /// <returns>The data points.</returns>
        /// <param name="count">How many data points to read.</param>
        public DataPoint[] ReadDataPointsMB(int count) {
            return DoGetMB(ReadDataPointsHelper, count);
        }
        protected DataPoint[] ReadDataPointsHelper(int count) {
            if (eventQueue.Count < count) {
                ErrorNotifier.Error(
                    new UnityException("Not enough data points! Check UnreadDataPointCount first."));
            }

            DataPoint[] dataPoints = new DataPoint[count];
            for (int i = 0; i < count; i++) {
                bool dequeued = false;
                while (!dequeued) {
                    dequeued = eventQueue.TryDequeue(out DataPoint readPoint);
                    dataPoints[i] = readPoint;
                }
            }

            return dataPoints;
        }

        public void DoReport(Dictionary<string, object> extraData = null) {
            DoMB(DoReportHelper, extraData);
        }
        protected void DoReportHelper(Dictionary<string, object> extraData = null) {
            var transformDict = new Dictionary<string, object>(extraData) ?? new();
            transformDict.Add("positionX", transform.position.x);
            transformDict.Add("positionY", transform.position.y);
            transformDict.Add("positionZ", transform.position.z);
            transformDict.Add("rotationX", transform.rotation.eulerAngles.x);
            transformDict.Add("rotationY", transform.rotation.eulerAngles.y);
            transformDict.Add("rotationZ", transform.rotation.eulerAngles.z);
            transformDict.Add("object reporting id", reportingID);
            eventQueue.Enqueue(new DataPoint(gameObject.name + " transform", TimeStamp(), transformDict));
        }

        // TODO: JPB: (needed) (feature) Convert DataReporter::TimeStamp to use Clock class
        public static DateTime TimeStamp() {
            return GetStartTime().Add(stopwatch.Elapsed);
        }

        public static DateTime GetStartTime() {
            return realWorldStartTime;
        }

        private void GenerateDefaultName() {
            reportingID = this.name + Guid.NewGuid();
        }
    }

}