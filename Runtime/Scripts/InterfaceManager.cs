
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace UnityEPL {

    public class InterfaceManager : SingletonEventMonoBehaviour<InterfaceManager> {
        public static new InterfaceManager Instance {
            get {
                var instance = SingletonEventMonoBehaviour<InterfaceManager>.Instance;
                if (instance == null) {
                    throw new InvalidOperationException("InterfaceManager not initialized. The starting scene of the game MUST be the manager scene.");
                }
                return instance;
            }
            private set { }
        }

        const string quitKey = "Escape"; // escape to quit
        const string SYSTEM_CONFIG = "config.json";

        //////////
        // Random Number Generators
        ////////// 
        public static ThreadLocal<System.Random> rnd { get; private set; } = new(() => { return new(); });
        public static ThreadLocal<System.Random> stableRnd { get; private set; } = null;

        //////////
        // ???
        //////////
        public FileManager fileManager;

        //////////
        // Devices that can be accessed by managed
        // scripts
        //////////
        public HostPC hostPC;
        public VideoControl videoControl;
        public TextDisplayer textDisplayer;
        public SoundRecorder recorder;
        //public RamulatorInterface ramulator;
        public ISyncBox syncBox;

        //////////
        // Input reporters
        //////////
        //public VoiceActivityDetection voiceActity;
        public ScriptedEventReporter eventReporter;
        public InputReporter inputReporter;
        public UIDataReporter uiReporter;
        private int eventsPerFrame;

        //////////
        // Provided AudioSources
        //////////
        public AudioSource highBeep;
        public AudioSource lowBeep;
        public AudioSource lowerBeep;
        public AudioSource playback;

        //////////
        // Event Loop Handling
        //////////
        public ConcurrentBag<EventLoop> eventLoops = new();
        public ConcurrentQueue<IEnumerator> events = new();

        //////////
        // StartTime
        //////////
        public DateTime StartTimeTS { get; protected set; }
        public TimeSpan TimeSinceStartupTS {
            get { return Clock.UtcNow - StartTimeTS; }
            protected set { }
        }
        public DateTime TimeStamp {
            get { return StartTimeTS.Add(TimeSinceStartupTS); }
            private set { }
        }

        //////////
        // Setup
        //////////

        protected void OnDestroy() {
            QuitTS();
        }

        void Update() {
            while (events.TryDequeue(out IEnumerator e)) {
                StartCoroutine(e);
            }
        }

        protected override void AwakeOverride() {
            StartTimeTS = Clock.UtcNow;
        }

        protected void Start() {
            // Unity internal event handling
            SceneManager.sceneLoaded += onSceneLoaded;

            // Create objects not tied to unity
            fileManager = new FileManager(this);

            // Setup Text Displayer
            textDisplayer = TextDisplayer.Instance;

            // Setup Input Reporters
            eventReporter = ScriptedEventReporter.Instance;
            inputReporter = InputReporter.Instance;
            //uiReporter = UIDataReporter.Instance;

            // Setup Configs
            var configs = SetupConfigs();
            GetExperiments(configs);
            eventsPerFrame = Config.eventsPerFrame ?? 5;

            // Setup Syncbox Interface
            if (!Config.isTest && !Config.noSyncbox) {
                syncBox.Init();
            }

            // Launch Startup Scene
            LaunchLauncher();
        }

        protected string[] SetupConfigs() {
#if !UNITY_WEBGL // System.IO
            Config.SetupSystemConfig(fileManager.ConfigPath());
#else // !UNITY_WEBGL
        Config.SetupSystemConfig(Application.streamingAssetsPath);
#endif // !UNITY_WEBGL

            // Get all configuration files
            string configPath = fileManager.ConfigPath();
            string[] configs = Directory.GetFiles(configPath, "*.json");
            if (configs.Length < 2) {
                ErrorNotifier.Error(new Exception("Configuration File Error"));
            }
            return configs;
        }

        protected void GetExperiments(string[] configs) {
            List<string> exps = new List<string>();

            for (int i = 0, j = 0; i < configs.Length; i++) {
                Debug.Log(configs[i]);
                if (!configs[i].Contains(SYSTEM_CONFIG))
                    exps.Add(Path.GetFileNameWithoutExtension(configs[i]));
                j++;
            }
            Config.availableExperiments = exps.ToArray();
        }

        //////////
        // Collect references to managed objects
        // and release references to non-active objects
        //////////
        private void onSceneLoaded(Scene scene, LoadSceneMode mode) {
            Debug.Log("onSceneLoaded");
            // TODO: JPB: (needed) Check
            //onKey = new ConcurrentQueue<Action<string, bool>>(); // clear keyhandler queue on scene change

            // Voice Activity Detector
            //GameObject voice = GameObject.Find("VAD");
            //if (voice != null) {
            //    voiceActity = voice.GetComponent<VoiceActivityDetection>();
            //    Debug.Log("Found VoiceActivityDetector");
            //}

            // Video Control
            GameObject video = GameObject.Find("VideoPlayer");
            if (video != null) {
                videoControl = video.GetComponent<VideoControl>();
                video.SetActive(false);
                Debug.Log("Found VideoPlayer");
            }

            // Beep Sounds
            GameObject sound = GameObject.Find("Sounds");
            if (sound != null) {
                lowBeep = sound.transform.Find("LowBeep").gameObject.GetComponent<AudioSource>();
                lowerBeep = sound.transform.Find("LowerBeep").gameObject.GetComponent<AudioSource>();
                highBeep = sound.transform.Find("HighBeep").gameObject.GetComponent<AudioSource>();
                playback = sound.transform.Find("Playback").gameObject.GetComponent<AudioSource>();
                Debug.Log("Found Sounds");
            }

            // Sound Recorder
            GameObject soundRecorder = GameObject.Find("SoundRecorder");
            if (soundRecorder != null) {
                recorder = soundRecorder.GetComponent<SoundRecorder>();
                Debug.Log("Found Sound Recorder");
            }

            // Ramulator Interface
            //GameObject ramulatorObject = GameObject.Find("RamulatorInterface");
            //if (ramulatorObject != null) {
            //    ramulator = ramulatorObject.GetComponent<RamulatorInterface>();
            //    Debug.Log("Found Ramulator");
            //}
        }

        private void onExperimentSceneLoaded(Scene scene, LoadSceneMode mode) {
            Debug.Log("onExperimentSceneLoaded");

            // Experiment Manager
            try {
                var expManager = scene.GetRootGameObjects().Where(go => go.name == Config.experimentClass).First();
                expManager.SetActive(true);
            } catch (InvalidOperationException exception) {
                ErrorNotifier.Error(new Exception(
                    $"Missing experiment GameObject that is the same name as the experiment class ({Config.experimentClass})" +
                    "If it exists, did you set it to un-active (this is needed incase you have multiple experiments per scene)",
                    exception));
            }

            SceneManager.sceneLoaded -= onExperimentSceneLoaded;
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

        public static IEnumerator DelayE(int millisecondsDelay) {
            //yield return new WaitForSeconds(millisecondsDelay / 1000.0f);
            yield return InterfaceManager.Delay(millisecondsDelay).ToEnumerator();
        }

        public static IEnumerator DelayE(int millisecondsDelay, CancellationToken cancellationToken) {
            yield return InterfaceManager.Delay(millisecondsDelay, cancellationToken).ToEnumerator();
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
        Instance.StartCoroutine(WaitForSeconds(seconds, tcs));
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
        Instance.StartCoroutine(WaitForSeconds(seconds, cancellationToken, tcs));
        await tcs.Task;
    }

    public static IEnumerator DelayE(int millisecondsDelay) {
        yield return InterfaceManager.Delay(millisecondsDelay).ToEnumerator();
    }

    public static IEnumerator DelayE(int millisecondsDelay, CancellationToken cancellationToken) {
        yield return InterfaceManager.Delay(millisecondsDelay, cancellationToken).ToEnumerator();
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

        protected void LaunchLauncher() {
            // Reset external hardware state if exiting task
            //syncBox.StopPulse();
            hostPC?.SendExitMsg();

            //mainEvents.Pause(true);
            for (int i = 0; i < SceneManager.sceneCount; ++i) {
                UnityEngine.Debug.Log(SceneManager.GetSceneAt(i).name);
            }
            SceneManager.LoadScene(Config.launcherScene);
        }

        // These can be called by anything
        public void PauseTS(bool pause) {
            DoTS<Bool>(PauseHelper, pause);
        }
        protected void PauseHelper(Bool pause) {
            // TODO: JPB: (needed) Implement pause functionality correctly
            if (pause) Time.timeScale = 0;
            else Time.timeScale = 1;
        }

        public void QuitTS() {
            hostPC?.Quit();
            DoTS(QuitHelper);
        }
        protected void QuitHelper() {
            foreach (var eventLoop in eventLoops) {
                //eventLoop.Stop();
                eventLoop.Abort();
            }
            ((MonoBehaviour)this).Quit();
        }

        public void LaunchExperimentTS() {
            DoTS(LaunchExperimentHelper);
        }
        protected IEnumerator LaunchExperimentHelper() {
            // launch scene with exp, 
            // instantiate experiment,
            // call start function

            // Check if settings are loaded
            if (Config.IsExperimentConfigSetup()) {
                stableRnd = new(() => new(Config.subject.GetHashCode()));

                UnityEngine.Cursor.visible = false;
                Application.runInBackground = true;

                // Make the game run as fast as possible
                QualitySettings.vSyncCount = Config.vSync;
                Application.targetFrameRate = Config.frameRate;

                // Create path for current participant/session
                fileManager.CreateSession();

                // Connect to HostPC
                if (Config.elememOn) {
                    hostPC = new ElememInterface();
                } else if (Config.ramulatorOn) {
                    // TODO: JPB: (needed) Add Ramulator integration
                    //hostPC = new RamulatorWrapper(this);
                }
                yield return hostPC?.Connect().ToEnumerator();
                yield return hostPC?.Configure().ToEnumerator();

                SceneManager.sceneLoaded += onExperimentSceneLoaded;
                SceneManager.LoadScene(Config.experimentScene);
            } else {
                throw new Exception("No experiment configuration loaded");
            }
        }

        public void LoadExperimentConfig(string name) {
            Do(LoadExperimentConfigHelper, name);
        }
        public void LoadExperimentConfigHelper(string name) {
            Config.experimentConfigName = name;
            Config.SetupExperimentConfig();
        }


        // Helpful functions

        public void LockCursor(CursorLockMode isLocked) {
            Do(LockCursorHelper, isLocked);
        }
        public void LockCursorTS(CursorLockMode isLocked) {
            DoTS(LockCursorHelper, isLocked);
        }
        public void LockCursorHelper(CursorLockMode isLocked) {
            UnityEngine.Cursor.lockState = isLocked;
            UnityEngine.Cursor.visible = isLocked == CursorLockMode.None;
        }





        // -------------------------------------
        // CODE FOR POSTER

        public class LightController {
            public async Task StartClosedLoop(int durationMS) { await Delay(1); }
        }
        public LightController lightController;

        SFB.ExtensionFilter[] fileExtensions = new[] {
            new SFB.ExtensionFilter("Videos", "mp4", "mov"),
            new SFB.ExtensionFilter("All Files", "*" ),
        };

        protected async Task ClosedLoopVideo() {
            await manager.videoControl.SelectVideoFile(Config.dataPath, fileExtensions);
            await manager.textDisplayer.PressAnyKey("Start Video", "Press a button to play the video");
            await manager.lightController.StartClosedLoop(manager.videoControl.videoLength);
            await manager.videoControl.PlayVideo();
            var loggingMsg = new Dictionary<string, object> { { "length", manager.videoControl.videoLength } };
            manager.eventReporter.ReportTS("Video Info", loggingMsg);
        }

        // -------------------------------------
    }

}