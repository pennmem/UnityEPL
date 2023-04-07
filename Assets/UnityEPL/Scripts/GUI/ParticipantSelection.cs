using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEPL {

    /// <summary>
    /// This is attached to the participant selection dropdown.  It is responsible for loading the information about the selected participant.
    /// 
    /// It also allows users to edit the loaded information with Increase/Decrease Session/List number buttons.
    /// </summary>
    public class ParticipantSelection : MonoBehaviour {
        InterfaceManager manager;
        public UnityEngine.UI.InputField participantNameInput;
        public UnityEngine.UI.Text sessionNumberText;
        public UnityEngine.UI.Text listNumberText;

        public static int nextSessionNumber = 0;
        public static int nextListNumber = 0;

        private bool experimentUpdated = false;
        // ^ updtaed by GUI event when experiment is
        // selected from dropdown menu

        void Awake() {
            GameObject mgr = GameObject.Find("InterfaceManager");
            manager = (InterfaceManager)mgr.GetComponent("InterfaceManager");
        }

        void Update() {
            // update participants when new experiments are loaded
            if (experimentUpdated && Config.experimentName != null) {
                experimentUpdated = false;
                FindParticipants();
            }
        }

        public void ExperimentUpdated() {
            experimentUpdated = true;
        }

        public void FindParticipants() {
            UnityEngine.UI.Dropdown dropdown = GetComponent<UnityEngine.UI.Dropdown>();

            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string>() { "Select participant", "New Participant" });

            string participantDirectory = manager.fileManager.ExperimentPath();
            if (Directory.Exists(participantDirectory)) {
                string[] filepaths = System.IO.Directory.GetDirectories(participantDirectory);
                List<string> filenames = new List<string>();

                for (int i = 0; i < filepaths.Length; i++)
                    if (manager.fileManager.isValidParticipant(System.IO.Path.GetFileName(filepaths[i])))
                        filenames.Add(System.IO.Path.GetFileName(filepaths[i]));

                dropdown.AddOptions(filenames);
            }
            dropdown.value = 0;
            dropdown.RefreshShownValue();

            nextSessionNumber = 0;
            nextListNumber = 0;
            UpdateTexts();
        }

        public void ParticipantSelected() {
            UnityEngine.UI.Dropdown dropdown = GetComponent<UnityEngine.UI.Dropdown>();
            if (dropdown.value <= 1) {
                participantNameInput.text = "New Participant";
            } else {
                LoadParticipant();
            }
        }

        public void LoadParticipant() {
            UnityEngine.UI.Dropdown dropdown = GetComponent<UnityEngine.UI.Dropdown>();
            string selectedParticipant = dropdown.captionText.text;

            if (!System.IO.Directory.Exists(manager.fileManager.ParticipantPath(selectedParticipant)))
                throw new UnityException("You tried to load a participant that doesn't exist.");

            participantNameInput.text = selectedParticipant;

            nextSessionNumber = manager.fileManager.CurrentSession(selectedParticipant);

            UpdateTexts();
        }

        public void LoadSession() {
            UnityEngine.UI.Dropdown dropdown = GetComponent<UnityEngine.UI.Dropdown>();
            string selectedParticipant = dropdown.captionText.text;
            string sessionFilePath = manager.fileManager.SessionPath(selectedParticipant, nextSessionNumber);
            UpdateTexts();
        }

        public void DecreaseSessionNumber() {
            if (nextSessionNumber > 0)
                nextSessionNumber--;
            LoadSession();
        }

        public void IncreaseSessionNumber() {
            nextSessionNumber++;
            LoadSession();
        }

        public void UpdateTexts() {
            sessionNumberText.text = nextSessionNumber.ToString();
        }
    }

}