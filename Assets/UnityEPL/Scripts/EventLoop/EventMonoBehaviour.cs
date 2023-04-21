#define EVENTMONOBEHAVIOR_TASK_OPERATORS
#define EVENTMONOBEHAVIOR_MANUAL_RESULT_SET

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using static UnityEPL.Blittability;

// TODO: JPB: (refactor) Clean up EventMonoBehaviour with UniTask
//            https://github.com/Cysharp/UniTask
//            https://cysharp.github.io/UniTask/api/Cysharp.Threading.Tasks.UniTaskExtensions.html#Cysharp_Threading_Tasks_UniTaskExtensions_AsUniTask_Task_System_Boolean_
//            https://cysharp.github.io/UniTask/api/Cysharp.Threading.Tasks.UniTaskExtensions.html#Cysharp_Threading_Tasks_UniTaskExtensions_AsTask_Cysharp_Threading_Tasks_UniTask_
//            https://cysharp.github.io/UniTask/api/Cysharp.Threading.Tasks.UniTaskExtensions.html

namespace UnityEPL {

    public abstract class EventMonoBehaviour : MonoBehaviour
    {
        protected InterfaceManager manager;
        protected int threadID;

        protected abstract void AwakeOverride();
        protected void Awake() {
            manager = InterfaceManager.Instance;
            threadID = Thread.CurrentThread.ManagedThreadId;
            AwakeOverride();
        }

        // This function is used to guarentee that a function is being called from
        // the main unity thread
        protected void MonoBehaviourSafetyCheck() {
            if (threadID != Thread.CurrentThread.ManagedThreadId) {
                ErrorNotifier.Error(new InvalidOperationException(
                    "Cannot call this function from a non-unity thread.\n" +
                    "Try using the thread safe version of this method"));
            }
        }


        // -------------------------------------
        // DoMB
        // Acts just like a function call, but guarentees thread safety
        // -------------------------------------
        // TODO: JPB: (feature) Add support for cancellation tokens in EventMonoBehavior DoMB functions

        protected IEnumerator DoMB(Func<IEnumerator> func) {
            MonoBehaviourSafetyCheck();
            yield return func();
        }
        protected IEnumerator DoMB<T>(Func<T, IEnumerator> func, T t) {
            MonoBehaviourSafetyCheck();
            yield return func(t);
        }
        protected IEnumerator DoMB<T, U>(Func<T, U, IEnumerator> func, T t, U u) {
            MonoBehaviourSafetyCheck();
            yield return func(t, u);
        }
        protected IEnumerator DoMB<T, U, V>(Func<T, U, V, IEnumerator> func, T t, U u, V v) {
            MonoBehaviourSafetyCheck();
            yield return func(t, u, v);
        }
        protected IEnumerator DoMB<T, U, V, W>(Func<T, U, V, W, IEnumerator> func, T t, U u, V v, W w) {
            MonoBehaviourSafetyCheck();
            yield return func(t, u, v, w);
        }

        protected void DoMB(Action func) {
            MonoBehaviourSafetyCheck();
            func();
        }
        protected void DoMB<T>(Action<T> func, T t) {
            MonoBehaviourSafetyCheck();
            func(t);
        }
        protected void DoMB<T, U>(Action<T, U> func, T t, U u) {
            MonoBehaviourSafetyCheck();
            func(t, u);
        }
        protected void DoMB<T, U, V>(Action<T, U, V> func, T t, U u, V v) {
            MonoBehaviourSafetyCheck();
            func(t, u, v);
        }
        protected void DoMB<T, U, V, W>(Action<T, U, V, W> func, T t, U u, V v, W w) {
            MonoBehaviourSafetyCheck();
            func(t, u, v, w);
        }


        // -------------------------------------
        // Do
        // -------------------------------------
        // TODO: JPB: (feature) Add support for cancellation tokens in EventMonoBehavior Do functions

