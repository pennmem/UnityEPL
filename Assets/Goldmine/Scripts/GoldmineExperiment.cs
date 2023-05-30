#define TIMELINE_SYSTEM
#define PICKUP_SYSTEM
//#define TIMED_TRIAL_SYSTEM
//#define SCALE_DIFFICULY_SYSTEM

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static UnityEPL.TextDisplayer;

namespace UnityEPL {

    public class GoldmineExperiment : ExperimentBase<GoldmineExperiment> {
        public enum ItemType {
            gold,
            gems
        }

        // Version number
        const string EXPERIMENT_VERSION = "2.1.0";

        // Globals
        protected const uint numTrialsInGame = 36; // must be divisible by 6
        protected const int delayDurationMs = 10000; // duration of the delay phases
        protected const int timelineDurationMs = 18000; // duration of the timeline phase
        protected const int timelineScoreDurationMs = 2000; // duration of the timeline score display
        protected const int encodingDurationMs = 30000; // duration of the encoding phase
        protected const int retrievalDurationMs = 30000; // duration of the retrieval phase
        protected const int returnToBasePenalty = -5; // penalty for not eturning to the base in time
        protected const int wrongDigPenalty = -2; // penalty for digging in the wrong place
        protected const int goldFoundReward = 10; // points for each gold piece found
        protected const int gemFoundReward = 10; // points for each gem found
        protected const int correctTimelineReward = 10; // points for each item correctly placed on timeline
        protected const int wrongTimelinePenalty = -2; // penalty for each item incorrectly placed on timeline or items not placed on timeline when they should be
        protected const float maxDigDistance = 4f; // max distance player can dig from items to get points
        protected const int eventsPerFrame = 5;

        // Global State Variables
        protected bool playerActive = false; // whether the player is in an active task state or not
        protected bool isTimedTrial = false;
        protected bool digEnabled = false;
        protected bool pickupEnabled = false;
        protected bool paused = false;
        protected bool showCountdown = false;
        protected bool controlsFrozen = true;
        protected int score = 0;
        protected uint doorIndex = 0;
        protected uint itemsSpawnedTotal = 0;
        protected uint itemsSpawnedThisTrial = 4;
        protected uint itemsPickedUpTotal = 0;
        protected uint itemsPickedUpThisTrial = 0;
        protected uint itemsDugUpTotal = 0;
        protected uint itemsDugUpThisTrial = 0;
        protected uint pickupsAttempted = 0; // how many times the player tried to pickup an item
        protected uint digsAttempted = 0; // how many times the player has dug for an item
        protected Queue<bool> pastTrialPerformance = new() { false, false };
        protected ItemType itemType = ItemType.gems;

        protected byte[] bytes;
        protected float timeLeft = 0;

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
        public Text timerDisplay; // text that says how much time is left in the current game phase
        public Text trialDisplay; // text that says how many trials have elapsed

        // External Runtime Collected Game Objects
        protected ControlPlayer controlPlayer;
        protected SpawnItems spawnItems;
        protected ControlBase controlBase;
        protected ControlCanvas controlMainCanvas;
        protected ControlTimeline controlTimeline;
        protected ControlEndOfGameCanvas controlEndOfGameCanvas;

        private GameObject minDistanceItem;
        private AudioSource pickupAudioSource;
        private AudioSource digAudioSource;


        // Setup functions

        protected override void AwakeOverride() {
            CollectReferences();
        }

        public void Start() {
            Run();
        }

        protected void Update() {
            // Track how much time is left in the current game state
            if (showCountdown) {
                timeLeft -= Time.deltaTime;
                timerDisplay.text = timeLeft.ToString("0.00");
            }

            // Track whether the player is in an active game state
            //baseReporter.reportView = playerActive;

            // See if we need to toggle the door open/close states
            if (playerActive) {
                if ((controlPlayer.playerInMine) && (!controlBase.allDoorsOpen)) {
                    // Open all doors
                    controlBase.OpenDoors(new bool[] { true, true, true });
                } else if ((controlPlayer.playerAtBase) && (controlBase.allDoorsOpen)) {
                    // Open the trial door
                    bool[] iDoors = { false, false, false };
                    iDoors[doorIndex] = true;
                    controlBase.OpenDoors(iDoors);
                }
            }
        }

