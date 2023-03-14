using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using static Blittability;

// TODO: JPB: (feature) There may be a way to allow WebGL to use multiple threads
//            You would need to write the async underbelly in c++ and link it into unity
//            https://pixel.engineer/posts/cross-platform-cc++-plugins-in-unity/
//            Also, turn on PlayerSettings.WebGL.threadsSupport
//            https://docs.unity3d.com/ScriptReference/PlayerSettings.WebGL-threadsSupport.html
//            I would make sure to check blittability on the c# side
//            This is also likely not worth the effort, unless WebGL becomes super important some day


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
        if (cts.IsCancellationRequested) {
            throw new OperationCanceledException("EventLoop has been stopped already.");
        }
#if !UNITY_WEBGL || UNITY_EDITOR // System.Threading
        Task.Factory.StartNew(func, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
#else
        func();
#endif
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

    // DoIn

    protected async void DoIn(int delayMs, Func<Task> func) {
        Do(async () => {
            await InterfaceManager2.Delay(delayMs);
            await func();
        });
    }
    protected async void DoIn<T>(int delayMs, Func<T, Task> func, T t)
            where T : struct {
        Do(async () => {
            await InterfaceManager2.Delay(delayMs);
            await func(t);
        });
    }
    protected async void DoIn<T, U>(int delayMs, Func<T, U, Task> func, T t, U u)
            where T : struct
            where U : struct {
        Do(async () => {
            await InterfaceManager2.Delay(delayMs);
            await func(t, u);
        });
    }
    protected async void DoIn<T, U, V>(int delayMs, Func<T, U, V, Task> func, T t, U u, V v)
            where T : struct
            where U : struct
            where V : struct {
        Do(async () => {
            await InterfaceManager2.Delay(delayMs);
            await func(t, u, v);
        });
    }
    protected async void DoIn<T, U, V, W>(int delayMs, Func<T, U, V, W, Task> func, T t, U u, V v, W w)
            where T : struct
            where U : struct
            where V : struct
            where W : struct {
        Do(async () => {
            await InterfaceManager2.Delay(delayMs);
            await func(t, u, v, w);
        });
    }

    // DoRepeating
    protected CancellationTokenSource DoRepeating(int delayMs, uint iterations, int intervalMs, Func<Task> func) {
        if (intervalMs <= 0) { throw new ArgumentOutOfRangeException("intervalMs <= 0");}
        CancellationTokenSource cts = new();
        Do(async () => {
            if (delayMs > 0) {
                await InterfaceManager2.Delay(delayMs);
            }
            uint iterationsLeft = iterations;
            while (iterationsLeft != 0) { // A negative value will loop forever
                if (cts.IsCancellationRequested) { break; }
                --iterationsLeft;
                await func();
                await InterfaceManager2.Delay(intervalMs);
            }
        });
        return cts;
    }
    protected CancellationTokenSource DoRepeating<T>(int delayMs, uint iterations, int intervalMs, Func<T, Task> func, T t)
            where T : struct {
        if (intervalMs <= 0) { throw new ArgumentOutOfRangeException("intervalMs <= 0");}
        CancellationTokenSource cts = new();
        Do(async () => {
            if (delayMs > 0) {
                await InterfaceManager2.Delay(delayMs);
            }
            uint iterationsLeft = iterations;
            while (iterationsLeft != 0) { // A negative value will loop forever
                if (cts.IsCancellationRequested) { break; }
                --iterationsLeft;
                await func(t);
                await InterfaceManager2.Delay(intervalMs);
            }
        });
        return cts;
    }
    protected CancellationTokenSource DoRepeating<T, U>(int delayMs, uint iterations, int intervalMs, Func<T, U, Task> func, T t, U u)
            where T : struct
            where U : struct {
        if (intervalMs <= 0) { throw new ArgumentOutOfRangeException("intervalMs <= 0");}
        CancellationTokenSource cts = new();
        Do(async () => {
            if (delayMs > 0) {
                await InterfaceManager2.Delay(delayMs);
            }
            uint iterationsLeft = iterations;
            while (iterationsLeft != 0) { // A negative value will loop forever
                if (cts.IsCancellationRequested) { break; }
                --iterationsLeft;
                await func(t, u);
                await InterfaceManager2.Delay(intervalMs);
            }
        });
        return cts;
    }
    protected CancellationTokenSource DoRepeating<T, U, V>(int delayMs, uint iterations, int intervalMs, Func<T, U, V, Task> func, T t, U u, V v)
            where T : struct
            where U : struct
            where V : struct {
        if (intervalMs <= 0) { throw new ArgumentOutOfRangeException("intervalMs <= 0");}
        CancellationTokenSource cts = new();
        Do(async () => {
            if (delayMs > 0) {
                await InterfaceManager2.Delay(delayMs);
            }
            uint iterationsLeft = iterations;
            while (iterationsLeft != 0) { // A negative value will loop forever
                if (cts.IsCancellationRequested) { break; }
                --iterationsLeft;
                await func(t, u, v);
                await InterfaceManager2.Delay(intervalMs);
            }
        });
        return cts;
    }
    protected CancellationTokenSource DoRepeating<T, U, V, W>(int delayMs, uint iterations, int intervalMs, Func<T, U, V, W, Task> func, T t, U u, V v, W w)
            where T : struct
            where U : struct
            where V : struct
            where W : struct {
        if (intervalMs <= 0) { throw new ArgumentOutOfRangeException("intervalMs <= 0");}
        CancellationTokenSource cts = new();
        Do(async () => {
            if (delayMs > 0) {
                await InterfaceManager2.Delay(delayMs);
            }
            uint iterationsLeft = iterations;
            while (iterationsLeft != 0) { // A negative value will loop forever
                if (cts.IsCancellationRequested) { break; }
                --iterationsLeft;
                await func(t, u, v, w);
                await InterfaceManager2.Delay(intervalMs);
            }
        });
        return cts;
    }

    // DoWaitFor

    protected Task DoWaitFor(Func<Task> func) {
        if (cts.IsCancellationRequested) {
            throw new OperationCanceledException("EventLoop has been stopped already.");
        }
#if !UNITY_WEBGL || UNITY_EDITOR // System.Threading
        return Task.Factory.StartNew(func, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler).Unwrap();
#else
        return func();
#endif
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
        T tCopy = t;
        U uCopy = u;
        return DoWaitFor(async () => { await func(tCopy, uCopy); });
    }
    protected Task DoWaitFor<T, U, V>(Func<T, U, V, Task> func, T t, U u, V v)
            where T : struct
            where U : struct
            where V : struct {
        AssertBlittable(t, u, v);
        T tCopy = t;
        U uCopy = u;
        V vCopy = v;
        return DoWaitFor(async () => { await func(tCopy, uCopy, vCopy); });
    }
    protected Task DoWaitFor<T, U, V, W>(Func<T, U, V, W, Task> func, T t, U u, V v, W w)
            where T : struct
            where U : struct
            where V : struct
            where W : struct {
        AssertBlittable(t, u, v, w);
        T tCopy = t;
        U uCopy = u;
        V vCopy = v;
        W wCopy = w;
        return DoWaitFor(async () => { await func(tCopy, uCopy, vCopy, wCopy); });
    }

    // DoGet

    protected Task<Z> DoGet<Z>(Func<Task<Z>> func) {
#if !UNITY_WEBGL || UNITY_EDITOR // System.Threading
        return Task.Factory.StartNew(func, cts.Token, TaskCreationOptions.DenyChildAttach, scheduler).Unwrap();
#else
        return func();
#endif
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
        T tCopy = t;
        U uCopy = u;
        return DoGet(async () => { return await func(tCopy, uCopy); });
    }
    protected Task<Z> DoGet<T, U, V, Z>(Func<T, U, V, Task<Z>> func, T t, U u, V v)
            where T : struct
            where U : struct
            where V : struct {
        AssertBlittable(t, u, v);
        T tCopy = t;
        U uCopy = u;
        V vCopy = v;
        return DoGet(async () => { return await func(tCopy, uCopy, vCopy); });
    }
    protected Task<Z> DoGet<T, U, V, W, Z>(Func<T, U, V, W, Task<Z>> func, T t, U u, V v, W w)
            where T : struct
            where U : struct
            where V : struct
            where W : struct {
        AssertBlittable(t, u, v, w);
        T tCopy = t;
        U uCopy = u;
        V vCopy = v;
        W wCopy = w;
        return DoGet(async () => { return await func(tCopy, uCopy, vCopy, wCopy); });
    }
}