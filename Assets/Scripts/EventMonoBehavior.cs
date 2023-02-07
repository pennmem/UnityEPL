using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

using static Blittability;

public abstract class EventMonoBehaviour : MonoBehaviour {
    protected InterfaceManager2 manager;
    private bool _baseInvoked = false;

    // TODO: JPB: Add this to documentation
    protected abstract void StartOverride();
    protected void Start() {
        manager = GameObject.Find("InterfaceManager").GetComponent<InterfaceManager2>();
        StartOverride();
    }

    // TODO: JPB: Add support for cancellation tokens in EventMonoBehavior Do functions

    // Do

    private void DoHelper(IEnumerator enumerator) {
        manager.events.Enqueue(enumerator);
    } 
    protected void Do(Func<IEnumerator> func) {
        DoHelper(func());
    }
    protected void Do<T>(Func<T, IEnumerator> func, T t)
            where T : struct {
        AssertBlittable(t);
        DoHelper(func(t));
    }
    protected void Do<T, U>(Func<T, U, IEnumerator> func, T t, U u)
            where T : struct
            where U : struct {
        AssertBlittable(t, u);
        DoHelper(func(t, u));
    }
    protected void Do<T, U, V>(Func<T, U, V, IEnumerator> func, T t, U u, V v)
            where T : struct
            where U : struct
            where V : struct {
        AssertBlittable(t, u, v);
        DoHelper(func(t, u, v));
    }
    protected void Do<T, U, V, W>(Func<T, U, V, W, IEnumerator> func, T t, U u, V v, W w)
            where T : struct
            where U : struct
            where V : struct
            where W : struct {
        AssertBlittable(t, u, v, w);
        DoHelper(func(t, u, v, w));
    }

    // DoIn

    protected async void DoIn(int millisecondsDelay, Func<IEnumerator> func) {
        await InterfaceManager2.Delay(millisecondsDelay);
        Do(func);
    }
    protected async void DoIn<T>(int millisecondsDelay, Func<T, IEnumerator> func, T t)
            where T : struct {
        await InterfaceManager2.Delay(millisecondsDelay);
        Do(func, t);
    }
    protected async void DoIn<T, U>(int millisecondsDelay, Func<T, U, IEnumerator> func, T t, U u)
            where T : struct
            where U : struct {
        await InterfaceManager2.Delay(millisecondsDelay);
        Do(func, t, u);
    }
    protected async void DoIn<T, U, V>(int millisecondsDelay, Func<T, U, V, IEnumerator> func, T t, U u, V v)
            where T : struct
            where U : struct
            where V : struct {
        await InterfaceManager2.Delay(millisecondsDelay);
        Do(func, t, u, v);
    }
    protected async void DoIn<T, U, V, W>(int millisecondsDelay, Func<T, U, V, W, IEnumerator> func, T t, U u, V v, W w)
            where T : struct
            where U : struct
            where V : struct
            where W : struct {
        await InterfaceManager2.Delay(millisecondsDelay);
        Do(func, t, u, v, w);
    }

    // DoRepeating

    // TODO: JPB: (feature) Add DoRepeating in the EventMonoBehavior

    // DoWaitFor - Simple version

    private IEnumerator TaskTrigger(IEnumerator func, TaskCompletionSource<bool> tcs) {
        yield return func;
        tcs.SetResult(true);
    }
    private Task DoWaitForHelper(IEnumerator enumerator) {
        var tcs = new TaskCompletionSource<bool>();
        manager.events.Enqueue(TaskTrigger(enumerator, tcs));
        return tcs.Task;
    }
    protected Task DoWaitFor(Func<IEnumerator> func) {
        return DoWaitForHelper(func());
    }
    protected Task DoWaitFor<T>(Func<T, IEnumerator> func, T t)
            where T : struct {
        AssertBlittable(t);
        return DoWaitForHelper(func(t));
    }
    protected Task DoWaitFor<T, U>(Func<T, U, IEnumerator> func, T t, U u)
            where T : struct
            where U : struct {
        AssertBlittable(t, u);
        return DoWaitForHelper(func(t, u));
    }
    protected Task DoWaitFor<T, U, V>(Func<T, U, V, IEnumerator> func, T t, U u, V v)
            where T : struct
            where U : struct
            where V : struct {
        AssertBlittable(t, u, v);
        return DoWaitForHelper(func(t, u, v));
    }
    protected Task DoWaitFor<T, U, V, W>(Func<T, U, V, W, IEnumerator> func, T t, U u, V v, W w)
            where T : struct
            where U : struct
            where V : struct
            where W : struct {
        AssertBlittable(t, u, v, w);
        return DoWaitForHelper(func(t, u, v, w));
    }

    // DoWaitFor - User is responsible for triggering these TaskCompletionSources