        private void CollectReferences() {
            // Get quick access to other object functions
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            controlPlayer = player.GetComponent<ControlPlayer>();
            spawnItems = spawner.GetComponent<SpawnItems>();
            controlBase = mineBase.GetComponent<ControlBase>();
            controlMainCanvas = mainCanvas.GetComponent<ControlCanvas>();
            pickupAudioSource = gameObject.GetComponents<AudioSource>()[0];
            digAudioSource = gameObject.GetComponents<AudioSource>()[1];
            //baseReporter = mineBase.GetComponent<WorldDataReporter>();
            controlTimeline = timelineCanvas.transform.Find("Timeline").GetComponent<ControlTimeline>();
        }


        // Trial functions

        protected override async Task TrialStates() {
            InitTrial();
            await PreEncodingDelayMsg();
            await Delay();
            await Encoding();
            await ReturnToBase();
            await PreTimelineMsg();
#if TIMELINE_SYSTEM
            await Timeline();
#endif // TIMELINE_SYSTEM
            await PreRetrievalDelayMsg();
            await Delay();
            await Retrieval();
            await ReturnToBase();
            EndTrial();
        }

        protected override Task PreTrials() {
            // Report experiment info
            LogExperimentInfo();

            string experimentName = "";
#if TIMELINE_SYSTEM
            experimentName += "Timeline";
#endif // TIMELINE_SYSTEM
            experimentName += "Goldmine";
            experimentName += "ReadOnly";
            manager.eventReporter.ReportScriptedEvent("experimentInfo", new() {
                {"experimentName", experimentName},
                {"experimentVersion", EXPERIMENT_VERSION},
                {"unityVersion", Application.unityVersion}
            });

            // Randomize half of the trials to timed and half to untimed.
            // Then, separately for timed and untimed trials, randomly assign 1/3 to each door index.
            bytes = DoorShuffle.TrialParameters(numTrialsInGame);

            // Setup the starting displays
            mainCanvas.SetActive(true);
            scheduledPauseCanvas.SetActive(false);
            endOfGameCanvas.SetActive(false);
            manager.eventReporter.ReportScriptedEvent("canvasActive", new() { { "canvasName", "MainCanvas" }, { "isActive", true } });
            manager.eventReporter.ReportScriptedEvent("canvasActive", new() { { "canvasName", "ScheduledPauseCanvas" }, { "isActive", false } });
            manager.eventReporter.ReportScriptedEvent("canvasActive", new() { { "canvasName", "EndOfGameCanvas" }, { "isActive", false } });

            controlMainCanvas.SetScoreDisplay(score.ToString(), "default", 0, false);
            controlEndOfGameCanvas = endOfGameCanvas.GetComponent<ControlEndOfGameCanvas>();

            return Task.CompletedTask;
        }

        protected override Task PostTrials() {
            FreezeAtBase();
            mainCanvas.SetActive(false);
            endOfGameCanvas.SetActive(true);

            manager.LockCursor(CursorLockMode.None);
            manager.eventReporter.ReportScriptedEvent("canvasActive", new() {
                { "canvasName", "MainCanvas" },
                { "isActive", false } });
            manager.eventReporter.ReportScriptedEvent("canvasActive", new() {
                { "canvasName", "EndOfGameCanvas" },
                { "isActive", true } });
            // Print end of game stats to the canvas
            var msg = (itemsDugUpTotal.ToString() + " (" + Math.Round(100f * itemsDugUpTotal / itemsSpawnedTotal).ToString() + "%)\n" + // gold found (% of spawned gold found)
                   Math.Round(100f * itemsDugUpTotal / digsAttempted).ToString() + "%\n" + // digging accuracy
                   score); // final score
            controlEndOfGameCanvas.SetStatDisplay(msg);
            controlEndOfGameCanvas.playAudio(true);

            return Task.CompletedTask;
        }

