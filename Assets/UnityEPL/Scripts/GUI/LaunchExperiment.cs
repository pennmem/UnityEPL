using System;
using System.Collections;
using UnityEngine;

namespace UnityEPL {

    /// <summary>
    /// This handles the button which launches the experiment.
    /// 
    /// DoLaunchExperiment is responsible for calling EditableExperiment.ConfigureExperiment with the proper parameters.
    /// </summary>
    public class LaunchExperiment : EventMonoBehaviour {
        protected override void StartOverride() { }

        public GameObject cantGoPrompt;
        public UnityEngine.UI.InputField participantNameInput;
        public UnityEngine.GameObject launchButton;

        public UnityEngine.GameObject syncButton;
        public UnityEngine.GameObject greyedLaunchButton;
        public UnityEngine.GameObject loadingButton;

        void Update() {
            launchButton.SetActive(isValidParticipant(participantNameInput.text));
            greyedLaunchButton.SetActive(!launchButton.activeSelf);

            if (isValidParticipant(participantNameInput.text)) {
                int sessionNumber = ParticipantSelection.nextSessionNumber;
                launchButton.GetComponentInChildren<UnityEngine.UI.Text>().text = "Start session " + sessionNumber.ToString();
            }
        }

        public async void DoSyncBoxTest() {
            if (!manager.syncBox?.IsRunning() ?? false) {
                syncButton.GetComponent<UnityEngine.UI.Button>().interactable = false;

                // TODO: JPB: (need) Fix Syncbox test
                manager.syncBox.StartPulse();
                await InterfaceManager.Delay(5000);
                //await InterfaceManager.Delay(Config.syncboxTestLength);
                manager.syncBox.StopPulse();

                syncButton.GetComponent<UnityEngine.UI.Button>().interactable = true;
            }
        }

        // activated by UI launch button
        public void DoLaunchExperiment() {
            if (manager.syncBox?.IsRunning() ?? false) {
                cantGoPrompt.GetComponent<UnityEngine.UI.Text>().text = "Can't start while Syncbox Test is running";
                cantGoPrompt.SetActive(true);
                return;
            }

            if (participantNameInput.text.Equals("")) {
                cantGoPrompt.GetComponent<UnityEngine.UI.Text>().text = "Please enter a participant";
                cantGoPrompt.SetActive(true);
                return;
            }
            if (!isValidParticipant(participantNameInput.text)) {
                cantGoPrompt.GetComponent<UnityEngine.UI.Text>().text = "Please enter a valid participant name (ex. R1123E or LTP123)";
                cantGoPrompt.SetActive(true);
                return;
            }

            int sessionNumber = ParticipantSelection.nextSessionNumber;

            Config.participantCode = participantNameInput.text;
            Config.session = sessionNumber;

            launchButton.SetActive(false);
            loadingButton.SetActive(true);

            // TODO: JPB: (needed) Should LaunchExperiment be in a Do
            manager.LaunchExperiment();
        }

        private bool isValidParticipant(string name) {
            return manager.fileManager.isValidParticipant(name);
        }
    }
}