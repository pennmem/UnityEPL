using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using UnityEPL;

namespace UnityEPLTests {

    public class EventLoopTests {
        // -------------------------------------
        // Globals
        // -------------------------------------

        const double DELAY_JITTER_MS = 2;


        // -------------------------------------
        // DoGet Tests
        //
        // THIS IS NOT HOW YOU SHOULD USE THIS FRAMEWORK
        // Below we are verifying that Do and DoGet work
        // In order to do this, we use a mutex class to guarantee that we are not creating threading issues
        // This methodology should be avoided because it can significantly slow your code down due to the locks
        // Instead just use DoGet like the rest of the example do, once we verify that DoGet works
        // -------------------------------------

        [Test]
        public void DoGetFunc() {
            Task.Run(async () => {
                var el = new EL();
                int i = el.mutex.Get();

                var result = await el.GetMutexValFunc();
                Assert.AreEqual(i, result); // Didn't mutate state
                Assert.AreEqual(el.mutex.Get(), result); // Didn't mutate state but cached value

                el.mutex.Mutate((int i) => { return i + 1; });

                result = await el.GetMutexValFunc();
                Assert.AreEqual(i + 1, result); // Didn't mutate state
                Assert.AreEqual(el.mutex.Get(), result); // Didn't mutate state but cached value
            }).Wait();
        }

        [Test]
        public void DoGetTask() {
            Task.Run(async () => {
                var el = new EL();
                int i = el.mutex.Get();

                var result = await el.GetMutexValTask();
                Assert.AreEqual(i, result); // Didn't mutate state
                Assert.AreEqual(el.mutex.Get(), result); // Didn't mutate state but cached value
            
                el.mutex.Mutate((int i) => { return i + 1; });

                result = await el.GetMutexValTask();
                Assert.AreEqual(i + 1, result); // Didn't mutate state
                Assert.AreEqual(el.mutex.Get(), result); // Didn't mutate state but cached value
            }).Wait();
        }

        // -------------------------------------
        // The rest of the tests
        // -------------------------------------

        [Test]
        public void DoAct() {
            Task.Run(async () => {
                var el = new EL();
                var i = await el.GetI();

                el.IncAct();

                Assert.AreEqual(i + 1, await el.GetI());
            }).Wait();
        }

        [Test]
        public void DoTask() {
            Task.Run(async () => {
                var el = new EL();
                var i = await el.GetI();

                el.IncTask();
                //await InterfaceManager.Delay(100);

                Assert.AreEqual(i + 1, await el.GetI());
            }).Wait();
        }

        [Test]
        public void DoInAct() {
            Task.Run(async () => {
                var el = new EL();
                int i = await el.GetI();

                el.DelayedIncAct(1000);

                await InterfaceManager.Delay(900);
                Assert.AreEqual(i, await el.GetI());

                await InterfaceManager.Delay(200);
                Assert.AreEqual(i + 1, await el.GetI());
            }).Wait();
        }

        [Test]
        public void DoInTask() {
            Task.Run(async () => {
                var el = new EL();
                int i = await el.GetI();

                el.DelayedIncTask(1000);

                await InterfaceManager.Delay(900);
                Assert.AreEqual(i, await el.GetI());

                await InterfaceManager.Delay(200);
                Assert.AreEqual(i + 1, await el.GetI());
            }).Wait();
        }

        [Test]
        public void DoRepeatingAct() {
            Task.Run(async () => {
                var el = new EL();
                int i = await el.GetI();

                el.IncThreeTimesAct();

                Assert.AreEqual(i + 1, await el.GetI());

                await InterfaceManager.Delay(900);
                Assert.AreEqual(i + 1, await el.GetI());

                await InterfaceManager.Delay(200);
                Assert.AreEqual(i + 2, await el.GetI());

                await InterfaceManager.Delay(1000);
                Assert.AreEqual(i + 3, await el.GetI());
            }).Wait();
        }

        [Test]
        public void DoRepeatingDelayedAct() {
            Task.Run(async () => {
                var el = new EL();
                int i = await el.GetI();

                el.DelayedIncThreeTimesAct();

                Assert.AreEqual(i, await el.GetI());

                await InterfaceManager.Delay(900);
                Assert.AreEqual(i, await el.GetI());

                await InterfaceManager.Delay(200);
                Assert.AreEqual(i + 1, await el.GetI());

                await InterfaceManager.Delay(1000);
                Assert.AreEqual(i + 2, await el.GetI());

                await InterfaceManager.Delay(1000);
                Assert.AreEqual(i + 3, await el.GetI());
            }).Wait();
        }

