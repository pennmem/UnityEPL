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
            public bool unpausable;

            public KeyRequest(TaskCompletionSource<KeyCode> tcs, List<KeyCode> keyCodes, TimeSpan timeout, bool unpausable) {
                this.tcs = tcs;
                this.keyCodes = keyCodes;
                this.timer = new Timer(timeout);
                this.unpausable = unpausable;
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
                        if ((keyReq.unpausable || Time.timeScale != 0) &&
                            (keyReq.keyCodes.Count == 0 || keyReq.keyCodes.Exists(x => x == vKey))) {
                            keyReq.tcs.SetResult(vKey);
                            tempKeyRequests.Remove(node);
                        }
                        node = node.Next;
                    }
                }
            }
        }

        public async Task<KeyCode> WaitForKeyTS() {
            return await GetKeyTS(null, false);
        }
        public async Task<KeyCode?> WaitForKeyTS(TimeSpan duration) {
            try {
                return await GetKeyTS(duration);
            } catch (TaskCanceledException) {
                return null;
            } 
        }
        public async Task<KeyCode> WaitForKeyTS(List<KeyCode> keyCodes) {
            return await GetKeyTS(keyCodes);
        }
        public async Task<KeyCode?> WaitForKeyTS(List<KeyCode> keyCodes, TimeSpan duration) {
            try {
                return await GetKeyTS(keyCodes, duration);
            } catch (TaskCanceledException) {
                return null;
            } 
        }

        // TODO: JPB: (refactor) Make GetKeyTS protected (only use WaitForKeyTS)
        public async Task<KeyCode> GetKeyTS(TimeSpan? duration = null, bool unpausable = false) {
            TimeSpan dur = duration ?? DateTime.MaxValue - Clock.UtcNow - TimeSpan.FromDays(1);
            return await DoGetManualTriggerTS<NativeArray<KeyCode>, TimeSpan, Bool, KeyCode>(GetKeyHelper, new(), dur, unpausable);
        }
        public async Task<KeyCode> GetKeyTS(List<KeyCode> keyCodes, TimeSpan? duration = null, bool unpausable = false) {
            TimeSpan dur = duration ?? DateTime.MaxValue - Clock.UtcNow - TimeSpan.FromDays(1);
            var nativeKeyCodes = keyCodes.ToNativeArray(AllocatorManager.Persistent);
            return await DoGetManualTriggerTS<NativeArray<KeyCode>, TimeSpan, Bool, KeyCode>(GetKeyHelper, nativeKeyCodes, dur, unpausable);
        }
        protected IEnumerator GetKeyHelper(TaskCompletionSource<KeyCode> tcs, NativeArray<KeyCode> keyCodes, TimeSpan duration, Bool unpausable) {
            var keyCodesList = keyCodes.ToList();
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode))) {
                if (Input.GetKeyDown(vKey) && (keyCodesList.Count == 0 || keyCodesList.Exists(x => x == vKey))) {
                    tcs.SetResult(vKey);
                    yield break;
                }
            }
            tempKeyRequests.AddLast(new KeyRequest(tcs, keyCodesList, duration, unpausable));
        }
    }


}