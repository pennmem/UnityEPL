using System;

namespace UnityEPL {

    public abstract class SingletonEventMonoBehaviour<T> : EventMonoBehaviour
            where T : SingletonEventMonoBehaviour<T> {

        private static T _Instance;
        public static T Instance {
            get {
                if (_Instance == null) {
                    throw new InvalidOperationException($"{typeof(T).Name} SingletonEventMonoBehavior has not initialized. Accessed before it's awake method has been called.");
                }
                return _Instance;
            }
            private set { }
        }

        protected new void Awake() {
            if (_Instance != null) {
                ErrorNotifier.Error(new InvalidOperationException($"Cannot create multiple {typeof(SingletonEventMonoBehaviour<T>).Name} Objects"));
            }
            _Instance = (T)this;
            DontDestroyOnLoad(this.gameObject);

            base.Awake();
        }
    }
}