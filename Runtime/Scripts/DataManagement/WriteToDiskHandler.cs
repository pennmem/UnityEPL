using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityEPL {

    [AddComponentMenu("UnityEPL/Handlers/Write to Disk Handler")]
    public class WriteToDiskHandler : DataHandler {
        protected override void AwakeOverride() { }

        //more output formats may be added in the future
        public enum FORMAT { JSON_LINES };
        protected FORMAT outputFormat = FORMAT.JSON_LINES;

        [HideInInspector]
        [SerializeField]
        private bool writeAutomatically = true;
        [HideInInspector]
        [SerializeField]
        private uint framesPerWrite = 30;

        private Queue<DataPoint> waitingPoints = new Queue<DataPoint>();

        public void SetFormat(FORMAT format) {
            Do(SetFormatHelper, format);
        }
        public void SetFormatMB(FORMAT format) {
            DoMB(SetFormatHelper, format);
        }
        protected void SetFormatHelper(FORMAT format) {
            outputFormat = format;
        }

        public void SetWriteAutomatically(Bool enable) {
            Do(SetWriteAutomaticallyHelper, enable);
        }
        public void SetWriteAutomaticallyMB(Bool enable) {
            DoMB(SetWriteAutomaticallyHelper, enable);
        }
        protected void SetWriteAutomaticallyHelper(Bool enable) {
            writeAutomatically = enable;
        }

        public async Task<bool> GetWriteAutomatically() {
            return await DoGet(GetWriteAutomaticallyHelper);
        }
        public bool GetWriteAutomaticallyMB() {
            return DoGetMB(GetWriteAutomaticallyHelper);
        }
        protected Bool GetWriteAutomaticallyHelper() {
            return writeAutomatically;
        }

        public void SetFramesPerWrite(uint framesPerWrite) {
            Do(SetFramesPerWriteHelper, framesPerWrite);
        }
        public void SetFramesPerWriteMB(uint framesPerWrite) {
            DoMB(SetFramesPerWriteHelper, framesPerWrite);
        }
        protected void SetFramesPerWriteHelper(uint framesPerWrite) {
            this.framesPerWrite = framesPerWrite;
        }

        public async Task<uint> GetFramesPerWrite() {
            return await DoGet(GetFramesPerWriteHelper);
        }
        public uint GetFramesPerWriteMB() {
            return DoGetMB(GetFramesPerWriteHelper);
        }
        protected uint GetFramesPerWriteHelper() {
            return framesPerWrite;
        }

        protected override void Update() {
            base.Update();

            if (Time.frameCount % framesPerWrite == 0)
                DoWrite();
        }

        protected override void HandleDataPoints(DataPoint[] dataPoints) {
            foreach (DataPoint dataPoint in dataPoints)
                waitingPoints.Enqueue(dataPoint);
        }

        /// <summary>
        /// Writes data from the waitingPoints queue to disk.  The waitingPoints queue will be automatically updated whenever reporters report data.
        /// 
        /// DoWrite() will also be automatically be called periodically according to the settings in the component inspector window, but you can invoke this manually if desired.
        /// </summary>
        protected void DoWrite() {
            while (waitingPoints.Count > 0) {
                string directory = manager.fileManager.SessionPath();
                if (directory == null) {
                    return;
                }
                Directory.CreateDirectory(directory);
                string filePath = Path.Combine(directory, "unnamed_file");

                DataPoint dataPoint = waitingPoints.Dequeue();
                string writeMe = "unrecognized type";
                string extensionlessFileName = "session";//DataReporter.GetStartTime ().ToString("yyyy-MM-dd HH mm ss");
                switch (outputFormat) {
                    case FORMAT.JSON_LINES:
                        writeMe = dataPoint.ToJSON();
                        filePath = Path.Combine(directory, extensionlessFileName + ".jsonl");
                        break;
                }
                File.AppendAllText(filePath, writeMe + System.Environment.NewLine);
            }
        }
    }

}