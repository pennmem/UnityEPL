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

// TODO: JPB: (refactor) This may be able to cancel current running tasks
//            This would require replacing the standard task scheduler with a SingleThreadTaskScheduler
//            Not sure how this would affect unity, because it would be on its thread...
#if UNITY_WEBGL && !UNITY_EDITOR // System.Threading
public class EventLoop4 {
    protected bool isStopped = false;

    public EventLoop4() {}

    ~EventLoop4() {
        Stop();
    }

    // Do

    protected void Do(Func<Task> func) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        func();
    }
    protected void Do<T>(Func<T, Task> func, T t) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        func(t);
    }
    protected void Do<T, U>(Func<T, U, Task> func, T t, U u) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        func(t, u);
    }
    protected void Do<T, U, V>(Func<T, U, V, Task> func, T t, U u, V v) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        func(t, u, v);
    }
    protected void Do<T, U, V, W>(Func<T, U, V, W, Task> func, T t, U u, V v, W w) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        func(t, u, v, w);
    }

    // DoWaitFor

    protected Task DoWaitFor(Func<Task> func) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func();
    }
    protected Task DoWaitFor<T>(Func<T, Task> func, T t) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func(t);
    }
    protected Task DoWaitFor<T, U>(Func<T, U, Task> func, T t, U u) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func(t, u);
    }
    protected Task DoWaitFor<T, U, V>(Func<T, U, V, Task> func, T t, U u, V v) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func(t, u, v);
    }
    protected Task DoWaitFor<T, U, V, W>(Func<T, U, V, W, Task> func, T t, U u, V v, W w) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func(t, u, v, w);
    }

    // DoGet

    protected Task<Z> DoGet<Z>(Func<Task<Z>> func) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func();
    }
    protected Task<Z> DoGet<T, Z>(Func<T, Task<Z>> func, T t) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func(t);
    }
    protected Task<Z> DoGet<T, U, Z>(Func<T, U, Task<Z>> func, T t, U u) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func(t, u);
    }
    protected Task<Z> DoGet<T, U, V, Z>(Func<T, U, V, Task<Z>> func, T t, U u, V v) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func(t, u, v);
    }
    protected Task<Z> DoGet<T, U, V, W, Z>(Func<T, U, V, W, Task<Z>> func, T t, U u, V v, W w) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func(t, u, v, w);
    }

    public void Stop() {
        isStopped = true;
    }
}
#else
public class EventLoop4 {
    protected SingleThreadTaskScheduler scheduler;
    protected CancellationTokenSource cts = new CancellationTokenSource();

    public EventLoop4() {
        scheduler = new SingleThreadTaskScheduler(cts.Token);
    }

    ~EventLoop4() {
        Stop();
    }

