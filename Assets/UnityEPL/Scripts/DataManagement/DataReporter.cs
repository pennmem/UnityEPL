using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

namespace UnityEPL {

    //this superclass implements an interface for retrieving behavioral events from a queue
    public abstract class DataReporter : MonoBehaviour {
        public string reportingID = "Object ID not set.";
        private static System.DateTime realWorldStartTime;
        private static System.Diagnostics.Stopwatch stopwatch;

        protected volatile static bool nativePluginRunning = false;
        private static bool startTimeInitialized = false;

        protected System.Collections.Concurrent.ConcurrentQueue<DataPoint> eventQueue = new ConcurrentQueue<DataPoint>();

        protected static double OSStartTime;
        private static float unityTimeStartTime;

        public DataHandler reportTo;

        protected bool IsMacOS() {
            return Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer;
        }

        void Awake() {
            // this can be set in the editor, change won't appear in code search
            if (reportingID == "Object ID not set.") {
                GenerateDefaultName();
            }

            if (!startTimeInitialized) {
                realWorldStartTime = System.DateTime.UtcNow;
                stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
                startTimeInitialized = true;
            }

            if (QualitySettings.vSyncCount == 0)
                Debug.LogWarning("vSync is off!  This will cause tearing, which will prevent meaningful reporting of frame-based time data.");
        }

        protected virtual void OnEnable() {
            if (!reportTo) {
                GameObject data = GameObject.Find("DataManager");
                if (data != null) {
                    reportTo = (DataHandler)data.GetComponent("DataHandler");
                }
            }

            if (reportTo) {
                reportTo.AddReporter(this);
            }
        }

        protected virtual void OnDisable() {
            if (reportTo) {
                eventQueue.Enqueue(new DataPoint(reportingID + "Disabled", TimeStamp(), new Dictionary<string, object>()));
                reportTo.RemoveReporter(this);
            }
        }

        /// <summary>
        /// The number of data points currently queued in this object.
        /// 
        /// Datapoints are dequeued when read. (Usually when  handled by a DataHandler.)
        /// </summary>
        /// <returns>The data point count.</returns>
        public int UnreadDataPointCount() {
            return eventQueue.Count;
        }

        /// <summary>
        /// If you want to be responsible yourself for handling data points, instead of letting DataHandlers handle them, you can call this.
        /// 
        /// Read datapoints will be dequeueud and not read by other users of this object.
        /// </summary>
        /// <returns>The data points.</returns>
        /// <param name="count">How many data points to read.</param>
        public DataPoint[] ReadDataPoints(int count) {
            if (eventQueue.Count < count) {
                throw new UnityException("Not enough data points!  Check UnreadDataPointCount first.");
            }

            DataPoint[] dataPoints = new DataPoint[count];
            for (int i = 0; i < count; i++) {
                bool dequeued = false;
                while (!dequeued) {
                    DataPoint readPoint;
                    dequeued = eventQueue.TryDequeue(out readPoint);
                    dataPoints[i] = readPoint;
                }
            }

            return dataPoints;
        }

        public void DoReport(System.Collections.Generic.Dictionary<string, object> extraData = null) {
            if (extraData == null)
                extraData = new System.Collections.Generic.Dictionary<string, object>();
            System.Collections.Generic.Dictionary<string, object> transformDict = new System.Collections.Generic.Dictionary<string, object>(extraData);
            transformDict.Add("positionX", transform.position.x);
            transformDict.Add("positionY", transform.position.y);
            transformDict.Add("positionZ", transform.position.z);
            transformDict.Add("rotationX", transform.rotation.eulerAngles.x);
            transformDict.Add("rotationY", transform.rotation.eulerAngles.y);
            transformDict.Add("rotationZ", transform.rotation.eulerAngles.z);
            transformDict.Add("object reporting id", reportingID);
            eventQueue.Enqueue(new DataPoint(gameObject.name + " transform", TimeStamp(), transformDict));
        }

        protected System.DateTime OSXTimestampToTimestamp(double OSXTimestamp) {
            double secondsSinceOSStart = OSXTimestamp - OSStartTime;
            return GetStartTime().AddSeconds(secondsSinceOSStart);
        }

        public static System.DateTime TimeStamp() {
            return GetStartTime().Add(stopwatch.Elapsed);
        }

        public static System.DateTime GetStartTime() {
            return realWorldStartTime;
        }

        private void GenerateDefaultName() {
            reportingID = this.name + System.Guid.NewGuid();
        }
    }

}