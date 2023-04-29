using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

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
            SetVideo();
            await manager.videoControl.PlayVideo();
        }
    }

}