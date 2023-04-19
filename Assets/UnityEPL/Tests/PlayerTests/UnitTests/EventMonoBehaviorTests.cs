#define EVENTMONOBEHAVIOR_TASK_OPERATORS
#define EVENTMONOBEHAVIOR_MANUAL_RESULT_SET

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

using UnityEPL;

namespace UnityEPLTests {

    public class EventMonoBehaviorTests {
        // -------------------------------------
        // Globals
        // -------------------------------------

        EMB emb;

        // TODO: JPB: (bug) Things should probably never take two frames
        const double ONE_FRAME_MS = 1000.0 / 120.0;
        const double TWO_FRAMES_MS = 1000.0 / 120.0 * 2;

        // -------------------------------------
        // Setup
        // -------------------------------------

        [UnitySetUp]
        public IEnumerator Setup() {
            if (InterfaceManager.Instance == null) SceneManager.LoadScene("manager");
            yield return null; // Wait for InterfaceManager Awake call

            if (emb == null) emb = new GameObject().AddComponent<EMB>();
        }


        // -------------------------------------
        // DoGet Tests
        //
        // THIS IS NOT HOW YOU SHOULD USE THIS FRAMEWORK
        // Below we are verifying that Do and DoGet work
        // In order to do this, we use a mutex class to guarantee that we are not creating threading issues
        // This methodology should be avoided because it can significantly slow your code down due to the locks
        // Instead just use DoGet like the rest of the example do, once we verify that DoGet works
        // -------------------------------------

        [UnityTest]
        public IEnumerator DoGetEnum() {
            int i = emb.mutex.Get();

            var task = emb.GetMutexValEnum();
            yield return task.ToEnumerator();
            Assert.AreEqual(i, task.Result); // Didn't mutate state
            Assert.AreEqual(emb.mutex.Get(), task.Result); // Didn't mutate state but cached value

            emb.mutex.Mutate((int i) => { return i + 1; });

            task = emb.GetMutexValEnum();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result); // Didn't mutate state
            Assert.AreEqual(emb.mutex.Get(), task.Result); // Didn't mutate state but cached value
        }

        [UnityTest]
        public IEnumerator DoGetFunc() {
            int i = emb.mutex.Get();

            var task = emb.GetMutexValFunc();
            yield return task.ToEnumerator();
            Assert.AreEqual(i, task.Result); // Didn't mutate state
            Assert.AreEqual(emb.mutex.Get(), task.Result); // Didn't mutate state but cached value

            emb.mutex.Mutate((int i) => { return i + 1; });

            task = emb.GetMutexValFunc();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result); // Didn't mutate state
            Assert.AreEqual(emb.mutex.Get(), task.Result); // Didn't mutate state but cached value
        }

#if EVENTMONOBEHAVIOR_TASK_OPERATORS
        [UnityTest]
        public IEnumerator DoGetTask() {
            int i = emb.mutex.Get();

            var task = emb.GetMutexValTask();
            yield return task.ToEnumerator();
            Assert.AreEqual(i, task.Result); // Didn't mutate state
            Assert.AreEqual(emb.mutex.Get(), task.Result); // Didn't mutate state but cached value

            emb.mutex.Mutate((int i) => { return i + 1; });

            task = emb.GetMutexValTask();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result); // Didn't mutate state
            Assert.AreEqual(emb.mutex.Get(), task.Result); // Didn't mutate state but cached value
        }
#endif // EVENTMONOBEHAVIOR_TASK_OPERATORS

#if EVENTMONOBEHAVIOR_MANUAL_RESULT_SET
        [UnityTest]
        public IEnumerator DoGetManualTriggerEnum() {
            int i = emb.mutex.Get();

            var task = emb.GetMutexValManualTriggerEnum();
            yield return task.ToEnumerator();
            Assert.AreEqual(i, task.Result); // Didn't mutate state
            Assert.AreEqual(emb.mutex.Get(), task.Result); // Didn't mutate state but cached value

            emb.mutex.Mutate((int i) => { return i + 1; });

            task = emb.GetMutexValManualTriggerEnum();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result); // Didn't mutate state
            Assert.AreEqual(emb.mutex.Get(), task.Result); // Didn't mutate state but cached value
        }

        [UnityTest]
        public IEnumerator DoGetManualTriggerFunc() {
            int i = emb.mutex.Get();

            var task = emb.GetMutexValManualTriggerFunc();
            yield return task.ToEnumerator();
            Assert.AreEqual(i, task.Result); // Didn't mutate state
            Assert.AreEqual(emb.mutex.Get(), task.Result); // Didn't mutate state but cached value

            emb.mutex.Mutate((int i) => { return i + 1; });

            task = emb.GetMutexValManualTriggerFunc();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result); // Didn't mutate state
            Assert.AreEqual(emb.mutex.Get(), task.Result); // Didn't mutate state but cached value
        }

