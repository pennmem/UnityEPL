using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public abstract class ExperimentBase2 : EventLoop2 {
public class ExperimentBase2 : EventLoop2 {
    protected States states;

    protected IEnumerator OuterEnumerator() {
        Debug.Log(1);
        yield return 1;
        var inner = DoGet<int>(InnerEnumerator());
        yield return inner;
        Debug.Log("DoGet: " + inner.Current);
        //yield return DoGet(InnerEnumerator());
        //yield return DoBlocking(InnerEnumerator());
        //yield return InnerEnumerator();
        Debug.Log(4);
        yield return 4;
    }

    protected IEnumerator InnerEnumerator() {
        Debug.Log(2);
        yield return 2;
        Debug.Log(3);
        yield return 3;
        yield return InnerInnerEnumerator();
    }

    protected IEnumerator InnerInnerEnumerator() {
        Debug.Log(8);
        yield return 8;
        Debug.Log(9);
        yield return 9;
    }

    protected IEnumerator A() {
        Debug.Log("a");
        yield return "a";
        Debug.Log("b");
        yield return "b";
    }

    public ExperimentBase2() {
        states = GenStates();

        Start();

        foreach (var stateEvent in states) {
            Console.WriteLine(stateEvent());
            Do(stateEvent());
        }
    }

    public States GenStates() {
        var s1 = new States() {
            A,
            A,
        };

        return new States() {
            s1,
            OuterEnumerator,
        };
    }
}
