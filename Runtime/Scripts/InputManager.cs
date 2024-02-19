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
            public Timer timer;

            public KeyRequest(TaskCompletionSource<KeyCode> tcs, List<KeyCode> keyCodes, TimeSpan timeout) {
                this.tcs = tcs;
                this.keyCodes = keyCodes;
                this.timer = new Timer(timeout);
            }
        }

        LinkedList<KeyRequest> tempKeyRequests = new();
        protected override void AwakeOverride() { }

        void Update() {
            // TODO: JPB: (refactor) Use new unity input system for key input
            //            Keyboard.current.anyKey.wasPressedThisFrame

            // Remove timed out requests
            var node = tempKeyRequests.First;
            while (node != null) {
                var keyReq = node.Value;
                if (keyReq.timer.IsFinished()) {
                    keyReq.tcs.SetCanceled();
                    tempKeyRequests.Remove(node);
                }
                node = node.Next;
            }
            // Check for button presses
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))) {
                if (Input.GetKeyDown(vKey)) {
                    node = tempKeyRequests.First;
                    while (node != null) {
                        var keyReq = node.Value;
                        if (keyReq.keyCodes.Count == 0 || keyReq.keyCodes.Exists(x => x == vKey)) {
                            keyReq.tcs.SetResult(vKey);
                            tempKeyRequests.Remove(node);
                        }
                        node = node.Next;
                    }
                }
            }
        }

        public Task<KeyCode> WaitForKeyTS() {
            return GetKeyTS(null);
        }
        public async Task<KeyCode?> WaitForKeyTS(TimeSpan duration) {
            try {
                return await GetKeyTS(duration);
            } catch (TaskCanceledException) {
                return null;
            } 
        }
        public Task<KeyCode> WaitForKeyTS(List<KeyCode> keyCodes) {
            return GetKeyTS(keyCodes);
        }
        public async Task<KeyCode?> WaitForKeyTS(List<KeyCode> keyCodes, TimeSpan duration) {
            try {
                return await GetKeyTS(keyCodes, duration);
            } catch (TaskCanceledException) {
                return null;
            } 
        }

        // TODO: JPB: (refactor) Make GetKeyTS protected (only use WaitForKeyTS)
        public Task<KeyCode> GetKeyTS(TimeSpan? duration = null) {
            TimeSpan dur = duration ?? DateTime.MaxValue - Clock.UtcNow - TimeSpan.FromDays(1);
            return DoGetManualTriggerTS<NativeArray<KeyCode>, TimeSpan, KeyCode>(GetKeyHelper, new(), dur);
        }
        public Task<KeyCode> GetKeyTS(List<KeyCode> keyCodes, TimeSpan? duration = null) {
            TimeSpan dur = duration ?? DateTime.MaxValue - Clock.UtcNow - TimeSpan.FromDays(1);
            var nativeKeyCodes = keyCodes.ToNativeArray(AllocatorManager.Persistent);
            return DoGetManualTriggerTS<NativeArray<KeyCode>, TimeSpan, KeyCode>(GetKeyHelper, nativeKeyCodes, dur);
        }
        protected IEnumerator GetKeyHelper(TaskCompletionSource<KeyCode> tcs, NativeArray<KeyCode> keyCodes, TimeSpan duration) {
            var keyCodesList = keyCodes.ToList();
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))) {
                if (Input.GetKeyDown(vKey) && (keyCodesList.Count == 0 || keyCodesList.Exists(x => x == vKey))) {
                    tcs.SetResult(vKey);
                    yield break;
                }
            }
            tempKeyRequests.AddLast(new KeyRequest(tcs, keyCodesList, duration));
        }
    }


}