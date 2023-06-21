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

            if (!IsInstatiated) {
                throw new Exception("THIS SHOULD NOT HAPPEN! ErrorNotifier was accessed before it's awake method has been called.", exception);
            }

            Instance.DoTS(() => { Instance.ErrorHelper(new Mutex<Exception>(exception)); });
            throw exception;
        }
        protected void ErrorHelper(Mutex<Exception> exception) {
            try {
                Exception e = exception.Get();
                // Only show first error on screen, but report all errors
                if (!gameObject.activeSelf) {
                    gameObject.SetActive(true);
                    var msg = e.Message == "" ? e.GetType().Name : e.Message;
                    TextDisplayer.Instance.Display("Error", "<color=red><b>Error</b></color>", msg);
                    Debug.Log($"Warning: {msg}\n{e.StackTrace}");
                }
                manager.eventReporter.LogTS("Error", new() {
                    { "message", e.Message },
                    { "stackTrace", e.StackTrace } });
                manager.PauseTS(true);
            } catch (Exception e) {
                Debug.Log("UNSAVEABLE ERROR IN ErrorHelper... Quitting...\n" + e);
                Debug.Assert(gameObject != null);
                Debug.Assert(manager != null);
                Debug.Assert(manager.eventReporter != null);
                manager.Quit();
            }
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
            TextDisplayer.Instance.Display("Warning", "<color=yellow><b>Warning</b></color>", message.ToString());
            Debug.Log($"Warning: {message}\n{stackTrace}");
            manager.eventReporter.LogTS("Warning", new() {
                { "message", message.ToString() },
                { "stackTrace", stackTrace.ToString() } });
            manager.PauseTS(true);
            message.Dispose();
            stackTrace.Dispose();
        }
    }
}