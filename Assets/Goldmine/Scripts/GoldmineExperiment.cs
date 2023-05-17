#define TIMELINE_SYSTEM
#define SCALE_DIFFICULY_SYSTEM
#define TIMED_TRIAL_SYSTEM

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting.YamlDotNet.Core;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.XR;
using static UnityEPL.TextDisplayer;

namespace UnityEPL {

    public class GoldmineExperiment : ExperimentBase {
        public GoldmineExperiment(InterfaceManager manager) {
            Run();
        }

        // Globals
        protected const uint numTrialsInGame = 36; // must be divisible by 6
        protected const int delayDuration = 10000; // duration of the delay phases
        protected const int timelineDuration = 18000; // duration of the timeline phase
        protected const int timelineScoreDuration = 2000; // duration of the timeline score display
        protected const int taskDuration = 30000; // duration of the task phases (encoding, retrieval)
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
        protected uint doorIndex = 0;
        protected uint itemsSpawnedTotal = 0;
        protected uint itemsSpawnedThisTrial = 0;
        protected uint itemsFoundTotal = 0;
        protected uint itemsFoundThisTrial = 0;
        protected uint score = 0;
        protected Queue<bool> pastTrialPerformance = new() { false, false };

        private byte[] bytes;

        // External Runtime Collected Game Objects
        protected ControlPlayer controlPlayer;
        protected SpawnItems spawnItems;
        protected ControlBase controlBase;
        protected ControlCanvas controlMainCanvas;
        protected ControlTimeline controlTimeline;
        protected ControlEndOfGameCanvas controlEndOfGameCanvas;

        protected GoldmineExperimentMB gameManager;

        private GameObject minDistanceItem;
        private AudioSource pickupAudioSource;
        private AudioSource digAudioSource;


        // Trial functions

        protected override async Task TrialStates() {
            
            InitTrial();
//            await PreEncodingDelayMsg();
//            await Delay();
//            await Encoding();
//            await ReturnToBase();
//            await PreTimelineMsg();
//#if TIMELINE_SYSTEM
//            await Timeline();
//#endif // TIMELINE_SYSTEM
//            await PreRetrievalDelayMsg();
//            await Delay();
//            await Retrieval();
//            await ReturnToBase();
            EndTrial();
        }

        protected override Task PreTrials() {
            // TODO: JPB: (needed) (goldmine) Fix PreTrials

            // Randomize half of the trials to timed and half to untimed.
            // Then, separately for timed and untimed trials, randomly assign 1/3 to each door index.
            bytes = DoorShuffle.TrialParameters(numTrialsInGame);
            UnityEngine.Debug.Log(bytes);

            // Setup the starting displays
            gameManager.mainCanvas.SetActive(true);
            gameManager.scheduledPauseCanvas.SetActive(false);
            gameManager.endOfGameCanvas.SetActive(false);
            manager.eventReporter.ReportScriptedEvent("canvasActive", new() { { "canvasName", "MainCanvas" }, { "isActive", true } });
            manager.eventReporter.ReportScriptedEvent("canvasActive", new() { { "canvasName", "ScheduledPauseCanvas" }, { "isActive", false } });
            manager.eventReporter.ReportScriptedEvent("canvasActive", new() { { "canvasName", "EndOfGameCanvas" }, { "isActive", false } });

            controlMainCanvas.SetScoreDisplay(score.ToString(), "default", 0, false);
            controlEndOfGameCanvas = gameManager.endOfGameCanvas.GetComponent<ControlEndOfGameCanvas>();

            return Task.CompletedTask;
        }

        protected override Task PostTrials() {
            // TODO: JPB: (needed) (goldmine) Fix PostTrials
            return Task.CompletedTask;
            //string msg;
            //FreezeAtBase();
            //mainCanvas.SetActive(false);
            //endOfGameCanvas.SetActive(true);

            //im.LockCursor(CursorLockMode.None);
            //im.scriptedInput.ReportScriptedEvent("canvasActive", new Dictionary<string, object> { { "canvasName", "MainCanvas" }, { "isActive", false } });
            //im.scriptedInput.ReportScriptedEvent("canvasActive", new Dictionary<string, object> { { "canvasName", "EndOfGameCanvas" }, { "isActive", true } });
            //// Print end of game stats to the canvas
            //msg = (state.itemsFoundTotal.ToString() + " (" + Math.Round(100f * state.itemsFoundTotal / state.itemsSpawnedTotal).ToString() + "%)\n" + // gold found (% of spawned gold found)
            //       Math.Round(100f * state.itemsFoundTotal / state.digsAttempted).ToString() + "%\n" + // digging accuracy
            //       state.score); // final score
            //controlEndOfGameCanvas.SetStatDisplay(msg);
            //controlEndOfGameCanvas.playAudio(true);
        }

        public void CollectReferences() {
            // Get quick access to other object functions
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GoldmineExperimentMB gameManager = GameObject.FindFirstObjectByType<GoldmineExperimentMB>();
            controlPlayer = player.GetComponent<ControlPlayer>();
            spawnItems = gameManager.spawner.GetComponent<SpawnItems>();
            controlBase = gameManager.mineBase.GetComponent<ControlBase>();
            controlMainCanvas = gameManager.mainCanvas.GetComponent<ControlCanvas>();
            pickupAudioSource = gameManager.gameObject.GetComponents<AudioSource>()[0];
            digAudioSource = gameManager.gameObject.GetComponents<AudioSource>()[1];
            //baseReporter = mineBase.GetComponent<WorldDataReporter>();
            controlTimeline = gameManager.timelineCanvas.transform.Find("Timeline").GetComponent<ControlTimeline>();
        }

