using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static Blittability;

public class EventMonoBehavior : MonoBehaviour {
    InterfaceManager2 manager;
    

    void Start() {
        manager = GameObject.Find("InterfaceManager").GetComponent<InterfaceManager2>();
    }



    protected void Do(Func<IEnumerator> func) {
        manager.events.Enqueue(func());
    }
    protected void Do<T>(Func<T, IEnumerator> func, T t)
            where T : struct {
        AssertBlittable(t);
        T tCopy = t;
        manager.events.Enqueue(func(t));
    }

    // Update is called once per frame
    void Update() {

    }
}

public class TextDisplayer : EventMonoBehavior {
    const long delay = 10000000000;
    public TextMesh tm;

    // Update is called once per frame
    void Update() {

    }

    public void UpdateText(StackString ss) {
        Do(UpdateTextHelper, ss);
    }

    public IEnumerator UpdateTextHelper(StackString ss) {
        tm.text = ss;
        yield break;
    }


}
