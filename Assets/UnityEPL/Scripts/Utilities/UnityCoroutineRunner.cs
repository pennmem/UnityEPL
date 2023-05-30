using System.Collections;
using UnityEngine;

// This allows you to call coroutines from a normal function (it blocks)
// This can only be used on the main thread (due to Unity restrictions)
// If you need to get around this, then add a UnityCoroutineRunner to InterfaceManager.
// This would need to be made thread safe though.
public class UnityCoroutineRunner : MonoBehaviour {
    public static UnityCoroutineRunner Generate() {
        var gameObject = new GameObject();
        gameObject.isStatic = true;
        return gameObject.AddComponent<UnityCoroutineRunner>();
    }

    // This is blocking
    public void RunCoroutine(IEnumerator enumerator) {
        this.StartCoroutine(enumerator);
    }

    UnityCoroutineRunner() { }
    ~UnityCoroutineRunner() { Destroy(transform.gameObject); }
}

