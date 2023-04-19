using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEPL {

    public class ErrorNotifier : SingletonEventMonoBehaviour<ErrorNotifier> {
        protected override void AwakeOverride() { }

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