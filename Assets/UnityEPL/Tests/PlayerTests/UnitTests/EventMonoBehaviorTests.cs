using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using UnityEPL;

namespace UnityEPLTests {

    public class EventMonoBehaviorTests {
        InterfaceManager manager;
        EMB emb;

        const double ONE_FRAME_MS = 1000.0 / 120.0;

        // SETUP

        [OneTimeSetUp]
        public void InterfaceManagerSetup() {
            manager = GameObject.FindObjectOfType<InterfaceManager>();
            if (manager == null) {
                manager = new GameObject().AddComponent<InterfaceManager>();
            }

            emb = new GameObject().AddComponent<EMB>();
        }

        // -------------------------------------
        // DoGet Test
        // -------------------------------------

        // THIS IS NOT HOW YOU SHOULD USE THIS FRAMEWORK
        // Below we are verifying that Do and DoGet work
        // In order to do this, we use a mutex class to guarantee that we are not creating threading issues
        // This methodology should be avoided because it can significantly slow your code down due to the locks
        // Instead just use DoGet like the rest of the example do, once we verify that DoGet works

        [UnityTest]
        public IEnumerator DoGet() {
            int i = emb.mutex.Get();

            var task = emb.GetMutexVal();
            yield return task.ToEnumerator();
            Assert.AreEqual(i, task.Result); // Didn't mutate state
            Assert.AreEqual(emb.mutex.Get(), task.Result); // Didn't mutate state but cached value

            emb.mutex.Mutate((int i) => { return i + 1; });

            task = emb.GetMutexVal();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result); // Didn't mutate state
            Assert.AreEqual(emb.mutex.Get(), task.Result); // Didn't mutate state but cached value
        }

        // -------------------------------------
        // The rest of the tests
        // -------------------------------------

        [UnityTest]
        public IEnumerator DoEnum() {
            var task = emb.GetI();
            yield return task.ToEnumerator();
            var i = task.Result;

            emb.IncEnum();

            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result);
        }

        [UnityTest]
        public IEnumerator DoAct() {
            var task = emb.GetI();
            yield return task.ToEnumerator();
            var i = task.Result;

            emb.IncAct();

            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result);
        }

        [UnityTest]
        public IEnumerator DoInEnum() {
            var task = emb.GetI();
            yield return task.ToEnumerator();
            var i = task.Result;

            emb.DelayedIncEnum(1000);

            yield return InterfaceManager.DelayE(900);
            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i, task.Result);

            yield return InterfaceManager.DelayE(200);
            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i+1, task.Result);
        }

        [UnityTest]
        public IEnumerator DoInAct() {
            var task = emb.GetI();
            yield return task.ToEnumerator();
            var i = task.Result;

            emb.DelayedIncAct(1000);

            yield return InterfaceManager.DelayE(900);
            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i, task.Result);

            yield return InterfaceManager.DelayE(200);
            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result);
        }

        [UnityTest]
        public IEnumerator DoWaitFor() {
            var task = emb.GetI();
            yield return task.ToEnumerator();
            var i = task.Result;

            var start = Clock.UtcNow;
            yield return emb.DelayedIncAndWait(1000).ToEnumerator();

            var diff = (Clock.UtcNow - start).TotalMilliseconds;
            Assert.GreaterOrEqual(diff, 1000);
            Assert.LessOrEqual(diff, 1000 + ONE_FRAME_MS);

            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i+1, task.Result);
        }

        

        class EMB : EventMonoBehaviour {
            protected override void StartOverride() {}

            public Mutex<int> mutex = new Mutex<int>(0);
            protected int i = 0;

            public void IncMutexVal() {
                Do(IncMutexValHelper);
            }
            protected IEnumerator IncMutexValHelper() {
                mutex.Mutate((int i) => { return i+1; });
                yield break;
            }

            public async Task<int> GetMutexVal() {
                return await DoGet<int>(GetMutexValHelper);
            }
            protected IEnumerator<int> GetMutexValHelper() {
                yield return mutex.Get();
            }

            public async Task<int> GetI() {
                return await DoGet<int>(GetIHelper);
            }
            protected IEnumerator<int> GetIHelper() {
                yield return i;
            }

            // This is used to wait until the an operation in the
            // event queue before this is done
            public async Task AwaitNothing() {
                await DoWaitFor(AwaitNothingHelper);
            }
            protected IEnumerator AwaitNothingHelper() {
                yield break;
            }

            public void IncEnum() {
                Do(IncEnumHelper);
            }
            protected IEnumerator IncEnumHelper() {
                i += 1;
                yield break;
            }

            public void IncAct() {
                Do(IncActHelper);
            }
            protected void IncActHelper() {
                i += 1;
            }

            public void DelayedIncEnum(int millisecondsDelay) {
                DoIn(millisecondsDelay, DelayedIncEnumHelper);
            }
            protected IEnumerator DelayedIncEnumHelper() {
                i += 1;
                yield break;
            }

            public void DelayedIncAct(int millisecondsDelay) {
                DoIn(millisecondsDelay, DelayedIncActHelper);
            }
            protected void DelayedIncActHelper() {
                i += 1;
            }

            public void IncThreeTimesEnum() {
                
            }
            protected IEnumerator IncThreeTimesEnumHelper() {
                yield return null;
            }

            public async Task DelayedIncAndWait(int millisecondsDelay) {
                await DoWaitFor(DelayedIncAndWaitHelper, millisecondsDelay);
            }
            protected IEnumerator DelayedIncAndWaitHelper(int millisecondsDelay) {
                yield return new WaitForSeconds((float)millisecondsDelay / 1000f);
                i += 1;
            }
        }
    }

}