#if EVENTMONOBEHAVIOR_TASK_OPERATORS
        [UnityTest]
        public IEnumerator DoGetManualTriggerTask() {
            int i = emb.mutex.Get();

            var task = emb.GetMutexValManualTriggerTask();
            yield return task.ToEnumerator();
            Assert.AreEqual(i, task.Result); // Didn't mutate state
            Assert.AreEqual(emb.mutex.Get(), task.Result); // Didn't mutate state but cached value

            emb.mutex.Mutate((int i) => { return i + 1; });

            task = emb.GetMutexValManualTriggerTask();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result); // Didn't mutate state
            Assert.AreEqual(emb.mutex.Get(), task.Result); // Didn't mutate state but cached value
        }
#endif // EVENTMONOBEHAVIOR_TASK_OPERATORS
#endif // EVENTMONOBEHAVIOR_MANUAL_RESULT_SET


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
        public IEnumerator DoRepeatingEnum() {
            var task = emb.GetI();
            yield return task.ToEnumerator();
            var i = task.Result;

            emb.IncThreeTimesEnum(0, 1000, 3);

            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result);

            yield return InterfaceManager.DelayE(900);
            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result);

            yield return InterfaceManager.DelayE(200);
            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 2, task.Result);

            yield return InterfaceManager.DelayE(1000);
            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 3, task.Result);
        }

        [UnityTest]
        public IEnumerator DoRepeatingDelayedEnum() {
            var task = emb.GetI();
            yield return task.ToEnumerator();
            var i = task.Result;

            emb.IncThreeTimesEnum(1000, 1000, 3);

            yield return InterfaceManager.DelayE(900);
            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i, task.Result);

            yield return InterfaceManager.DelayE(200);
            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result);

            yield return InterfaceManager.DelayE(1000);
            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 2, task.Result);

            yield return InterfaceManager.DelayE(1000);
            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 3, task.Result);
        }

        [UnityTest]
        public IEnumerator DoRepeatingAct() {
            var task = emb.GetI();
            yield return task.ToEnumerator();
            var i = task.Result;

            emb.IncThreeTimesAct(0, 1000, 3);

            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result);

            yield return InterfaceManager.DelayE(900);
            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result);

            yield return InterfaceManager.DelayE(200);
            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 2, task.Result);

            yield return InterfaceManager.DelayE(1000);
            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 3, task.Result);
        }

        [UnityTest]
        public IEnumerator DoRepeatingDelayedAct() {
            var task = emb.GetI();
            yield return task.ToEnumerator();
            var i = task.Result;

            emb.IncThreeTimesAct(1000, 1000, 3);

            yield return InterfaceManager.DelayE(900);
            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i, task.Result);

            yield return InterfaceManager.DelayE(200);
            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result);

            yield return InterfaceManager.DelayE(1000);
            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 2, task.Result);

            yield return InterfaceManager.DelayE(1000);
            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 3, task.Result);
        }

        [UnityTest]
        public IEnumerator DoWaitForEnum() {
            var task = emb.GetI();
            yield return task.ToEnumerator();
            var i = task.Result;

            var start = Clock.UtcNow;
            yield return emb.DelayedIncAndWaitEnum(1000).ToEnumerator();

            var diff = (Clock.UtcNow - start).TotalMilliseconds;
            Assert.GreaterOrEqual(diff, 1000);
            Assert.LessOrEqual(diff, 1000 + TWO_FRAMES_MS);

            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i+1, task.Result);
        }

        [UnityTest]
        public IEnumerator DoWaitForAct() {
            var task = emb.GetI();
            yield return task.ToEnumerator();
            var i = task.Result;

            var start = Clock.UtcNow;
            yield return emb.DelayedIncAndWaitAct(1000).ToEnumerator();

            var diff = (Clock.UtcNow - start).TotalMilliseconds;
            Assert.GreaterOrEqual(diff, 1000);
            Assert.LessOrEqual(diff, 1000 + ONE_FRAME_MS);

            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result);
        }

