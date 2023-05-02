using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEPL {

    /// <summary>
    /// This handles the button which launches the experiment.
    /// 
    /// DoLaunchExperiment is responsible for calling EditableExperiment.ConfigureExperiment with the proper parameters.
    /// </summary>
    public class LaunchExperiment : EventMonoBehaviour {
        protected override void AwakeOverride() { }

        public GameObject cantGoPrompt;
        public InputField participantNameInput;
        public GameObject launchButton;

        public GameObject syncButton;
        public GameObject greyedLaunchButton;
        public GameObject loadingButton;

        void Update() {
            launchButton.SetActive(isValidParticipant(participantNameInput.text));
            greyedLaunchButton.SetActive(!launchButton.activeSelf);

            if (isValidParticipant(participantNameInput.text)) {
                int sessionNumber = ParticipantSelection.nextSessionNumber;
                launchButton.GetComponentInChildren<Text>().text = "Start session " + sessionNumber.ToString();
            }
        }

        public async void DoSyncBoxTestMB() {
            await DoWaitForMB(DoSyncBoxTestHelper);
        }
        protected async Task DoSyncBoxTestHelper() {
            if (!manager.syncBox?.IsRunning() ?? false) {
                syncButton.GetComponent<Button>().interactable = false;

                // TODO: JPB: (need) Fix Syncbox test
                manager.syncBox.StartPulse();
                await InterfaceManager.Delay(5000);
                //await InterfaceManager.Delay(Config.syncboxTestLength);
                manager.syncBox.StopPulse();

                syncButton.GetComponent<Button>().interactable = true;
            }
        }

        // activated by UI launch button
        public void DoLaunchExperimentMB() {
            DoMB(DoLaunchExperimentHelper);
        }
        protected void DoLaunchExperimentHelper() {
            if (manager.syncBox?.IsRunning() ?? false) {
                cantGoPrompt.GetComponent<Text>().text = "Can't start while Syncbox Test is running";
                cantGoPrompt.SetActive(true);
                return;
            }

            if (participantNameInput.text.Equals("")) {
                cantGoPrompt.GetComponent<Text>().text = "Please enter a participant";
                cantGoPrompt.SetActive(true);
                return;
            }
            if (!isValidParticipant(participantNameInput.text)) {
                cantGoPrompt.GetComponent<Text>().text = "Please enter a valid participant name (ex. R1123E or LTP123)";
                cantGoPrompt.SetActive(true);
                return;
            }

            int sessionNumber = ParticipantSelection.nextSessionNumber;

            Config.subject = participantNameInput.text;
            Config.session = sessionNumber;

            launchButton.SetActive(false);
            loadingButton.SetActive(true);

            manager.LaunchExperiment();
        }

        private bool isValidParticipant(string name) {
            return manager.fileManager.isValidParticipant(name);
        }
    }
}