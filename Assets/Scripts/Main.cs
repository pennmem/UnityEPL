using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;
using System.Diagnostics;
using UnityEngine.UI;
using Unity.Collections;
using System.Text.RegularExpressions;
using System.Text;

using UnityEPL;

public class Main : EventMonoBehaviour {
    protected override void AwakeOverride() { }

    const long delay = 10000000000;

    void Start() {
        UnityEngine.Debug.Log("ThreadID - Start: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        //UnityEngine.Debug.Log(ThreadPool.SetMinThreads(1, 1));
        //UnityEngine.Debug.Log(ThreadPool.SetMaxThreads(1, 1));

        //UnityEngine.Debug.Log(UnsafeUtility.IsBlittable(typeof(StackString)));
    }
}


