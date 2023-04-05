using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

public class InterfaceManager : EventMonoBehaviour {
    //////////
    // Singleton Boilerplate
    // makes sure that only one Experiment Manager
    // can exist in a scene and that this object
    // is not destroyed when changing scenes
    //////////

    private static InterfaceManager _instance;

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
        while (events.TryDequeue(out e)) {
            StartCoroutine(e);
        }
    }

    //////////
    // Experiment Settings and Experiment object
    // that is instantiated once launch is called
    ////////// 
    // global random number source, wrapped so that out of thread 
    // access doesn't break generation
    public static ThreadLocal<System.Random> rnd = new ThreadLocal<System.Random>(() => new System.Random());

    //////////
    // ???
    //////////
    public FileManager fileManager;

    //////////
    // Testing things
    //////////
    public TestTextDisplayer testTextDisplayer;
    public VideoManager videoManager;

    //////////
    // IDK what label // TODO: JPB: Rename this
    //////////
    

    //////////
    // Devices that can be accessed by managed
    // scripts
    //////////
    //public NetworkInterface hostPC;
    //public VideoControl videoControl;
    public TextDisplayer textDisplayer;
    //public SoundRecorder recorder;
    public GameObject warning;
    public AudioSource highBeep;
    public AudioSource lowBeep;
    public AudioSource lowerBeep;
    public AudioSource playback;
    //public RamulatorInterface ramulator;
    public InputManager inputManager;
    //public ISyncBox syncBox;

    //////////
    // Input reporters
    //////////
    //public VoiceActivityDetection voiceActity;
    public ScriptedEventReporter eventReporter;
    public InputReporter peripheralInput;
    public UIDataReporter uiInput;
    private int eventsPerFrame;


    protected override void StartOverride() {
        // Unity internal event handling
        SceneManager.sceneLoaded += onSceneLoaded;

        // create objects not tied to unity
        // TODO: JPB: (needed) Move into onSceneLoaded? 
        fileManager = new FileManager(this);
        //testTextDisplayer = GameObject.Find("TextDisplayer").GetComponent<TestTextDisplayer>();
        inputManager = this.transform.GetComponent<InputManager>();
        videoManager = this.transform.GetComponent<VideoManager>();

        //var exp = new TestExperiment(this);
    }

    //////////
    // collect references to managed objects
    // and release references to non-active objects
    //////////
    void onSceneLoaded(Scene scene, LoadSceneMode mode) {
        // TODO: JPB: (needed) Check
        //onKey = new ConcurrentQueue<Action<string, bool>>(); // clear keyhandler queue on scene change

        // Text Displayer
        GameObject canvas = GameObject.Find("MemoryWordCanvas");
        if (canvas != null) {
            textDisplayer = canvas.GetComponent<TextDisplayer>();
            Debug.Log("Found TextDisplay");
        }

        // Input Reporters
        GameObject inputReporters = GameObject.Find("DataManager");
        if (inputReporters != null) {
            eventReporter = inputReporters.GetComponent<ScriptedEventReporter>();
            peripheralInput = inputReporters.GetComponent<InputReporter>();
            uiInput = inputReporters.GetComponent<UIDataReporter>();
            Debug.Log("Found InputReporters");
        }

        // Voice Activity Detector
        //GameObject voice = GameObject.Find("VAD");
        //if (voice != null) {
        //    voiceActity = voice.GetComponent<VoiceActivityDetection>();
        //    Debug.Log("Found VoiceActivityDetector");
        //}

        // Video Control
        //GameObject video = GameObject.Find("VideoPlayer");
        //if (video != null) {
        //    videoControl = video.GetComponent<VideoControl>();
        //    video.SetActive(false);
        //    Debug.Log("Found VideoPlayer");
        //}

        // Beep Sounds
        //GameObject sound = GameObject.Find("Sounds");
        //if (sound != null) {
        //    lowBeep = sound.transform.Find("LowBeep").gameObject.GetComponent<AudioSource>();
        //    lowerBeep = sound.transform.Find("LowerBeep").gameObject.GetComponent<AudioSource>();
        //    highBeep = sound.transform.Find("HighBeep").gameObject.GetComponent<AudioSource>();
        //    playback = sound.transform.Find("Playback").gameObject.GetComponent<AudioSource>();
        //    Debug.Log("Found Sounds");
        //}

        // Sound Recorder
        //GameObject soundRecorder = GameObject.Find("SoundRecorder");
        //if (soundRecorder != null) {
        //    recorder = soundRecorder.GetComponent<SoundRecorder>();
        //    Debug.Log("Found Sound Recorder");
        //}

        // Ramulator Interface
        //GameObject ramulatorObject = GameObject.Find("RamulatorInterface");
        //if (ramulatorObject != null) {
        //    ramulator = ramulatorObject.GetComponent<RamulatorInterface>();
        //    Debug.Log("Found Ramulator");
        //}
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
            throw new ArgumentOutOfRangeException($"millisecondsDelay <= 0 ({millisecondsDelay})");
        } else if (millisecondsDelay == 0) {
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

    public void ReportEvent(string type, Dictionary<string, object> data = null) {
        eventReporter.ReportScriptedEvent(type, data);
    }
    public void ReportEvent(string type, DateTime time, Dictionary<string, object> data = null) {
        eventReporter.ReportScriptedEvent(type, time, data);
    }

    // This should also be moved to ErrorNotification class
    public void Notify(Exception e) {
        // TODO: JPB: Notify needs to actually use the ErrorNotification
        throw new NotImplementedException("Notify needs to actually use the ErrorNotification", e);
    }
}
