using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEPL {

    public class RepFRExperiment : ExperimentBase {
        public RepFRExperiment(InterfaceManager manager) {
            Run();
        }

        protected void SetVideo() {
            // absolute video path
            string videoPath = System.IO.Path.Combine(manager.fileManager.ExperimentRoot(), Config.introductionVideo);

            if (videoPath == null) {
                throw new Exception("Video resource not found");
            }

            manager.videoControl.SetVideo(videoPath, true);
        }

        protected override async Task TrialStates() {
            await RecordTest();
            //SetVideo();
            //await manager.videoControl.PlayVideo();
            UnityEngine.Debug.Log("Video Done");
        }

        protected override Task PreTrials() { return Task.CompletedTask; }
        protected override Task PostTrials() { return Task.CompletedTask; }

        // NOTE: rather than use flags for the audio test, this is entirely based off of timings.
        // Since there is processing latency (which seems to be unity version dependent), this
        // is really a hack that lets us get through the mic test unscathed. More time critical
        // applications need a different approach
        protected async Task RecordTest() {
            string wavPath = System.IO.Path.Combine(manager.fileManager.SessionPath(), "microphone_test_"
                        + DataReporter.TimeStamp().ToString("yyyy-MM-dd_HH_mm_ss") + ".wav");

            await manager.PlayLowBeep();
            manager.recorder.StartRecording(wavPath);
            manager.textDisplayer.DisplayText("microphone test recording", "<color=red>Recording...</color>");
            await InterfaceManager.Delay(Config.micTestDuration);

            manager.textDisplayer.DisplayText("microphone test playing", "<color=green>Playing...</color>");
            manager.SetPlayback(await manager.recorder.StopRecording());
            manager.PlayPlayback();
            await InterfaceManager.Delay(Config.micTestDuration);
        }
    }

}