        private void DoHelper(IEnumerator enumerator) {
            manager.events.Enqueue(enumerator);
        }
        protected void Do(Func<IEnumerator> func)
        {
            DoHelper(func());
        }
        protected void Do<T>(Func<T, IEnumerator> func, T t)
                where T : struct {
            AssertBlittable(t);
            DoHelper(func(t));
        }
        protected void Do<T, U>(Func<T, U, IEnumerator> func, T t, U u)
                where T : struct
                where U : struct
        {
            AssertBlittable(t, u);
            DoHelper(func(t, u));
        }
        protected void Do<T, U, V>(Func<T, U, V, IEnumerator> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
        {
            AssertBlittable(t, u, v);
            DoHelper(func(t, u, v));
        }
        protected void Do<T, U, V, W>(Func<T, U, V, W, IEnumerator> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct
        {
            AssertBlittable(t, u, v, w);
            DoHelper(func(t, u, v, w));
        }

        protected void Do(Action func)
        {
            DoHelper(EnumeratorCaller(func));
        }
        protected void Do<T>(Action<T> func, T t)
                where T : struct
        {
            AssertBlittable(t);
            DoHelper(EnumeratorCaller(func, t));
        }
        protected void Do<T, U>(Action<T, U> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable(t);
            AssertBlittable(u);
            DoHelper(EnumeratorCaller(func, t, u));
        }
        protected void Do<T, U, V>(Action<T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable(t);
            AssertBlittable(u);
            AssertBlittable(v);
            DoHelper(EnumeratorCaller(func, t, u, v));
        }
        protected void Do<T, U, V, W>(Action<T, U, V, W> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable(t);
            AssertBlittable(u);
            AssertBlittable(v);
            AssertBlittable(w);
            DoHelper(EnumeratorCaller(func, t, u, v, w));
        }


        // -------------------------------------
        // DoIn
        // -------------------------------------

        protected void DoIn(int millisecondsDelay, Func<IEnumerator> func)
        {
            DoHelper(DelayedEnumeratorCaller(millisecondsDelay, func())); ;
        }
        protected void DoIn<T>(int millisecondsDelay, Func<T, IEnumerator> func, T t)
                where T : struct
        {
            AssertBlittable(t);
            DoHelper(DelayedEnumeratorCaller(millisecondsDelay, func(t)));
        }
        protected void DoIn<T, U>(int millisecondsDelay, Func<T, U, IEnumerator> func, T t, U u)
                where T : struct
                where U : struct
        {
            AssertBlittable(t, u);
            DoHelper(DelayedEnumeratorCaller(millisecondsDelay, func(t, u)));
        }
        protected void DoIn<T, U, V>(int millisecondsDelay, Func<T, U, V, IEnumerator> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
        {
            AssertBlittable(t, u, v);
            DoHelper(DelayedEnumeratorCaller(millisecondsDelay, func(t, u, v)));
        }
        protected void DoIn<T, U, V, W>(int millisecondsDelay, Func<T, U, V, W, IEnumerator> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct
        {
            AssertBlittable(t, u, v, w);
            DoHelper(DelayedEnumeratorCaller(millisecondsDelay, func(t, u, v, w)));
        }

        protected void DoIn(int millisecondsDelay, Action func)
        {
            DoHelper(DelayedEnumeratorCaller(millisecondsDelay, func));
        }
        protected void DoIn<T>(int millisecondsDelay, Action<T> func, T t)
                where T : struct
        {
            AssertBlittable(t);
            DoHelper(DelayedEnumeratorCaller(millisecondsDelay, func, t));
        }
        protected void DoIn<T, U>(int millisecondsDelay, Action<T, U> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable(t, u);
            DoHelper(DelayedEnumeratorCaller(millisecondsDelay, func, t, u));
        }
        protected void DoIn<T, U, V>(int millisecondsDelay, Action<T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable(t, u, v);
            DoHelper(DelayedEnumeratorCaller(millisecondsDelay, func, t, u, v));
        }
        protected void DoIn<T, U, V, W>(int millisecondsDelay, Action<T, U, V, W> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable(t, u, v, w);
            DoHelper(DelayedEnumeratorCaller(millisecondsDelay, func, t, u, v, w));
        }


        // -------------------------------------
        // DoRepeatingMB
        // -------------------------------------

        protected IEnumerator DoRepeatingMB(int delayMs, int intervalMs, uint? iterations, Func<IEnumerator> func) {
            MonoBehaviourSafetyCheck();
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();

            if (delayMs == 0) {
                var startTime = Clock.UtcNow;
                yield return func();
                delayMs = intervalMs - (int)(Clock.UtcNow - startTime).TotalMilliseconds;
                if (delayMs < 0) {
                    throw new TimeoutException("DoRepeating execution took longer than the interval assigned");
                } else if (iterations > 1) {
                    DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations - 1, func));
                }
            } else {
                DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func));
            }

            yield return cts;
        }
        protected IEnumerator DoRepeatingMB<T>(int delayMs, int intervalMs, uint? iterations, Func<T, IEnumerator> func, T t) {
            MonoBehaviourSafetyCheck();
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();

            if (delayMs == 0) {
                var startTime = Clock.UtcNow;
                yield return func(t);
                delayMs = intervalMs - (int)(Clock.UtcNow - startTime).TotalMilliseconds;
                if (delayMs < 0) {
                    throw new TimeoutException("DoRepeating execution took longer than the interval assigned");
                } else if (iterations > 1) {
                    DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations - 1, func, t));
                }
            } else {
                DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t));
            }

            yield return cts;
        }
        protected IEnumerator DoRepeatingMB<T, U>(int delayMs, int intervalMs, uint? iterations, Func<T, U, IEnumerator> func, T t, U u) {
            MonoBehaviourSafetyCheck();
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();

            if (delayMs == 0) {
                var startTime = Clock.UtcNow;
                yield return func(t, u);
                delayMs = intervalMs - (int)(Clock.UtcNow - startTime).TotalMilliseconds;
                if (delayMs < 0) {
                    throw new TimeoutException("DoRepeating execution took longer than the interval assigned");
                } else if (iterations > 1) {
                    DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations - 1, func, t, u));
                }
            } else {
                DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t, u));
            }

            yield return cts;
        }
        protected IEnumerator DoRepeatingMB<T, U, V>(int delayMs, int intervalMs, uint? iterations, Func<T, U, V, IEnumerator> func, T t, U u, V v) {
            MonoBehaviourSafetyCheck();
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();

            if (delayMs == 0) {
                var startTime = Clock.UtcNow;
                yield return func(t, u, v);
                delayMs = intervalMs - (int)(Clock.UtcNow - startTime).TotalMilliseconds;
                if (delayMs < 0) {
                    throw new TimeoutException("DoRepeating execution took longer than the interval assigned");
                } else if (iterations > 1) {
                    DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations - 1, func, t, u, v));
                }
            } else {
                DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t, u, v));
            }

            yield return cts;
        }
        protected IEnumerator DoRepeatingMB<T, U, V, W>(int delayMs, int intervalMs, uint? iterations, Func<T, U, V, W, IEnumerator> func, T t, U u, V v, W w) {
            MonoBehaviourSafetyCheck();
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();

            if (delayMs == 0) {
                var startTime = Clock.UtcNow;
                yield return func(t, u, v, w);
                delayMs = intervalMs - (int)(Clock.UtcNow - startTime).TotalMilliseconds;
                if (delayMs < 0) {
                    throw new TimeoutException("DoRepeating execution took longer than the interval assigned");
                } else if (iterations > 1) {
                    DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations - 1, func, t, u, v, w));
                }
            } else {
                DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t, u, v, w));
            }

            yield return cts;
        }

        protected CancellationTokenSource DoRepeatingMB(int delayMs, int intervalMs, uint? iterations, Action func) {
            MonoBehaviourSafetyCheck();
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            if (delayMs == 0) {
                var startTime = Clock.UtcNow;
                func();
                delayMs = intervalMs - (int)(Clock.UtcNow - startTime).TotalMilliseconds;
                if (delayMs < 0) {
                    throw new TimeoutException("DoRepeating execution took longer than the interval assigned");
                } else if (iterations > 1) {
                    DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations - 1, func));
                } 
            } else {
                DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func));
            }

            return cts;
        }
        protected CancellationTokenSource DoRepeatingMB<T>(int delayMs, int intervalMs, uint? iterations, Action<T> func, T t) {
            MonoBehaviourSafetyCheck();
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            if (delayMs == 0) {
                var startTime = Clock.UtcNow;
                func(t);
                delayMs = intervalMs - (int)(Clock.UtcNow - startTime).TotalMilliseconds;
                if (delayMs < 0) {
                    throw new TimeoutException("DoRepeating execution took longer than the interval assigned");
                } else if (iterations > 1) {
                    DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations - 1, func, t));
                }
            } else {
                DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t));
            }

            return cts;
        }
        protected CancellationTokenSource DoRepeatingMB<T, U>(int delayMs, int intervalMs, uint? iterations, Action<T, U> func, T t, U u) {
            MonoBehaviourSafetyCheck();
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            if (delayMs == 0) {
                var startTime = Clock.UtcNow;
                func(t, u);
                delayMs = intervalMs - (int)(Clock.UtcNow - startTime).TotalMilliseconds;
                if (delayMs < 0) {
                    throw new TimeoutException("DoRepeating execution took longer than the interval assigned");
                } else if (iterations > 1) {
                    DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations - 1, func, t, u));
                }
            } else {
                DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t, u));
            }

            return cts;
        }
        protected CancellationTokenSource DoRepeatingMB<T, U, V>(int delayMs, int intervalMs, uint? iterations, Action<T, U, V> func, T t, U u, V v) {
            MonoBehaviourSafetyCheck();
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            if (delayMs == 0) {
                var startTime = Clock.UtcNow;
                func(t, u, v);
                delayMs = intervalMs - (int)(Clock.UtcNow - startTime).TotalMilliseconds;
                if (delayMs < 0) {
                    throw new TimeoutException("DoRepeating execution took longer than the interval assigned");
                } else if (iterations > 1) {
                    DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations - 1, func, t, u, v));
                }
            } else {
                DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t, u, v));
            }

            return cts;
        }
        protected CancellationTokenSource DoRepeatingMB<T, U, V, W>(int delayMs, int intervalMs, uint? iterations, Action<T, U, V, W> func, T t, U u, V v, W w) {
            MonoBehaviourSafetyCheck();
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            if (delayMs == 0) {
                var startTime = Clock.UtcNow;
                func(t, u, v, w);
                delayMs = intervalMs - (int)(Clock.UtcNow - startTime).TotalMilliseconds;
                if (delayMs < 0) {
                    throw new TimeoutException("DoRepeating execution took longer than the interval assigned");
                } else if (iterations > 1) {
                    DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations - 1, func, t, u, v, w));
                }
            } else {
                DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t, u, v, w));
            }
            
            return cts;
        }


        // -------------------------------------
        // DoRepeating
        // -------------------------------------

        protected CancellationTokenSource DoRepeating(int delayMs, int intervalMs, uint? iterations, Func<IEnumerator> func) {
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func));
            return cts;
        }
        protected CancellationTokenSource DoRepeating<T>(int delayMs, int intervalMs, uint? iterations, Func<T, IEnumerator> func, T t)
                where T : struct {
            AssertBlittable(t);
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t));
            return cts;
        }
        protected CancellationTokenSource DoRepeating<T, U>(int delayMs, int intervalMs, uint? iterations, Func<T, U, IEnumerator> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable(t, u);
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t, u));
            return cts;
        }
        protected CancellationTokenSource DoRepeating<T, U, V>(int delayMs, int intervalMs, uint? iterations, Func<T, U, V, IEnumerator> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable(t, u, v);
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t, u, v));
            return cts;
        }
        protected CancellationTokenSource DoRepeating<T, U, V, W>(int delayMs, int intervalMs, uint? iterations, Func<T, U, V, W, IEnumerator> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable(t, u, v, w);
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t, u, v, w));
            return cts;
        }

        protected CancellationTokenSource DoRepeating(int delayMs, int intervalMs, uint? iterations, Action func) {
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func));
            return cts;
        }
        protected CancellationTokenSource DoRepeating<T>(int delayMs, int intervalMs, uint? iterations, Action<T> func, T t)
                where T : struct {
            AssertBlittable(t);
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t));
            return cts;
        }
        protected CancellationTokenSource DoRepeating<T, U>(int delayMs, int intervalMs, uint? iterations, Action<T, U> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable(t, u);
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t, u));
            return cts;
        }
        protected CancellationTokenSource DoRepeating<T, U, V>(int delayMs, int intervalMs, uint? iterations, Action<T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable(t, u, v);
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t, u, v));
            return cts;
        }
        protected CancellationTokenSource DoRepeating<T, U, V, W>(int delayMs, int intervalMs, uint? iterations, Action<T, U, V, W> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable(t, u, v, w);
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t, u, v, w));
            return cts;
        }


        // -------------------------------------
        // DoWaitFor
        // -------------------------------------

        private Task DoWaitForHelper(IEnumerator enumerator)
        {
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(TaskTrigger(tcs, enumerator));
            return tcs.Task;
        }
        protected Task DoWaitFor(Func<IEnumerator> func)
        {
            return DoWaitForHelper(func());
        }
        protected Task DoWaitFor<T>(Func<T, IEnumerator> func, T t)
                where T : struct
        {
            AssertBlittable(t);
            return DoWaitForHelper(func(t));
        }
        protected Task DoWaitFor<T, U>(Func<T, U, IEnumerator> func, T t, U u)
                where T : struct
                where U : struct
        {
            AssertBlittable(t, u);
            return DoWaitForHelper(func(t, u));
        }
        protected Task DoWaitFor<T, U, V>(Func<T, U, V, IEnumerator> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
        {
            AssertBlittable(t, u, v);
            return DoWaitForHelper(func(t, u, v));
        }
        protected Task DoWaitFor<T, U, V, W>(Func<T, U, V, W, IEnumerator> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct
        {
            AssertBlittable(t, u, v, w);
            return DoWaitForHelper(func(t, u, v, w));
        }

        protected Task DoWaitFor(Action func) {
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(TaskTrigger(tcs, func));
            return tcs.Task;
        }
        protected Task DoWaitFor<T>(Action<T> func, T t)
                where T : struct {
            AssertBlittable(t);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t));
            return tcs.Task;
        }
        protected Task DoWaitFor<T, U>(Action<T, U> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable(t, u);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t, u));
            return tcs.Task;
        }
        protected Task DoWaitFor<T, U, V>(Action<T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable(t, u, v);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t, u, v));
            return tcs.Task;
        }
        protected Task DoWaitFor<T, U, V, W>(Action<T, U, V, W> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable(t, u, v, w);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t, u, v, w));
            return tcs.Task;
        }