        // Actions that occur at the beginning of a trial
        protected void InitTrial() {
            // Log
            manager.eventReporter.ReportScriptedEvent("gameState", new() { { "stateName", "InitTrial" } });

            // Reset the player
            FreezeAtBase();

            // Set up random variables for the trial
#if TIMED_TRIAL_SYSTEM
            isTimedTrial = DoorShuffle.IsTimed(bytes[trialNum - 1]);
#endif // TIMED_TRIAL_SYSTEM
            doorIndex = DoorShuffle.DoorIndex(bytes[trialNum - 1]);

            // Update trial display
            trialDisplay.text = "TRIAL " + trialNum;

            // Reset displays
            controlMainCanvas.ResetCentralDisplay();

            // Recent trial-wise items found count
            itemsPickedUpThisTrial = 0;
            itemsDugUpThisTrial = 0;
        }

        // Actions that occur at the beginning of a practice trial
        protected void InitPracticeTrial() {
            // Log
            manager.eventReporter.ReportScriptedEvent("gameState", new() { { "stateName", "InitTrial" } });

            // Reset the player
            FreezeAtBase();

            // Set up random variables for the trial
#if TIMED_TRIAL_SYSTEM
            isTimedTrial = InterfaceManager.rnd.Value.Next(2) == 1;
#endif // TIMED_TRIAL_SYSTEM
            doorIndex = (uint)InterfaceManager.rnd.Value.Next(3);

            // Update trial display
            trialDisplay.text = "TRIAL " + trialNum;

            // Reset displays
            controlMainCanvas.ResetCentralDisplay();

            // Recent trial-wise items found count
            itemsPickedUpThisTrial = 0;
            itemsDugUpThisTrial = 0;
        }

        // Execute the pre-encoding delay message 
        protected async Task PreEncodingDelayMsg() {
            // Log
            manager.eventReporter.ReportScriptedEvent("gameState", new() { { "stateName", "PreEncodingDelayMsg" } });

            // Update canvas display
            controlMainCanvas.ShowBackground(2f);
            controlMainCanvas.SetCentralDisplay2($"Get ready to\nsearch for {GetItemTypeStr()}", "default", 2f);

            await InterfaceManager.Delay(2000);
        }

        // Execute the delay interval 
        protected async Task Delay() {
            // Log
            manager.eventReporter.ReportScriptedEvent("gameState", new() { { "stateName", "Delay" } });

            FreezeAtBase();
            await InterfaceManager.Delay(delayDurationMs);
        }

