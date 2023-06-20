using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityEPL {

    public struct KeyMsg {
        public string key;
        public bool down;

        public KeyMsg(string key, bool down) {
            this.key = key;
            this.down = down;
        }
    }

    public abstract class ExperimentBase<T> : SingletonEventMonoBehaviour<T>
            where T : ExperimentBase<T> {
        TaskCompletionSource<KeyMsg> tcs = new TaskCompletionSource<KeyMsg>();

        protected InputManager inputManager;
        protected TextDisplayer textDisplayer;
        protected ErrorNotifier errorNotifier;
        protected ScriptedEventReporter scriptedEventReporter;

        public ExperimentBase() {
            this.inputManager = InputManager.Instance;
            this.textDisplayer = TextDisplayer.Instance;
            this.errorNotifier = ErrorNotifier.Instance;
            this.scriptedEventReporter = ScriptedEventReporter.Instance;
        }

        private bool endTrials = false;
        protected uint trialNum { get; private set; } = 0;

        protected abstract Task PreTrials();
        protected abstract Task TrialStates();
        protected abstract Task PostTrials();

        protected void EndTrials() {
            endTrials = true;
        }

        protected async void Run() {
            await DoWaitForTS(RunHelper);
        }

        protected async Task RunHelper() {
            await PreTrials();
            while (!endTrials) {
                trialNum++;
                await TrialStates();
            }
            await PostTrials();
            manager.QuitTS();
        }

        protected void LogExperimentInfo() {
            //write versions to logfile
            Dictionary<string, object> versionsData = new() {
                { "application version", Application.version },
                { "build date", BuildInfo.ToString() }, // compiler magic, gives compile date
                { "experiment version", Config.experimentName },
                { "logfile version", "0" },
                { "participant", Config.subject },
                { "session", Config.session },
            };

            manager.eventReporter.ReportTS("session start", versionsData);
        }
    }

}