using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using static UnityEPL.Blittability;

namespace UnityEPL {

    // TODO: JPB: (bug) There may be a bug in the WebGL side because it just calls the function instead of putting it into a queue
    //            I don't think this is an issue because everything should be running in a single thread,
    //            but I haven't thought it through enough to be sure.
    // TODO: JPB: (feature) There may be a way to allow WebGL to use multiple threads
    //            You would need to write the async underbelly in c++ and link it into unity
    //            https://pixel.engineer/posts/cross-platform-cc++-plugins-in-unity/
    //            Also, turn on PlayerSettings.WebGL.threadsSupport
    //            https://docs.unity3d.com/ScriptReference/PlayerSettings.WebGL-threadsSupport.html
    //            I would make sure to check blittability on the c# side
    //            This is also likely not worth the effort, unless WebGL becomes super important some day


    public class EventLoop {
        protected SingleThreadTaskScheduler scheduler;
        protected CancellationTokenSource cts = new CancellationTokenSource();
        protected InterfaceManager manager;
        protected DateTime startTime;

        public EventLoop() {
            scheduler = new SingleThreadTaskScheduler(cts.Token);
            manager = InterfaceManager.Instance;
            manager.eventLoops.Add(this);

            // Init threadlocal variables
            DoTS(async () => {
                startTime = Clock.UtcNow;
                await InterfaceManager.Delay(1);
            });
        }

        ~EventLoop() {
            Stop();
        }

        public void Stop() {
            cts.Cancel();
        }

        public async void Abort() {
            cts.Cancel();
            await InterfaceManager.Delay(5000);
            scheduler.Abort();
        }

