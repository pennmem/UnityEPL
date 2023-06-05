using System;
using System.Threading;

namespace UnityEPL {

    public class Mutex<T> {
        private T val;
        private Mutex mutex;

        public Mutex(T val) {
            this.val = val;
            this.mutex = new Mutex();
        }

        public void Mutate(Func<T, T> mutator) {
            mutex.WaitOne();
            val = mutator(val);
            mutex.ReleaseMutex();
        }

        public ReadOnly<T> Get() {
            mutex.WaitOne();
            var ret = new ReadOnly<T>(val);
            mutex.ReleaseMutex();
            return ret;
        }

        public ReadOnly<T> MutateGet(Func<T, T> mutator) {
            mutex.WaitOne();
            val = mutator(val);
            var ret = new ReadOnly<T>(val);
            mutex.ReleaseMutex();
            return ret;
        }
    }

    public ref struct ReadOnly<T> {
        T val;

        public ReadOnly(T val) {
            this.val = val;
        }

        public static implicit operator T(ReadOnly<T> ro) {
            return ro.val;
        }
    }

}