#if EVENTMONOBEHAVIOR_TASK_OPERATORS
        protected Task DoWaitFor(Func<Task> func) {
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(TaskTrigger(tcs, func));
            return tcs.Task;
        }
        protected Task DoWaitFor<T>(Func<T, Task> func, T t)
                where T : struct {
            AssertBlittable(t);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t));
            return tcs.Task;
        }
        protected Task DoWaitFor<T, U>(Func<T, U, Task> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable(t, u);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t, u));
            return tcs.Task;
        }
        protected Task DoWaitFor<T, U, V>(Func<T, U, V, Task> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable(t, u, v);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t, u, v));
            return tcs.Task;
        }
        protected Task DoWaitFor<T, U, V, W>(Func<T, U, V, W, Task> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable(t, u, v, w);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t, u, v, w));
            return tcs.Task;
        }
#endif // EVENTMONOBEHAVIOR_TASK_OPERATORS


        // -------------------------------------
        // DoGetMB
        // -------------------------------------

        protected Z DoGetMB<Z>(Func<Z> func) {
            MonoBehaviourSafetyCheck();
            return func();
        }
        protected Z DoGetMB<T, Z>(Func<T, Z> func, T t) {
            MonoBehaviourSafetyCheck();
            return func(t);
        }
        protected Z DoGetMB<T, U, Z>(Func<T, U, Z> func, T t, U u) {
            MonoBehaviourSafetyCheck();
            return func(t, u);
        }
        protected Z DoGetMB<T, U, V, Z>(Func<T, U, V, Z> func, T t, U u, V v) {
            MonoBehaviourSafetyCheck();
            return func(t, u, v);
        }
        protected Z DoGetMB<T, U, V, W, Z>(Func<T, U, V, W, Z> func, T t, U u, V v, W w) {
            MonoBehaviourSafetyCheck();
            return func(t, u, v, w);
        }


        // -------------------------------------
        // DoGet
        // -------------------------------------

        private Task<Z> DoGetHelper<Z>(IEnumerator enumerator)
                where Z : struct {
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(TaskTrigger(tcs, enumerator));
            return tcs.Task;
        }
        protected Task<Z> DoGet<Z>(Func<IEnumerator> func)
                where Z : struct {
            return DoGetHelper<Z>(func());
        }
        protected Task<Z> DoGet<T, Z>(Func<T, IEnumerator> func, T t)
                where T : struct
                where Z : struct {
            AssertBlittable(t);
            return DoGetHelper<Z>(func(t));
        }
        protected Task<Z> DoGet<T, U, Z>(Func<T, U, IEnumerator> func, T t, U u)
                where T : struct
                where U : struct
                where Z : struct {
            AssertBlittable(t, u);
            return DoGetHelper<Z>(func(t, u));
        }
        protected Task<Z> DoGet<T, U, V, Z>(Func<T, U, V, IEnumerator> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
                where Z : struct {
            AssertBlittable(t, u, v);
            return DoGetHelper<Z>(func(t, u, v));
        }
        protected Task<Z> DoGet<T, U, V, W, Z>(Func<T, U, V, W, IEnumerator> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct
                where Z : struct {
            AssertBlittable(t, u, v, w);
            return DoGetHelper<Z>(func(t, u, v, w));
        }

        protected Task<Z> DoGet<Z>(Func<Z> func)
                where Z : struct {
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(TaskTrigger(tcs, func));
            return tcs.Task;
        }
        protected Task<Z> DoGet<T, Z>(Func<T, Z> func, T t)
                where T : struct
                where Z : struct {
            AssertBlittable(t);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t));
            return tcs.Task;
        }
        protected Task<Z> DoGet<T, U, Z>(Func<T, U, Z> func, T t, U u)
                where T : struct
                where U : struct
                where Z : struct {
            AssertBlittable(t, u);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t, u));
            return tcs.Task;
        }
        protected Task<Z> DoGet<T, U, V, Z>(Func<T, U, V, Z> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
                where Z : struct {
            AssertBlittable(t, u, v);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t, u, v));
            return tcs.Task;
        }
        protected Task<Z> DoGet<T, U, V, W, Z>(Func<T, U, V, W, Z> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct
                where Z : struct {
            AssertBlittable(t, u, v, w);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t, u, v, w));
            return tcs.Task;
        }

