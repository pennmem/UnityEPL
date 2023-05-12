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

    public abstract class EventMonoBehaviour : MonoBehaviour {
        protected InterfaceManager manager;
        protected int threadID;
        protected bool awakeCompleted = false;

        protected abstract void AwakeOverride();
        protected void Awake() {
            manager = InterfaceManager.Instance;
            threadID = Thread.CurrentThread.ManagedThreadId;
            AwakeOverride();
            awakeCompleted = true;
        }

        // This function is used to check if an EventMonoBehvaiour has finished it's awake call
        public async Task<bool> IsAwakeCompleted() {
            return await DoGet(IsAwakeCompletedHelper);
        }
        public bool IsAwakeCompletedMB() {
            return DoGetMB(IsAwakeCompletedHelper);
        }
        protected Bool IsAwakeCompletedHelper() {
            return awakeCompleted;
        }

        // This function is used to guarentee that a function is being called from
        // the main unity thread
        protected void MonoBehaviourSafetyCheck() {
            //Debug.Log($"{threadID} {Thread.CurrentThread.ManagedThreadId}");
            if (threadID != Thread.CurrentThread.ManagedThreadId) {
                ErrorNotifier.Error(new InvalidOperationException(
                    "Cannot call this function from a non-unity thread.\n" +
                    "Try using the thread safe version of this method"));
            }
        }

        /// <summary>
        /// Run an iterator function that might throw an exception.
        /// Handle the exception by using the ErrorNotifier
        /// Handle pausing as well
        /// https://www.jacksondunstan.com/articles/3718
        /// If for some reason this project needs a to define a custom Enumerator instead of using this function,
        /// it is also defined in this webpage (this may also be more efficient...)
        /// </summary>
        /// <param name="enumerator">Iterator function to run</param>
        /// <returns>An enumerator that runs the given enumerator</returns>
        /// /// TODO: JPB: (needed) Implement pausing in MakeEventEnumerator
        private IEnumerator MakeEventEnumerator(IEnumerator enumerator) {
            while (true) {
                object current;
                try {
                    if (enumerator.MoveNext() == false) {
                        break;
                    }
                    current = enumerator.Current;
                } catch (Exception e) {
                    ErrorNotifier.Error(e);
                    yield break;
                }
                yield return current;
            }
        }
        /// <summary>
        /// This replaces the normal MonoBehaviour::StartCoroutine and adds other features,
        /// such as exception handling and pausing
        /// </summary>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        /// TODO: JPB: (needed) (bug) Add error safety to DoMB, DoRepeatingMB, DoWaitForMB, and DoGetMB
        protected new Coroutine StartCoroutine(IEnumerator enumerator) {
            return base.StartCoroutine(MakeEventEnumerator(enumerator));
        }


        // -------------------------------------
        // DoMB
        // Acts just like a function call, but guarentees thread safety
        // -------------------------------------
        // TODO: JPB: (feature) Add support for cancellation tokens in EventMonoBehavior DoMB functions

        protected void DoMB(Action func) {
            MonoBehaviourSafetyCheck();
            try {
                func();
            } catch (Exception e) {
                ErrorNotifier.Error(e);
            }
            
        }
        protected void DoMB<T>(Action<T> func, T t) {
            MonoBehaviourSafetyCheck();
            try {
                func(t);
            } catch (Exception e) {
                ErrorNotifier.Error(e);
            }
        }
        protected void DoMB<T, U>(Action<T, U> func, T t, U u) {
            MonoBehaviourSafetyCheck();
            try {
                func(t, u);
            } catch (Exception e) {
                ErrorNotifier.Error(e);
            }
        }
        protected void DoMB<T, U, V>(Action<T, U, V> func, T t, U u, V v) {
            MonoBehaviourSafetyCheck();
            try {
                func(t, u, v);
            } catch (Exception e) {
                ErrorNotifier.Error(e);
            }
        }
        protected void DoMB<T, U, V, W>(Action<T, U, V, W> func, T t, U u, V v, W w) {
            MonoBehaviourSafetyCheck();
            try {
                func(t, u, v, w);
            } catch (Exception e) {
                ErrorNotifier.Error(e);
            }
        }


        // -------------------------------------
        // Do
        // -------------------------------------
        // TODO: JPB: (feature) Add support for cancellation tokens in EventMonoBehavior Do functions

        private void DoHelper(IEnumerator enumerator) {
            manager.events.Enqueue(enumerator);
        }

        protected void Do(Func<IEnumerator> func) {
            DoHelper(func());
        }
        protected void Do<T>(Func<T, IEnumerator> func, T t)
                where T : struct {
            AssertBlittable<T>();
            DoHelper(func(t));
        }
        protected void Do<T, U>(Func<T, U, IEnumerator> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable<T, U>();
            DoHelper(func(t, u));
        }
        protected void Do<T, U, V>(Func<T, U, V, IEnumerator> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable<T, U, V>();
            DoHelper(func(t, u, v));
        }
        protected void Do<T, U, V, W>(Func<T, U, V, W, IEnumerator> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable<T, U, V, W>();
            DoHelper(func(t, u, v, w));
        }

        protected void Do(Action func) {
            DoHelper(EnumeratorCaller(func));
        }
        protected void Do<T>(Action<T> func, T t)
                where T : struct {
            AssertBlittable<T>();
            DoHelper(EnumeratorCaller(func, t));
        }
        protected void Do<T, U>(Action<T, U> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable<T, U>();
            DoHelper(EnumeratorCaller(func, t, u));
        }
        protected void Do<T, U, V>(Action<T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable<T, U, V>();
            DoHelper(EnumeratorCaller(func, t, u, v));
        }
        protected void Do<T, U, V, W>(Action<T, U, V, W> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable<T, U, V, W>();
            DoHelper(EnumeratorCaller(func, t, u, v, w));
        }


        // -------------------------------------
        // DoIn
        // -------------------------------------

        protected void DoIn(int millisecondsDelay, Func<IEnumerator> func) {
            DoHelper(DelayedEnumeratorCaller(millisecondsDelay, func()));
            ;
        }
        protected void DoIn<T>(int millisecondsDelay, Func<T, IEnumerator> func, T t)
                where T : struct {
            AssertBlittable<T>();
            DoHelper(DelayedEnumeratorCaller(millisecondsDelay, func(t)));
        }
        protected void DoIn<T, U>(int millisecondsDelay, Func<T, U, IEnumerator> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable<T, U>();
            DoHelper(DelayedEnumeratorCaller(millisecondsDelay, func(t, u)));
        }
        protected void DoIn<T, U, V>(int millisecondsDelay, Func<T, U, V, IEnumerator> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable<T, U, V>();
            DoHelper(DelayedEnumeratorCaller(millisecondsDelay, func(t, u, v)));
        }
        protected void DoIn<T, U, V, W>(int millisecondsDelay, Func<T, U, V, W, IEnumerator> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable<T, U, V, W>();
            DoHelper(DelayedEnumeratorCaller(millisecondsDelay, func(t, u, v, w)));
        }

        protected void DoIn(int millisecondsDelay, Action func) {
            DoHelper(DelayedEnumeratorCaller(millisecondsDelay, func));
        }
        protected void DoIn<T>(int millisecondsDelay, Action<T> func, T t)
                where T : struct {
            AssertBlittable<T>();
            DoHelper(DelayedEnumeratorCaller(millisecondsDelay, func, t));
        }
        protected void DoIn<T, U>(int millisecondsDelay, Action<T, U> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable<T, U>();
            DoHelper(DelayedEnumeratorCaller(millisecondsDelay, func, t, u));
        }
        protected void DoIn<T, U, V>(int millisecondsDelay, Action<T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable<T, U, V>();
            DoHelper(DelayedEnumeratorCaller(millisecondsDelay, func, t, u, v));
        }
        protected void DoIn<T, U, V, W>(int millisecondsDelay, Action<T, U, V, W> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable<T, U, V, W>();
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
                yield return MakeEventEnumerator(func());
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
                yield return MakeEventEnumerator(func(t));
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
                yield return MakeEventEnumerator(func(t, u));
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
                yield return MakeEventEnumerator(func(t, u, v));
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
                yield return MakeEventEnumerator(func(t, u, v, w));
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
            AssertBlittable<T>();
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t));
            return cts;
        }
        protected CancellationTokenSource DoRepeating<T, U>(int delayMs, int intervalMs, uint? iterations, Func<T, U, IEnumerator> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable<T, U>();
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t, u));
            return cts;
        }
        protected CancellationTokenSource DoRepeating<T, U, V>(int delayMs, int intervalMs, uint? iterations, Func<T, U, V, IEnumerator> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable<T, U, V>();
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
            AssertBlittable<T, U, V, W>();
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
            AssertBlittable<T>();
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t));
            return cts;
        }
        protected CancellationTokenSource DoRepeating<T, U>(int delayMs, int intervalMs, uint? iterations, Action<T, U> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable<T, U>();
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t, u));
            return cts;
        }
        protected CancellationTokenSource DoRepeating<T, U, V>(int delayMs, int intervalMs, uint? iterations, Action<T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable<T, U, V>();
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
            AssertBlittable<T, U, V, W>();
            if (intervalMs <= 0) { throw new ArgumentOutOfRangeException($"intervalMs <= 0 ({intervalMs})"); }

            CancellationTokenSource cts = new();
            DoHelper(RepeatingEnumeratorCaller(cts, delayMs, intervalMs, iterations, func, t, u, v, w));
            return cts;
        }


        // -------------------------------------
        // DoWaitForMB
        // -------------------------------------

        protected IEnumerator DoWaitForMB(Func<IEnumerator> func) {
            MonoBehaviourSafetyCheck();
            yield return MakeEventEnumerator(func());
        }
        protected IEnumerator DoWaitForMB<T>(Func<T, IEnumerator> func, T t) {
            MonoBehaviourSafetyCheck();
            yield return MakeEventEnumerator(func(t));
        }
        protected IEnumerator DoWaitForMB<T, U>(Func<T, U, IEnumerator> func, T t, U u) {
            MonoBehaviourSafetyCheck();
            yield return MakeEventEnumerator(func(t, u));
        }
        protected IEnumerator DoWaitForMB<T, U, V>(Func<T, U, V, IEnumerator> func, T t, U u, V v) {
            MonoBehaviourSafetyCheck();
            yield return MakeEventEnumerator(func(t, u, v));
        }
        protected IEnumerator DoWaitForMB<T, U, V, W>(Func<T, U, V, W, IEnumerator> func, T t, U u, V v, W w) {
            MonoBehaviourSafetyCheck();
            yield return MakeEventEnumerator(func(t, u, v, w));
        }

