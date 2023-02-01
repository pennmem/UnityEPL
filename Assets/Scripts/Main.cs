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

        testEventLoop.DelayedGet();
        testEventLoop.DelayedStop();
        testEventLoop.DelayedTriggerKeyPress(default);
        KeyMsg keyMsg = await testEventLoop.WaitOnKey(default);
        UnityEngine.Debug.Log("Start - WaitOnKey: " + keyMsg.key);
        await Task.Delay(2000);
        testEventLoop.DelayedGet();

        
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
}

// TODO: JPB: This may be able to cancel current running tasks
//            This would require replacing the standard task scheduler with a SingleThreadTaskScheduler
//            Not sure how this would affect unity, because it would be on its thread...
#if UNITY_WEBGL && !UNITY_EDITOR // System.Threading
public class EventLoop4 {
    protected bool isStopped = false;

    public EventLoop4() {}

    ~EventLoop4() {
        Stop();
    }

    protected void Do(Func<Task> func) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        func();
    }

    protected Task DoWaitFor(Func<Task> func) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func();
    }

    protected Task<T> DoGet<T>(Func<Task<T>> func) {
        if (isStopped) throw new OperationCanceledException("EventLoop has been stopped already.");
        return func();
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

    // TODO: JPB: (feature) The Do functions could be improved with C# Source Generators
    //            Ex: any number of variadic arguments
    //            Ex: attribute on original method to generate the call to Do automatically
    //            https://itnext.io/this-is-how-variadic-arguments-could-work-in-c-e2034a9c241
    //            https://github.com/WhiteBlackGoose/InductiveVariadics
    //            This above link needs to be changed to include support for generic constraints
    //            Get it working in unity: https://medium.com/@EnescanBektas/using-source-generators-in-the-unity-game-engine-140ff0cd0dc
    //            This may also currently requires Roslyn https://forum.unity.com/threads/released-roslyn-c-runtime-c-compiler.651505/
    //            Intro to Source Generators: https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/

    // Do

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

    protected void AssertBlittable<T>(T t)
            where T : struct {
        if (UnsafeUtility.IsBlittable(typeof(T))) {
            throw new ArgumentException("The first argument is not a blittable type.");
        }
    }
    protected void AssertBlittable<T, U>(T t, U u)
            where T : struct
            where U : struct {
        if (UnsafeUtility.IsBlittable(typeof(T))) {
            throw new ArgumentException("The first argument is not a blittable type.");
        } else if (UnsafeUtility.IsBlittable(typeof(U))) {
            throw new ArgumentException("The second argument is not a blittable type.");
        }
    }
    protected void AssertBlittable<T, U, V>(T t, U u, V v)
            where T : struct
            where U : struct
            where V : struct {
        if (UnsafeUtility.IsBlittable(typeof(T))) {
            throw new ArgumentException("The first argument is not a blittable type.");
        } else if (UnsafeUtility.IsBlittable(typeof(U))) {
            throw new ArgumentException("The second argument is not a blittable type.");
        } else if (UnsafeUtility.IsBlittable(typeof(V))) {
            throw new ArgumentException("The third argument is not a blittable type.");
        }
    }
    protected void AssertBlittable<T, U, V, W>(T t, U u, V v, W w)
            where T : struct
            where U : struct
            where V : struct
            where W : struct {
        if (UnsafeUtility.IsBlittable(typeof(T))) {
            throw new ArgumentException("The first argument is not a blittable type.");
        } else if (UnsafeUtility.IsBlittable(typeof(U))) {
            throw new ArgumentException("The second argument is not a blittable type.");
        } else if (UnsafeUtility.IsBlittable(typeof(V))) {
            throw new ArgumentException("The third argument is not a blittable type.");
        } else if (UnsafeUtility.IsBlittable(typeof(W))) {
            throw new ArgumentException("The fourth argument is not a blittable type.");
        }
    }
}
#endif


public sealed class SingleThreadTaskScheduler : TaskScheduler {
    [ThreadStatic]
    private static bool _isExecuting;

    private readonly CancellationToken _cancellationToken;

    private readonly BlockingCollection<Task> _taskQueue;

    private readonly Thread _singleThread;
    public override int MaximumConcurrencyLevel => 1;

    public SingleThreadTaskScheduler(CancellationToken cancellationToken) {
        this._cancellationToken = cancellationToken;
        this._taskQueue = new BlockingCollection<Task>();
        _singleThread = new Thread(RunOnCurrentThread) { Name = "STTS Thread", IsBackground = true };
        _singleThread.Start();
    }

    private void RunOnCurrentThread() {
        _isExecuting = true;

        try {
            foreach (var task in _taskQueue.GetConsumingEnumerable(_cancellationToken)) {
                TryExecuteTask(task);
            }
        } catch (OperationCanceledException) {
        } finally {
            _isExecuting = false;
        }
    }

    protected override IEnumerable<Task> GetScheduledTasks() => _taskQueue.ToList();

    protected override void QueueTask(Task task) {
        try {
            _taskQueue.Add(task, _cancellationToken);
        } catch (OperationCanceledException) {
        }
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) {
        // We'd need to remove the task from queue if it was already queued. 
        // That would be too hard.
        if (taskWasPreviouslyQueued)
            return false;

        return _isExecuting && TryExecuteTask(task);
    }
}

