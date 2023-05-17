using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
        protected override void AwakeOverride() { }

        const string quitKey = "Escape"; // escape to quit
        const string SYSTEM_CONFIG = "config.json";

        //////////
        // Experiment Settings and Experiment object
        // that is instantiated once launch is called
        ////////// 
        // global random number source, wrapped so that out of thread 
        // access doesn't break generation
        public static ThreadLocal<System.Random> rnd { get; private set; } = new();
        public static ThreadLocal<System.Random> stableRnd { get; private set; } = null;

        //////////
        // ???
        //////////
        private ExperimentBase exp;
        public FileManager fileManager;

        //////////
        // Testing things
        //////////
        // TODO: JPB: (needed) (refactor) Remove TestTextDisplayer
        public TestTextDisplayer testTextDisplayer;

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

        // TODO: JPB: (needed) (refactor) Move below variables out of Interface Manager
        protected AudioSource highBeep;
        protected AudioSource lowBeep;
        protected AudioSource lowerBeep;
        protected AudioSource playback;
        public Task PlayLowBeep() {
            return DoWaitFor(PlayLowBeepHelper);
        }
        public IEnumerator PlayLowBeepHelper() {
            lowBeep.Play();
            yield return InterfaceManager.DelayE((int)(lowBeep.clip.length * 1000) + 100);
        }
        public void SetPlayback(AudioClip audioClip) {
            Do(() => { playback.clip = audioClip; });
        }
        public void PlayPlayback() {
            Do(playback.Play);
        }





        public ConcurrentBag<EventLoop> eventLoops = new();

        public ConcurrentQueue<IEnumerator> events = new();

        private void OnDestroy() {
            Quit();
        }

        void Update() {
            while (events.TryDequeue(out IEnumerator e)) {
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
            onSceneLoaded(scene, mode);

            string className = $"{typeof(ExperimentBase).Namespace}.{Config.experimentClass}";
            Type classType = Type.GetType(className);
            exp = (ExperimentBase)Activator.CreateInstance(classType, new object[] { this });

            LogExperimentInfo();

            // Start Syncbox
            //syncBox.StartPulse();

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

        protected void LogExperimentInfo() {
            //write versions to logfile
            Dictionary<string, object> versionsData = new() {
                { "application version", Application.version },
                { "build date", BuildInfo.ToString() }, // compiler magic, gives compile date
                { "experiment version", Config.experimentName },
                { "logfile version", "0" },
                { "participant", Config.subject },
                { "session", Config.session },
            };

            eventReporter.ReportScriptedEvent("session start", versionsData);
        }


        // These can be called by anything
        public void Pause(bool pause) {
            Do<Bool>(PauseHelper, pause);
        }
        protected void PauseHelper(Bool pause) {
            // TODO: JPB: (needed) Implement pause functionality correctly
            if (pause) Time.timeScale = 0;
            else Time.timeScale = 1;
        }

        public void Quit() {
            hostPC?.Quit();
            Do(QuitHelper);
        }
        protected void QuitHelper() {
            foreach (var eventLoop in eventLoops) {
                //eventLoop.Stop();
                eventLoop.Abort();
            }
            ((MonoBehaviour)this).Quit();
        }

        public void LaunchExperiment() {
            Do(LaunchExperimentHelper);
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

                SceneManager.sceneLoaded -= onSceneLoaded;
                SceneManager.sceneLoaded += onExperimentSceneLoaded;
                SceneManager.LoadScene(Config.experimentScene);
            } else {
                throw new Exception("No experiment configuration loaded");
            }
        }

        public void LoadExperimentConfigMB(string name) {
            DoMB(LoadExperimentConfigHelper, name);
        }
        public void LoadExperimentConfigHelper(string name) {
            Config.experimentConfigName = name;
            Config.SetupExperimentConfig();
        }


        // Helpful functions

        public void LockCursor(CursorLockMode isLocked) {
            Do(LockCursorHelper, isLocked);
        }
        public void LockCursorMB(CursorLockMode isLocked) {
            DoMB(LockCursorHelper, isLocked);
        }
        public void LockCursorHelper(CursorLockMode isLocked) {
            UnityEngine.Cursor.lockState = isLocked;
            UnityEngine.Cursor.visible = isLocked == CursorLockMode.None;
        }
    }

}