#if EVENTMONOBEHAVIOR_TASK_OPERATORS
        protected Task<Z> DoGet<Z>(Func<Task<Z>> func)
                where Z : struct {
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(TaskTrigger(tcs, func));
            return tcs.Task;
        }
        protected Task<Z> DoGet<T, Z>(Func<T, Task<Z>> func, T t)
                where T : struct
                where Z : struct {
            AssertBlittable(t);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t));
            return tcs.Task;
        }
        protected Task<Z> DoGet<T, U, Z>(Func<T, U, Task<Z>> func, T t, U u)
                where T : struct
                where U : struct
                where Z : struct {
            AssertBlittable(t, u);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t, u));
            return tcs.Task;
        }
        protected Task<Z> DoGet<T, U, V, Z>(Func<T, U, V, Task<Z>> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
                where Z : struct {
            AssertBlittable(t, u, v);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t, u, v));
            return tcs.Task;
        }
        protected Task<Z> DoGet<T, U, V, W, Z>(Func<T, U, V, W, Task<Z>> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct
                where Z : struct {
            AssertBlittable(t, u, v, w);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t, u, v, w));
            return tcs.Task;
        }
#endif // EVENTMONOBEHAVIOR_TASK_OPERATORS

#if EVENTMONOBEHAVIOR_MANUAL_RESULT_SET
        // -------------------------------------
        // DoWaitForManualTrigger
        // 
        // User is responsible for triggering these TaskCompletionSources
        // Do NOT use this unless you really know what you're doing (and there is no other option)
        // 
        // // JPB: This is currently used in the InputManager
        // -------------------------------------

        protected Task DoWaitForManualTrigger(Func<TaskCompletionSource<bool>, IEnumerator> func) {
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(func(tcs));
            return tcs.Task;
        }
        protected Task DoWaitForManualTrigger<T>(Func<TaskCompletionSource<bool>, T, IEnumerator> func, T t)
                where T : struct {
            AssertBlittable(t);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(func(tcs, t));
            return tcs.Task;
        }
        protected Task DoWaitForManualTrigger<T, U>(Func<TaskCompletionSource<bool>, T, U, IEnumerator> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable(t, u);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(func(tcs, t, u));
            return tcs.Task;
        }
        protected Task DoWaitForManualTrigger<T, U, V>(Func<TaskCompletionSource<bool>, T, U, V, IEnumerator> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable(t, u, v);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(func(tcs, t, u, v));
            return tcs.Task;
        }

        protected Task DoWaitForManualTrigger(Action<TaskCompletionSource<bool>> func) {
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs));
            return tcs.Task;
        }
        protected Task DoWaitForManualTrigger<T>(Action<TaskCompletionSource<bool>, T> func, T t)
                where T : struct {
            AssertBlittable(t);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t));
            return tcs.Task;
        }
        protected Task DoWaitForManualTrigger<T, U>(Action<TaskCompletionSource<bool>, T, U> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable(t, u);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t, u));
            return tcs.Task;
        }
        protected Task DoWaitForManualTrigger<T, U, V>(Action<TaskCompletionSource<bool>, T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable(t, u, v);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t, u, v));
            return tcs.Task;
        }

