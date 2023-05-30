using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEPL;

public class StartGame : MonoBehaviour {
    public AudioClip titleSong;
    private InterfaceManager manager;

    public Dropdown sceneSelection;
    public InputField participantCode;
    public InputField session;
    public GameObject confirmationCanvas;
    public GameObject startCanvas;


    public void Awake() {
        Debug.Log("Waking up");
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;

        manager = InterfaceManager.Instance;

        string[] scenes = Config.availableScenes;

        sceneSelection.AddOptions(new List<string>(scenes));
        SetScene();
    }

    public void Start() {
        ShowConfirmation();
    }

    public void LoadParticipant() {
        Dropdown dropdown = GetComponent<Dropdown>();
        string selectedParticipant = participantCode.text;

        participantCode.text = selectedParticipant;

        int nextSessionNumber = manager.fileManager.CurrentSession(selectedParticipant);

        session.text = nextSessionNumber.ToString();
    }

    public void SetScene() {
        string value = sceneSelection.options[sceneSelection.value].text;
        // TODO: JPB: (needed) (goldmine) SetScene should go away
        //            The tutorial should be handled as a different game?
        //            LoadTutorial and LoadExperiment
        //Config.experimentScene = value;
    }

    public bool SetParticipantData() {
        // Get participant code
        // get session number
        int sessionNum;
        if (manager.fileManager.isValidParticipant(participantCode.text) && int.TryParse(session.text, out sessionNum)) {

            Config.subject = participantCode.text;
            Config.session = Convert.ToInt32(session.text);

            return true;
        }
        return false;
    }

    public void LoadTutorial() {
        if (SetParticipantData()) {
            //manager.ChangeSetting("sceneToLaunch", (string)manager.GetSetting("tutorialScene"));

            manager.LaunchExperimentTS();
            //ShowConfirmation();
        } else {
            ErrorNotifier.Warning(new InvalidOperationException("Please set participant code and session"));
            //manager.Do(new EventBase<string, int>(manager.ShowWarning, "Please set participant code and session", 5000));
        }
    }

    public void LoadExperiment() {
        if (SetParticipantData()) {
            //manager.ChangeSetting("sceneToLaunch", Config.experimentScene);
            //ShowConfirmation();
            manager.LaunchExperimentTS();
        } else {
            ErrorNotifier.Warning(new InvalidOperationException("Please set participant code and session"));
            //manager.Do(new EventBase<string, int>(manager.ShowWarning, "Please set participant code and session", 5000));
        }
    }

    private void ShowConfirmation() {
        startCanvas.SetActive(false);
        confirmationCanvas.SetActive(true);
    }

    private void ShowStart() {
        if (titleSong) {
            AudioSource.PlayClipAtPoint(titleSong, gameObject.transform.position, 1f);
        }

        startCanvas.SetActive(true);
        confirmationCanvas.SetActive(false);
    }

    public void Quit() {
        manager.QuitTS();
    }

    public void Continue() {
        ShowStart();
    }
}
