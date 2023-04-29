using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace UnityEPL {

    public class InterfaceManager : SingletonEventMonoBehaviour<InterfaceManager> {
        protected override void AwakeOverride() { }

        const string quitKey = "Escape"; // escape to quit
        const string SYSTEM_CONFIG = "config.json";

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
        private ExperimentBase exp;
        public FileManager fileManager;

        //////////
        // Testing things
        //////////
        public TestTextDisplayer testTextDisplayer;

        //////////
        // IDK what label // TODO: JPB: Rename this
        //////////


        //////////
        // Devices that can be accessed by managed
        // scripts
        //////////
        //public NetworkInterface hostPC;
        public VideoControl videoControl;
        public TextDisplayer textDisplayer;
        //public SoundRecorder recorder;
        public AudioSource highBeep;
        public AudioSource lowBeep;
        public AudioSource lowerBeep;
        public AudioSource playback;
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

        public ConcurrentQueue<IEnumerator> events = new ConcurrentQueue<IEnumerator>();
        void Update() {
            while (events.TryDequeue(out IEnumerator e)) {
                // TODO: JPB: (needed) Wrap all Coroutines in IEnumerator that displays Errors on exception
                //            This will be far more complicated because you can't yield inside a try catch
                //            To fix this, I will have to make my own Couroutine calling class
                //            This will be needed anyway to implement pausing and guarantees on thread ordering anyway
                StartCoroutine(e);
            }
        }

        protected void Start() {
            // Unity internal event handling
            SceneManager.sceneLoaded += onSceneLoaded;

            // create objects not tied to unity
            fileManager = new FileManager(this);
            
            // Setup configs
            var configs = SetupConfigs();
            GetExperiments(configs);

            eventsPerFrame = Config.eventsPerFrame ?? 5;

            // Syncbox interface
            if (!Config.isTest && !Config.noSyncbox) {
                syncBox.Init();
            }

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
        // collect references to managed objects
        // and release references to non-active objects
        //////////
        private void onSceneLoaded(Scene scene, LoadSceneMode mode) {
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
                inputReporter = inputReporters.GetComponent<InputReporter>();
                uiReporter = inputReporters.GetComponent<UIDataReporter>();
                Debug.Log("Found InputReporters");
            }

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

        private void onExperimentSceneLoaded(Scene scene, LoadSceneMode mode) {
            onSceneLoaded(scene, mode);

            string className = $"{typeof(ExperimentBase).Namespace}.{Config.experimentClass}";
            Type classType = Type.GetType(className);
            exp = (ExperimentBase)Activator.CreateInstance(classType, new object[] { this });

            SceneManager.sceneLoaded -= onExperimentSceneLoaded;
            SceneManager.sceneLoaded += onSceneLoaded;
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

        protected void LaunchLauncher() {
            // Reset external hardware state if exiting task
            //syncBox.StopPulse();
            //hostPC?.Disconnect();

            //mainEvents.Pause(true);
            for (int i = 0; i < SceneManager.sceneCount; ++i) {
                UnityEngine.Debug.Log(SceneManager.GetSceneAt(i).name);
            }
            SceneManager.LoadScene(Config.launcherScene);
        }

        protected void LogExperimentInfo() {
            //write versions to logfile
            Dictionary<string, object> versionsData = new Dictionary<string, object>();
            versionsData.Add("application version", Application.version);
            versionsData.Add("build date", BuildInfo.ToString()); // compiler magic, gives compile date
            versionsData.Add("experiment version", Config.experimentName);
            versionsData.Add("logfile version", "0");
            versionsData.Add("participant", Config.participantCode);
            versionsData.Add("session", Config.session);

            ReportEvent("session start", versionsData);
        }


        // These can be called by anything

        public void ReportEvent(string type, Dictionary<string, object> data = null) {
            eventReporter.ReportScriptedEvent(type, data);
        }
        public void ReportEvent(string type, DateTime time, Dictionary<string, object> data = null) {
            eventReporter.ReportScriptedEvent(type, time, data);
        }

        public void Pause(bool pause) {
            // TODO: JPB: (needed) Implement pause functionality correctly
            if (pause) Time.timeScale = 0;
            else Time.timeScale = 1;
        }

        public void Quit() {
            Do(this.Quit);
        }

        // These should only be called by other EventMonoBehaviors

        public void LoadExperimentConfig(string name) {
            Config.experimentConfigName = name;
            Config.SetupExperimentConfig();
        }

        //public void LaunchExperiment() {
        //    Do(LaunchExperimentHandler);
        //}

        public void LaunchExperimentMB() {
            //StartCoroutine(DoWaitForMB(LaunchExperimentHelper));
            DoMB(LaunchExperimentHelper);
        }
        protected void LaunchExperimentHelper() {
            // launch scene with exp, 
            // instantiate experiment,
            // call start function

            // Check if settings are loaded
            if (Config.IsExperimentConfigSetup()) {

                UnityEngine.Cursor.visible = false;
                Application.runInBackground = true;

                // Make the game run as fast as possible
                QualitySettings.vSyncCount = Config.vSync;
                Application.targetFrameRate = Config.frameRate;

                // Create path for current participant/session
                fileManager.CreateSession();

                // Start Syncbox
                //syncBox.StartPulse();

                // Connect to HostPC
                //if (Config.elemem) {
                //    hostPC = new ElememInterface(this);
                //} else if (Config.ramulator) {
                //    hostPC = new RamulatorWrapper(this);
                //}

                LogExperimentInfo();

                SceneManager.sceneLoaded -= onSceneLoaded;
                SceneManager.sceneLoaded += onExperimentSceneLoaded;
                SceneManager.LoadScene(Config.experimentScene);
            } else {
                throw new Exception("No experiment configuration loaded");
            }
        }
    }

}