#if EVENTMONOBEHAVIOR_TASK_OPERATORS
        protected async Task DoWaitForMB(Func<Task> func) {
            MonoBehaviourSafetyCheck();
            try {
                await func();
            } catch (Exception e) {
                ErrorNotifier.Error(e);
                throw e; // This is a duplication, but C# can't tell Error always throws an exception
            }
        }
        protected async Task DoWaitForMB<T>(Func<T, Task> func, T t) {
            MonoBehaviourSafetyCheck();
            try {
                await func(t);
            } catch (Exception e) {
                ErrorNotifier.Error(e);
                throw e; // This is a duplication, but C# can't tell Error always throws an exception
            }
        }
        protected async Task DoWaitForMB<T, U>(Func<T, U, Task> func, T t, U u) {
            MonoBehaviourSafetyCheck();
            try {
                await func(t, u);
            } catch (Exception e) {
                ErrorNotifier.Error(e);
                throw e; // This is a duplication, but C# can't tell Error always throws an exception
            }
        }
        protected async Task DoWaitForMB<T, U, V>(Func<T, U, V, Task> func, T t, U u, V v) {
            MonoBehaviourSafetyCheck();
            try {
                await func(t, u, v);
            } catch (Exception e) {
                ErrorNotifier.Error(e);
                throw e; // This is a duplication, but C# can't tell Error always throws an exception
            }
        }
        protected async Task DoWaitForMB<T, U, V, W>(Func<T, U, V, W, Task> func, T t, U u, V v, W w) {
            MonoBehaviourSafetyCheck();
            try {
                await func(t, u, v, w);
            } catch (Exception e) {
                ErrorNotifier.Error(e);
                throw e; // This is a duplication, but C# can't tell Error always throws an exception
            }
        }