#if EVENTMONOBEHAVIOR_TASK_OPERATORS
        protected Task DoWaitForManualTrigger(Func<TaskCompletionSource<bool>, Task> func) {
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs));
            return tcs.Task;
        }
        protected Task DoWaitForManualTrigger<T>(Func<TaskCompletionSource<bool>, T, Task> func, T t)
                where T : struct {
            AssertBlittable(t);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t));
            return tcs.Task;
        }
        protected Task DoWaitForManualTrigger<T, U>(Func<TaskCompletionSource<bool>, T, U, Task> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable(t, u);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t, u));
            return tcs.Task;
        }
        protected Task DoWaitForManualTrigger<T, U, V>(Func<TaskCompletionSource<bool>, T, U, V, Task> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable(t, u, v);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t, u, v));
            return tcs.Task;
        }
#endif // EVENTMONOBEHAVIOR_TASK_OPERATORS

        // -------------------------------------
        // DoGetManualTrigger
        // 
        // User is responsible for triggering these TaskCompletionSources with the result
        // Do NOT use this unless you really know what you're doing (and there is no other option)
        // 
        // JPB: This is currently used in the InputManager
        // 
        // TODO: JPB: (bug) There is a bug in these that is does not guarentee that the returned result is actually blittable
        // It only guarentees that it is a struct
        // The struct could have a reference (aka a class) in it, which would create an unsafe reference across threads
        // The calling passed in IEnumerator/Func has to call AssertBlittable on the result
        // -------------------------------------

        protected Task<Z> DoGetManualTrigger<Z>(Func<TaskCompletionSource<Z>, IEnumerator> func) {
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(func(tcs));
            return tcs.Task;
        }
        protected Task<Z> DoGetManualTrigger<T, Z>(Func<TaskCompletionSource<Z>, T, IEnumerator> func, T t)
                where T : struct {
            AssertBlittable(t);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(func(tcs, t));
            return tcs.Task;
        }
        protected Task<Z> DoGetManualTrigger<T, U, Z>(Func<TaskCompletionSource<Z>, T, U, IEnumerator> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable(t, u);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(func(tcs, t, u));
            return tcs.Task;
        }
        protected Task<Z> DoGetManualTrigger<T, U, V, Z>(Func<TaskCompletionSource<Z>, T, U, V, IEnumerator> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable(t, u, v);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(func(tcs, t, u, v));
            return tcs.Task;
        }

        protected Task<Z> DoGetManualTrigger<Z>(Action<TaskCompletionSource<Z>> func)
                where Z : struct {
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs));
            return tcs.Task;
        }
        protected Task<Z> DoGetManualTrigger<T, Z>(Action<TaskCompletionSource<Z>, T> func, T t)
                where T : struct
                where Z : struct {
            AssertBlittable(t);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t));
            return tcs.Task;
        }
        protected Task<Z> DoGetManualTrigger<T, U, Z>(Action<TaskCompletionSource<Z>, T, U> func, T t, U u)
                where T : struct
                where U : struct
                where Z : struct {
            AssertBlittable(t, u);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t, u));
            return tcs.Task;
        }
        protected Task<Z> DoGetManualTrigger<T, U, V, Z>(Action<TaskCompletionSource<Z>, T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
                where Z : struct {
            AssertBlittable(t, u, v);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t, u, v));
            return tcs.Task;
        }