        // Execute the encoding interval
        protected async Task Encoding() {
            // Log
            manager.eventReporter.ReportScriptedEvent("gameState", new() { { "stateName", "Encoding" } });

            playerActive = true;

#if PICKUP_SYSTEM
            // Show the dig crosshair
            pickupEnabled = true;
            digCrosshair.SetActive(true);
#endif // PICKUP_SYSTEM

            // Unfreeze the player
            controlPlayer.Freeze(false);

            // Determine if the current trial is timed
            manager.eventReporter.ReportScriptedEvent("timedTrial", new() { { "isTimedTrial", isTimedTrial } });

            // Open one door at random
            bool[] iDoors = { false, false, false };
            iDoors[doorIndex] = true;
            controlBase.OpenDoors(iDoors, true, true);

            // Spawn items in the environment
            switch (itemType) {
                case ItemType.gold:
                    spawnItems.SpawnGold(itemsSpawnedThisTrial);
                    break;
                case ItemType.gems:
                    spawnItems.SpawnGems(itemsSpawnedThisTrial);
                    break;
            }

#if PICKUP_SYSTEM
            // Initialize the spawned items for pickup
            foreach (var item in spawnItems.GetItems()) {
                item.GetComponent<PickupItem>().InitPickup();
            }
#endif // PICKUP_SYSTEM

            // Update canvas displays
            string itemTypeStr = GetItemTypeStr();
#if PICKUP_SYSTEM
            controlMainCanvas.SetTopDisplay("PICK UP" + itemTypeStr.ToUpper(), "default", 0.75f);
            controlMainCanvas.SetTaskDirectionsDisplay("PICK UP " + itemTypeStr.ToUpper() + ": " + itemsSpawnedThisTrial.ToString() + " LEFT");
#else
            controlMainCanvas.SetTopDisplay("FIND " + itemsSpawnedThisTrial.ToString() + " " + itemTypeStr.ToUpper(), "default", 0.75f);
            controlMainCanvas.SetTaskDirectionsDisplay("FIND " + itemsSpawnedThisTrial.ToString() + " " + itemTypeStr.ToUpper());
#endif // PICKUP_SYSTEM

#if TIMED_TRIAL_SYSTEM
            controlMainCanvas.SetTimedTrialDisplay("TIME PENALTY", "negative");
            controlMainCanvas.SetBottomDisplay("TIME PENALTY", "negative", 0.75f);
#else
            //controlMainCanvas.SetTimedTrialDisplay("NO TIME PENALTY", "positive");
#endif // TIMED_TRIAL_SYSTEM

            // Display countdown
            timeLeft = encodingDurationMs;
            showCountdown = true;

            await InterfaceManager.Delay(encodingDurationMs);
        }

        // Execute the return to base interval
        protected async Task ReturnToBase() {
            // Log
            manager.eventReporter.ReportScriptedEvent("gameState", new() { { "stateName", "ReturnToBase" } });

            playerActive = true;

            // Find and hide gold pieces in the environment
            spawnItems.HideItems();
            pickupEnabled = false;
            digEnabled = false;
            digCrosshair.SetActive(false);

            if (controlPlayer.playerInMine) {
                controlMainCanvas.SetTaskDirectionsDisplay("RETURN TO BASE");
                controlMainCanvas.SetCentralDisplay("RETURN TO BASE", "default", 1.5f);
                if (isTimedTrial) {
                    UpdateScore(returnToBasePenalty);
                }
            }

            await DoWaitWhile(() => controlPlayer.playerInMine);
            //await ToCoroutineTask(new WaitWhile(() => controlPlayer.playerInMine));
        }

        // Execute the pre-encoding message 
        protected async Task PreTimelineMsg() {
            // Log
            manager.eventReporter.ReportScriptedEvent("gameState", new() { { "stateName", "PreTimelineMsg" } });

            // Update canvas display
            controlMainCanvas.ShowBackground(2f);
            controlMainCanvas.SetCentralDisplay2($"Get ready to\nplace {GetItemTypeStr()} on timeline", "default", 2f);

            await InterfaceManager.Delay(2000);
        }

