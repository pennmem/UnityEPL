using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEPL;

public class ControlTutorialCanvas : MonoBehaviour {
    public Text centralDisplay; // large text in the middle of the screen
    protected InterfaceManager manager;

    void Awake() {
        manager = InterfaceManager.Instance;
    }

    public void SetCentralDisplay(string msg) {
        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "TutorialCanvas" }, { "canvasChildObjectName", "CentralDisplay" }, { "textDisplayed", msg } });
        centralDisplay.text = msg;
    }

    public void ResetCentralDisplay() {
        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "TutorialCanvas" }, { "canvasChildObjectName", "CentralDisplay" }, { "textDisplayed", "" } });
        centralDisplay.text = "";
    }
}
