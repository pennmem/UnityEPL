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
        public CPSExperiment(InterfaceManager manager) {
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
            await SetupExp();
            await ShowVideo();
            await FinishExp();
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