        protected async Task TimeoutTask(Task task, int timeoutMs, string timeoutMessage = null) {
            Task timeoutTask = InterfaceManager.Delay(timeoutMs, cts.Token);
            var completedTask = await Task.WhenAny(task, timeoutTask);
            await completedTask;
            if (completedTask == timeoutTask) {
                var msg = timeoutMessage ?? $"Task Timed out after {timeoutMs}ms";
                throw new TimeoutException(timeoutMessage);
            }
        }
        protected async Task<Z> TimeoutTask<Z>(Task<Z> task, int timeoutMs, string timeoutMessage = null) {
            Task timeoutTask = InterfaceManager.Delay(timeoutMs, cts.Token);
            var completedTask = await Task.WhenAny(task, timeoutTask);
            await completedTask;
            if (completedTask == timeoutTask) {
                var msg = timeoutMessage ?? $"Task Timed out after {timeoutMs}ms";
                throw new TimeoutException(timeoutMessage);
            }
            return await task;
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

        protected async void DoTS(Action func) {
            if (cts.IsCancellationRequested) {
                throw new OperationCanceledException("EventLoop has been stopped already.");
            }
            
            await StartTask(func);
        }
        protected void DoTS<T>(Action<T> func, T t)
                where T : struct {
            AssertBlittable<T>();
            DoTS(() => { func(t); });
        }
        protected void DoTS<T, U>(Action<T, U> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable<T, U>();
            DoTS(() => { func(t, u); });
        }
        protected void DoTS<T, U, V>(Action<T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable<T, U, V>();
            DoTS(() => { func(t, u, v); });
        }
        protected void DoTS<T, U, V, W>(Action<T, U, V, W> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable<T, U, V, W>();
            DoTS(() => { func(t, u, v, w); });
        }

        protected async void DoTS(Func<Task> func) {
            if (cts.IsCancellationRequested) {
                throw new OperationCanceledException("EventLoop has been stopped already.");
            }
            
            await StartTask(func);
            //Debug.Log($"Starting Do: {t.Id}");
            //StartTask(func);
        }
        protected void DoTS<T>(Func<T, Task> func, T t)
                where T : struct {
            AssertBlittable<T>();
            DoTS(async () => { await func(t); });
        }
        protected void DoTS<T, U>(Func<T, U, Task> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable<T, U>();
            DoTS(async () => { await func(t, u); });
        }
        protected void DoTS<T, U, V>(Func<T, U, V, Task> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable<T, U, V>();
            DoTS(async () => { await func(t, u, v); });
        }
        protected void DoTS<T, U, V, W>(Func<T, U, V, W, Task> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable<T, U, V, W>();
            DoTS(async () => { await func(t, u, v, w); });
        }

        // DoIn

        protected void DoInTS(int delayMs, Action func) {
            DoTS(async () => {
                await InterfaceManager.Delay(delayMs);
                func();
            });
        }
        protected void DoInTS<T>(int delayMs, Action<T> func, T t)
                where T : struct {
            DoTS(async () => {
                await InterfaceManager.Delay(delayMs);
                func(t);
            });
        }
        protected void DoInTS<T, U>(int delayMs, Action<T, U> func, T t, U u)
                where T : struct
                where U : struct {
            DoTS(async () => {
                await InterfaceManager.Delay(delayMs);
                func(t, u);
            });
        }
        protected void DoInTS<T, U, V>(int delayMs, Action<T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            DoTS(async () => {
                await InterfaceManager.Delay(delayMs);
                func(t, u, v);
            });
        }
        protected void DoInTS<T, U, V, W>(int delayMs, Action<T, U, V, W> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            DoTS(async () => {
                await InterfaceManager.Delay(delayMs);
                func(t, u, v, w);
            });
        }

        protected void DoInTS(int delayMs, Func<Task> func) {
            DoTS(async () => {
                await InterfaceManager.Delay(delayMs);
                await func();
            });
        }
        protected void DoInTS<T>(int delayMs, Func<T, Task> func, T t)
                where T : struct {
            DoTS(async () => {
                await InterfaceManager.Delay(delayMs);
                await func(t);
            });
        }
        protected void DoInTS<T, U>(int delayMs, Func<T, U, Task> func, T t, U u)
                where T : struct
                where U : struct {
            DoTS(async () => {
                await InterfaceManager.Delay(delayMs);
                await func(t, u);
            });
        }
        protected void DoInTS<T, U, V>(int delayMs, Func<T, U, V, Task> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            DoTS(async () => {
                await InterfaceManager.Delay(delayMs);
                await func(t, u, v);
            });
        }
        protected void DoInTS<T, U, V, W>(int delayMs, Func<T, U, V, W, Task> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            DoTS(async () => {
                await InterfaceManager.Delay(delayMs);
                await func(t, u, v, w);
            });
        }

        // DoRepeating

        protected CancellationTokenSource DoRepeatingTS(int delayMs, int intervalMs, uint? iterations, Action func) {
            if (intervalMs <= 0) {
                throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})");
            }
            CancellationTokenSource cts = new();
            DoTS(async () => {
                await InterfaceManager.Delay(delayMs);

                uint totalIterations = iterations ?? uint.MaxValue;
                var initTime = Clock.UtcNow;
                for (int i = 0; i < totalIterations; ++i) {
                    if (cts.IsCancellationRequested) { break; }
                    func();
                    var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                    await InterfaceManager.Delay((int)delayTime);
                }
            });
            return cts;
        }
        protected CancellationTokenSource DoRepeatingTS<T>(int delayMs, int intervalMs, uint? iterations, Action<T> func, T t)
                where T : struct {
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }
            CancellationTokenSource cts = new();
            DoTS(async (t) => {
                await InterfaceManager.Delay(delayMs);

                uint totalIterations = iterations ?? uint.MaxValue;
                var initTime = Clock.UtcNow;
                for (int i = 0; i < totalIterations; ++i) {
                    if (cts.IsCancellationRequested) { break; }
                    func(t);
                    var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                    await InterfaceManager.Delay((int)delayTime);
                }
            }, t);
            return cts;
        }
        protected CancellationTokenSource DoRepeatingTS<T, U>(int delayMs, int intervalMs, uint? iterations, Action<T, U> func, T t, U u)
                where T : struct
                where U : struct {
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }
            CancellationTokenSource cts = new();
            DoTS(async (t, u) => {
                await InterfaceManager.Delay(delayMs);

                uint totalIterations = iterations ?? uint.MaxValue;
                var initTime = Clock.UtcNow;
                for (int i = 0; i < totalIterations; ++i) {
                    if (cts.IsCancellationRequested) { break; }
                    func(t, u);
                    var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                    await InterfaceManager.Delay((int)delayTime);
                }
            }, t, u);
            return cts;
        }
        protected CancellationTokenSource DoRepeatingTS<T, U, V>(int delayMs, int intervalMs, uint? iterations, Action<T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }
            CancellationTokenSource cts = new();
            DoTS(async (t, u, v) => {
                await InterfaceManager.Delay(delayMs);

                uint totalIterations = iterations ?? uint.MaxValue;
                var initTime = Clock.UtcNow;
                for (int i = 0; i < totalIterations; ++i) {
                    if (cts.IsCancellationRequested) { break; }
                    func(t, u, v);
                    var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                    await InterfaceManager.Delay((int)delayTime);
                }
            }, t, u, v);
            return cts;
        }
        protected CancellationTokenSource DoRepeatingTS<T, U, V, W>(int delayMs, int intervalMs, uint? iterations, Action<T, U, V, W> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }
            CancellationTokenSource cts = new();
            DoTS(async (t, u, v, w) => {
                await InterfaceManager.Delay(delayMs);

                uint totalIterations = iterations ?? uint.MaxValue;
                var initTime = Clock.UtcNow;
                for (int i = 0; i < totalIterations; ++i) {
                    if (cts.IsCancellationRequested) { break; }
                    func(t, u, v, w);
                    var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                    await InterfaceManager.Delay((int)delayTime);
                }
            }, t, u, v, w);
            return cts;
        }

        protected CancellationTokenSource DoRepeatingTS(int delayMs, int intervalMs, uint? iterations, Func<Task> func) {
            if (intervalMs <= 0) {
                throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})");
            }
            CancellationTokenSource cts = new();
            DoTS(async () => {
                await InterfaceManager.Delay(delayMs);

                uint totalIterations = iterations ?? uint.MaxValue;
                var initTime = Clock.UtcNow;
                for (int i = 0; i < totalIterations; ++i) {
                    if (cts.IsCancellationRequested) { break; }
                    await func();
                    var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                    await InterfaceManager.Delay((int)delayTime);
                }
            });
            return cts;
        }
        protected CancellationTokenSource DoRepeatingTS<T>(int delayMs, int intervalMs, uint? iterations, Func<T, Task> func, T t)
                where T : struct {
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }
            CancellationTokenSource cts = new();
            DoTS(async (t) => {
                await InterfaceManager.Delay(delayMs);

                uint totalIterations = iterations ?? uint.MaxValue;
                var initTime = Clock.UtcNow;
                for (int i = 0; i < totalIterations; ++i) {
                    if (cts.IsCancellationRequested) { break; }
                    await func(t);
                    var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                    await InterfaceManager.Delay((int)delayTime);
                }
            }, t);
            return cts;
        }
        protected CancellationTokenSource DoRepeatingTS<T, U>(int delayMs, int intervalMs, uint? iterations, Func<T, U, Task> func, T t, U u)
                where T : struct
                where U : struct {
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }
            CancellationTokenSource cts = new();
            DoTS(async (t, u) => {
                await InterfaceManager.Delay(delayMs);

                uint totalIterations = iterations ?? uint.MaxValue;
                var initTime = Clock.UtcNow;
                for (int i = 0; i < totalIterations; ++i) {
                    if (cts.IsCancellationRequested) { break; }
                    await func(t, u);
                    var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                    await InterfaceManager.Delay((int)delayTime);
                }
            }, t, u);
            return cts;
        }
        protected CancellationTokenSource DoRepeatingTS<T, U, V>(int delayMs, int intervalMs, uint? iterations, Func<T, U, V, Task> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }
            CancellationTokenSource cts = new();
            DoTS(async (t, u, v) => {
                await InterfaceManager.Delay(delayMs);

                uint totalIterations = iterations ?? uint.MaxValue;
                var initTime = Clock.UtcNow;
                for (int i = 0; i < totalIterations; ++i) {
                    if (cts.IsCancellationRequested) { break; }
                    await func(t, u, v);
                    var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                    await InterfaceManager.Delay((int)delayTime);
                }
            }, t, u, v);
            return cts;
        }
        protected CancellationTokenSource DoRepeatingTS<T, U, V, W>(int delayMs, int intervalMs, uint? iterations, Func<T, U, V, W, Task> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }
            CancellationTokenSource cts = new();
            DoTS(async (t, u, v, w) => {
                await InterfaceManager.Delay(delayMs);

                uint totalIterations = iterations ?? uint.MaxValue;
                var initTime = Clock.UtcNow;
                for (int i = 0; i < totalIterations; ++i) {
                    if (cts.IsCancellationRequested) { break; }
                    await func(t, u, v, w);
                    var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                    await InterfaceManager.Delay((int)delayTime);
                }
            }, t, u, v, w);
            return cts;
        }

        // DoWaitFor

        protected async Task DoWaitForTS(Action func) {
            if (cts.IsCancellationRequested) {
                throw new OperationCanceledException("EventLoop has been stopped already.");
            }
            await StartTask(func);
        }
        protected async Task DoWaitForTS<T>(Action<T> func, T t)
                where T : struct {
            AssertBlittable<T>();
            await DoWaitForTS(() => { func(t); });
        }
        protected async Task DoWaitForTS<T, U>(Action<T, U> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable<T, U>();
            await DoWaitForTS(() => { func(t, u); });
        }
        protected async Task DoWaitForTS<T, U, V>(Action<T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable<T, U, V>();
            await DoWaitForTS(() => { func(t, u, v); });
        }
        protected async Task DoWaitForTS<T, U, V, W>(Action<T, U, V, W> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable<T, U, V, W>();
            await DoWaitForTS(() => { func(t, u, v, w); });
        }

        protected async Task DoWaitForTS(Func<Task> func) {
            if (cts.IsCancellationRequested) {
                throw new OperationCanceledException("EventLoop has been stopped already.");
            }
            await await StartTask(func);
        }
        protected async Task DoWaitForTS<T>(Func<T, Task> func, T t)
                where T : struct {
            AssertBlittable<T>();
            await DoWaitForTS(async () => { await func(t); });
        }
        protected async Task DoWaitForTS<T, U>(Func<T, U, Task> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable<T, U>();
            await DoWaitForTS(async () => { await func(t, u); });
        }
        protected async Task DoWaitForTS<T, U, V>(Func<T, U, V, Task> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable<T, U, V>();
            await DoWaitForTS(async () => { await func(t, u, v); });
        }
        protected async Task DoWaitForTS<T, U, V, W>(Func<T, U, V, W, Task> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable<T, U, V, W>();
            await DoWaitForTS(async () => { await func(t, u, v, w); });
        }

        // DoGet

        protected async Task<Z> DoGetTS<Z>(Func<Z> func)
                where Z : struct {
            AssertBlittable<Z>();
            return await StartTask<Z>(func);
        }
        protected async Task<Z> DoGetTS<T, Z>(Func<T, Z> func, T t)
                where T : struct
                where Z : struct {
            AssertBlittable<T>();
            return await DoGetTS(() => { return func(t); });
        }
        protected async Task<Z> DoGetTS<T, U, Z>(Func<T, U, Z> func, T t, U u)
                where T : struct
                where U : struct
                where Z : struct {
            AssertBlittable<T, U>();
            return await DoGetTS(() => { return func(t, u); });
        }
        protected async Task<Z> DoGetTS<T, U, V, Z>(Func<T, U, V, Z> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
                where Z : struct {
            AssertBlittable<T, U, V>();
            return await DoGetTS(() => { return func(t, u, v); });
        }
        protected async Task<Z> DoGetTS<T, U, V, W, Z>(Func<T, U, V, W, Z> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct
                where Z : struct {
            AssertBlittable<T, U, V, W>();
            return await DoGetTS(() => { return func(t, u, v, w); });
        }

        protected async Task<Z> DoGetTS<Z>(Func<Task<Z>> func)
                where Z : struct {
            AssertBlittable<Z>();
            return await await StartTask(func);
        }
        protected async Task<Z> DoGetTS<T, Z>(Func<T, Task<Z>> func, T t)
                where T : struct
                where Z : struct {
            AssertBlittable<T>();
            return await DoGetTS(async () => { return await func(t); });
        }
        protected async Task<Z> DoGetTS<T, U, Z>(Func<T, U, Task<Z>> func, T t, U u)
                where T : struct
                where U : struct
                where Z : struct {
            AssertBlittable<T, U>();
            return await DoGetTS(async () => { return await func(t, u); });
        }
        protected async Task<Z> DoGetTS<T, U, V, Z>(Func<T, U, V, Task<Z>> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
                where Z : struct {
            AssertBlittable<T, U, V>();
            return await DoGetTS(async () => { return await func(t, u, v); });
        }
        protected async Task<Z> DoGetTS<T, U, V, W, Z>(Func<T, U, V, W, Task<Z>> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct
                where Z : struct {
            AssertBlittable<T, U, V, W>();
            return await DoGetTS(async () => { return await func(t, u, v, w); });
        }


        // DoGetRelaxed
        // Only use these methods if you create the value in the function and you know it isn't used anywhere else
        // TODO: JPB: (needed) Figure out how to handle non-blittable return types in DoGet
        //            And add tests for these

        protected async Task<Z> DoGetRelaxedTS<Z>(Func<Z> func) {
            return await StartTask<Z>(func);
        }
        protected async Task<Z> DoGetRelaxedTS<T, Z>(Func<T, Z> func, T t)
                where T : struct {
            return await DoGetRelaxedTS(() => { return func(t); });
        }

        protected async Task<Z> DoGetRelaxedTS<Z>(Func<Task<Z>> func) {
            return await await StartTask(func);
        }
        protected async Task<Z> DoGetRelaxedTS<T, Z>(Func<T, Task<Z>> func, T t)
                where T : struct {
            AssertBlittable<T>();
            return await DoGetRelaxedTS(async () => { return await func(t); });
        }


        // Helper Functions for error handling

        private static Action TaskErrorHandler(Action func) {
            return () => {
                try {
                    func();
                } catch (Exception e) {
                    ErrorNotifier.ErrorTS(e);
                }
            };
        }
        private static Func<Task> TaskErrorHandler(Func<Task> func) {
            return async () => {
                try {
                    await func();
                } catch (Exception e) {
                    ErrorNotifier.ErrorTS(e);
                }
            };
        }
        private static Func<Task<Z>> TaskErrorHandler<Z>(Func<Task<Z>> func) {
            return async () => {
                try {
                    var ret = await func();
                    return ret;
                } catch (Exception e) {
                    ErrorNotifier.ErrorTS(e);
                    throw e; // never called
                }
            };
        }
        private static Func<Z> TaskErrorHandler<Z>(Func<Z> func) {
            return () => {
                try {
                    var ret = func();
                    return ret;
                } catch (Exception e) {
                    ErrorNotifier.ErrorTS(e);
                    throw e; // never called
                }
            };
        }

#if !UNITY_WEBGL && !UNITY_EDITOR // System.Threading
        private async Task StartTask(Action func) {
            cts.Token.ThrowIfCancellationRequested();
            return await Task.Factory.StartNew(TaskErrorHandler(func), cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
        }
        //private Task<Task> StartTask(Func<Task> func) {
        //    return Task.Factory.StartNew(TaskErrorHandler(func), cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
        //}
        //private Task<Task<Z>> StartTask<Z>(Func<Task<Z>> func) {
        //    return Task.Factory.StartNew(TaskErrorHandler(func), cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
        //}
        private async Task<Z> StartTask<Z>(Func<Z> func) {
            cts.Token.ThrowIfCancellationRequested();
            return await Task.Factory.StartNew(TaskErrorHandler(func), cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
        }
#else
        private async Task StartTask(Action func) {
            cts.Token.ThrowIfCancellationRequested();
            await Task.Factory.StartNew(TaskErrorHandler(func), cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
        }
        //private Task<Task> StartTask(Func<Task> func) {
        //    return Task.Factory.StartNew(TaskErrorHandler(func), cts.Token);
        //}
        //private Task<Task<Z>> StartTask<Z>(Func<Task<Z>> func) {
        //    return Task.Factory.StartNew(TaskErrorHandler(func), cts.Token);
        //}
        private async Task<Z> StartTask<Z>(Func<Z> func) {
            cts.Token.ThrowIfCancellationRequested();
            return await Task.Factory.StartNew(TaskErrorHandler(func), cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
        }
#endif

    }
}