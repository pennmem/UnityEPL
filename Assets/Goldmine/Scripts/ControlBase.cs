using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEPL;

public class ControlBase : MonoBehaviour {
    public GameObject[] openDoors; // order should be right, left, middle
    public GameObject[] closedDoors; // order should be right, left, middle
    public AudioClip doorOpenSFX;
    public GameObject mainCanvas; // the main canvas on which text is displayed
    public bool allDoorsOpen;
    protected InterfaceManager manager;
    private ControlCanvas controlMainCanvas;

    void Awake() {
        controlMainCanvas = mainCanvas.GetComponent<ControlCanvas>();
        manager = InterfaceManager.Instance;
    }

    public void OpenDoors(bool[] iDoors, bool playSFX = false, bool showArrow = false) {
        int nDoorsOpen = 0;

        // Log
        manager.eventReporter.ReportScriptedEvent("baseDoorsOpen", new() { { "rightDoor", iDoors[0] }, { "leftDoor", iDoors[1] }, { "middleDoor", iDoors[2] } });

        // Open or close each door in turn
        for (int iDoor = 0; iDoor < iDoors.Length; iDoor++) {
            openDoors[iDoor].SetActive(iDoors[iDoor]);
            closedDoors[iDoor].SetActive(!iDoors[iDoor]);

            if (iDoors[iDoor]) {
                nDoorsOpen++;
            }

            // Play door opening sound
            if ((playSFX) & (doorOpenSFX != null)) {
                if (iDoors[iDoor]) {
                    AudioSource.PlayClipAtPoint(doorOpenSFX, openDoors[iDoor].transform.position, 0.4f);
                }
            }
        }

        // Keep track of how many doors are open
        if (nDoorsOpen == openDoors.Length) {
            allDoorsOpen = true;
        } else {
            allDoorsOpen = false;
        }

        // Determine if we should show a guidance arrow
        if (showArrow) {
            if ((iDoors[0]) & (!iDoors[1]) & (!iDoors[2])) {
                controlMainCanvas.ShowRightArrow(0.75f);
            } else if ((iDoors[1]) & (!iDoors[0]) & (!iDoors[2])) {
                controlMainCanvas.ShowLeftArrow(0.75f);
            }
        }
    }
}