#if EVENTMONOBEHAVIOR_TASK_OPERATORS
        protected Task<Z> DoGetManualTrigger<Z>(Func<TaskCompletionSource<Z>, Task> func)
                where Z : struct {
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs));
            return tcs.Task;
        }
        protected Task<Z> DoGetManualTrigger<T, Z>(Func<TaskCompletionSource<Z>, T, Task> func, T t)
                where T : struct
                where Z : struct {
            AssertBlittable(t);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t));
            return tcs.Task;
        }
        protected Task<Z> DoGetManualTrigger<T, U, Z>(Func<TaskCompletionSource<Z>, T, U, Task> func, T t, U u)
                where T : struct
                where U : struct
                where Z : struct {
            AssertBlittable(t, u);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t, u));
            return tcs.Task;
        }
        protected Task<Z> DoGetManualTrigger<T, U, V, Z>(Func<TaskCompletionSource<Z>, T, U, V, Task> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
                where Z : struct {
            AssertBlittable(t, u, v);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t, u, v));
            return tcs.Task;
        }
#endif // EVENTMONOBEHAVIOR_TASK_OPERATORS
#endif // EVENTMONOBEHAVIOR_MANUAL_RESULT_SET


        // -------------------------------------
        // EnumeratorCaller
        // -------------------------------------

        private IEnumerator EnumeratorCaller(Action func) {
            func();
            yield break;
        }
        private IEnumerator EnumeratorCaller<T>(Action<T> func, T t) {
            func(t);
            yield break;
        }
        private IEnumerator EnumeratorCaller<T, U>(Action<T, U> func, T t, U u) {
            func(t, u);
            yield break;
        }
        private IEnumerator EnumeratorCaller<T, U, V>(Action<T, U, V> func, T t, U u, V v) {
            func(t, u, v);
            yield break;
        }
        private IEnumerator EnumeratorCaller<T, U, V, W>(Action<T, U, V, W> func, T t, U u, V v, W w) {
            func(t, u, v, w);
            yield break;
        }

#if EVENTMONOBEHAVIOR_MANUAL_RESULT_SET
        private IEnumerator EnumeratorCaller<T>(Func<T, Task> func, T t) {
            yield return func(t).ToEnumerator();
        }
        private IEnumerator EnumeratorCaller<T, U>(Func<T, U, Task> func, T t, U u) {
            yield return func(t, u).ToEnumerator();
        }
        private IEnumerator EnumeratorCaller<T, U, V>(Func<T, U, V, Task> func, T t, U u, V v) {
            yield return func(t, u, v).ToEnumerator();
        }
        private IEnumerator EnumeratorCaller<T, U, V, W>(Func<T, U, V, W, Task> func, T t, U u, V v, W w) {
            yield return func(t, u, v, w).ToEnumerator();
        }
