using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEPL;

// Respawn an object at the chosen spawn point
public class ControlPlayer : MonoBehaviour {
    public GameObject spawnPoint;
    public float baseXmin;
    public float baseXmax;
    public float baseZmin;
    public float baseZmax;
    public bool playerAtBase;
    public bool playerInMine;
    public int framesPerTransformReport = 3; // Rate at which player position gets written to the logfile
    protected InterfaceManager Manager;
    private Vector3 respawnPosition;
    private Quaternion respawnRotation;
    private Controller controller;
    private MouseLooker mouseLooker;
    private bool paused = false;
    private bool frozen = false;
    private float baseXminn;
    private float baseXmaxx;
    private float baseZminn;
    private float baseZmaxx;

    void Awake() {
        controller = gameObject.GetComponent<Controller>();
        mouseLooker = gameObject.GetComponent<MouseLooker>();
        respawnPosition = spawnPoint.transform.position;
        respawnRotation = spawnPoint.transform.rotation;

        GameObject mgr = GameObject.Find("InterfaceManager");
        if (mgr) {
            Manager = (InterfaceManager)mgr.GetComponent("InterfaceManager");
        }
    }

    void Start() {
        baseXminn = baseXmin - 0.5f;
        baseXmaxx = baseXmax + 0.5f;
        baseZminn = baseZmin - 0.5f;
        baseZmaxx = baseZmax + 0.5f;
    }

    void Update() {
        // Determine if player is at the base
        if ((gameObject.transform.position.x >= baseXmin) && (gameObject.transform.position.x <= baseXmax) &&
            (gameObject.transform.position.z >= baseZmin) && (gameObject.transform.position.z <= baseZmax)) // formerly 5.5
        {
            playerAtBase = true;
        } else {
            playerAtBase = false;
        }

        // Determine if player is in the mine (i.e. not in the gray zone between mine and base where the base doors are)
        if ((gameObject.transform.position.x >= baseXminn) && (gameObject.transform.position.x <= baseXmaxx) &&
            (gameObject.transform.position.z >= baseZminn) && (gameObject.transform.position.z <= baseZmaxx)) // formerly 6.0
        {
            playerInMine = false;
        } else {
            playerInMine = true;
        }

        // TODO: JPB: (needed) Fix pickup, digging, and log player position

        // Determine if player has clicked the mouse
        //if (!paused && (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump"))) {
        //    // If the player fires, then relock the cursor
        //    // LockCursor(true);

        //    // Initiate pickup
        //    GameManager.gm.gameEvents.Do(new EventBase(GameManager.gm.PickupItem));

        //    // Initiate digging
        //    GameManager.gm.gameEvents.Do(new EventBase(GameManager.gm.DigForItem));
        //}

        //// Log player position
        //if ((GameManager.gm.playerActive) && (Time.frameCount % framesPerTransformReport == 0)) {
        //    Manager.eventReporter.ReportScriptedEvent("playerTransform", new() {
        //        {"positionX", gameObject.transform.position.x},
        //        {"positionZ", gameObject.transform.position.z},
        //        {"rotationY", gameObject.transform.rotation.eulerAngles.y},
        //        {"playerAtBase", playerAtBase}});
        //}
    }

    // Move player to the spawn point
    public void Respawn() {
        gameObject.transform.position = respawnPosition;
        gameObject.transform.rotation = respawnRotation;

        Manager.eventReporter.ReportScriptedEvent("playerRespawnTransform", new() {
                {"positionX", gameObject.transform.position.x},
                {"positionZ", gameObject.transform.position.z},
                {"rotationY", gameObject.transform.rotation.eulerAngles.y},
                {"playerAtBase", true}});

        mouseLooker.ResetLook(respawnRotation);
    }

    // Freeze or unfreeze the player controls
    public void Freeze(bool isFrozen) {
        frozen = isFrozen;
        controller.Freeze(isFrozen);
        mouseLooker.Freeze(isFrozen);
    }

    public void Pause(bool isPaused) {
        paused = isPaused;
        if (paused) {
            controller.Freeze(paused);
            mouseLooker.Freeze(paused);
        } else {
            Freeze(frozen);
        }
    }

    public static float EuclideanDistance(Transform t1, Transform t2) {
        return Mathf.Sqrt(((t1.position.x - t2.position.x) * (t1.position.x - t2.position.x)) +
                          ((t1.position.z - t2.position.z) * (t1.position.z - t2.position.z)));
    }

}
