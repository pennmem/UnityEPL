using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var exp = new ExperimentBase2();

        //IEnumerator outer = OuterEnumerator();
        //YieldedEvent myEnumerator = new YieldedEvent(outer);
        //int i = 1;
        //while (myEnumerator.MoveNext()) {
        //    Console.WriteLine("i: " + i++);
        //    object current = myEnumerator.Current;
        //    Thread.Sleep(10);
        //    Console.WriteLine(current);
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
