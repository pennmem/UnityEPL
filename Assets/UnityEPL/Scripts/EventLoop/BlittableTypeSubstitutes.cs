using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;

namespace UnityEPL {

    // Information on blittability
    // https://learn.microsoft.com/en-us/dotnet/framework/interop/blittable-and-non-blittable-types

    // TODO: JPB: (refactor) Remove Bool and Char structs once we have blittable bools/chars or use IComponentData

    public struct Bool {
        private byte _val;
        public Bool(bool b) {
            if (b) {
                _val = 1;
            } else {
                _val = 0;
            }
        }

        public static implicit operator bool(Bool b) => b._val == 1;
        public static implicit operator Bool(bool b) => new Bool(b);

        public static bool operator true(Bool b) => b._val == 1;
        public static bool operator false(Bool b) => b._val == 0;

        public static bool operator ==(Bool x, Bool y) => x._val == y._val;
        public static bool operator !=(Bool x, Bool y) => !(x == y);
        public static bool operator ==(Bool x, bool y) => (y && x) || (!x && !y);
        public static bool operator !=(Bool x, bool y) => !(x == y);
        public static bool operator ==(bool x, Bool y) => (y && x) || (!x && !y);
        public static bool operator !=(bool x, Bool y) => !(x == y);

        public override string ToString() => ((bool)this).ToString();

        public override bool Equals(object obj) {
            return (obj is Bool b) && (_val == b._val);
        }

        public override int GetHashCode() {
            return HashCode.Combine(_val);
        }
    }

    public struct Char {
        private UInt16 _val;
        public Char(char c) {
            _val = (UInt16)c;
        }
        public static implicit operator char(Char c) => (char)c._val;
        public static implicit operator Char(char c) => new Char(c);
        public override string ToString() => ((char)_val).ToString();
    }

    public struct StackString {
        NativeArray<UInt16> _val;

        public StackString(String s) {
            _val = new NativeArray<UInt16>(s.Length, Allocator.Persistent);
            for (int i = 0; i < s.Length; ++i) {
                _val[i] = s[i];
            }
        }

        public UInt16 this[int key] {
            get => _val[key];
            set => _val[key] = value;
        }

        public static implicit operator String(StackString ss) {
            var data = ss._val.ToArray();
            byte[] asBytes = new byte[data.Length * sizeof(ushort)];
            Buffer.BlockCopy(data, 0, asBytes, 0, asBytes.Length);
            return Encoding.Unicode.GetString(asBytes);
        }
        public static implicit operator StackString(String s) {
            return new StackString(s);
        }

        public override string ToString() {
            return this;
        }
    }

    public static class UnityEplExtensions {
        public static NativeArray<UInt16> ToNativeArray(this string s) {
            var na = new NativeArray<UInt16>(s.Length, Allocator.Persistent);
            for (int i = 0; i < s.Length; ++i) {
                na[i] = s[i];
            }
            return na;
        }

        public static String ToString(this NativeArray<UInt16> na) {
            var data = na.ToArray();
            byte[] asBytes = new byte[data.Length * sizeof(ushort)];
            Buffer.BlockCopy(data, 0, asBytes, 0, asBytes.Length);
            return Encoding.Unicode.GetString(asBytes);
        }
    }

    public static class Blittability {
        // AssertBlittable

        public static bool IsPassable(Type t) {
            return UnsafeUtility.IsBlittable(t)
                || t == typeof(bool)
                || t == typeof(char);
        }

        // TODO: JPB: (feature) Maybe use IComponentData from com.unity.entities when it releases
        //            This will also allow for bool and char to be included in the structs
        //            https://docs.unity3d.com/Packages/com.unity.entities@0.17/api/Unity.Entities.IComponentData.html
        public static void AssertBlittable<T>(T t)
                where T : struct {
            if (!IsPassable(typeof(T))) {
                throw new ArgumentException("The first argument is not a blittable type.");
            }
        }
        public static void AssertBlittable<T, U>(T t, U u)
                where T : struct
                where U : struct {
            if (!IsPassable(typeof(T))) {
                throw new ArgumentException("The first argument is not a blittable type.");
            } else if (!IsPassable(typeof(U))) {
                throw new ArgumentException("The second argument is not a blittable type.");
            }
        }
        public static void AssertBlittable<T, U, V>(T t, U u, V v)
                where T : struct
                where U : struct
                where V : struct {
            if (!IsPassable(typeof(T))) {
                throw new ArgumentException("The first argument is not a blittable type.");
            } else if (!IsPassable(typeof(U))) {
                throw new ArgumentException("The second argument is not a blittable type.");
            } else if (!IsPassable(typeof(V))) {
                throw new ArgumentException("The third argument is not a blittable type.");
            }
        }
        public static void AssertBlittable<T, U, V, W>(T t, U u, V v, W w)
                where T : struct
                where U : struct
                where V : struct
                where W : struct {
            if (!IsPassable(typeof(T))) {
                throw new ArgumentException("The first argument is not a blittable type.");
            } else if (!IsPassable(typeof(U))) {
                throw new ArgumentException("The second argument is not a blittable type.");
            } else if (!IsPassable(typeof(V))) {
                throw new ArgumentException("The third argument is not a blittable type.");
            } else if (!IsPassable(typeof(W))) {
                throw new ArgumentException("The fourth argument is not a blittable type.");
            }
        }
    }
}