using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityEPL {

    public class TestExperiment : ExperimentBase<TestExperiment> {
        protected override void AwakeOverride() {  }

        protected void Start() {
            Run();
        }

        protected override Task PostTrials() { return Task.CompletedTask; }
        protected override Task PreTrials() { return Task.CompletedTask; }

        protected async Task RepeatedGetKey() {
            var key = await inputManager.GetKeyTS();
            UnityEngine.Debug.Log("Got key " + key);
            _ = DoWaitForTS(RepeatedGetKey);
        }

        protected override async Task TrialStates() {
            //await manager.textDisplayer.AwaitableUpdateText("AwaitableUpdateText");
            //await InterfaceManager2.Delay(1000);
            //manager.textDisplayer.UpdateText("DONE");
            //var a = await manager.textDisplayer.ReturnableUpdateText("ReturnableUpdateText");
            //UnityEngine.Debug.Log("DoGet: " + a);

            var cts = DoRepeatingTS(1000, 500, 10, () => { UnityEngine.Debug.Log("Repeat"); });
            await inputManager.GetKeyTS();
            cts.Cancel();
            await inputManager.GetKeyTS();

            //var key = await manager.inputManager.GetKey();
            //UnityEngine.Debug.Log("Got key " + key);

            //DelayedGet();
            //DelayedStop();
            //DelayedTriggerKeyPress();
            //KeyMsg keyMsg = await WaitOnKey(default);
            //UnityEngine.Debug.Log("MainStates - WaitOnKey: " + keyMsg.key);
            //manager.textDisplayer.UpdateText("UpdateText");
            //await InterfaceManager2.Delay(1000);
            //await DelayedGet();
        }
    }

}