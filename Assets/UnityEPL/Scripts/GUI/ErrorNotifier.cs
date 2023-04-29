using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace UnityEPL {

    public class ErrorNotifier : SingletonEventMonoBehaviour<ErrorNotifier> {
        protected override void AwakeOverride() {
            gameObject.SetActive(false);
        }

        public static void Error(Exception exception) {
            if (exception.StackTrace == null) {
                try { // This is used to get the stack trace
                    throw exception;
                } catch (Exception e) {
                    exception = e;
                }
            }

            Instance.Do(() => { Instance.ErrorHelper(new Mutex<Exception>(exception)); });
            throw exception;
        }
        protected void ErrorHelper(Mutex<Exception> exception) {
            Exception e = exception.Get();
            gameObject.SetActive(true);
            var textDisplayer = gameObject.GetComponent<TextDisplayer>();
            textDisplayer.DisplayMB("Error", "Error", e.Message);
            manager.eventReporter.ReportScriptedEvent("Error",
                new Dictionary<string, object>{
                    { "message", e.Message },
                    { "stackTrace", e.StackTrace } });
            manager.Pause(true);
            throw e;
        }

        public static void Warning(Exception exception) {
            if (exception.StackTrace == null) {
                try { // This is used to get the stack trace
                    throw exception;
                } catch (Exception e) {
                    exception = e;
                }
            }

            Instance.Do(Instance.WarningHelper, exception.Message.ToNativeText(), exception.StackTrace.ToNativeText());
        }
        protected void WarningHelper(NativeText message, NativeText stackTrace) {
            gameObject.SetActive(true);
            var textDisplayer = gameObject.GetComponent<TextDisplayer>();
            textDisplayer.DisplayMB("Warning", "Warning", message.ToString());
            Debug.Log($"Warning: {message}\n{stackTrace}");
            manager.eventReporter.ReportScriptedEvent("Warning",
                new Dictionary<string, object>{
                    { "message", message.ToString() },
                    { "stackTrace", stackTrace.ToString() } });
            manager.Pause(true);
            message.Dispose();
            stackTrace.Dispose();
        }
    }
}