using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEPL {

    public class ErrorNotifier : EventMonoBehaviour {
        //////////
        // Singleton Boilerplate
        // makes sure that only one Experiment Manager
        // can exist in a scene and that this object
        // is not destroyed when changing scenes
        //////////

        public static ErrorNotifier Instance { get; private set; } = null;

        protected override void AwakeOverride() {
            if (Instance != null) { //&& Instance != this) {
                throw new System.InvalidOperationException("Cannot create multiple ErrorNotification Objects");
            }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            gameObject.SetActive(false);
        }

        public static void Error(Exception e) {
            var go = Instance.gameObject;
            var manager = InterfaceManager.Instance;

            go.SetActive(true);
            var textDisplayer = go.GetComponent<TextDisplayer>();
            textDisplayer.Display("Error", "Error", e.Message);
            manager.eventReporter?.ReportScriptedEvent("Error",
                new Dictionary<string, object>{
                    { "message", e.Message },
                    { "stackTrace", e.StackTrace } });
            manager.Pause(true);
        }

        public void Warning(Exception e) {
            var go = Instance.gameObject;
            var manager = InterfaceManager.Instance;

            go.SetActive(true);
            var textDisplayer = go.GetComponent<TextDisplayer>();
            textDisplayer.Display("Warning", "Warning", e.Message);
            manager.eventReporter?.ReportScriptedEvent("Error",
                new Dictionary<string, object>{
                    { "message", e.Message },
                    { "stackTrace", e.StackTrace } });
            manager.Pause(true);
        }
    }
}