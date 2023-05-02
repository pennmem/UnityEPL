using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEPL.TextDisplayer;

namespace UnityEPL {

    public class CPSExperiment : ExperimentBase {
        private ElememInterface elememInterface;

        public CPSExperiment(InterfaceManager manager) {
            CheckElememInUse();
            Run();
        }

        protected void SetVideo() {
            // absolute video path
            string videoPath = Path.Combine(manager.fileManager.ExperimentRoot(), Config.video);

            if (videoPath == null) {
                throw new Exception("Video resource not found");
            }
            
            manager.videoControl.SetVideo(videoPath, true);
        }

        public override async Task MainStates() {
            //SetVideo();
            //await manager.videoControl.PlayVideo();

            //elememInterface.SendTrialMessage(0, true);

            await DoCPSVideo();

            //elememInterface.SendExitMessage();
            //textDisplayer.DisplayText("display end message", "Woo!  The experiment is over.");

        }

        private void CheckElememInUse() {
            if (manager.hostPC is ElememInterface elememInterface) {
                this.elememInterface = elememInterface;
            } else {
                throw new Exception("CPS experiment must use Elemem");
            }
        }

        protected async Task DoCPSVideo() {
            string startingPath = Path.Combine(manager.fileManager.ParticipantPath(), "..", "..", "CPS_Movies");
            var extensions = new[] {
                new SFB.ExtensionFilter("Videos", "mp4", "mov"),
                new SFB.ExtensionFilter("All Files", "*" ),
            };

            //string[] videoPaths = new string[0];
            //while (videoPaths.Length == 0) {
            //    videoPaths = SFB.StandaloneFileBrowser.OpenFilePanel("Select Video To Watch", startingPath, extensions, false);
            //}
            //UnityEngine.Debug.Log(videoPaths[0].Replace("%20", " "));
            //string videoPath = videoPaths[0].Replace("%20", " ");
            var videoPath = await manager.FilePicker(startingPath, extensions);
            manager.videoControl.SetVideo(videoPath);

            // yield return PressAnyKey("In this experiment, you will watch a short educational film lasting about twenty-five minutes. Please pay attention to the film to the best of your ability. You will be asked a series of questions about the video after its completion. After the questionnaire, you will have the opportunity to take a break.\n\n Press any key to begin watching.");
            // TODO: JPB: Display and wait for keypress

            Dictionary<string, object> movieInfo = new() {
                { "movie title", Path.GetFileName(videoPath) },
                { "movie path", Path.GetDirectoryName(videoPath)},
                { "movie duration seconds", manager.videoControl.VideoLength()}
            };
            manager.eventReporter.ReportScriptedEvent("movie", movieInfo);

            //elememInterface.SetElememState("ENCODING", movieInfo);

            //elememInterface.SendCCLStartMessage(videoPlayer.VideoDurationSeconds() - 10); // Remove 10s to not overrun video legnth
            await manager.videoControl.PlayVideo();
        }
    }

}