        // Timeline
        protected async Task Timeline() {
            // Setup

            // Log
            manager.eventReporter.ReportScriptedEvent("gameState", new() { { "stateName", "Timeline" } });

            // Reset the player
            FreezeAtBase();

            // Set scale of timeline
            controlTimeline.scale = encodingDurationMs / 1000f;

            // Show the timeline
            timelineCanvas.SetActive(true);
            // The following two lines are a hack because unity wasn't displaying the camera correctly
            timelineCanvas.GetComponent<Canvas>().worldCamera.enabled = false;
            timelineCanvas.GetComponent<Canvas>().worldCamera.enabled = true;

            // Spawn the timeline items
            switch (itemType) {
                case ItemType.gold:
                    controlTimeline.SpawnTimelineItems(spawnItems.goldObject, 8);
                    break;
                case ItemType.gems:
                    controlTimeline.SpawnTimelineItems(spawnItems.gemObjects);
                    break;
            }

            // Unlock the mouse
            manager.LockCursor(CursorLockMode.None);

            // Update canvas displays
            controlMainCanvas.SetTaskDirectionsDisplay($"PLACE {GetItemTypeStr().ToUpper()} ON TIMELINE");

            // Display countdown
            timeLeft = timelineDurationMs;
            showCountdown = true;


            // Player does the timeline

            await InterfaceManager.Delay(timelineDurationMs);


            // Teardown

            // Report item times
            var timelineItems = controlTimeline.GetItemTimes();
            manager.eventReporter.ReportScriptedEvent("timeline", new() { { "chosenTimelineItems", timelineItems } });
            //Debug.Log(JsonConvert.SerializeObject(new Dictionary<string, object> { { "items", timelineItems } }));

            // Update the score
            // TODO: JPB: (bug) There is a bug in the gold only timeline for points
            // TODO: JPB: (feature) Add scoring for how close item is to actual time
            //                      +5 on timeline, +1 to +5 for closeness, -2 not on timeline, -2 incorrect on timeline
            // Note: make sure changes here happen in TutorialTimelineEnd too
            int scoreDelta = 0;
            var spawnableItems = new GameObject[0];

            switch (itemType) {
                case ItemType.gold:
                    spawnableItems = Enumerable.Repeat(spawnItems.goldObject, 3).ToArray();
                    break;
                case ItemType.gems:
                    spawnableItems = spawnItems.gemObjects;
                    break;
            }

            var spawnedItems = spawnItems.GetItems();
            foreach (var item in spawnableItems) {
                bool isItemInTimeline = timelineItems.Any(x => (string)x["name"] == item.name);
                bool isItemSpawnedAndPickedUp = spawnedItems.Any(x => (x.name == item.name) && x.GetComponent<PickupItem>().isPickedUp);
                UnityEngine.Debug.Log(item.name + " " + isItemInTimeline + " " + isItemSpawnedAndPickedUp);

                if (isItemSpawnedAndPickedUp && isItemInTimeline) {
                    // Item correctly placed on timeline
                    scoreDelta += correctTimelineReward;
                } else if (!isItemSpawnedAndPickedUp && isItemInTimeline) {
                    // Item incorrectly placed on timeline
                    scoreDelta += wrongTimelinePenalty;
                } else if (isItemSpawnedAndPickedUp && !isItemInTimeline) {
                    // Item not placed on timeline when it should be
                    scoreDelta += wrongTimelinePenalty;
                }
            }
            UpdateScore(scoreDelta);

            // Lock the mouse
            manager.LockCursor(CursorLockMode.Locked);

            // Show timeline score
            await InterfaceManager.Delay(timelineScoreDurationMs);

            // Delete timeline items
            foreach (var item in controlTimeline.GetTimelineItems()) {
                Destroy(item);
            }

            // Hide the timeline 
            timelineCanvas.SetActive(false);
        }

        // Execute the pre-retrieval delay message
        protected async Task PreRetrievalDelayMsg() {
            // Log
            manager.eventReporter.ReportScriptedEvent("gameState", new() { { "stateName", "PreRetrievalDelayMsg" } });

            // Reset the player
            FreezeAtBase();

            // Reset displays
            controlMainCanvas.ResetCentralDisplay();

            // Update canvas display
            controlMainCanvas.ShowBackground(2f);
            controlMainCanvas.SetCentralDisplay2($"Visualize a path\nto the {GetItemTypeStr()}", "default", 2f);

            await InterfaceManager.Delay(2000);
        }

