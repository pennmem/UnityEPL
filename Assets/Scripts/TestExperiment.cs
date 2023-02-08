using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;



public class TestExperiment4 : ExperimentBase4 {
    public TestExperiment4(InterfaceManager2 manager) : base(manager) {
        Run();
    }

    protected async Task RepeatedGetKey() {
        var key = await manager.inputManager.GetKey();
        Debug.Log("Got key " + key);
        Do(RepeatedGetKey);
    }

    public override async Task MainStates() {
        //await InterfaceManager2.Delay(1000);
        //manager.textDisplayer.UpdateText("UpdateText");
        //await InterfaceManager2.Delay(1000);
        await manager.textDisplayer.AwaitableUpdateText("AwaitableUpdateText");
        await InterfaceManager2.Delay(1000);
        manager.textDisplayer.UpdateText("DONE");
        var a = await manager.textDisplayer.ReturnableUpdateText("ReturnableUpdateText");
        Debug.Log("DoGet: " + a);


        var key = await manager.inputManager.GetKey();
        Debug.Log("Got key " + key);

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

public class TestExperiment : ExperimentBase2 {
    public TestExperiment(InterfaceManager2 manager) : base(manager) {
        Run();
    }

    public override IEnumerator<IEnumerator> MainStates() {
        yield return s1();
        yield return OuterEnumerator();
    }

    public IEnumerator s1() {
        yield return A();
    }
}
