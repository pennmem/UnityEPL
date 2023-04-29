using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace UnityEPL {
    public class TestTextDisplayer : EventMonoBehaviour {
        protected override void AwakeOverride() { }

        const long delay = 10000000000;
        public TextMesh tm;

        public void UpdateText(string ss) {
            Do(UpdateTextHelper, ss.ToNativeText());
        }
        protected IEnumerator UpdateTextHelper(NativeText ss) {
            tm.text = ss.ToString();
            Debug.Log(ss);
            ss.Dispose();
            yield break;
        }


        public Task AwaitableUpdateText(string ss) {
            return DoWaitFor(AwaitableUpdateTextHelper, ss.ToNativeText());
        }
        protected IEnumerator AwaitableUpdateTextHelper(NativeText ss) {
            Debug.Log(1 + " - " + DateTime.Now);
            tm.text = "1";
            yield return new WaitForSeconds(1);
            Debug.Log(2 + " - " + DateTime.Now);
            tm.text = "2";
            yield return new WaitForSeconds(1);
            Debug.Log(ss + " - " + DateTime.Now);
            tm.text = ss.ToString();
            ss.Dispose();
            yield break;
        }

        public Task<int> ReturnableUpdateText(string ss) {
            return DoGet<NativeText, int>(ReturnableUpdateTextHelper, ss.ToNativeText());
        }
        protected IEnumerator ReturnableUpdateTextHelper(NativeText ss) {
            Debug.Log(1 + " - " + DateTime.Now);
            tm.text = ss.ToString();
            yield return new WaitForSeconds(1);
            tm.text = ss.ToString();
            yield return 69;
            ss.Dispose();
        }
    }

}