    public void Stop() {
        cts.Cancel();
        Task.Factory.StartNew(() => { }, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
    }

    // Do

    // TODO: JPB: (refactor) The Do functions could be improved with C# Source Generators
    //            Ex: any number of variadic arguments
    //            Ex: attribute on original method to generate the call to Do automatically
    //            https://itnext.io/this-is-how-variadic-arguments-could-work-in-c-e2034a9c241
    //            https://github.com/WhiteBlackGoose/InductiveVariadics
    //            This above link needs to be changed to include support for generic constraints
    //            Get it working in unity: https://medium.com/@EnescanBektas/using-source-generators-in-the-unity-game-engine-140ff0cd0dc
    //            This may also currently requires Roslyn https://forum.unity.com/threads/released-roslyn-c-runtime-c-compiler.651505/
    //            Intro to Source Generators: https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/

    protected void Do(Func<Task> func) {
        Task.Factory.StartNew(func, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
    }
    protected void Do<T>(Func<T, Task> func, T t)
            where T : struct {
        AssertBlittable(t);
        T tCopy = t;
        Do(async () => { await func(tCopy); });
    }
    protected void Do<T, U>(Func<T, U, Task> func, T t, U u)
            where T : struct
            where U : struct {
        AssertBlittable(t, u);
        T tCopy = t;
        U uCopy = u;
        Do(async () => { await func(tCopy, uCopy); });
    }
    protected void Do<T, U, V>(Func<T, U, V, Task> func, T t, U u, V v)
            where T : struct
            where U : struct
            where V : struct {
        AssertBlittable(t, u, v);
        T tCopy = t;
        U uCopy = u;
        V vCopy = v;
        Do(async () => { await func(tCopy, uCopy, vCopy); });
    }
    protected void Do<T, U, V, W>(Func<T, U, V, W, Task> func, T t, U u, V v, W w)
            where T : struct
            where U : struct
            where V : struct
            where W : struct {
        AssertBlittable(t, u, v, w);
        T tCopy = t;
        U uCopy = u;
        V vCopy = v;
        W wCopy = w;
        Do(async () => { await func(tCopy, uCopy, vCopy, wCopy); });
    }

    // DoWaitFor

    protected Task DoWaitFor(Func<Task> func) {
        return Task.Factory.StartNew(func, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler).Unwrap();
    }
    protected Task DoWaitFor<T>(Func<T, Task> func, T t)
            where T : struct {
        AssertBlittable(t);
        T tCopy = t;
        return DoWaitFor(async () => { await func(tCopy); });
    }
    protected Task DoWaitFor<T, U>(Func<T, U, Task> func, T t, U u)
            where T : struct
            where U : struct {
        AssertBlittable(t, u);
        T tCopy = t; U uCopy = u;
        return DoWaitFor(async () => { await func(tCopy, uCopy); });
    }
    protected Task DoWaitFor<T, U, V>(Func<T, U, V, Task> func, T t, U u, V v)
            where T : struct
            where U : struct
            where V : struct {
        AssertBlittable(t, u, v);
        T tCopy = t; U uCopy = u; V vCopy = v;
        return DoWaitFor(async () => { await func(tCopy, uCopy, vCopy); });
    }
    protected Task DoWaitFor<T, U, V, W>(Func<T, U, V, W, Task> func, T t, U u, V v, W w)
            where T : struct
            where U : struct
            where V : struct
            where W : struct {
        AssertBlittable(t, u, v, w);
        T tCopy = t; U uCopy = u; V vCopy = v; W wCopy = w;
        return DoWaitFor(async () => { await func(tCopy, uCopy, vCopy, wCopy); });
    }

    // DoGet

    protected Task<Z> DoGet<Z>(Func<Task<Z>> func) {
        return Task.Factory.StartNew(func, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler).Unwrap();
    }
    protected Task<Z> DoGet<T, Z>(Func<T, Task<Z>> func, T t)
            where T : struct {
        AssertBlittable(t);
        T tCopy = t;
        return DoGet(async () => { return await func(tCopy); });
    }
    protected Task<Z> DoGet<T, U, Z>(Func<T, U, Task<Z>> func, T t, U u)
            where T : struct
            where U : struct {
        AssertBlittable(t, u);
        T tCopy = t; U uCopy = u;
        return DoGet(async () => { return await func(tCopy, uCopy); });
    }
    protected Task<Z> DoGet<T, U, V, Z>(Func<T, U, V, Task<Z>> func, T t, U u, V v)
            where T : struct
            where U : struct
            where V : struct {
        AssertBlittable(t, u, v);
        T tCopy = t; U uCopy = u; V vCopy = v;
        return DoGet(async () => { return await func(tCopy, uCopy, vCopy); });
    }
    protected Task<Z> DoGet<T, U, V, W, Z>(Func<T, U, V, W, Task<Z>> func, T t, U u, V v, W w)
            where T : struct
            where U : struct
            where V : struct
            where W : struct {
        AssertBlittable(t, u, v, w);
        T tCopy = t; U uCopy = u; V vCopy = v; W wCopy = w;
        return DoGet(async () => { return await func(tCopy, uCopy, vCopy, wCopy); });
    }

    // AssertBlittable

    protected static bool IsPassable<T>(T t) {
        return UnsafeUtility.IsBlittable(typeof(T))
            || typeof(T) == typeof(bool)
            || typeof(T) == typeof(char);
    }

    // TODO: JPB: (feature) Maybe use IComponentData from com.unity.entities when it releases
    //            This will also allow for bool and char to be included in the structs
    //            https://docs.unity3d.com/Packages/com.unity.entities@0.17/api/Unity.Entities.IComponentData.html
    protected static void AssertBlittable<T>(T t)
            where T : struct {
        if (!UnsafeUtility.IsBlittable(typeof(T))) {
            throw new ArgumentException("The first argument is not a blittable type.");
        }
    }
    protected static void AssertBlittable<T, U>(T t, U u)
            where T : struct
            where U : struct {
        if (!UnsafeUtility.IsBlittable(typeof(T))) {
            throw new ArgumentException("The first argument is not a blittable type.");
        } else if (!UnsafeUtility.IsBlittable(typeof(U))) {
            throw new ArgumentException("The second argument is not a blittable type.");
        }
    }
    protected static void AssertBlittable<T, U, V>(T t, U u, V v)
            where T : struct
            where U : struct
            where V : struct {
        if (!UnsafeUtility.IsBlittable(typeof(T))) {
            throw new ArgumentException("The first argument is not a blittable type.");
        } else if (!UnsafeUtility.IsBlittable(typeof(U))) {
            throw new ArgumentException("The second argument is not a blittable type.");
        } else if (!UnsafeUtility.IsBlittable(typeof(V))) {
            throw new ArgumentException("The third argument is not a blittable type.");
        }
    }
    protected static void AssertBlittable<T, U, V, W>(T t, U u, V v, W w)
            where T : struct
            where U : struct
            where V : struct
            where W : struct {
        if (!UnsafeUtility.IsBlittable(typeof(T))) {
            throw new ArgumentException("The first argument is not a blittable type.");
        } else if (!UnsafeUtility.IsBlittable(typeof(U))) {
            throw new ArgumentException("The second argument is not a blittable type.");
        } else if (!UnsafeUtility.IsBlittable(typeof(V))) {
            throw new ArgumentException("The third argument is not a blittable type.");
        } else if (!UnsafeUtility.IsBlittable(typeof(W))) {
            throw new ArgumentException("The fourth argument is not a blittable type.");
        }
    }
}

// TODO: JPB: (refactor) Remove Bool struct we have blittable bools or use IComponentData
public readonly struct Bool {
    private readonly byte _val;
    public Bool(bool b) {
        if (b) {
            _val = 1;
        } else {
            _val = 0;
        }
    }
    public static implicit operator bool(Bool b) => b._val != 0;
    public static implicit operator Bool(bool b) => new Bool(b);
}
#endif