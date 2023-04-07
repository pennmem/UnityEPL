using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityEPL {

    public class EventMonoBehaviorTests {
        // A Test behaves as an ordinary method
        [Test]
        public void EventMonoBehaviorTestsSimplePasses() {
            // Use the Assert class to test conditions

            UnityEngine.Debug.Log("AHHH");
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator EventMonoBehaviorTestsWithEnumeratorPasses() {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }

        class EMB : UnityEPL.EventMonoBehaviour {
            protected override void StartOverride() {}
        }
    }

}