        // Execute the retrieval interval
        protected async Task Retrieval() {
            // Log
            manager.eventReporter.ReportScriptedEvent("gameState", new() { { "stateName", "Retrieval" } });

            playerActive = true;
            digEnabled = true;

            // Unfreeze the player
            controlPlayer.Freeze(false);

            // Show the dig crosshair
            digCrosshair.SetActive(true);

            // Open one door at random
            bool[] iDoors = { false, false, false };
            iDoors[doorIndex] = true;
            controlBase.OpenDoors(iDoors, true, true);

            // Update canvas displays
            controlMainCanvas.SetTopDisplay($"DIG FOR {GetItemTypeStr().ToUpper()}", "default", 0.75f);
            controlMainCanvas.SetTaskDirectionsDisplay($"DIG FOR {GetItemTypeStr().ToUpper()}: {itemsPickedUpThisTrial} LEFT");

            if (isTimedTrial) {
                controlMainCanvas.SetTimedTrialDisplay("TIME PENALTY", "negative");
                controlMainCanvas.SetBottomDisplay("TIME PENALTY", "negative", 0.75f);
            } else {
                //controlMainCanvas.SetTimedTrialDisplay("NO TIME PENALTY", "positive");
            }

            await InterfaceManager.Delay(retrievalDurationMs);
        }

        // Helper Functions

        protected async void EndTrial() {
            // Log
            manager.eventReporter.ReportScriptedEvent("gameState", new() { { "stateName", "EndOfTrial" } });

            // Find and destroy gold pieces in the environment
            spawnItems.DestroyItems();

            // Update trial tracking info
            manager.eventReporter.ReportScriptedEvent("trialComplete", new() { { "trialsCompleted", trialNum } });
            itemsSpawnedTotal += itemsSpawnedThisTrial;
            itemsPickedUpTotal += itemsPickedUpThisTrial;
            itemsDugUpTotal += itemsDugUpThisTrial;

            // Update performance tracker over the past 2 trials
            // and decide how many items will be spawned on the next trial
#if SCALE_DIFFICULY_SYSTEM
            pastTrialPerformance.Dequeue();
            pastTrialPerformance.Enqueue(itemsDugUpThisTrial == itemsSpawnedThisTrial);
            if (pastTrialPerformance.All(x => x)) {
                itemsSpawnedThisTrial++;
            } else if (pastTrialPerformance.All(x => !x) && itemsSpawnedThisTrial > 1) {
                itemsSpawnedThisTrial--;
            }
#endif // SCALE_DIFFICULY_SYSTEM

            // Decide what the next action will be (end game, pause game, or continue to the next trial)
            if (trialNum == numTrialsInGame) {
                EndTrials();
            } else if ((trialNum > 0) && (trialNum % 12 == 0)) {
                await Pause();
            }
        }

        protected async Task Pause() {
            manager.eventReporter.ReportScriptedEvent("gamePaused", new() { { "isPaused", true }, { "pauseType", "scheduledPause" } });
            FreezeAtBase();
            manager.LockCursor(CursorLockMode.None);
            mainCanvas.SetActive(false);
            scheduledPauseCanvas.SetActive(true);
            manager.eventReporter.ReportScriptedEvent("canvasActive", new() { { "canvasName", "MainCanvas" }, { "isActive", false } });
            manager.eventReporter.ReportScriptedEvent("canvasActive", new() { { "canvasName", "ScheduledPauseCanvas" }, { "isActive", true } });
            manager.PauseTS(true);

            await InputManager.Instance.WaitForKey();

            manager.PauseTS(false);
            manager.eventReporter.ReportScriptedEvent("gamePaused", new() { { "isPaused", false }, { "pauseType", "scheduledPause" } });
            mainCanvas.SetActive(true);
            scheduledPauseCanvas.SetActive(false);
            manager.eventReporter.ReportScriptedEvent("canvasActive", new() { { "canvasName", "MainCanvas" }, { "isActive", true } });
            manager.eventReporter.ReportScriptedEvent("canvasActive", new() { { "canvasName", "ScheduledPauseCanvas" }, { "isActive", false } });
            manager.LockCursor(CursorLockMode.Locked);
        }

