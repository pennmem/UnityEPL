using System;
using System.Collections;
using System.Collections.Generic;
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

        protected abstract void StartOverride();
        protected void Start()
        {
            manager = FindObjectOfType<InterfaceManager>();
            Debug.Log(manager == null);
            StartOverride();
        }

        protected static void Quit()
        {
            Debug.Log("Quitting");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        // Action to Enumerator Conversion

        protected IEnumerator EnumeratorCaller(Action func)
        {
            func();
            yield return null;
        }
        protected IEnumerator EnumeratorCaller<T>(Action<T> func, T t)
        {
            func(t);
            yield return null;
        }
        protected IEnumerator EnumeratorCaller<T, U>(Action<T, U> func, T t, U u) {
            func(t, u);
            yield return null;
        }
        protected IEnumerator EnumeratorCaller<T, U, V>(Action<T, U, V> func, T t, U u, V v) {
            func(t, u, v);
            yield return null;
        }
        protected IEnumerator EnumeratorCaller<T, U, V, W>(Action<T, U, V, W> func, T t, U u, V v, W w) {
            func(t, u, v, w);
            yield return null;
        }

        // Task to Enumerator Conversion
        protected IEnumerator EnumeratorCaller(Func<Task> func) {
            yield return func().ToEnumerator();
        }
        protected IEnumerator EnumeratorCaller<T>(Func<T, Task> func, T t) {
            yield return func(t).ToEnumerator();
        }
        protected IEnumerator EnumeratorCaller<T, U>(Func<T, U, Task> func, T t, U u) {
            yield return func(t, u).ToEnumerator();
        }
        protected IEnumerator EnumeratorCaller<T, U, V>(Func<T, U, V, Task> func, T t, U u, V v) {
            yield return func(t, u, v).ToEnumerator();
        }
        protected IEnumerator EnumeratorCaller<T, U, V, W>(Func<T, U, V, W, Task> func, T t, U u, V v, W w) {
            yield return func(t, u, v, w).ToEnumerator();
        }

        // Do
        // TODO: JPB: (feature) Add support for cancellation tokens in EventMonoBehavior Do functions

        private void DoHelper(IEnumerator enumerator)
        {
            // TODO: JPB: (needed) Find out why tests can't find the manager.
            if (manager == null) { manager = FindObjectOfType<InterfaceManager>(); }
            manager.events.Enqueue(enumerator);
        }
        protected void Do(Func<IEnumerator> func)
        {
            DoHelper(func());
        }
        protected void Do<T>(Func<T, IEnumerator> func, T t)
                where T : struct
        {
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

        // DoIn

        protected async void DoIn(int millisecondsDelay, Func<IEnumerator> func)
        {
            await InterfaceManager.Delay(millisecondsDelay);
            Do(func);
        }
        protected async void DoIn<T>(int millisecondsDelay, Func<T, IEnumerator> func, T t)
                where T : struct
        {
            await InterfaceManager.Delay(millisecondsDelay);
            Do(func, t);
        }
        protected async void DoIn<T, U>(int millisecondsDelay, Func<T, U, IEnumerator> func, T t, U u)
                where T : struct
                where U : struct
        {
            await InterfaceManager.Delay(millisecondsDelay);
            Do(func, t, u);
        }
        protected async void DoIn<T, U, V>(int millisecondsDelay, Func<T, U, V, IEnumerator> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
        {
            await InterfaceManager.Delay(millisecondsDelay);
            Do(func, t, u, v);
        }
        protected async void DoIn<T, U, V, W>(int millisecondsDelay, Func<T, U, V, W, IEnumerator> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct
        {
            await InterfaceManager.Delay(millisecondsDelay);
            Do(func, t, u, v, w);
        }

        protected async void DoIn(int millisecondsDelay, Action func)
        {
            await InterfaceManager.Delay(millisecondsDelay);
            Do(func);
        }
        protected async void DoIn<T>(int millisecondsDelay, Action<T> func, T t)
                where T : struct
        {
            await InterfaceManager.Delay(millisecondsDelay);
            Do(func, t);
        }
        protected async void DoIn<T, U>(int millisecondsDelay, Action<T, U> func, T t, U u)
                where T : struct
                where U : struct {
            await InterfaceManager.Delay(millisecondsDelay);
            Do(func, t, u);
        }
        protected async void DoIn<T, U, V>(int millisecondsDelay, Action<T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            await InterfaceManager.Delay(millisecondsDelay);
            Do(func, t, u, v);
        }
        protected async void DoIn<T, U, V, W>(int millisecondsDelay, Action<T, U, V, W> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            await InterfaceManager.Delay(millisecondsDelay);
            Do(func, t, u, v, w);
        }

        // DoRepeating

        // TODO: JPB: (feature) Add DoRepeating in the EventMonoBehavior

        // DoWaitFor - Simple version

        private IEnumerator TaskTrigger(TaskCompletionSource<bool> tcs, IEnumerator func)
        {
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

        // DoWaitFor - User is responsible for triggering these TaskCompletionSources
        // JPB: Not sure what purpose I had in mind for these...

        protected Task DoWaitForSelfTrigger(Func<TaskCompletionSource<bool>, IEnumerator> func)
        {
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(func(tcs));
            return tcs.Task;
        }
        protected Task DoWaitForSelfTrigger<T>(Func<TaskCompletionSource<bool>, T, IEnumerator> func, T t)
                where T : struct
        {
            AssertBlittable(t);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(func(tcs, t));
            return tcs.Task;
        }
        protected Task DoWaitForSelfTrigger<T, U>(Func<TaskCompletionSource<bool>, T, U, IEnumerator> func, T t, U u)
                where T : struct
                where U : struct
        {
            AssertBlittable(t, u);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(func(tcs, t, u));
            return tcs.Task;
        }
        protected Task DoWaitForSelfTrigger<T, U, V>(Func<TaskCompletionSource<bool>, T, U, V, IEnumerator> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
        {
            AssertBlittable(t, u, v);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(func(tcs, t, u, v));
            return tcs.Task;
        }

        protected Task DoWaitForSelfTrigger(Action<TaskCompletionSource<bool>> func) {
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs));
            return tcs.Task;
        }
        protected Task DoWaitForSelfTrigger<T>(Action<TaskCompletionSource<bool>, T> func, T t)
                where T : struct {
            AssertBlittable(t);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t));
            return tcs.Task;
        }
        protected Task DoWaitForSelfTrigger<T, U>(Action<TaskCompletionSource<bool>, T, U> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable(t, u);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t, u));
            return tcs.Task;
        }
        protected Task DoWaitForSelfTrigger<T, U, V>(Action<TaskCompletionSource<bool>, T, U, V> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable(t, u, v);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t, u, v));
            return tcs.Task;
        }

        protected Task DoWaitForSelfTrigger(Func<TaskCompletionSource<bool>, Task> func) {
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs));
            return tcs.Task;
        }
        protected Task DoWaitForSelfTrigger<T>(Func<TaskCompletionSource<bool>, T, Task> func, T t)
                where T : struct {
            AssertBlittable(t);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t));
            return tcs.Task;
        }
        protected Task DoWaitForSelfTrigger<T, U>(Func<TaskCompletionSource<bool>, T, U, Task> func, T t, U u)
                where T : struct
                where U : struct {
            AssertBlittable(t, u);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t, u));
            return tcs.Task;
        }
        protected Task DoWaitForSelfTrigger<T, U, V>(Func<TaskCompletionSource<bool>, T, U, V, Task> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            AssertBlittable(t, u, v);
            var tcs = new TaskCompletionSource<bool>();
            manager.events.Enqueue(EnumeratorCaller(func, tcs, t, u, v));
            return tcs.Task;
        }

        // DoGet - Simple version

        private IEnumerator TaskTrigger<Z>(IEnumerator func, TaskCompletionSource<Z> tcs)
        {
            yield return func;
            tcs.SetResult((Z)func.Current);
        }
        private Task<Z> DoGetHelper<Z>(IEnumerator enumerator)
        {
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(TaskTrigger(enumerator, tcs));
            return tcs.Task;
        }
        protected Task<Z> DoGet<Z>(Func<IEnumerator> func)
        {
            return DoGetHelper<Z>(func());
        }
        protected Task<Z> DoGet<T, Z>(Func<T, IEnumerator> func, T t)
                where T : struct
        {
            AssertBlittable(t);
            return DoGetHelper<Z>(func(t));
        }
        protected Task<Z> DoGet<T, U, Z>(Func<T, U, IEnumerator> func, T t, U u)
                where T : struct
                where U : struct
        {
            AssertBlittable(t, u);
            return DoGetHelper<Z>(func(t, u));
        }
        protected Task<Z> DoGet<T, U, V, Z>(Func<T, U, V, IEnumerator> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
        {
            AssertBlittable(t, u, v);
            return DoGetHelper<Z>(func(t, u, v));
        }
        protected Task<Z> DoGet<T, U, V, W, Z>(Func<T, U, V, W, IEnumerator> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct
        {
            AssertBlittable(t, u, v, w);
            return DoGetHelper<Z>(func(t, u, v, w));
        }

        // DoGet - User is responsible for triggering these TaskCompletionSources

        protected Task<Z> DoGet<Z>(Func<TaskCompletionSource<Z>, IEnumerator> func)
        {
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(func(tcs));
            return tcs.Task;
        }
        protected Task<Z> DoGet<T, Z>(Func<TaskCompletionSource<Z>, T, IEnumerator> func, T t)
                where T : struct
        {
            AssertBlittable(t);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(func(tcs, t));
            return tcs.Task;
        }
        protected Task<Z> DoGet<T, U, Z>(Func<TaskCompletionSource<Z>, T, U, IEnumerator> func, T t, U u)
                where T : struct
                where U : struct
        {
            AssertBlittable(t, u);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(func(tcs, t, u));
            return tcs.Task;
        }
        protected Task<Z> DoGet<T, U, V, Z>(Func<TaskCompletionSource<Z>, T, U, V, IEnumerator> func, T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct
        {
            AssertBlittable(t, u, v);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(func(tcs, t, u, v));
            return tcs.Task;
        }
        protected Task<Z> DoGet<T, U, V, W, Z>(Func<TaskCompletionSource<Z>, T, U, V, W, IEnumerator> func, T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct
        {
            AssertBlittable(t, u, v, w);
            var tcs = new TaskCompletionSource<Z>();
            manager.events.Enqueue(func(tcs, t, u, v, w));
            return tcs.Task;
        }
    }

}