#endif // EVENTMONOBEHAVIOR_TASK_OPERATORS

        // -------------------------------------
        // DoWaitFor
        // -------------------------------------

        private Task DoWaitForHelper(IEnumerator enumerator) {
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(TaskTrigger(tcs, enumerator));
            return tcs.Task;
        }
        protected Task DoWaitFor(Func<IEnumerator> func) {
            return DoWaitForHelper(func());
        }
        protected Task DoWaitFor<T>(Func<T, IEnumerator> func, T t)
                where T : struct {
            AssertBlittable<T>();
            return DoWaitForHelper(func(t));
        }
        protected Task DoWaitFor<T, U>(Func<T, U, IEnumerator> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable<T, U>();
            return DoWaitForHelper(func(t, u));
        }
        protected Task DoWaitFor<T, U, V>(Func<T, U, V, IEnumerator> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable<T, U, V>();
            return DoWaitForHelper(func(t, u, v));
        }
        protected Task DoWaitFor<T, U, V, W>(Func<T, U, V, W, IEnumerator> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable<T, U, V, W>();
            return DoWaitForHelper(func(t, u, v, w));
        }

        protected Task DoWaitFor(Action func) {
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(TaskTrigger(tcs, func));
            return tcs.Task;
        }
        protected Task DoWaitFor<T>(Action<T> func, T t)
                where T : struct {
            AssertBlittable<T>();
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t));
            return tcs.Task;
        }
        protected Task DoWaitFor<T, U>(Action<T, U> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable<T, U>();
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t, u));
            return tcs.Task;
        }
        protected Task DoWaitFor<T, U, V>(Action<T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable<T, U, V>();
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t, u, v));
            return tcs.Task;
        }
        protected Task DoWaitFor<T, U, V, W>(Action<T, U, V, W> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable<T, U, V, W>();
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
            AssertBlittable<T>();
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t));
            return tcs.Task;
        }
        protected Task DoWaitFor<T, U>(Func<T, U, Task> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable<T, U>();
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t, u));
            return tcs.Task;
        }
        protected Task DoWaitFor<T, U, V>(Func<T, U, V, Task> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable<T, U, V>();
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t, u, v));
            return tcs.Task;
        }
        protected Task DoWaitFor<T, U, V, W>(Func<T, U, V, W, Task> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            AssertBlittable<T, U, V, W>();
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
            try {
                return func();
            } catch (Exception e) {
                ErrorNotifier.Error(e);
                throw e; // This is a duplication, but C# can't tell Error always throws an exception
            }
        }
        protected Z DoGetMB<T, Z>(Func<T, Z> func, T t) {
            MonoBehaviourSafetyCheck();
            try {
                return func(t);
            } catch (Exception e) {
                ErrorNotifier.Error(e);
                throw e; // This is a duplication, but C# can't tell Error always throws an exception
            }
        }
        protected Z DoGetMB<T, U, Z>(Func<T, U, Z> func, T t, U u) {
            MonoBehaviourSafetyCheck();
            try {
                return func(t, u);
            } catch (Exception e) {
                ErrorNotifier.Error(e);
                throw e; // This is a duplication, but C# can't tell Error always throws an exception
            }
        }
        protected Z DoGetMB<T, U, V, Z>(Func<T, U, V, Z> func, T t, U u, V v) {
            MonoBehaviourSafetyCheck();
            try {
                return func(t, u, v);
            } catch (Exception e) {
                ErrorNotifier.Error(e);
                throw e; // This is a duplication, but C# can't tell Error always throws an exception
            }
        }
        protected Z DoGetMB<T, U, V, W, Z>(Func<T, U, V, W, Z> func, T t, U u, V v, W w) {
            MonoBehaviourSafetyCheck();
            try {
                return func(t, u, v, w);
            } catch (Exception e) {
                ErrorNotifier.Error(e);
                throw e; // This is a duplication, but C# can't tell Error always throws an exception
            }
        }

