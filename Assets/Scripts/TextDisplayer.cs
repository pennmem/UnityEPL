using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class TextDisplayer : EventMonoBehavior {
    const long delay = 10000000000;
    public TextMesh tm;

    protected override void StartOverride() {}

    public void UpdateText(StackString ss) {
        Do(UpdateTextHelper, ss);
    }
    protected IEnumerator UpdateTextHelper(StackString ss) {
        tm.text = ss;
        Debug.Log(ss);
        yield break;
    }


    public Task AwaitableUpdateText(StackString ss) {
        return DoWaitFor(AwaitableUpdateTextHelper, ss);
    }
    protected IEnumerator AwaitableUpdateTextHelper(StackString ss) {
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

    public Task<int> ReturnableUpdateText(StackString ss) {
        return DoGet<StackString, int>(ReturnableUpdateTextHelper, ss);
    }
    protected IEnumerator ReturnableUpdateTextHelper(StackString ss) {
        Debug.Log(1 + " - " + DateTime.Now);
        tm.text = ss;
        yield return new WaitForSeconds(1);
        tm.text = ss;
        yield return 69;
    }
}
