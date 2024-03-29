﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;
namespace UnityEPL {

    [AddComponentMenu("UnityEPL/Singleton Reporters/Event Reporter")]
    public class EventReporter : DataReporter2<EventReporter> {
        public void LogTS(string type, Dictionary<string, object> data = null) {
            manager.hostPC?.SendUncheckedLogMsgTS(type, data ?? new());
            var time = Clock.UtcNow;
            LogLocalTS(type, time, data);
        }
        public void LogTS(string type, DateTime time, Dictionary<string, object> data = null) {
            manager?.hostPC.SendUncheckedLogMsgTS(type, data ?? new());
            LogLocalTS(type, time, data);
        }

        // TODO: JPB: (needed) (bug) Make LogLocalTS use a blittable type instead of Dictionary
        //            Or at least have it use Mutex.
        //            Even better, just make DataPoint a Native type and then use that

        // Do not use this unless you don't want the message logged to the HostPC or any other location.
        public void LogLocalTS(string type, DateTime time, Dictionary<string, object> data = null) {
            DoTS(() => {
                LogHelper(type.ToNativeText(), time, data);
            });
            //DoTS((type, time) => {
            //    ReportHelper(type, time, data);
            //}, type.ToNativeText(), time);
            //DoTS<NativeText, BlitDateTime, Dictionary<string, object>>(ReportScriptedEventHelper, type.ToNativeText(), time, data);
        }

        protected void LogHelper(NativeText type, BlitDateTime time, Dictionary<string, object> data = null) {
            DoWrite(new DataPoint(type.ToString(), time, data));
            type.Dispose();
        }
    }

    public class DataReporter2<T> : SingletonEventMonoBehaviour<T> where T : DataReporter2<T> {
        public enum FORMAT { JSON_LINES };

        protected FORMAT outputFormat = FORMAT.JSON_LINES;
        protected string filePath = "";
        string extensionlessFileName = "session";
        protected bool experimentPathSetup = false;

        protected override void AwakeOverride() { }
        protected void Start() {
            string directory = manager.fileManager.DataPath();
            switch (outputFormat) {
                case FORMAT.JSON_LINES:
                    filePath = Path.Combine(directory, extensionlessFileName + ".jsonl");
                    break;
            }
        }

        protected void DoWrite(DataPoint dataPoint) {
            if (!experimentPathSetup) {
                try {
                    string directory = manager.fileManager.SessionPath();
                    switch (outputFormat) {
                        case FORMAT.JSON_LINES:
                            filePath = Path.Combine(directory, extensionlessFileName + ".jsonl");
                            break;
                    }
                    experimentPathSetup = true;
                } catch { }
            }

            string lineOutput = "unrecognized type";
            switch (outputFormat) {
                case FORMAT.JSON_LINES:
                    lineOutput = dataPoint.ToJSON();
                    break;
            }

            File.AppendAllText(filePath, lineOutput + Environment.NewLine);
        }
    }
}