#if EVENTMONOBEHAVIOR_TASK_OPERATORS
        [UnityTest]
        public IEnumerator DoWaitForTask() {
            var task = emb.GetI();
            yield return task.ToEnumerator();
            var i = task.Result;

            var start = Clock.UtcNow;
            yield return emb.DelayedIncAndWaitTask(1000).ToEnumerator();

            var diff = (Clock.UtcNow - start).TotalMilliseconds;
            Assert.GreaterOrEqual(diff, 1000);
            Assert.LessOrEqual(diff, 1000 + TWO_FRAMES_MS);

            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result);
        }
#endif // EVENTMONOBEHAVIOR_TASK_OPERATORS

#if EVENTMONOBEHAVIOR_MANUAL_RESULT_SET
        [UnityTest]
        public IEnumerator DoWaitForManualTriggerEnum() {
            var task = emb.GetI();
            yield return task.ToEnumerator();
            var i = task.Result;

            var start = Clock.UtcNow;
            yield return emb.DelayedIncAndWaitManualTriggerEnum(1000).ToEnumerator();

            var diff = (Clock.UtcNow - start).TotalMilliseconds;
            Assert.GreaterOrEqual(diff, 1000);
            Assert.LessOrEqual(diff, 1000 + TWO_FRAMES_MS);

            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result);
        }

        [UnityTest]
        public IEnumerator DoWaitForManualTriggerAct() {
            var task = emb.GetI();
            yield return task.ToEnumerator();
            var i = task.Result;

            var start = Clock.UtcNow;
            yield return emb.DelayedIncAndWaitManualTriggerAct(1000).ToEnumerator();

            var diff = (Clock.UtcNow - start).TotalMilliseconds;
            Assert.GreaterOrEqual(diff, 1000);
            Assert.LessOrEqual(diff, 1000 + ONE_FRAME_MS);

            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result);
        }

#if EVENTMONOBEHAVIOR_TASK_OPERATORS
        [UnityTest]
        public IEnumerator DoWaitForManualTriggerTask() {
            var task = emb.GetI();
            yield return task.ToEnumerator();
            var i = task.Result;

            var start = Clock.UtcNow;
            yield return emb.DelayedIncAndWaitManualTriggerTask(1000).ToEnumerator();

            var diff = (Clock.UtcNow - start).TotalMilliseconds;
            Assert.GreaterOrEqual(diff, 1000);
            Assert.LessOrEqual(diff, 1000 + TWO_FRAMES_MS);

            task = emb.GetI();
            yield return task.ToEnumerator();
            Assert.AreEqual(i + 1, task.Result);
        }