#endif // EVENTMONOBEHAVIOR_MANUAL_RESULT_SET


        // -------------------------------------
        // DelayedEnumeratorCaller
        // -------------------------------------

        private IEnumerator DelayedEnumeratorCaller(int millisecondsDelay, IEnumerator func) {
            yield return InterfaceManager.DelayE(millisecondsDelay);
            yield return func;
        }
        private IEnumerator DelayedEnumeratorCaller(int millisecondsDelay, Action func) {
            yield return InterfaceManager.DelayE(millisecondsDelay);
            func();
        }
        private IEnumerator DelayedEnumeratorCaller<T>(int millisecondsDelay, Action<T> func, T t) {
            yield return InterfaceManager.DelayE(millisecondsDelay);
            func(t);
        }
        private IEnumerator DelayedEnumeratorCaller<T, U>(int millisecondsDelay, Action<T, U> func, T t, U u) {
            yield return InterfaceManager.DelayE(millisecondsDelay);
            func(t, u);
        }
        private IEnumerator DelayedEnumeratorCaller<T, U, V>(int millisecondsDelay, Action<T, U, V> func, T t, U u, V v) {
            yield return InterfaceManager.DelayE(millisecondsDelay);
            func(t, u, v);
        }
        private IEnumerator DelayedEnumeratorCaller<T, U, V, W>(int millisecondsDelay, Action<T, U, V, W> func, T t, U u, V v, W w) {
            yield return InterfaceManager.DelayE(millisecondsDelay);
            func(t, u, v, w);
        }


        // -------------------------------------
        // RepeatingEnumeratorCaller
        // -------------------------------------

        private IEnumerator RepeatingEnumeratorCaller(CancellationTokenSource cts, int delayMs, int intervalMs, uint? iterations, Func<IEnumerator> func) {
            if (delayMs != 0) { yield return InterfaceManager.DelayE(delayMs); }

            uint totalIterations = iterations ?? uint.MaxValue;
            var initTime = Clock.UtcNow;
            for (int i = 0; i < totalIterations; ++i) {
                if (cts.IsCancellationRequested) { break; }
                yield return func();
                var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                if (delayTime < 0) { throw new TimeoutException("DoRepeating execution took longer than the interval assigned"); }
                yield return InterfaceManager.DelayE((int)delayTime);
            }
        }
        private IEnumerator RepeatingEnumeratorCaller<T>(CancellationTokenSource cts, int delayMs, int intervalMs, uint? iterations, Func<T, IEnumerator> func, T t) {
            if (delayMs != 0) { yield return InterfaceManager.DelayE(delayMs); }

            uint totalIterations = iterations ?? uint.MaxValue;
            var initTime = Clock.UtcNow;
            for (int i = 0; i < totalIterations; ++i) {
                if (cts.IsCancellationRequested) { break; }
                yield return func(t);
                var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                if (delayTime < 0) { throw new TimeoutException("DoRepeating execution took longer than the interval assigned"); }
                yield return InterfaceManager.DelayE((int)delayTime);
            }
        }
        private IEnumerator RepeatingEnumeratorCaller<T, U>(CancellationTokenSource cts, int delayMs, int intervalMs, uint? iterations, Func<T, U, IEnumerator> func, T t, U u) {
            if (delayMs != 0) { yield return InterfaceManager.DelayE(delayMs); }

            uint totalIterations = iterations ?? uint.MaxValue;
            var initTime = Clock.UtcNow;
            for (int i = 0; i < totalIterations; ++i) {
                if (cts.IsCancellationRequested) { break; }
                yield return func(t, u);
                var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                if (delayTime < 0) { throw new TimeoutException("DoRepeating execution took longer than the interval assigned"); }
                yield return InterfaceManager.DelayE((int)delayTime);
            }
        }
        private IEnumerator RepeatingEnumeratorCaller<T, U, V>(CancellationTokenSource cts, int delayMs, int intervalMs, uint? iterations, Func<T, U, V, IEnumerator> func, T t, U u, V v) {
            if (delayMs != 0) { yield return InterfaceManager.DelayE(delayMs); }

            uint totalIterations = iterations ?? uint.MaxValue;
            var initTime = Clock.UtcNow;
            for (int i = 0; i < totalIterations; ++i) {
                if (cts.IsCancellationRequested) { break; }
                yield return func(t, u, v);
                var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                if (delayTime < 0) { throw new TimeoutException("DoRepeating execution took longer than the interval assigned"); }
                yield return InterfaceManager.DelayE((int)delayTime);
            }
        }
        private IEnumerator RepeatingEnumeratorCaller<T, U, V, W>(CancellationTokenSource cts, int delayMs, int intervalMs, uint? iterations, Func<T, U, V, W, IEnumerator> func, T t, U u, V v, W w) {
            if (delayMs != 0) { yield return InterfaceManager.DelayE(delayMs); }

            uint totalIterations = iterations ?? uint.MaxValue;
            var initTime = Clock.UtcNow;
            for (int i = 0; i < totalIterations; ++i) {
                if (cts.IsCancellationRequested) { break; }
                yield return func(t, u, v, w);
                var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                if (delayTime < 0) { throw new TimeoutException("DoRepeating execution took longer than the interval assigned"); }
                yield return InterfaceManager.DelayE((int)delayTime);
            }
        }

        private IEnumerator RepeatingEnumeratorCaller(CancellationTokenSource cts, int delayMs, int intervalMs, uint? iterations, Action func) {
            if (delayMs != 0) { yield return InterfaceManager.DelayE(delayMs); }

            uint totalIterations = iterations ?? uint.MaxValue;
            var initTime = Clock.UtcNow;
            for (int i = 0; i < totalIterations; ++i) {
                if (cts.IsCancellationRequested) { break; }
                func();
                var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                if (delayTime < 0) { throw new TimeoutException("DoRepeating execution took longer than the interval assigned"); }
                yield return InterfaceManager.DelayE((int)delayTime);
            }
        }
        private IEnumerator RepeatingEnumeratorCaller<T>(CancellationTokenSource cts, int delayMs, int intervalMs, uint? iterations, Action<T> func, T t) {
            if (delayMs != 0) { yield return InterfaceManager.DelayE(delayMs); }

            uint totalIterations = iterations ?? uint.MaxValue;
            var initTime = Clock.UtcNow;
            for (int i = 0; i < totalIterations; ++i) {
                if (cts.IsCancellationRequested) { break; }
                func(t);
                var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                if (delayTime < 0) { throw new TimeoutException("DoRepeating execution took longer than the interval assigned"); }
                yield return InterfaceManager.DelayE((int)delayTime);
            }
        }
        private IEnumerator RepeatingEnumeratorCaller<T, U>(CancellationTokenSource cts, int delayMs, int intervalMs, uint? iterations, Action<T, U> func, T t, U u) {
            if (delayMs != 0) { yield return InterfaceManager.DelayE(delayMs); }

            uint totalIterations = iterations ?? uint.MaxValue;
            var initTime = Clock.UtcNow;
            for (int i = 0; i < totalIterations; ++i) {
                if (cts.IsCancellationRequested) { break; }
                func(t, u);
                var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                if (delayTime < 0) { throw new TimeoutException("DoRepeating execution took longer than the interval assigned"); }
                yield return InterfaceManager.DelayE((int)delayTime);
            }
        }
        private IEnumerator RepeatingEnumeratorCaller<T, U, V>(CancellationTokenSource cts, int delayMs, int intervalMs, uint? iterations, Action<T, U, V> func, T t, U u, V v) {
            if (delayMs != 0) { yield return InterfaceManager.DelayE(delayMs); }

            uint totalIterations = iterations ?? uint.MaxValue;
            var initTime = Clock.UtcNow;
            for (int i = 0; i < totalIterations; ++i) {
                if (cts.IsCancellationRequested) { break; }
                func(t, u, v);
                var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                if (delayTime < 0) { throw new TimeoutException("DoRepeating execution took longer than the interval assigned"); }
                yield return InterfaceManager.DelayE((int)delayTime);
            }
        }
        private IEnumerator RepeatingEnumeratorCaller<T, U, V, W>(CancellationTokenSource cts, int delayMs, int intervalMs, uint? iterations, Action<T, U, V, W> func, T t, U u, V v, W w) {
            if (delayMs != 0) { yield return InterfaceManager.DelayE(delayMs); }

            uint totalIterations = iterations ?? uint.MaxValue;
            var initTime = Clock.UtcNow;
            for (int i = 0; i < totalIterations; ++i) {
                if (cts.IsCancellationRequested) { break; }
                func(t, u, v, w);
                var delayTime = (i + 1) * intervalMs - (Clock.UtcNow - initTime).TotalMilliseconds;
                if (delayTime < 0) { throw new TimeoutException("DoRepeating execution took longer than the interval assigned"); }
                yield return InterfaceManager.DelayE((int)delayTime);
            }
        }


        // -------------------------------------
        // TaskTrigger
        // Enumerator, Action, Function, and Task to Enumerator
        // -------------------------------------

        private IEnumerator TaskTrigger(TaskCompletionSource<bool> tcs, IEnumerator func) {
            yield return func;
            tcs.SetResult(true);
        }

        private IEnumerator TaskTrigger(TaskCompletionSource<bool> tcs, Action func) {
            func();
            tcs.SetResult(true);
            yield break;
        }
        private IEnumerator TaskTrigger<T>(TaskCompletionSource<bool> tcs, Action<T> func, T t) {
            func(t);
            tcs.SetResult(true);
            yield break;
        }
        private IEnumerator TaskTrigger<T, U>(TaskCompletionSource<bool> tcs, Action<T, U> func, T t, U u) {
            func(t, u);
            tcs.SetResult(true);
            yield break;
        }
        private IEnumerator TaskTrigger<T, U, V>(TaskCompletionSource<bool> tcs, Action<T, U, V> func, T t, U u, V v) {
            func(t, u, v);
            tcs.SetResult(true);
            yield break;
        }
        private IEnumerator TaskTrigger<T, U, V, W>(TaskCompletionSource<bool> tcs, Action<T, U, V, W> func, T t, U u, V v, W w) {
            func(t, u, v, w);
            tcs.SetResult(true);
            yield break;
        }

