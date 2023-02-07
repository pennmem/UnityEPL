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

    // TODO: JPB: Add support for cancellation token
    // TODO: JPB: Add support for up to 4 arguments

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
}
