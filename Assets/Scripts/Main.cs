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


public struct KeyMsg {
    public string key;
    public bool down;

    public KeyMsg(string key, bool down) {
        this.key = key;
        this.down = down;
    }
}

public class Main : MonoBehaviour
{
    const long delay = 10000000000;

    TestEventLoop testEventLoop = new TestEventLoop();

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

public class TestEventLoop : EventLoop4 {
    TaskCompletionSource<KeyMsg> tcs = new TaskCompletionSource<KeyMsg>();

    public Task<KeyMsg> WaitOnKey(TaskCompletionSource<KeyMsg> tcs) {
        return DoGet(async () => {
            return await WaitOnKeyHelper(tcs);
        });
    }
    async Task<KeyMsg> WaitOnKeyHelper(TaskCompletionSource<KeyMsg> tcs) {
        UnityEngine.Debug.Log("WaitOnKey: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        KeyMsg keyMsg = await this.tcs.Task;
        UnityEngine.Debug.Log("Key: " + keyMsg.key);
        UnityEngine.Debug.Log("WaitOnKey: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        return keyMsg;
    }

    public Task DelayedTriggerKeyPress(TaskCompletionSource<KeyMsg> tcs) {
        return DoWaitFor(async () => {
            await DelayedTriggerKeyPressHelper(tcs);
        });
    }
    async Task DelayedTriggerKeyPressHelper(TaskCompletionSource<KeyMsg> tcs) {
        UnityEngine.Debug.Log("DelayedTriggerKeyPress: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        await InterfaceManager2.Delay(1000);
        UnityEngine.Debug.Log("DelayedTriggerKeyPress: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        tcs?.SetResult(new KeyMsg("meep", true));
    }

    public void DelayedStop() {
        Do(async () => {
            await DelayedStopHelper();
        });
    }
    async Task DelayedStopHelper() {
        UnityEngine.Debug.Log("DelayedStop: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        await InterfaceManager2.Delay(2000);
        UnityEngine.Debug.Log("DelayedStop: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        Stop();
    }

    public Task<int> DelayedGet() {
        return DoGet(async () => {
            return await DelayedGetHelper();
        });
    }
    async Task<int> DelayedGetHelper() {
        UnityEngine.Debug.Log("DelayedGet: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        await InterfaceManager2.Delay(3000);
        UnityEngine.Debug.Log("DelayedGet: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        return 5;
    }

    public void ThrowException(int i) {
        Do(ThrowExceptionHelper, i);
    }
    public Task ThrowExceptionHelper(int i) {
        UnityEngine.Debug.Log("Throwing Exception");
        throw new Exception("Test Exception " + i);
    }
}