        protected void FreezeAtBase() {
            playerActive = false;

            // Respawn and freeze the player
            controlPlayer.Respawn();
            controlPlayer.Freeze(true);

            // Close all doors
            controlBase.OpenDoors(new bool[] { false, false, false });

            // Update canvas displays
            controlMainCanvas.SetTaskDirectionsDisplay("WAIT");
            controlMainCanvas.SetTimedTrialDisplay("");
        }

        protected string GetItemTypeStr() {
            return itemType == ItemType.gold ? "gold" : "items";
        }

        // Perform a pickup action (during encoding period only)
        public void PickupItem() {
            if (!pickupEnabled) {
                return;
            }

            float minDistance = float.MaxValue;

            // Register a dig
            pickupsAttempted++;

            // Play the dig sound
            if (pickupAudioSource) {
                pickupAudioSource.Play();
            }

            // Find closest item in the environment
            var items = spawnItems.GetVisibleItems();
            foreach (var item in items) {
                float distance = ControlPlayer.EuclideanDistance(digCrosshair.transform, item.transform);
                if (distance < minDistance) {
                    minDistance = distance;
                    minDistanceItem = item;
                }
            }

            // Add or subtract points depending on whether dig was successful
            if (minDistance <= maxDigDistance) {
                manager.eventReporter.ReportScriptedEvent("pickup", new() {
                    {"successful", true},
                    {"distanceFromNearestItem", minDistance},
                    {"nearestItemPositionX", minDistanceItem.transform.position.x},
                    {"nearestItemPositionZ", minDistanceItem.transform.position.z},
                    {"nearestItemName", minDistanceItem.name}});
                itemsPickedUpThisTrial++;
                controlMainCanvas.SetTaskDirectionsDisplay("PICK UP " + GetItemTypeStr().ToUpper() + ": " + (items.Length - 1).ToString() + " LEFT");
                minDistanceItem.GetComponent<PickupItem>().Pickup();
                spawnItems.HideItem(minDistanceItem);
            } else if (minDistance == float.MaxValue) // i.e. all items have been dug
              {
                manager.eventReporter.ReportScriptedEvent("pickup", new() {
                    {"successful", false},
                    {"distanceFromNearestItem", -1}, // these -1s are for finding instances but should be removed from analysis
                    {"nearestItemPositionX", -1},
                    {"nearestItemPositionZ", -1},
                    {"nearestItemName", -1}});
                if (itemNotFoundEffect) {
                    Vector3 spawnPosition = gameObject.transform.position + new Vector3(0f, -1.18f, 0f);
                    Instantiate(itemNotFoundEffect, playerAnimationSpawnPoint.transform.position, Quaternion.identity); // +new Vector3(0f, -1.18f, 1f)
                }
            } else {
                manager.eventReporter.ReportScriptedEvent("pickup", new() {
                    {"successful", false},
                    {"distanceFromNearestItem", minDistance},
                    {"nearestItemPositionX", minDistanceItem.transform.position.x},
                    {"nearestItemPositionZ", minDistanceItem.transform.position.z},
                    {"nearestItemName", minDistanceItem.name}});
                if (itemNotFoundEffect) {
                    Vector3 spawnPosition = gameObject.transform.position + new Vector3(0f, -1.18f, 0f);
                    Instantiate(itemNotFoundEffect, playerAnimationSpawnPoint.transform.position, Quaternion.identity); // +new Vector3(0f, -1.18f, 1f)
                }
            }
        }

