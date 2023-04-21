using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEPL {

    public class ErrorNotifier : SingletonEventMonoBehaviour<ErrorNotifier> {
        protected override void AwakeOverride() { }

        public static void Error<T>(T exception) where T : Exception {
            if (exception.StackTrace == null) {
                try { // This is used to get the stack trace
                    throw exception;
                } catch (T e) {
                    exception = e;
                }
            }

            Instance.Do<StackString, StackString>(Instance.ErrorHelper, exception.Message, exception.StackTrace);
            throw exception;
        }
        protected void ErrorHelper(StackString message, StackString stackTrace) {
            gameObject.SetActive(true);
            var textDisplayer = gameObject.GetComponent<TextDisplayer>();
            textDisplayer.Display("Error", "Error", message);
            manager.eventReporter.ReportScriptedEvent("Error",
                new Dictionary<string, object>{
                    { "message", message },
                    { "stackTrace", stackTrace } });
            manager.Pause(true);
        }

        public static void Warning<T>(T exception) where T : Exception {
            if (exception.StackTrace == null) {
                try { // This is used to get the stack trace
                    throw exception;
                } catch (T e) {
                    exception = e;
                }
            }

            Instance.Do<StackString, StackString>(Instance.ErrorHelper, exception.Message, exception.StackTrace);
        }
        protected void WarningHelper(Exception e) {
            gameObject.SetActive(true);
            var textDisplayer = gameObject.GetComponent<TextDisplayer>();
            textDisplayer.Display("Warning", "Warning", e.Message);
            manager.eventReporter.ReportScriptedEvent("Warning",
                new Dictionary<string, object>{
                    { "message", e.Message },
                    { "stackTrace", e.StackTrace } });
            manager.Pause(true);
        }
    }
}