        protected void InitTrial() {
            // Log
            manager.eventReporter.ReportScriptedEvent("gameState", new Dictionary<string, object> { { "stateName", "InitTrial" } });

            // Reset the player
            FreezeAtBase();

            // Set up random variables for the trial
#if TIMED_TRIAL_SYSTEM
            isTimedTrial = DoorShuffle.IsTimed(bytes[trialNum-1]);
#endif // TIMED_TRIAL_SYSTEM
            doorIndex = DoorShuffle.DoorIndex(bytes[trialNum-1]);

            // Update trial display
            gameManager.trialDisplay.text = "TRIAL " + trialNum;

            // Reset displays
            controlMainCanvas.ResetCentralDisplay();

            // Recent trial-wise items found count
            itemsFoundThisTrial = 0;
        }

        protected void InitPracticeTrial() {
            // Log
            manager.eventReporter.ReportScriptedEvent("gameState", new Dictionary<string, object> { { "stateName", "InitTrial" } });

            // Reset the player
            FreezeAtBase();

            // Set up random variables for the trial
#if TIMED_TRIAL_SYSTEM
            isTimedTrial = InterfaceManager.rnd.Value.Next(2) == 1;
#endif // TIMED_TRIAL_SYSTEM
            doorIndex = (uint) InterfaceManager.rnd.Value.Next(3);

            // Update trial display
            gameManager.trialDisplay.text = "TRIAL " + trialNum;

            // Reset displays
            controlMainCanvas.ResetCentralDisplay();

            // Recent trial-wise items found count
            itemsFoundThisTrial = 0;
        }


        protected async void EndTrial() {
            // Log
            manager.eventReporter.ReportScriptedEvent("gameState", new() { { "stateName", "EndOfTrial" } });

            // Find and destroy gold pieces in the environment
            spawnItems.DestroyItems();

            // Update trial tracking info
            manager.eventReporter.ReportScriptedEvent("trialComplete", new() { { "trialsCompleted", trialNum } });
            itemsFoundTotal += itemsFoundThisTrial;
            itemsSpawnedTotal += itemsSpawnedThisTrial;

            // Update performance tracker over the past 2 trials
            // and decide how many items will be spawned on the next trial
#if SCALE_DIFFICULY_SYSTEM
            pastTrialPerformance.Dequeue();
            pastTrialPerformance.Enqueue(itemsFoundThisTrial == itemsSpawnedThisTrial);
            pastTrialPerformance.All(x => x);

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
            gameManager.mainCanvas.SetActive(false);
            gameManager.scheduledPauseCanvas.SetActive(true);
            manager.eventReporter.ReportScriptedEvent("canvasActive", new() { { "canvasName", "MainCanvas" }, { "isActive", false } });
            manager.eventReporter.ReportScriptedEvent("canvasActive", new() { { "canvasName", "ScheduledPauseCanvas" }, { "isActive", true } });
            manager.Pause(true);

            await InputManager.Instance.WaitForKey();

            manager.Pause(false);
            manager.eventReporter.ReportScriptedEvent("gamePaused", new() { { "isPaused", false }, { "pauseType", "scheduledPause" } });
            gameManager.mainCanvas.SetActive(true);
            gameManager.scheduledPauseCanvas.SetActive(false);
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




        protected async Task SetupExp() {
            if (manager.hostPC == null) {
                throw new Exception("CPS experiment must use a Host PC.\n The hostPC is null");
            }
            await manager.hostPC.SendTrialMsg(0, true);
        }

        protected async Task FinishExp() {
            await manager.hostPC.SendExitMsg();
            await manager.textDisplayer.PressAnyKey("display end message", "Woo!  The experiment is over.\n\n Press any key to quit.");
        }

        protected async Task ShowVideo() {
            string startingPath = Path.Combine(manager.fileManager.ParticipantPath(), "..", "..", "CPS_Movies");
            var extensions = new[] {
                new SFB.ExtensionFilter("Videos", "mp4", "mov"),
                new SFB.ExtensionFilter("All Files", "*" ),
            };

            var videoPath = await manager.videoControl.SelectVideoFile(startingPath, extensions);
            UnityEngine.Debug.Log(videoPath);
            Dictionary<string, object> movieInfo = new() {
                { "movie title", Path.GetFileName(videoPath) },
                { "movie path", Path.GetDirectoryName(videoPath)},
                { "movie duration seconds", await manager.videoControl.VideoLength()}
            };
            manager.eventReporter.ReportScriptedEvent("movie", movieInfo);

            await manager.textDisplayer.PressAnyKey("instructions", "In this experiment, you will watch a short educational film lasting about twenty-five minutes. Please pay attention to the film to the best of your ability. You will be asked a series of questions about the video after its completion. After the questionnaire, you will have the opportunity to take a break.\n\n Press any key to begin watching.");

            UnityEngine.Debug.Log(1);
            await manager.hostPC.SendStateMsg(HostPC.StateMsg.ENCODING, movieInfo);

            // Remove 10s to not overrun video legnth
            UnityEngine.Debug.Log(2);
            var cclLength = await manager.videoControl.VideoLength() - 10.0;
            await manager.hostPC.SendCCLStartMsg(Convert.ToInt32(cclLength));
            await manager.videoControl.PlayVideo();
        }
    }

}