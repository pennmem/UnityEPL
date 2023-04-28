using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityEPL {

    public class RepFRExperiment : ExperimentBase {
        public RepFRExperiment(InterfaceManager manager) : base(manager) {
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

        public override async Task MainStates() {
            SetVideo();
            await manager.videoControl.PlayVideo();
            UnityEngine.Debug.Log("Video Done");
        }
    }

}