        // Perform a dig action (during retrieval period only)
        public void DigForItem() {
            if (!digEnabled) {
                return;
            }

            int itemFoundReward = 0;
            switch (itemType) {
                case ItemType.gold:
                    itemFoundReward = goldFoundReward;
                    break;
                case ItemType.gems:
                    itemFoundReward = gemFoundReward;
                    break;
            }

            float minDistance = float.MaxValue;

            // Register a dig
            digsAttempted++;

            // Play the dig sound
            if (digAudioSource) {
                digAudioSource.Play();
            }

            // Find closest item in the environment
            var items = spawnItems.GetItems();
            foreach (var item in items) {
#if PICKUP_SYSTEM
                // Don't allow digging of items that weren't picked up
                if (!item.GetComponent<PickupItem>().isPickedUp) {
                    continue;
                }
#endif // PICKUP_SYSTEM

                float distance = ControlPlayer.EuclideanDistance(digCrosshair.transform, item.transform);
                if (distance < minDistance) {
                    minDistance = distance;
                    minDistanceItem = item;
                }
            }

            // Add or subtract points depending on whether dig was successful
            if (minDistance <= maxDigDistance) {
                UpdateScore(itemFoundReward);
                manager.eventReporter.ReportScriptedEvent("dig", new() {
                    {"successful", true},
                    {"distanceFromNearestItem", minDistance},
                    {"nearestItemPositionX", minDistanceItem.transform.position.x},
                    {"nearestItemPositionZ", minDistanceItem.transform.position.z},
                    {"nearestItemName", minDistanceItem.name}});
                itemsDugUpThisTrial++;
                var itemsLeft = itemsPickedUpThisTrial - itemsDugUpThisTrial;
                controlMainCanvas.SetTaskDirectionsDisplay($"DIG FOR {GetItemTypeStr().ToUpper()}: {itemsLeft} LEFT");
                if (itemFoundEffect) {
                    Instantiate(itemFoundEffect, minDistanceItem.transform.position, Quaternion.identity);
                }
                Destroy(minDistanceItem);
            } else if (minDistance == float.MaxValue) { // i.e. all items have been dug
                UpdateScore(wrongDigPenalty);
                manager.eventReporter.ReportScriptedEvent("dig", new() {
                    {"successful", false},
                    {"distanceFromNearestItem", -1}, // these -1s are for finding instances but should be removed from analysis
                    {"nearestItemPositionX", -1},
                    {"nearestItemPositionZ", -1},
                    {"nearestItemName", -1}});
                if (itemNotFoundEffect) {
                    Vector3 spawnPosition = gameObject.transform.position + new Vector3(0f, -1.18f, 0f);
                    Instantiate(itemNotFoundEffect, playerAnimationSpawnPoint.transform.position, Quaternion.identity); // +new Vector3(0f, -1.18f, 1f)
                }
            } else {
                UpdateScore(wrongDigPenalty);
                manager.eventReporter.ReportScriptedEvent("dig", new() {
                    {"successful", false},
                    {"distanceFromNearestItem", minDistance},
                    {"nearestItemPositionX", minDistanceItem.transform.position.x},
                    {"nearestItemPositionZ", minDistanceItem.transform.position.z},
                    {"nearestItemName", minDistanceItem.name}});
                if (itemNotFoundEffect) {
                    Vector3 spawnPosition = gameObject.transform.position + new Vector3(0f, -1.18f, 0f);
                    Instantiate(itemNotFoundEffect, playerAnimationSpawnPoint.transform.position, Quaternion.identity); // +new Vector3(0f, -1.18f, 1f)
                }
            }
        }

        // Update the score and notify player
        public void UpdateScore(int scoreChange) {
            score += scoreChange;
            manager.eventReporter.ReportScriptedEvent("score", new() {
                { "scoreChange", scoreChange },
                { "scoreTotal", score } });

            if (scoreChange > 0) {
                controlMainCanvas.SetScoreDisplay(score.ToString(), "positive", 1f);
                if (pointGainSFX) {
                    AudioSource.PlayClipAtPoint(pointGainSFX, player.transform.position, 0.15f);
                }
            } else if (scoreChange < 0) {
                controlMainCanvas.SetScoreDisplay(score.ToString(), "negative", 1f);
                if (pointLossSFX) {
                    AudioSource.PlayClipAtPoint(pointLossSFX, player.transform.position, 0.15f);
                }
            }
        }
    }
}