#if EVENTMONOBEHAVIOR_TASK_OPERATORS
        private IEnumerator TaskTrigger(TaskCompletionSource<bool> tcs, Func<Task> func) {
            yield return func().ToEnumerator();
            tcs.SetResult(true);
        }
        private IEnumerator TaskTrigger<T>(TaskCompletionSource<bool> tcs, Func<T, Task> func, T t) {
            yield return func(t).ToEnumerator();
            tcs.SetResult(true);
        }
        private IEnumerator TaskTrigger<T, U>(TaskCompletionSource<bool> tcs, Func<T, U, Task> func, T t, U u) {
            yield return func(t, u).ToEnumerator();
            tcs.SetResult(true);
        }
        private IEnumerator TaskTrigger<T, U, V>(TaskCompletionSource<bool> tcs, Func<T, U, V, Task> func, T t, U u, V v) {
            yield return func(t, u, v).ToEnumerator();
            tcs.SetResult(true);
        }
        private IEnumerator TaskTrigger<T, U, V, W>(TaskCompletionSource<bool> tcs, Func<T, U, V, W, Task> func, T t, U u, V v, W w) {
            yield return func(t, u, v, w).ToEnumerator();
            tcs.SetResult(true);
        }
#endif // EVENTMONOBEHAVIOR_TASK_OPERATORS

        private IEnumerator TaskTrigger<Z>(TaskCompletionSource<Z> tcs, IEnumerator func)
        where Z : struct {
            yield return func;
            Z ret = (Z)func.Current;
            AssertBlittable(ret);
            tcs.SetResult(ret);
        }

        private IEnumerator TaskTrigger<Z>(TaskCompletionSource<Z> tcs, Func<Z> func)
                where Z : struct {
            Z ret = func();
            AssertBlittable(ret);
            tcs.SetResult(ret);
            yield break;
        }
        private IEnumerator TaskTrigger<T, Z>(TaskCompletionSource<Z> tcs, Func<T, Z> func, T t)
                where Z : struct {
            Z ret = func(t);
            AssertBlittable(ret);
            tcs.SetResult(ret);
            yield break;
        }
        private IEnumerator TaskTrigger<T, U, Z>(TaskCompletionSource<Z> tcs, Func<T, U, Z> func, T t, U u)
                where Z : struct {
            Z ret = func(t, u);
            AssertBlittable(ret);
            tcs.SetResult(ret);
            yield break;
        }
        private IEnumerator TaskTrigger<T, U, V, Z>(TaskCompletionSource<Z> tcs, Func<T, U, V, Z> func, T t, U u, V v)
                where Z : struct {
            Z ret = func(t, u, v);
            AssertBlittable(ret);
            tcs.SetResult(ret);
            yield break;
        }
        private IEnumerator TaskTrigger<T, U, V, W, Z>(TaskCompletionSource<Z> tcs, Func<T, U, V, W, Z> func, T t, U u, V v, W w)
                where Z : struct {
            Z ret = func(t, u, v, w);
            AssertBlittable(ret);
            tcs.SetResult(ret);
            yield break;
        }

#if EVENTMONOBEHAVIOR_TASK_OPERATORS
        private IEnumerator TaskTrigger<Z>(TaskCompletionSource<Z> tcs, Func<Task<Z>> func)
                where Z : struct {
            var task = func();
            yield return task.ToEnumerator();
            Z ret = task.Result;
            AssertBlittable(ret);
            tcs.SetResult(ret);
        }
        private IEnumerator TaskTrigger<T, Z>(TaskCompletionSource<Z> tcs, Func<T, Task<Z>> func, T t)
                where Z : struct {
            var task = func(t);
            yield return task.ToEnumerator();
            Z ret = task.Result;
            AssertBlittable(ret);
            tcs.SetResult(ret);
        }
        private IEnumerator TaskTrigger<T, U, Z>(TaskCompletionSource<Z> tcs, Func<T, U, Task<Z>> func, T t, U u)
                where Z : struct {
            var task = func(t, u);
            yield return task.ToEnumerator();
            Z ret = task.Result;
            AssertBlittable(ret);
            tcs.SetResult(ret);
        }
        private IEnumerator TaskTrigger<T, U, V, Z>(TaskCompletionSource<Z> tcs, Func<T, U, V, Task<Z>> func, T t, U u, V v)
                where Z : struct {
            var task = func(t, u, v);
            yield return task.ToEnumerator();
            Z ret = task.Result;
            AssertBlittable(ret);
            tcs.SetResult(ret);
        }
        private IEnumerator TaskTrigger<T, U, V, W, Z>(TaskCompletionSource<Z> tcs, Func<T, U, V, W, Task<Z>> func, T t, U u, V v, W w)
                where Z : struct {
            var task = func(t, u, v, w);
            yield return task.ToEnumerator();
            Z ret = task.Result;
            AssertBlittable(ret);
            tcs.SetResult(ret);
        }
#endif // EVENTMONOBEHAVIOR_TASK_OPERATORS
    }

}