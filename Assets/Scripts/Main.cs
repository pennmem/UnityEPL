using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using Unity.VisualScripting;
using Unity.Collections.LowLevel.Unsafe;
using System.Diagnostics;

public class Main : MonoBehaviour
{
    const long delay = 10000000000;

    // Start is called before the first frame update
    async void Start()
    {
        UnityEngine.Debug.Log("ThreadID - Start: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        //UnityEngine.Debug.Log(ThreadPool.SetMinThreads(1, 1));
        //UnityEngine.Debug.Log(ThreadPool.SetMaxThreads(1, 1));

        //testEventLoop.DelayedGet();
        //testEventLoop.DelayedStop();
        //testEventLoop.DelayedTriggerKeyPress(default);
        //KeyMsg keyMsg = await testEventLoop.WaitOnKey(default);
        //UnityEngine.Debug.Log("Start - WaitOnKey: " + keyMsg.key);
        //await Task.Delay(2000);
        //testEventLoop.DelayedGet();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}



