using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityEPL {

    public class InputManager : EventMonoBehaviour {
        protected override void StartOverride() { }

        LinkedList<TaskCompletionSource<KeyCode>> tempKeyRequests = new LinkedList<TaskCompletionSource<KeyCode>>();

        void Update() {
            // TODO: JPB: (refactor) Use new unity imput system for key input
            //            Keyboard.current.anyKey.wasPressedThisFrame
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))) {
                if (Input.GetKeyDown(vKey)) {
                    var node = tempKeyRequests.First;
                    while (node != null) {
                        var next = node.Next;
                        node.Value.SetResult(vKey);
                        tempKeyRequests.Remove(node);
                        node = next;
                    }
                }
            }
        }

        public Task WaitForKey() {
            return DoGetManualTrigger<KeyCode>(GetKeyHelper);
        }
        public Task<KeyCode> GetKey() {
            return DoGetManualTrigger<KeyCode>(GetKeyHelper);
        }
        protected IEnumerator GetKeyHelper(TaskCompletionSource<KeyCode> tcs) {
            tempKeyRequests.AddLast(tcs);
            yield break;
        }
    }


}