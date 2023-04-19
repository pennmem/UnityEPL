using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

using UnityEPL;

namespace UnityEPLTests {

    public class ErrorNotificationTests {
        [UnitySetUp]
        public IEnumerator Setup() {
            if (InterfaceManager.Instance == null) SceneManager.LoadScene("manager");
            yield return null; // Wait for InterfaceManager Awake call
        }

        [UnityTest]
        public IEnumerator MakeErrorNotification() {
            var inputText = "TESTING";
            ErrorNotifier.Error(new Exception(inputText));
            yield return null; // Wait for next frame
            var actualText = ErrorNotifier.Instance.transform
                .Find("Black Background").Find("Stimulus")
                .GetComponent<Text>().text;
            Assert.AreEqual(inputText, actualText);
            Assert.IsTrue(ErrorNotifier.Instance.isActiveAndEnabled);
        }

        [UnityTest]
        public IEnumerator MakeWarningNotification() {
            var inputText = "TESTING";
            ErrorNotifier.Error(new Exception(inputText));
            yield return null; // Wait for next frame
            var actualText = ErrorNotifier.Instance.transform
                .Find("Black Background").Find("Stimulus")
                .GetComponent<Text>().text;
            Assert.AreEqual(inputText, actualText);
            Assert.IsTrue(ErrorNotifier.Instance.isActiveAndEnabled);
        }

    }

}


