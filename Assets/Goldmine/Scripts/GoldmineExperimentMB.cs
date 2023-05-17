using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEPL;

public class GoldmineExperimentMB : EventMonoBehaviour {
    protected override void AwakeOverride() { }

    // External Compile Time Collected Game Objects
    public GameObject player; // the player
    public GameObject playerAnimationSpawnPoint; // spawn point for animations that appear on the ground in front of the player
    public GameObject digCrosshair; // object with the dig crosshair image
    public GameObject spawner; // the item spawner
    public GameObject mineBase; // base where the player spawns
    public GameObject mainCanvas; // the main canvas on which text is displayed
    public GameObject itemFoundEffect; // particle system that plays when player digs an item is found
    public GameObject itemNotFoundEffect; // particle system that plays when player digs an item is not found
    public GameObject timelineCanvas;
    public GameObject scheduledPauseCanvas;
    public GameObject endOfGameCanvas;
    public AudioClip pointGainSFX; // sound that plays when points are added
    public AudioClip pointLossSFX; // sound that plays when points are subtracted

    // HUD text displays
    public Text timerDisplay; // text that says how much time is left in the current game phase
    public Text trialDisplay; // text that says how many trials have elapsed
}
