using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

using UnityEPL;

namespace UnityEPLTests {

    public class SingletonEventMonoBehaviorTests {
        // -------------------------------------
        // Globals
        // -------------------------------------

        bool isSetup = false;

        // -------------------------------------
        // Setup
        // -------------------------------------

        [UnitySetUp]
        public IEnumerator Setup() {
            if (!isSetup) {
                isSetup = true;
                SceneManager.LoadScene("manager");
                yield return null; // Wait for InterfaceManager Awake call
            }
        }

        // -------------------------------------
        // General Tests
        // -------------------------------------

        [UnityTest]
        public IEnumerator Creation() {
            var semb = new GameObject().AddComponent<SEMB>();
            Assert.AreNotEqual(null, semb);
            GameObject.Destroy(semb);

            yield break;
        }

        [UnityTest]
        public IEnumerator Singleton() {
            var semb = new GameObject().AddComponent<SEMB2>();
            Assert.AreNotEqual(null, semb);

            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: .*"));
            var failing = new GameObject().AddComponent<SEMB2>();

            GameObject.Destroy(semb);
            GameObject.Destroy(failing);

            yield break;
        }

        class SEMB : SingletonEventMonoBehaviour<SEMB> {
            protected override void AwakeOverride() { }
        }

        class SEMB2 : SingletonEventMonoBehaviour<SEMB2> {
            protected override void AwakeOverride() { }
        }

    }

}


