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

            Instance.DoTS(() => { Instance.ErrorHelper(new Mutex<Exception>(exception)); });
            throw exception;
        }
        protected void ErrorHelper(Mutex<Exception> exception) {
            Exception e = exception.Get();
            // Only show first error on screen, but report all errors
            if (!gameObject.activeSelf) { 
                gameObject.SetActive(true);
                var textDisplayer = gameObject.GetComponent<TextDisplayer>();
                var msg = e.Message == "" ? e.GetType().Name : e.Message;
                textDisplayer.Display("Error", "Error", msg);
            }
            manager.eventReporter.ReportTS("Error", new() {
                { "message", e.Message },
                { "stackTrace", e.StackTrace } });
            manager.PauseTS(true);
        }

        public static void Warning(Exception exception) {
            if (exception.StackTrace == null) {
                try { // This is used to get the stack trace
                    throw exception;
                } catch (Exception e) {
                    exception = e;
                }
            }

            Instance.DoTS(Instance.WarningHelper, exception.Message.ToNativeText(), exception.StackTrace.ToNativeText());
        }
        protected void WarningHelper(NativeText message, NativeText stackTrace) {
            gameObject.SetActive(true);
            var textDisplayer = gameObject.GetComponent<TextDisplayer>();
            textDisplayer.Display("Warning", "Warning", message.ToString());
            Debug.Log($"Warning: {message}\n{stackTrace}");
            manager.eventReporter.ReportTS("Warning", new() {
                { "message", message.ToString() },
                { "stackTrace", stackTrace.ToString() } });
            manager.PauseTS(true);
            message.Dispose();
            stackTrace.Dispose();
        }
    }
}