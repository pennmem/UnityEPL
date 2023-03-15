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
        IEnumerator e; 
        while (events.TryDequeue(out e)){
            StartCoroutine(e);
        }
    }

    //////////
    // Devices that can be accessed by managed
    // scripts
    //////////
    public TextDisplayer textDisplayer;
    public InputManager inputManager;
    public VideoManager videoManager;

    void Start()
    {
        textDisplayer = GameObject.Find("TextDisplayer").GetComponent<TextDisplayer>();
        inputManager = this.transform.GetComponent<InputManager>();
        videoManager = this.transform.GetComponent<VideoManager>();

        var exp = new TestExperiment4(this);
    }

    // TODO: JPB: (feature) Make InterfaceManager.Delay() pause aware
    // https://devblogs.microsoft.com/pfxteam/cooperatively-pausing-async-methods/
#if !UNITY_WEBGL || UNITY_EDITOR // System.Threading
    public static async Task Delay(int millisecondsDelay) {
        if (millisecondsDelay < 0) {
            throw new ArgumentOutOfRangeException($"millisecondsDelay <= 0 ({millisecondsDelay})");
        } else if (millisecondsDelay == 0) {
            return;
        }

        await Task.Delay(millisecondsDelay);
    }

    public static async Task Delay(int millisecondsDelay, CancellationToken cancellationToken) {
        if (millisecondsDelay < 0) {
            throw new ArgumentOutOfRangeException($"millisecondsDelay <= 0 ({millisecondsDelay})"); }
        else if (millisecondsDelay == 0) {
            return;
        }

        await Task.Delay(millisecondsDelay, cancellationToken);
    }
#else
    public static async Task Delay(int millisecondsDelay) {
        if (millisecondsDelay < 0) {
            throw new ArgumentOutOfRangeException($"millisecondsDelay <= 0 ({millisecondsDelay})"); }
        else if (millisecondsDelay == 0) {
            return;
        }

        var tcs = new TaskCompletionSource<bool>();
        float seconds = ((float)millisecondsDelay) / 1000;
        _instance.StartCoroutine(WaitForSeconds(seconds, tcs));
        await tcs.Task;
    }

    public static async Task Delay(int millisecondsDelay, CancellationToken cancellationToken) {
        if (millisecondsDelay < 0) {
            throw new ArgumentOutOfRangeException($"millisecondsDelay <= 0 ({millisecondsDelay})"); }
        else if (millisecondsDelay == 0) {
            return;
        }

        var tcs = new TaskCompletionSource<bool>();
        float seconds = ((float)millisecondsDelay) / 1000;
        _instance.StartCoroutine(WaitForSeconds(seconds, cancellationToken, tcs));
        await tcs.Task;
    }

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
