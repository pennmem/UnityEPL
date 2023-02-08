using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public struct KeyMsg {
    public string key;
    public bool down;

    public KeyMsg(string key, bool down) {
        this.key = key;
        this.down = down;
    }
}

public abstract class ExperimentBase4 : EventLoop4 {
    TaskCompletionSource<KeyMsg> tcs = new TaskCompletionSource<KeyMsg>();

    protected InterfaceManager2 manager;

    public ExperimentBase4(InterfaceManager2 manager) {
        this.manager = manager;
    }

    public void Run() {
        Do(MainStates);
    }

    public abstract Task MainStates();

    public Task<KeyMsg> WaitOnKey(TaskCompletionSource<KeyMsg> tcs) {
        return DoGet(async () => {
            return await WaitOnKeyHelper(tcs);
        });
    }
    async Task<KeyMsg> WaitOnKeyHelper(TaskCompletionSource<KeyMsg> tcs) {
        UnityEngine.Debug.Log("WaitOnKey: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        this.tcs = new TaskCompletionSource<KeyMsg>(); // This is bad and wrong
        KeyMsg keyMsg = await this.tcs.Task;
        UnityEngine.Debug.Log("WaitOnKey: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        return keyMsg;
    }

    public Task DelayedTriggerKeyPress() {
        return DoWaitFor(DelayedTriggerKeyPressHelper);
    }
    async Task DelayedTriggerKeyPressHelper() {
        UnityEngine.Debug.Log("DelayedTriggerKeyPress: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        await InterfaceManager2.Delay(1000);
        UnityEngine.Debug.Log("DelayedTriggerKeyPress: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        tcs?.SetResult(new KeyMsg("meep", true));
    }

    public void DelayedStop() {
        Do(DelayedStopHelper);
    }
    async Task DelayedStopHelper() {
        UnityEngine.Debug.Log("DelayedStop: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        await InterfaceManager2.Delay(2000);
        UnityEngine.Debug.Log("DelayedStop: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        Stop();
    }

    public Task<int> DelayedGet() {
        return DoGet(DelayedGetHelper);
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