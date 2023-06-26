using System;

namespace UnityEPL {

    public abstract class SingletonEventMonoBehaviour<T> : EventMonoBehaviour
            where T : SingletonEventMonoBehaviour<T> {

        protected static bool IsInstatiated { get; private set; } = false;
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

        protected SingletonEventMonoBehaviour() {
            if (typeof(T) == typeof(InterfaceManager)) {
                _Instance = (T)this;
            }
        }

        protected new void Awake() {
            if (IsInstatiated) {
                ErrorNotifier.ErrorTS(new InvalidOperationException($"Cannot create multiple {typeof(SingletonEventMonoBehaviour<T>).Name} Objects"));
            }
            IsInstatiated = true;
            _Instance = (T)this;
            DontDestroyOnLoad(this.gameObject);

            base.Awake();
        }
    }
}