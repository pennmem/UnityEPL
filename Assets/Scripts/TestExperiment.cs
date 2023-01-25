using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