#if EVENTMONOBEHAVIOR_TASK_OPERATORS
        protected async Task<Z> DoGetMB<Z>(Func<Task<Z>> func) {
            MonoBehaviourSafetyCheck();
            try {
                return await func();
            } catch (Exception e) {
                ErrorNotifier.Error(e);
                throw e; // This is a duplication, but C# can't tell Error always throws an exception
            }
        }
        protected async Task<Z> DoGetMB<T, Z>(Func<T, Task<Z>> func, T t) {
            MonoBehaviourSafetyCheck();
            try {
                return await func(t);
            } catch (Exception e) {
                ErrorNotifier.Error(e);
                throw e; // This is a duplication, but C# can't tell Error always throws an exception
            }
        }
        protected async Task<Z> DoGetMB<T, U, Z>(Func<T, U, Task<Z>> func, T t, U u) {
            MonoBehaviourSafetyCheck();
            try {
                return await func(t, u);
            } catch (Exception e) {
                ErrorNotifier.Error(e);
                throw e; // This is a duplication, but C# can't tell Error always throws an exception
            }
        }
        protected async Task<Z> DoGetMB<T, U, V, Z>(Func<T, U, V, Task<Z>> func, T t, U u, V v) {
            MonoBehaviourSafetyCheck();
            try {
                return await func(t, u, v);
            } catch (Exception e) {
                ErrorNotifier.Error(e);
                throw e; // This is a duplication, but C# can't tell Error always throws an exception
            }
        }
        protected async Task<Z> DoGetMB<T, U, V, W, Z>(Func<T, U, V, W, Task<Z>> func, T t, U u, V v, W w) {
            MonoBehaviourSafetyCheck();
            try {
                return await func(t, u, v, w);
            } catch (Exception e) {
                ErrorNotifier.Error(e);
                throw e; // This is a duplication, but C# can't tell Error always throws an exception
            }
        }
