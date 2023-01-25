using System;
using System.Collections;
using System.Collections.Generic;

using StateEvent = System.Func<System.Collections.IEnumerator>;

public class States : IEnumerable<StateEvent> {
    public List<StateEvent> stateEvents {
        get; protected set;
    }

    public States() {
        stateEvents = new List<StateEvent>();
    }

    public void Add(StateEvent stateEvent) {
        stateEvents.Add(stateEvent);
    }

    public void Add(States states) {
        stateEvents.AddRange(states);
    }

    public IEnumerator<StateEvent> GetEnumerator() {
        return stateEvents.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }


    // HOW TO USE

    public static IEnumerator A() {
        yield return 1;
        yield return 2;
    }

    public static void Do(IEnumerator e) {
        while (e.MoveNext()) {
            Console.WriteLine(e.Current);
        }
    }

    public static void HowToUse() {
        var s1 = new States() { A, A };
        var states = new States { s1, A };

        foreach (var stateEvent in states) {
            Console.WriteLine(stateEvent());
            Do(stateEvent());
        }
    }
}

