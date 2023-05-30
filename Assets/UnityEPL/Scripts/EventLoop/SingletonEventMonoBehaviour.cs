using System;

namespace UnityEPL {

    public abstract class SingletonEventMonoBehaviour<T> : EventMonoBehaviour
            where T : SingletonEventMonoBehaviour<T> {

        public static T Instance { get; private set; }

        protected new void Awake() {
            if (Instance != null) {
                ErrorNotifier.Error(new InvalidOperationException($"Cannot create multiple {typeof(SingletonEventMonoBehaviour<T>).Name} Objects"));
            }
            Instance = (T)this;
            DontDestroyOnLoad(this.gameObject);

            base.Awake();
        }
    }
}