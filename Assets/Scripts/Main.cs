using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using static Main;
using Unity.VisualScripting;

public class Main : MonoBehaviour
{
    const long delay = 10000000000;

    public struct KeyMsg {
        public string key;
        public bool down;

        public KeyMsg(string key, bool down) {
            this.key = key;
            this.down = down;
        }
    }

    public class Input {
        TaskCompletionSource<KeyMsg> tcs = new TaskCompletionSource<KeyMsg>();

        Input() {}
    }

    TaskCompletionSource<KeyMsg> tcs = new TaskCompletionSource<KeyMsg>();

    async Task T1() {
        Debug.Log("ThreadID - T1: " + Thread.CurrentThread.ManagedThreadId);
        KeyMsg keyMsg = await tcs.Task;
        Debug.Log("Key: " + keyMsg.key);
        
        Debug.Log("T1 - 1");
        for (long i = 0; i < delay; i++) ;
        Debug.Log("T1 - 2");
        await Task.Delay(3000);
        Debug.Log("T1 - 3");
        for (long i = 0; i < delay; i++) ;
        Debug.Log("T1 - 4");
    }

    async Task T2() {
        Debug.Log("ThreadID - T2: " + Thread.CurrentThread.ManagedThreadId);
        Debug.Log("T2 - 1");
        for (long i = 0; i < delay; i++) ;
        Debug.Log("T2 - 2");
        await Task.Delay(3000);
        Debug.Log("T2 - 3");
        for (long i = 0; i < delay; i++) ;
        Debug.Log("T2 - 4");
        KeyMsg keyMsg = await tcs.Task;
        Debug.Log("Key: " + keyMsg.key);
    }

    void T() {
        //Thread.Sleep(1);
        var t1 = T1();
        var t2 = T2();
    }

    async void S() {
        Debug.Log("ThreadID - S: " + Thread.CurrentThread.ManagedThreadId);
        await Task.Delay(3000);
        tcs?.SetResult(new KeyMsg("meep", true));
    }

    async void Waiting() {
        Debug.Log("ThreadID - Waiting: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        KeyMsg keyMsg = await tcs.Task;
        Debug.Log("Key: " + keyMsg.key);
        Debug.Log("ThreadID - Waiting: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
    }

    async Task Trigger() {
        Debug.Log("ThreadID - Trigger: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        var cts = new CancellationTokenSource();
        //Cancel(cts);
        await InterfaceManager2.Delay(3000, cts.Token);
        Debug.Log("ThreadID - Trigger: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        tcs?.SetResult(new KeyMsg("meep", true));
    }

    async Task Cancel(CancellationTokenSource cts) {
        Debug.Log("ThreadID - Cancel: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        await InterfaceManager2.Delay(2000);
        Debug.Log("ThreadID - Cancel: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        cts.Cancel();
    }

    async Task<int> DelayedGet() {
        await InterfaceManager2.Delay(1000);
        return 5;
    }

    SingleThreadTaskScheduler scheduler;
    CancellationTokenSource cts = new CancellationTokenSource();

    TestEventLoop testEventLoop = new TestEventLoop();

    // Start is called before the first frame update
    async void Start()
    {
        Debug.Log("ThreadID - Start: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        //Debug.Log(ThreadPool.SetMinThreads(1, 1));
        //Debug.Log(ThreadPool.SetMaxThreads(1, 1));
        //Trigger();
        //Waiting();
        //Cancel();

        //scheduler = new SingleThreadTaskScheduler(cts.Token);
        //Task.Factory.StartNew(Trigger, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
        //Task.Factory.StartNew(Waiting, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
        //Task.Factory.StartNew(async () => { await Cancel(cts); }, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
        //var getVal = await Task.Factory.StartNew(DelayedGet, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler).Unwrap();
        //Debug.Log("DelayedGet: " + getVal);

        testEventLoop.DelayedGet();
        testEventLoop.DelayedStop();
        testEventLoop.DelayedTriggerKeyPress(default);
        KeyMsg keyMsg = await testEventLoop.WaitOnKey(default);
        Debug.Log("Start - WaitOnKey: " + keyMsg.key);
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
        Debug.Log("WaitOnKey: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        KeyMsg keyMsg = await this.tcs.Task;
        Debug.Log("Key: " + keyMsg.key);
        Debug.Log("WaitOnKey: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        return keyMsg;
    }

    public Task DelayedTriggerKeyPress(TaskCompletionSource<KeyMsg> tcs) {
        return DoWaitFor(async () => {
            await DelayedTriggerKeyPressHelper(tcs);
        });
    }
    async Task DelayedTriggerKeyPressHelper(TaskCompletionSource<KeyMsg> tcs) {
        Debug.Log("DelayedTriggerKeyPress: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        await InterfaceManager2.Delay(1000);
        Debug.Log("DelayedTriggerKeyPress: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        tcs?.SetResult(new KeyMsg("meep", true));
    }

    public void DelayedStop() {
        Do(async () => {
            await DelayedStopHelper();
        });
    }
    async Task DelayedStopHelper() {
        Debug.Log("ThreadID - Cancel: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        await InterfaceManager2.Delay(2000);
        Debug.Log("ThreadID - Cancel: " + Thread.CurrentThread.ManagedThreadId + " " + DateTime.Now);
        cts.Cancel();
    }

    public Task<int> DelayedGet() {
        return DoGet(async () => {
            return await DelayedGetHelper();
        });
    }
    async Task<int> DelayedGetHelper() {
        await InterfaceManager2.Delay(3000);
        return 5;
    }
}

public class EventLoop4 {
    protected SingleThreadTaskScheduler scheduler;
    protected CancellationTokenSource cts = new CancellationTokenSource();

    public EventLoop4() {
        scheduler = new SingleThreadTaskScheduler(cts.Token);
    }

    ~EventLoop4() {
        Stop();
    }

    protected void Do(Func<Task> func) {
        Task.Factory.StartNew(func, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
    }

    protected Task DoWaitFor(Func<Task> func) {
        return Task.Factory.StartNew(func, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler).Unwrap();
    }

    protected Task<T> DoGet<T>(Func<Task<T>> func) {
        return Task.Factory.StartNew(func, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler).Unwrap();
    }

    public void Stop() {
        cts.Cancel();
        Task.Factory.StartNew(() => {}, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
    }
}

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

