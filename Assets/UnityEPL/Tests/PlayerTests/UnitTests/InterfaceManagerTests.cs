using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

using UnityEPL;

namespace UnityEPLTests {

    public class InterfaceManagerTests {
        const double ONE_FRAME_MS = 1000.0 / 120.0;
        const double DELAY_JITTER_MS = 9;
        // TODO: JPB: (bug) The acceptable jitter for InterfaceManager.Delay() should be less than 9ms

        [UnitySetUp]
        public IEnumerator Setup() {
            if (InterfaceManager.Instance == null) SceneManager.LoadScene("manager");
            yield return null; // Wait for InterfaceManager Awake call
        }

        [Test]
        public void Creation() {
            Assert.AreNotEqual(null, InterfaceManager.Instance);
        }

        // Async Delay has 9ms leniency (because it's bad)
        [Test]
        public void Delay() {
            Task.Run(async () => {
                var start = Clock.UtcNow;
                await InterfaceManager.Delay(1000);
                var diff = (Clock.UtcNow - start).TotalMilliseconds;
                Assert.GreaterOrEqual(diff, 1000);
                Assert.LessOrEqual(diff, 1000 + DELAY_JITTER_MS);
            }).Wait();
        }

        // Enumerator Delay has 9ms leniency (due to frame linking at 120fps)
        [UnityTest]
        public IEnumerator DelayE() {
            var start = Clock.UtcNow;
            yield return InterfaceManager.DelayE(1000);
            var diff = (Clock.UtcNow - start).TotalMilliseconds;
            Assert.GreaterOrEqual(diff, 1000);
            Assert.LessOrEqual(diff, 1000 + ONE_FRAME_MS);
        }

        // Enumerator Delay has 3ms leniency
        [UnityTest]
        public IEnumerator IEnumeratorDelay() {
            var start = Clock.UtcNow;
            yield return InterfaceManager.Delay(1000).ToEnumerator();
            var diff = (Clock.UtcNow - start).TotalMilliseconds;
            Assert.GreaterOrEqual(diff, 1000);
            Assert.LessOrEqual(diff, 1003);
        }

        
    }

}