#endif // EVENTMONOBEHAVIOR_TASK_OPERATORS
#endif // EVENTMONOBEHAVIOR_MANUAL_RESULT_SET


        // -------------------------------------
        // EventMonoBehavior Helper Class
        // -------------------------------------

        class EMB : EventMonoBehaviour {
            protected override void AwakeOverride() { }

            public Mutex<int> mutex = new Mutex<int>(0);
            protected int i = 0;


            // Test DoGet and DoGetManualTrigger with mutex

            public async Task<int> GetMutexValEnum() {
                return await DoGet<int>(GetMutexValEnumHelper);
            }
            protected IEnumerator<int> GetMutexValEnumHelper() {
                yield return mutex.Get();
            }

            public async Task<int> GetMutexValFunc() {
                return await DoGet<int>(GetMutexValFuncHelper);
            }
            protected int GetMutexValFuncHelper() {
                return mutex.Get();
            }

            public async Task<int> GetMutexValTask() {
                return await DoGet<int>(GetMutexValTaskHelper);
            }
            protected async Task<int> GetMutexValTaskHelper() {
                await InterfaceManager.Delay(1);
                return mutex.Get();
            }

            public async Task<int> GetMutexValManualTriggerEnum() {
                return await DoGetManualTrigger<int>(GetMutexValManualTriggerEnumHelper);
            }
            protected IEnumerator GetMutexValManualTriggerEnumHelper(TaskCompletionSource<int> tcs) {
                tcs.SetResult(mutex.Get());
                yield return null;
            }

            public async Task<int> GetMutexValManualTriggerFunc() {
                return await DoGetManualTrigger<int>(GetMutexValManualTriggerFuncHelper);
            }
            protected void GetMutexValManualTriggerFuncHelper(TaskCompletionSource<int> tcs) {
                tcs.SetResult(mutex.Get());
            }

            public async Task<int> GetMutexValManualTriggerTask() {
                return await DoGetManualTrigger<int>(GetMutexValManualTriggerTaskHelper);
            }
            protected Task GetMutexValManualTriggerTaskHelper(TaskCompletionSource<int> tcs) {
                tcs.SetResult(mutex.Get());
                return tcs.Task;
            }


            // Test the rest of the functions with i and GetI

            public async Task<int> GetI() {
                return await DoGet<int>(GetIHelper);
            }
            protected IEnumerator<int> GetIHelper() {
                yield return i;
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

            public void IncThreeTimesEnum(int delayMs, int intervalMs, uint? iterations) {
                DoRepeating(delayMs, intervalMs, iterations, IncThreeTimesEnumHelper);
            }
            protected IEnumerator IncThreeTimesEnumHelper() {
                i += 1;
                yield break;
            }

            public void IncThreeTimesAct(int delayMs, int intervalMs, uint? iterations) {
                DoRepeating(delayMs, intervalMs, iterations, IncThreeTimesActHelper);
            }
            protected void IncThreeTimesActHelper() {
                i += 1;
            }

            public async Task DelayedIncAndWaitEnum(int millisecondsDelay) {
                await DoWaitFor(DelayedIncAndWaitEnumHelper, millisecondsDelay);
            }
            protected IEnumerator DelayedIncAndWaitEnumHelper(int millisecondsDelay) {
                yield return new WaitForSeconds((float)millisecondsDelay / 1000f);
                i += 1;
            }

            public async Task DelayedIncAndWaitAct(int millisecondsDelay) {
                await DoWaitFor(DelayedIncAndWaitActHelper, millisecondsDelay);
            }
            protected void DelayedIncAndWaitActHelper(int millisecondsDelay) {
                var start = Clock.UtcNow;
                while ((Clock.UtcNow - start).TotalMilliseconds < 1000.0) ;
                i += 1;
            }

            public async Task DelayedIncAndWaitTask(int millisecondsDelay) {
                await DoWaitFor(DelayedIncAndWaitTaskHelper, millisecondsDelay);
            }
            protected async Task DelayedIncAndWaitTaskHelper(int millisecondsDelay) {
                await InterfaceManager.Delay(1000);
                i += 1;
            }



            public async Task DelayedIncAndWaitManualTriggerEnum(int millisecondsDelay) {
                await DoWaitForManualTrigger(DelayedIncAndWaitManualTriggerEnumHelper, millisecondsDelay);
            }
            protected IEnumerator DelayedIncAndWaitManualTriggerEnumHelper(TaskCompletionSource<bool> tcs, int millisecondsDelay) {
                yield return new WaitForSeconds((float)millisecondsDelay / 1000f);
                i += 1;
                tcs.SetResult(true);
            }

            public async Task DelayedIncAndWaitManualTriggerAct(int millisecondsDelay) {
                await DoWaitForManualTrigger(DelayedIncAndWaitManualTriggerActHelper, millisecondsDelay);
            }
            protected void DelayedIncAndWaitManualTriggerActHelper(TaskCompletionSource<bool> tcs, int millisecondsDelay) {
                var start = Clock.UtcNow;
                while ((Clock.UtcNow - start).TotalMilliseconds < 1000.0) ;
                i += 1;
                tcs.SetResult(true);
            }

            public async Task DelayedIncAndWaitManualTriggerTask(int millisecondsDelay) {
                await DoWaitForManualTrigger(DelayedIncAndWaitManualTriggerTaskHelper, millisecondsDelay);
            }
            protected async Task DelayedIncAndWaitManualTriggerTaskHelper(TaskCompletionSource<bool> tcs, int millisecondsDelay) {
                await InterfaceManager.Delay(1000);
                i += 1;
                tcs.SetResult(true);
            }
        }
    }

}


