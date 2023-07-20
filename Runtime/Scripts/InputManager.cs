using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace UnityEPL {

    public class InputManager : SingletonEventMonoBehaviour<InputManager> {
        private struct KeyRequest {
            public TaskCompletionSource<KeyCode> tcs;
            public List<KeyCode> keyCodes;

            public KeyRequest(TaskCompletionSource<KeyCode> tcs, List<KeyCode> keyCodes) {
                this.tcs = tcs;
                this.keyCodes = keyCodes;
            }
        }

        LinkedList<KeyRequest> tempKeyRequests = new();
        protected override void AwakeOverride() { }

        void Update() {
            // TODO: JPB: (refactor) Use new unity input system for key input
            //            Keyboard.current.anyKey.wasPressedThisFrame
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))) {
                if (Input.GetKeyDown(vKey)) {
                    var node = tempKeyRequests.First;
                    while (node != null) {
                        var next = node.Next;
                        if (node.Value.keyCodes.Count == 0 || node.Value.keyCodes.Exists(x => x == vKey)) {
                            node.Value.tcs.SetResult(vKey);
                            tempKeyRequests.Remove(node);
                        }
                        node = next;
                    }
                }
            }
        }

        public Task<KeyCode> GetKeyTS() {
            return DoGetManualTriggerTS<NativeArray<KeyCode>, KeyCode>(GetKeyHelper, new());
        }
        public Task<KeyCode> GetKeyTS(List<KeyCode> keyCodes) {
            var nativeKeyCodes = keyCodes.ToNativeArray(AllocatorManager.Persistent);
            return DoGetManualTriggerTS<NativeArray<KeyCode>, KeyCode>(GetKeyHelper, nativeKeyCodes);
        }
        public Task WaitForKeyTS() {
            return GetKeyTS();
        }
        public Task WaitForKeyTS(List<KeyCode> keyCodes) {
            return GetKeyTS(keyCodes);
        }
        protected IEnumerator GetKeyHelper(TaskCompletionSource<KeyCode> tcs, NativeArray<KeyCode> keyCodes) {
            var keyCodesList = keyCodes.ToList();
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))) {
                if (Input.GetKeyDown(vKey) && (keyCodesList.Count == 0 || keyCodesList.Exists(x => x == vKey))) {
                    tcs.SetResult(vKey);
                    yield break;
                }
            }
            tempKeyRequests.AddLast(new KeyRequest(tcs, keyCodesList));
        }
    }


}