using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

using UnityEPL;

namespace UnityEPLTests {

    public class ErrorNotifierTests {
        [UnitySetUp]
        public IEnumerator Setup() {
            if (InterfaceManager.Instance == null) SceneManager.LoadScene("manager");
            yield return null; // Wait for InterfaceManager Awake call
        }

        [UnityTest]
        public IEnumerator MakeErrorNotification() {
            var inputText = "TESTING";

            LogAssert.Expect(LogType.Exception, new Regex("Exception: .*"));

            Assert.Throws<Exception>(() => {
                ErrorNotifier.Error(new Exception(inputText));
            });

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

            ErrorNotifier.Warning(new Exception(inputText));

            yield return null; // Wait for next frame
            var actualText = ErrorNotifier.Instance.transform
                .Find("Black Background").Find("Stimulus")
                .GetComponent<Text>().text;
            Assert.AreEqual(inputText, actualText);
            Assert.IsTrue(ErrorNotifier.Instance.isActiveAndEnabled);
        }

    }

}


