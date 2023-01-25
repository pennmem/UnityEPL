using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ExperimentBase2 : EventLoop2 {
    protected InterfaceManager2 manager;

    public ExperimentBase2(InterfaceManager2 manager) {
        this.manager = manager;
    }

    protected IEnumerator OuterEnumerator() {
        Debug.Log(1);
        yield return 1;
        //yield return DoBlocking(InnerEnumerator());
        var inner = DoGet<int>(InnerEnumerator());
        yield return inner;
        Debug.Log("DoGet: " + inner.Current);
        Debug.Log(6);
        yield return 6;
    }

    protected IEnumerator InnerEnumerator() {
        Debug.Log(2);
        yield return 2;
        Debug.Log(3);
        yield return 3;
        yield return InnerInnerEnumerator();
    }

    protected IEnumerator InnerInnerEnumerator() {
        Debug.Log(4);
        yield return 4;
        Debug.Log(5);
        yield return 5;
    }

    protected IEnumerator A() {
        Debug.Log("a");
        yield return "a";
        Debug.Log("b");
        yield return "b";
    }

    public void Run() {
        Start();
        Do(RunEnumerator());
    } 

    protected IEnumerator RunEnumerator() {
        var states = MainStates();
        while (states.MoveNext()) {
            var stateEvent = states.Current;
            yield return DoBlocking(stateEvent);
        }
    }

    public IEnumerator s1() {
        yield return A();
    }

    public abstract IEnumerator<IEnumerator> MainStates();
}