using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InterfaceManager2 : MonoBehaviour
{
    //////////
    // Singleton Boilerplate
    // makes sure that only one Experiment Manager
    // can exist in a scene and that this object
    // is not destroyed when changing scenes
    //////////
    
    private static InterfaceManager2 _instance;

    // pass references, rather than relying on Global
    //public static InterfaceManager2 Instance { get { return _instance; } }

    protected void Awake() {
        if (_instance != null && _instance != this) {
            throw new System.InvalidOperationException("Cannot create multiple InterfaceManager Objects");
        } else {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    //////////
    // Singleton Boilerplate
    // makes sure that only one Experiment Manager
    // can exist in a scene and that this object
    // is not destroyed when changing scenes
    //////////

    public ConcurrentQueue<IEnumerator> events = new ConcurrentQueue<IEnumerator>();

    void Update() {
        foreach (var e in events) {
            StartCoroutine(e);
        }
    }

    //////////
    // Devices that can be accessed by managed
    // scripts
    //////////
    public TextDisplayer textDisplayer;

    // Start is called before the first frame update
    void Start()
    {
        textDisplayer = GameObject.Find("TextDisplayer").GetComponent<TextDisplayer>();
        

        var exp = new TestExperiment4(this);
    }

    // TODO: JPB: Make InterfaceManager.Delay() pause aware
    public static async Task Delay(int millisecondsDelay) {
#if UNITY_WEBGL && !UNITY_EDITOR // System.Threading
        var tcs = new TaskCompletionSource<bool>();
        float seconds = ((float)millisecondsDelay) / 1000;
        _instance.StartCoroutine(WaitForSeconds(seconds, tcs));
        await tcs.Task;
#else
        await Task.Delay(millisecondsDelay);
#endif
    }

    public static async Task Delay(int millisecondsDelay, CancellationToken cancellationToken) {
#if UNITY_WEBGL && !UNITY_EDITOR // System.Threading
        var tcs = new TaskCompletionSource<bool>();
        float seconds = ((float)millisecondsDelay) / 1000;
        _instance.StartCoroutine(WaitForSeconds(seconds, cancellationToken, tcs));
        await tcs.Task;
#else
        await Task.Delay(millisecondsDelay, cancellationToken);
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR // System.Threading
    protected static IEnumerator WaitForSeconds(float seconds, TaskCompletionSource<bool> tcs) {
        yield return new WaitForSeconds(seconds);
        tcs?.SetResult(true);
    }

    protected static IEnumerator WaitForSeconds(float seconds, CancellationToken cancellationToken, TaskCompletionSource<bool> tcs) {
        var endTime = Time.fixedTime + seconds;
        Console.WriteLine(seconds);
        Console.WriteLine(Time.fixedTime);
        Console.WriteLine(endTime);
        while (Time.fixedTime < endTime) {
            if (cancellationToken.IsCancellationRequested) {
                Console.WriteLine("CANCELLED");
                tcs?.SetResult(false);
                yield break;
            }
            yield return null;
        }
        tcs?.SetResult(true);
    }
#endif
}