        [Test]
        public void DoRepeatingTask() {
            Task.Run(async () => {
                var el = new EL();
                int i = await el.GetI();

                el.IncThreeTimesTask();

                Assert.AreEqual(i + 1, await el.GetI());

                await InterfaceManager.Delay(900);
                Assert.AreEqual(i + 1, await el.GetI());

                await InterfaceManager.Delay(200);
                Assert.AreEqual(i + 2, await el.GetI());

                await InterfaceManager.Delay(1000);
                Assert.AreEqual(i + 3, await el.GetI());
            }).Wait();
        }

        [Test]
        public void DoRepeatingDelayedTask() {
            Task.Run(async () => {
                var el = new EL();
                int i = await el.GetI();

                el.DelayedIncThreeTimesTask();

                Assert.AreEqual(i, await el.GetI());

                await InterfaceManager.Delay(900);
                Assert.AreEqual(i, await el.GetI());

                await InterfaceManager.Delay(200);
                Assert.AreEqual(i + 1, await el.GetI());

                await InterfaceManager.Delay(1000);
                Assert.AreEqual(i + 2, await el.GetI());

                await InterfaceManager.Delay(1000);
                Assert.AreEqual(i + 3, await el.GetI());
            }).Wait();
        }

        [Test]
        public void DoWaitForAct() {
            Task.Run(async () => {
                var el = new EL();
                int i = await el.GetI();

                var start = Clock.UtcNow;
                await el.DelayedIncAndWaitAct(1000);

                var diff = (Clock.UtcNow - start).TotalMilliseconds;
                Assert.GreaterOrEqual(diff, 1000);
                Assert.LessOrEqual(diff, 1000 + DELAY_JITTER_MS);

                Assert.AreEqual(i + 1, await el.GetI());
            }).Wait();
        }

        [Test]
        public void DoWaitForTask() {
            Task.Run(async () => {
                var el = new EL();
                int i = await el.GetI();

                var start = Clock.UtcNow;
                await el.DelayedIncAndWaitTask(1000);

                var diff = (Clock.UtcNow - start).TotalMilliseconds;
                Assert.GreaterOrEqual(diff, 1000);
                Assert.LessOrEqual(diff, 1000 + DELAY_JITTER_MS);

                Assert.AreEqual(i + 1, await el.GetI());
            }).Wait();
        }
    }


    class EL : EventLoop {
        public Mutex<int> mutex = new Mutex<int>(0);
        protected int i = 0;


        // Test DoGet and DoGetManualTrigger with mutex

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


        // Test the rest of the functions with i and GetI

        public async Task<int> GetI() {
            return await DoGet<int>(GetIHelper);
        }
        protected int GetIHelper() {
            return i;
        }

        public void IncAct() {
            Do(IncActHelper);
        }
        protected void IncActHelper() {
            i += 1;
        }

        public void IncTask() {
            Do(IncTaskHelper);
        }
        protected async Task IncTaskHelper() {
            i += 1;
        }

        public void DelayedIncAct(int millisecondsDelay) {
            DoIn(millisecondsDelay, IncActHelper);
        }
        protected void DelayedIncActHelper() {
            i += 1;
        }

        public void DelayedIncTask(int millisecondsDelay) {
            DoIn(millisecondsDelay, DelayedIncTaskHelper);
        }
        protected async Task DelayedIncTaskHelper() {
            i += 1;
        }

        public void IncThreeTimesAct() {
            DoRepeating(0, 1000, 3, IncThreeTimesActHelper);
        }
        public void DelayedIncThreeTimesAct() {
            DoRepeating(1000, 1000, 3, IncThreeTimesActHelper);
        }
        protected void IncThreeTimesActHelper() {
            i += 1;
        }

        public void IncThreeTimesTask() {
            DoRepeating(0, 1000, 3, IncThreeTimesTaskHelper);
        }
        public void DelayedIncThreeTimesTask() {
            DoRepeating(1000, 1000, 3, IncThreeTimesTaskHelper);
        }
        protected async Task IncThreeTimesTaskHelper() {
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
    }
}
