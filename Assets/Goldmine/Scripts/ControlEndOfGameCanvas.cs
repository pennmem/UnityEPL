using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEPL;

public class ControlEndOfGameCanvas : MonoBehaviour {
    public Text statDisplay; // large text in the middle of the screen
    protected InterfaceManager manager;
    private new AudioSource audio;

    void Awake() {
        audio = GetComponent<AudioSource>();
        manager = InterfaceManager.Instance;
    }

    public void SetStatDisplay(string msg) {
        manager.eventReporter.ReportScriptedEvent("canvasDisplay", new() { { "canvasName", "EndOfGameCanvas" }, { "canvasChildObjectName", "StatDisplay" }, { "textDisplayed", msg } });
        statDisplay.text = msg;
    }

    public void playAudio(bool play) {
        if (audio) {
            if (play) {
                audio.Play();
            } else {
                audio.Stop();
            }
        }
    }
}