    protected Task DoWaitFor(Func<TaskCompletionSource<bool>, IEnumerator> func) {
        var tcs = new TaskCompletionSource<bool>();
        manager.events.Enqueue(func(tcs));
        return tcs.Task;
    }
    protected Task DoWaitFor<T>(Func<TaskCompletionSource<bool>, T, IEnumerator> func, T t)
            where T : struct {
        AssertBlittable(t);
        var tcs = new TaskCompletionSource<bool>();
        manager.events.Enqueue(func(tcs, t));
        return tcs.Task;
    }
    protected Task DoWaitFor<T, U>(Func<TaskCompletionSource<bool>, T, U, IEnumerator> func, T t, U u)
            where T : struct
            where U : struct {
        AssertBlittable(t, u);
        var tcs = new TaskCompletionSource<bool>();
        manager.events.Enqueue(func(tcs, t, u));
        return tcs.Task;
    }
    protected Task DoWaitFor<T, U, V>(Func<TaskCompletionSource<bool>, T, U, V, IEnumerator> func, T t, U u, V v)
            where T : struct
            where U : struct
            where V : struct {
        AssertBlittable(t, u, v);
        var tcs = new TaskCompletionSource<bool>();
        manager.events.Enqueue(func(tcs, t, u, v));
        return tcs.Task;
    }
    protected Task DoWaitFor<T, U, V, W>(Func<TaskCompletionSource<bool>, T, U, V, W, IEnumerator> func, T t, U u, V v, W w)
            where T : struct
            where U : struct
            where V : struct
            where W : struct {
        AssertBlittable(t, u, v, w);
        var tcs = new TaskCompletionSource<bool>();
        manager.events.Enqueue(func(tcs, t, u, v, w));
        return tcs.Task;
    }

    // DoGet - Simple version

    private IEnumerator TaskTrigger<Z>(IEnumerator func, TaskCompletionSource<Z> tcs) {
        yield return func;
        tcs.SetResult((Z) func.Current);
    }
    private Task<Z> DoGetHelper<Z>(IEnumerator enumerator) {
        var tcs = new TaskCompletionSource<Z>();
        manager.events.Enqueue(TaskTrigger(enumerator, tcs));
        return tcs.Task;
    }
    protected Task<Z> DoGet<Z>(Func<IEnumerator> func) {
        return DoGetHelper<Z>(func());
    }
    protected Task<Z> DoGet<T, Z>(Func<T, IEnumerator> func, T t)
            where T : struct {
        AssertBlittable(t);
        return DoGetHelper<Z>(func(t));
    }
    protected Task<Z> DoGet<T, U, Z>(Func<T, U, IEnumerator> func, T t, U u)
            where T : struct
            where U : struct {
        AssertBlittable(t, u);
        return DoGetHelper<Z>(func(t, u));
    }
    protected Task<Z> DoGet<T, U, V, Z>(Func<T, U, V, IEnumerator> func, T t, U u, V v)
            where T : struct
            where U : struct
            where V : struct {
        AssertBlittable(t, u, v);
        return DoGetHelper<Z>(func(t, u, v));
    }
    protected Task<Z> DoGet<T, U, V, W, Z>(Func<T, U, V, W, IEnumerator> func, T t, U u, V v, W w)
            where T : struct
            where U : struct
            where V : struct
            where W : struct {
        AssertBlittable(t, u, v, w);
        return DoGetHelper<Z>(func(t, u, v, w));
    }

    // DoGet - User is responsible for triggering these TaskCompletionSources

    protected Task<Z> DoGet<Z>(Func<TaskCompletionSource<Z>, IEnumerator> func) {
        var tcs = new TaskCompletionSource<Z>();
        manager.events.Enqueue(func(tcs));
        return tcs.Task;
    }
    protected Task<Z> DoGet<T, Z>(Func<TaskCompletionSource<Z>, T, IEnumerator> func, T t)
            where T : struct {
        AssertBlittable(t);
        var tcs = new TaskCompletionSource<Z>();
        manager.events.Enqueue(func(tcs, t));
        return tcs.Task;
    }
    protected Task<Z> DoGet<T, U, Z>(Func<TaskCompletionSource<Z>, T, U, IEnumerator> func, T t, U u)
            where T : struct
            where U : struct {
        AssertBlittable(t, u);
        var tcs = new TaskCompletionSource<Z>();
        manager.events.Enqueue(func(tcs, t, u));
        return tcs.Task;
    }
    protected Task<Z> DoGet<T, U, V, Z>(Func<TaskCompletionSource<Z>, T, U, V, IEnumerator> func, T t, U u, V v)
            where T : struct
            where U : struct
            where V : struct {
        AssertBlittable(t, u, v);
        var tcs = new TaskCompletionSource<Z>();
        manager.events.Enqueue(func(tcs, t, u, v));
        return tcs.Task;
    }
    protected Task<Z> DoGet<T, U, V, W, Z>(Func<TaskCompletionSource<Z>, T, U, V, W, IEnumerator> func, T t, U u, V v, W w)
            where T : struct
            where U : struct
            where V : struct
            where W : struct {
        AssertBlittable(t, u, v, w);
        var tcs = new TaskCompletionSource<Z>();
        manager.events.Enqueue(func(tcs, t, u, v, w));
        return tcs.Task;
    }
}
