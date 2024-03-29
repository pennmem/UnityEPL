using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Collections;
using UnityEPL;
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace UnityEPLTests {

    public class StackDictionaryTests {
        struct A {
            public int a;
            public int b;
        }


        public static byte[] ObjectToByteArray(object obj) {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream()) {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        public static object ByteArrayToObject(byte[] arrBytes) {
            using (var memStream = new MemoryStream()) {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
            }
        }

        [Test]
        public void Creation() {
            //NativeHashMap<, StackString> hm = new();

            //var nt = new NativeText("Test", Allocator.Persistent);
            //Assert.IsTrue(UnsafeUtility.IsBlittable(typeof(DateTime)));
            //Blittability.AssertBlittable<NativeText>();
            //string s = nt.ToString();
            //Assert.AreEqual("Test", s);

            //NativeParallelHashMap<NativeText, NativeText> hm = new(1, Allocator.Persistent);
            //hm.Add(new("Test", Allocator.Persistent), new("Me", Allocator.Persistent));
            //Dictionary<string, object> dict = new() { { "Test", "Me" } };
            //Assert.AreEqual(JSON.Convert(hm), JSON.Convert(dict));

            //A a = new A() { a = 0, b = 0, s = "hello" };

            //var nr = new NativeReference<A>(a, Allocator.Persistent);

            //var nb = new NativeArray<byte>(Marshal.SizeOf<A>(), Allocator.Persistent);
            //nb.ReinterpretStore<A>(0, a);
            //Blittability.AssertBlittable(nb);
            //var b = nb.ReinterpretLoad<A>(0);
            //Assert.AreEqual(a, b);

            //var sd = StackDynamic2.Instantiate(a);
            //Blittability.AssertBlittable(sd);
            //var b2 = sd.Get<A>();
            //Assert.AreEqual(a, b2);
        }
    }
}