#endif // EVENTMONOBEHAVIOR_TASK_OPERATORS

        // -------------------------------------
        // DoGet
        // -------------------------------------

        private Task<Z> DoGetHelper<Z>(IEnumerator enumerator)
                where Z : struct {
            AssertBlittable<Z>();
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
            AssertBlittable<T>();
            return DoGetHelper<Z>(func(t));
        }
        protected Task<Z> DoGet<T, U, Z>(Func<T, U, IEnumerator> func, T t, U u)
                where T : struct
                where U : struct
                where Z : struct {
            AssertBlittable<T, U>();
            return DoGetHelper<Z>(func(t, u));
        }
        protected Task<Z> DoGet<T, U, V, Z>(Func<T, U, V, IEnumerator> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
                where Z : struct {
            AssertBlittable<T, U, V>();
            return DoGetHelper<Z>(func(t, u, v));
        }
        protected Task<Z> DoGet<T, U, V, W, Z>(Func<T, U, V, W, IEnumerator> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct
                where Z : struct {
            AssertBlittable<T, U, V, W>();
            return DoGetHelper<Z>(func(t, u, v, w));
        }

        protected Task<Z> DoGet<Z>(Func<Z> func)
                where Z : struct {
            AssertBlittable<Z>();
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(TaskTrigger(tcs, func));
            return tcs.Task;
        }
        protected Task<Z> DoGet<T, Z>(Func<T, Z> func, T t)
                where T : struct
                where Z : struct {
            AssertBlittable<T, Z>();
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t));
            return tcs.Task;
        }
        protected Task<Z> DoGet<T, U, Z>(Func<T, U, Z> func, T t, U u)
                where T : struct
                where U : struct
                where Z : struct {
            AssertBlittable<T, U, Z>();
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t, u));
            return tcs.Task;
        }
        protected Task<Z> DoGet<T, U, V, Z>(Func<T, U, V, Z> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
                where Z : struct {
            AssertBlittable<T, U, V, Z>();
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
            AssertBlittable<T, U, V, W, Z>();
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t, u, v, w));
            return tcs.Task;
        }

