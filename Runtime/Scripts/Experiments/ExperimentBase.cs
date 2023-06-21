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
        protected EventReporter eventReporter;

        public ExperimentBase() {
            this.inputManager = InputManager.Instance;
            this.textDisplayer = TextDisplayer.Instance;
            this.errorNotifier = ErrorNotifier.Instance;
            this.eventReporter = EventReporter.Instance;
        }

        private bool endTrials = false;
        private bool endPracticeTrials = false;
        protected uint trialNum { get; private set; } = 0;
        protected uint practiceTrialNum { get; private set; } = 0;

        protected abstract Task PreTrialStates();
        protected abstract Task PracticeTrialStates();
        protected abstract Task TrialStates();
        protected abstract Task PostTrialStates();

        protected void EndTrials() {
            endTrials = true;
        }
        protected void EndPracticeTrials() {
            endPracticeTrials = true;
        }

        protected async void Run() {
            await DoWaitForTS(RunHelper);
        }
        protected async Task RunHelper() {
            await PreTrialStates();
            while (!endPracticeTrials) {
                practiceTrialNum++;
                await PracticeTrialStates();
            }
            while (!endTrials) {
                trialNum++;
                await TrialStates();
            }
            await PostTrialStates();
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

            manager.eventReporter.LogTS("session start", versionsData);
        }

        protected async Task RepeatOnRequest(Func<Task> func, string description, string displayText) {
            var repeat = true;
            while (repeat) {
                await func();

                textDisplayer.Display(description, "", displayText);
                var keyCode = await inputManager.GetKeyTS(new() { KeyCode.Y, KeyCode.N });
                repeat = keyCode == KeyCode.Y;
            }
        }
    }

}