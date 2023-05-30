using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using UnityEPL;

namespace UnityEPLTests {

    public class MutexTests {
        [Test]
        public void MutexSafety() {
            Mutex<Int32> m = new(0);
            var a = m.Get();
            a = new ReadOnly<Int32>(1);
            Assert.AreNotEqual((Int32)m.Get(), (Int32)a);
        }

        [Test]
        public void MutexMutation() {
            Mutex<Int32> m = new(0);
            Assert.AreEqual(0, (int)m.Get());
            m.Mutate((int i) => { return i + 1; });
            Assert.AreEqual(1, (int)m.Get());
        }
    }

}
