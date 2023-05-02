using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
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

        public EventLoop() {
            scheduler = new SingleThreadTaskScheduler(cts.Token);
            manager = InterfaceManager.Instance;
            manager.eventLoops.Add(this);

            // Init threadlocal variables
            Do(async () => {
                var _ = Clock.UtcNow;
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
            if (await Task.WhenAny(task, timeoutTask) == timeoutTask) {
                var msg = timeoutMessage ?? $"Task Timed out after {timeoutMs}ms";
                throw new TimeoutException(timeoutMessage);
            }
        }
        protected async Task<Z> TimeoutTask<Z>(Task<Z> task, int timeoutMs, string timeoutMessage = null) {
            Task timeoutTask = InterfaceManager.Delay(timeoutMs, cts.Token);
            if (await Task.WhenAny(task, timeoutTask) == timeoutTask) {
                var msg = timeoutMessage ?? $"Task Timed out after {timeoutMs}ms";
            }
            return task.Result;
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

        protected void Do(Action func) {
            if (cts.IsCancellationRequested) {
                throw new OperationCanceledException("EventLoop has been stopped already.");
            }
#if !UNITY_WEBGL || UNITY_EDITOR // System.Threading
            StartTask(func);
            //StartTask(() => { ErrorNotifier.Error(new Exception("Test")); func(); });
#else
        func();
#endif
        }
        protected void Do<T>(Action<T> func, T t)
                where T : struct {
            AssertBlittable(t);
            Do(() => { func(t); });
        }
        protected void Do<T, U>(Action<T, U> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable(t, u);
            Do(() => { func(t, u); });
        }
        protected void Do<T, U, V>(Action<T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable(t, u, v);
            Do(() => { func(t, u, v); });
        }
        protected void Do<T, U, V, W>(Action<T, U, V, W> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable(t, u, v, w);
            Do(() => { func(t, u, v, w); });
        }

        protected void Do(Func<Task> func) {
            if (cts.IsCancellationRequested) {
                throw new OperationCanceledException("EventLoop has been stopped already.");
            }
#if !UNITY_WEBGL || UNITY_EDITOR // System.Threading
            StartTask(func);
#else
        func();
#endif
        }
        protected void Do<T>(Func<T, Task> func, T t)
                where T : struct {
            AssertBlittable(t);
            Do(async () => { await func(t); });
        }
        protected void Do<T, U>(Func<T, U, Task> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable(t, u);
            Do(async () => { await func(t, u); });
        }
        protected void Do<T, U, V>(Func<T, U, V, Task> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable(t, u, v);
            Do(async () => { await func(t, u, v); });
        }
        protected void Do<T, U, V, W>(Func<T, U, V, W, Task> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable(t, u, v, w);
            Do(async () => { await func(t, u, v, w); });
        }

        // DoIn

        protected void DoIn(int delayMs, Action func) {
            Do(async () => {
                await InterfaceManager.Delay(delayMs);
                func();
            });
        }
        protected void DoIn<T>(int delayMs, Action<T> func, T t)
                where T : struct {
            Do(async () => {
                await InterfaceManager.Delay(delayMs);
                func(t);
            });
        }
        protected void DoIn<T, U>(int delayMs, Action<T, U> func, T t, U u)
                where T : struct
                where U : struct {
            Do(async () => {
                await InterfaceManager.Delay(delayMs);
                func(t, u);
            });
        }
        protected void DoIn<T, U, V>(int delayMs, Action<T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            Do(async () => {
                await InterfaceManager.Delay(delayMs);
                func(t, u, v);
            });
        }
        protected void DoIn<T, U, V, W>(int delayMs, Action<T, U, V, W> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            Do(async () => {
                await InterfaceManager.Delay(delayMs);
                func(t, u, v, w);
            });
        }

        protected void DoIn(int delayMs, Func<Task> func) {
            Do(async () => {
                await InterfaceManager.Delay(delayMs);
                await func();
            });
        }
        protected void DoIn<T>(int delayMs, Func<T, Task> func, T t)
                where T : struct {
            Do(async () => {
                await InterfaceManager.Delay(delayMs);
                await func(t);
            });
        }
        protected void DoIn<T, U>(int delayMs, Func<T, U, Task> func, T t, U u)
                where T : struct
                where U : struct {
            Do(async () => {
                await InterfaceManager.Delay(delayMs);
                await func(t, u);
            });
        }
        protected void DoIn<T, U, V>(int delayMs, Func<T, U, V, Task> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            Do(async () => {
                await InterfaceManager.Delay(delayMs);
                await func(t, u, v);
            });
        }
        protected void DoIn<T, U, V, W>(int delayMs, Func<T, U, V, W, Task> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            Do(async () => {
                await InterfaceManager.Delay(delayMs);
                await func(t, u, v, w);
            });
        }

        // DoRepeating

        protected CancellationTokenSource DoRepeating(int delayMs, int intervalMs, uint? iterations, Action func) {
            if (intervalMs <= 0) {
                throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})");
            }
            CancellationTokenSource cts = new();
            Do(async () => {
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
        protected CancellationTokenSource DoRepeating<T>(int delayMs, int intervalMs, uint? iterations, Action<T> func, T t)
                where T : struct {
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }
            CancellationTokenSource cts = new();
            Do(async (t) => {
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
        protected CancellationTokenSource DoRepeating<T, U>(int delayMs, int intervalMs, uint? iterations, Action<T, U> func, T t, U u)
                where T : struct
                where U : struct {
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }
            CancellationTokenSource cts = new();
            Do(async (t, u) => {
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
        protected CancellationTokenSource DoRepeating<T, U, V>(int delayMs, int intervalMs, uint? iterations, Action<T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }
            CancellationTokenSource cts = new();
            Do(async (t, u, v) => {
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
        protected CancellationTokenSource DoRepeating<T, U, V, W>(int delayMs, int intervalMs, uint? iterations, Action<T, U, V, W> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }
            CancellationTokenSource cts = new();
            Do(async (t, u, v, w) => {
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

        protected CancellationTokenSource DoRepeating(int delayMs, int intervalMs, uint? iterations, Func<Task> func) {
            if (intervalMs <= 0) {
                throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})");
            }
            CancellationTokenSource cts = new();
            Do(async () => {
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
        protected CancellationTokenSource DoRepeating<T>(int delayMs, int intervalMs, uint? iterations, Func<T, Task> func, T t)
                where T : struct {
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }
            CancellationTokenSource cts = new();
            Do(async (t) => {
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
        protected CancellationTokenSource DoRepeating<T, U>(int delayMs, int intervalMs, uint? iterations, Func<T, U, Task> func, T t, U u)
                where T : struct
                where U : struct {
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }
            CancellationTokenSource cts = new();
            Do(async (t, u) => {
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
        protected CancellationTokenSource DoRepeating<T, U, V>(int delayMs, int intervalMs, uint? iterations, Func<T, U, V, Task> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }
            CancellationTokenSource cts = new();
            Do(async (t, u, v) => {
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
        protected CancellationTokenSource DoRepeating<T, U, V, W>(int delayMs, int intervalMs, uint? iterations, Func<T, U, V, W, Task> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }
            CancellationTokenSource cts = new();
            Do(async (t, u, v, w) => {
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

        protected Task DoWaitFor(Action func) {
            if (cts.IsCancellationRequested) {
                throw new OperationCanceledException("EventLoop has been stopped already.");
            }
#if !UNITY_WEBGL || UNITY_EDITOR // System.Threading
            return StartTask(func);
#else
        return func();
#endif
        }
        protected Task DoWaitFor<T>(Action<T> func, T t)
                where T : struct {
            AssertBlittable(t);
            return DoWaitFor(() => { func(t); });
        }
        protected Task DoWaitFor<T, U>(Action<T, U> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable(t, u);
            return DoWaitFor(() => { func(t, u); });
        }
        protected Task DoWaitFor<T, U, V>(Action<T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable(t, u, v);
            return DoWaitFor(() => { func(t, u, v); });
        }
        protected Task DoWaitFor<T, U, V, W>(Action<T, U, V, W> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable(t, u, v, w);
            return DoWaitFor(() => { func(t, u, v, w); });
        }

        protected Task DoWaitFor(Func<Task> func) {
            if (cts.IsCancellationRequested) {
                throw new OperationCanceledException("EventLoop has been stopped already.");
            }
#if !UNITY_WEBGL || UNITY_EDITOR // System.Threading
            return StartTask(func).Unwrap();
#else
        return func();
#endif
        }
        protected Task DoWaitFor<T>(Func<T, Task> func, T t)
                where T : struct {
            AssertBlittable(t);
            return DoWaitFor(async () => { await func(t); });
        }
        protected Task DoWaitFor<T, U>(Func<T, U, Task> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable(t, u);
            return DoWaitFor(async () => { await func(t, u); });
        }
        protected Task DoWaitFor<T, U, V>(Func<T, U, V, Task> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable(t, u, v);
            return DoWaitFor(async () => { await func(t, u, v); });
        }
        protected Task DoWaitFor<T, U, V, W>(Func<T, U, V, W, Task> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable(t, u, v, w);
            return DoWaitFor(async () => { await func(t, u, v, w); });
        }

        // DoGet

        protected Task<Z> DoGet<Z>(Func<Z> func)
                where Z : struct {
#if !UNITY_WEBGL || UNITY_EDITOR // System.Threading
            Func<Z> safeFunc = () => {
                var ret = func();
                AssertBlittable(ret);
                return ret;
            };
            return StartTask<Z>(safeFunc);
#else
            var ret = func();
            AssertBlittable(ret);
            return ret;
#endif
        }
        protected Task<Z> DoGet<T, Z>(Func<T, Z> func, T t)
                where T : struct
                where Z : struct {
            AssertBlittable(t);
            return DoGet(() => { return func(t); });
        }
        protected Task<Z> DoGet<T, U, Z>(Func<T, U, Z> func, T t, U u)
                where T : struct
                where U : struct
                where Z : struct {
            AssertBlittable(t, u);
            return DoGet(() => { return func(t, u); });
        }
        protected Task<Z> DoGet<T, U, V, Z>(Func<T, U, V, Z> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
                where Z : struct {
            AssertBlittable(t, u, v);
            return DoGet(() => { return func(t, u, v); });
        }
        protected Task<Z> DoGet<T, U, V, W, Z>(Func<T, U, V, W, Z> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct
                where Z : struct {
            AssertBlittable(t, u, v, w);
            return DoGet(() => { return func(t, u, v, w); });
        }

        protected Task<Z> DoGet<Z>(Func<Task<Z>> func)
                where Z : struct {
#if !UNITY_WEBGL || UNITY_EDITOR // System.Threading
            Func<Task<Z>> safeFunc = async () => {
                var ret = await func();
                AssertBlittable(ret);
                return ret;
            };
            return StartTask(safeFunc).Unwrap();
#else
            var ret = func();
            AssertBlittable(ret);
            return ret;
#endif
        }
        protected Task<Z> DoGet<T, Z>(Func<T, Task<Z>> func, T t)
                where T : struct
                where Z : struct {
            AssertBlittable(t);
            return DoGet(async () => { return await func(t); });
        }
        protected Task<Z> DoGet<T, U, Z>(Func<T, U, Task<Z>> func, T t, U u)
                where T : struct
                where U : struct
                where Z : struct {
            AssertBlittable(t, u);
            return DoGet(async () => { return await func(t, u); });
        }
        protected Task<Z> DoGet<T, U, V, Z>(Func<T, U, V, Task<Z>> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
                where Z : struct {
            AssertBlittable(t, u, v);
            return DoGet(async () => { return await func(t, u, v); });
        }
        protected Task<Z> DoGet<T, U, V, W, Z>(Func<T, U, V, W, Task<Z>> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct
                where Z : struct {
            AssertBlittable(t, u, v, w);
            return DoGet(async () => { return await func(t, u, v, w); });
        }


        // Helper Functions for error handling

        private static Action TaskErrorHandler(Action func) {
            return () => {
                try {
                    func();
                } catch (Exception e) {
                    ErrorNotifier.Error(e);
                    throw e;
                }
            };
        }
        private static Func<Task> TaskErrorHandler(Func<Task> func) {
            return async () => {
                try {
                    await func();
                } catch (Exception e) {
                    ErrorNotifier.Error(e);
                    throw e;
                }
            };
        }
        private static Func<Task<Z>> TaskErrorHandler<Z>(Func<Task<Z>> func) {
            return async () => {
                try {
                    var ret = await func();
                    return ret;
                } catch (Exception e) {
                    ErrorNotifier.Error(e);
                    throw e;
                }
            };
        }
        private static Func<Z> TaskErrorHandler<Z>(Func<Z> func) {
            return () => {
                try {
                    var ret = func();
                    return ret;
                } catch (Exception e) {
                    ErrorNotifier.Error(e);
                    throw e;
                }
            };
        }

        private Task StartTask(Action func) {
            return Task.Factory.StartNew(TaskErrorHandler(func), cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
        }
        private Task<Task> StartTask(Func<Task> func) {
            return Task.Factory.StartNew(TaskErrorHandler(func), cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
        }
        private Task<Task<Z>> StartTask<Z>(Func<Task<Z>> func) {
            return Task.Factory.StartNew(TaskErrorHandler(func), cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
        }
        private Task<Z> StartTask<Z>(Func<Z> func) {
            return Task.Factory.StartNew(TaskErrorHandler(func), cts.Token, TaskCreationOptions.DenyChildAttach, scheduler);
        }
    }

}