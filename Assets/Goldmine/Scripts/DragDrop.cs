using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEPL;

public class DragDrop : MonoBehaviour {
    public new Camera camera;

    protected Vector3 originalPos;
    protected Vector3 lastMousePos;
    protected CanvasGroup canvasGroup;
    protected Collider collidedItem = null;
    protected InterfaceManager manager;
    protected ControlTimeline controlTimeline;

    void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
        manager = InterfaceManager.Instance;

        GameObject timelineCanvas = GameObject.Find("TimelineCanvas");
        controlTimeline = timelineCanvas.GetComponentInChildren<ControlTimeline>();
    }

    void Start() {
        var boxCollider = transform.GetComponent<BoxCollider>();
        boxCollider.enabled = true;
        boxCollider.isTrigger = true;

        originalPos = transform.position;
    }

    private void OnMouseDown() {
        lastMousePos = camera.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseDrag() {
        Vector3 currMousePos = camera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouseDelta = currMousePos - lastMousePos;
        transform.position += new Vector3(mouseDelta.x, mouseDelta.y, 0f);
        lastMousePos = currMousePos;
    }

    private void OnMouseUp() {
        if (collidedItem != null) {
            collidedItem.GetComponent<ControlTimeline>().SetItemPositionOnTimeline(transform);
        } else {
            transform.position = originalPos;
        }
        manager.eventReporter.ReportScriptedEvent("timelineItemMoved", new() {
            { "name", transform.name },
            { "chosenTime", controlTimeline.GetItemTime(transform) } });
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.GetComponent<ControlTimeline>() != null) {
            collidedItem = other;
        }
    }

    private void OnTriggerExit(Collider other) {
        collidedItem = null;
    }
}
