using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TestExperiment4 : ExperimentBase4 {
    public TestExperiment4(InterfaceManager2 manager) : base(manager) {
        Run();
    }

    public override async Task MainStates() {
        DelayedGet();
        DelayedStop();
        DelayedTriggerKeyPress();
        KeyMsg keyMsg = await WaitOnKey(default);
        UnityEngine.Debug.Log("MainStates - WaitOnKey: " + keyMsg.key);
        manager.textDisplayer.UpdateText("UpdateText");
        await Task.Delay(2000);
        await DelayedGet();
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