#if EVENTMONOBEHAVIOR_TASK_OPERATORS
        protected Task<Z> DoGet<Z>(Func<Task<Z>> func)
                where Z : struct {
            AssertBlittable<Z>();
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(TaskTrigger(tcs, func));
            return tcs.Task;
        }
        protected Task<Z> DoGet<T, Z>(Func<T, Task<Z>> func, T t)
                where T : struct
                where Z : struct {
            AssertBlittable<T, Z>();
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t));
            return tcs.Task;
        }
        protected Task<Z> DoGet<T, U, Z>(Func<T, U, Task<Z>> func, T t, U u)
                where T : struct
                where U : struct
                where Z : struct {
            AssertBlittable<T, U, Z>();
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(TaskTrigger(tcs, func, t, u));
            return tcs.Task;
        }
        protected Task<Z> DoGet<T, U, V, Z>(Func<T, U, V, Task<Z>> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
                where Z : struct {
            AssertBlittable<T, U, V, Z>();
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
            AssertBlittable<T, U, V, W, Z>();
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
            AssertBlittable<T>();
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(func(tcs, t));
            return tcs.Task;
        }
        protected Task DoWaitForManualTrigger<T, U>(Func<TaskCompletionSource<bool>, T, U, IEnumerator> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable<T, U>();
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(func(tcs, t, u));
            return tcs.Task;
        }
        protected Task DoWaitForManualTrigger<T, U, V>(Func<TaskCompletionSource<bool>, T, U, V, IEnumerator> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable<T, U, V>();
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
            AssertBlittable<T>();
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t));
            return tcs.Task;
        }
        protected Task DoWaitForManualTrigger<T, U>(Action<TaskCompletionSource<bool>, T, U> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable<T, U>();
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t, u));
            return tcs.Task;
        }
        protected Task DoWaitForManualTrigger<T, U, V>(Action<TaskCompletionSource<bool>, T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable<T, U, V>();
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
            AssertBlittable<T>();
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t));
            return tcs.Task;
        }
        protected Task DoWaitForManualTrigger<T, U>(Func<TaskCompletionSource<bool>, T, U, Task> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable<T, U>();
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t, u));
            return tcs.Task;
        }
        protected Task DoWaitForManualTrigger<T, U, V>(Func<TaskCompletionSource<bool>, T, U, V, Task> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable<T, U, V>();
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
            AssertBlittable<T>();
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(func(tcs, t));
            return tcs.Task;
        }
        protected Task<Z> DoGetManualTrigger<T, U, Z>(Func<TaskCompletionSource<Z>, T, U, IEnumerator> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable<T, U>();
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(func(tcs, t, u));
            return tcs.Task;
        }
        protected Task<Z> DoGetManualTrigger<T, U, V, Z>(Func<TaskCompletionSource<Z>, T, U, V, IEnumerator> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable<T, U, V>();
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
            AssertBlittable<T>();
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t));
            return tcs.Task;
        }
        protected Task<Z> DoGetManualTrigger<T, U, Z>(Action<TaskCompletionSource<Z>, T, U> func, T t, U u)
                where T : struct
                where U : struct
                where Z : struct {
            AssertBlittable<T, U>();
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t, u));
            return tcs.Task;
        }
        protected Task<Z> DoGetManualTrigger<T, U, V, Z>(Action<TaskCompletionSource<Z>, T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
                where Z : struct {
            AssertBlittable<T, U, V>();
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
            AssertBlittable<T>();
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t));
            return tcs.Task;
        }
        protected Task<Z> DoGetManualTrigger<T, U, Z>(Func<TaskCompletionSource<Z>, T, U, Task> func, T t, U u)
                where T : struct
                where U : struct
                where Z : struct {
            AssertBlittable<T, U>();
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t, u));
            return tcs.Task;
        }
        protected Task<Z> DoGetManualTrigger<T, U, V, Z>(Func<TaskCompletionSource<Z>, T, U, V, Task> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
                where Z : struct {
            AssertBlittable<T, U, V>();
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
                yield return MakeEventEnumerator(func());
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
                yield return MakeEventEnumerator(func(t));
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
                yield return MakeEventEnumerator(func(t, u));
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
                yield return MakeEventEnumerator(func(t, u, v));
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
                yield return MakeEventEnumerator(func(t, u, v, w));
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

        private IEnumerator TaskTrigger<Z>(TaskCompletionSource<Z> tcs, IEnumerator func) {
            yield return func;
            Z ret = (Z)func.Current;
            tcs.SetResult(ret);
        }

        private IEnumerator TaskTrigger<Z>(TaskCompletionSource<Z> tcs, Func<Z> func) {
            Z ret = func();
            tcs.SetResult(ret);
            yield break;
        }
        private IEnumerator TaskTrigger<T, Z>(TaskCompletionSource<Z> tcs, Func<T, Z> func, T t) {
            Z ret = func(t);
            tcs.SetResult(ret);
            yield break;
        }
        private IEnumerator TaskTrigger<T, U, Z>(TaskCompletionSource<Z> tcs, Func<T, U, Z> func, T t, U u) {
            Z ret = func(t, u);
            tcs.SetResult(ret);
            yield break;
        }
        private IEnumerator TaskTrigger<T, U, V, Z>(TaskCompletionSource<Z> tcs, Func<T, U, V, Z> func, T t, U u, V v) {
            Z ret = func(t, u, v);
            tcs.SetResult(ret);
            yield break;
        }
        private IEnumerator TaskTrigger<T, U, V, W, Z>(TaskCompletionSource<Z> tcs, Func<T, U, V, W, Z> func, T t, U u, V v, W w) {
            Z ret = func(t, u, v, w);
            tcs.SetResult(ret);
            yield break;
        }

#if EVENTMONOBEHAVIOR_TASK_OPERATORS
        private IEnumerator TaskTrigger<Z>(TaskCompletionSource<Z> tcs, Func<Task<Z>> func) {
            var task = func();
            yield return task.ToEnumerator();
            Z ret = task.Result;
            tcs.SetResult(ret);
        }
        private IEnumerator TaskTrigger<T, Z>(TaskCompletionSource<Z> tcs, Func<T, Task<Z>> func, T t) {
            var task = func(t);
            yield return task.ToEnumerator();
            Z ret = task.Result;
            tcs.SetResult(ret);
        }
        private IEnumerator TaskTrigger<T, U, Z>(TaskCompletionSource<Z> tcs, Func<T, U, Task<Z>> func, T t, U u) {
            var task = func(t, u);
            yield return task.ToEnumerator();
            Z ret = task.Result;
            tcs.SetResult(ret);
        }
        private IEnumerator TaskTrigger<T, U, V, Z>(TaskCompletionSource<Z> tcs, Func<T, U, V, Task<Z>> func, T t, U u, V v) {
            var task = func(t, u, v);
            yield return task.ToEnumerator();
            Z ret = task.Result;
            tcs.SetResult(ret);
        }
        private IEnumerator TaskTrigger<T, U, V, W, Z>(TaskCompletionSource<Z> tcs, Func<T, U, V, W, Task<Z>> func, T t, U u, V v, W w) {
            var task = func(t, u, v, w);
            yield return task.ToEnumerator();
            Z ret = task.Result;
            tcs.SetResult(ret);
        }
#endif // EVENTMONOBEHAVIOR_TASK_OPERATORS
    }

}