using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static Blittability;

public class EventMonoBehavior : MonoBehaviour {
    protected InterfaceManager2 manager;

    void Start() {
        manager = GameObject.Find("InterfaceManager").GetComponent<InterfaceManager2>();
    }

    // TODO: JPB: Add support for cancellation token

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

    protected IEnumerator Wrapper(IEnumerator func, TaskCompletionSource<bool> tcs) {
        yield return func;
        tcs.SetResult(true);

        //try {
        //    yield return func;
        //} catch (Exception e) {
        //    tcs.SetException(e);
        //}
        //tcs.SetResult(true);
    }

    private Task DoWaitForHelper(IEnumerator enumerator) {
        var tcs = new TaskCompletionSource<bool>();
        manager.events.Enqueue(Wrapper(enumerator, tcs));
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
}

public class TextDisplayer : EventMonoBehavior {
    const long delay = 10000000000;
    public TextMesh tm;

    // Update is called once per frame
    void Update() {

    }

    public void UpdateText(StackString ss) {
        Do(UpdateTextHelper, ss);
    }
    protected IEnumerator UpdateTextHelper(StackString ss) {
        tm.text = ss;
        yield break;
    }


    public Task AwaitableUpdateText(StackString ss)
    {
        return DoWaitFor(AwaitableUpdateTextHelper, ss);
    }
    protected IEnumerator AwaitableUpdateTextHelper(StackString ss)
    {
        Debug.Log(1 + " - " + DateTime.Now);
        tm.text = "1";
        yield return new WaitForSeconds(1);
        Debug.Log(2 + " - " + DateTime.Now);
        tm.text = "2";
        yield return new WaitForSeconds(1);
        Debug.Log(ss + " - " + DateTime.Now);
        tm.text = ss;
        yield break;
    }
}
