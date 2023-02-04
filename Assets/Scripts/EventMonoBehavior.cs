using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

using static Blittability;

public abstract class EventMonoBehavior : MonoBehaviour {
    protected InterfaceManager2 manager;
    private bool _baseInvoked = false;

    // TODO: JPB: Add this to documentation
    protected abstract void StartOverride();
    protected void Start() {
        manager = GameObject.Find("InterfaceManager").GetComponent<InterfaceManager2>();
        StartOverride();
    }

    // TODO: JPB: Add support for cancellation token

    private void DoHelper(System.Collections.IEnumerator enumerator) {
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

    // Update is called once per frame
    void Update() {

    }

    private IEnumerator TaskTrigger<Z>(IEnumerator func, TaskCompletionSource<Z> tcs)
    {
        yield return func;
        tcs.SetResult((Z) func.Current);
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
}
