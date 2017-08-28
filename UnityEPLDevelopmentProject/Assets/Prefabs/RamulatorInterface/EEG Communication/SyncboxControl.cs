using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
#if UNITY_STANDALONE_WIN
using LabJack.LabJackUD;
using LabJack;
#endif

	public class SyncboxControl : MonoBehaviour {
		Experiment exp { get { return Experiment.Instance; } }

	#if FREIBURG
	[DllImport ("FreiburgSyncboxPlugin")]
	private static extern IntPtr OpenUSB();
	[DllImport ("FreiburgSyncboxPlugin")]
	private static extern IntPtr CloseUSB();
	[DllImport ("FreiburgSyncboxPlugin")]
	private static extern IntPtr TurnLEDOn();
	[DllImport ("FreiburgSyncboxPlugin")]
	private static extern IntPtr TurnLEDOff();
	[DllImport ("FreiburgSyncboxPlugin")]
	private static extern int CheckUSB ();
#else
		[DllImport ("ASimplePlugin")]
		private static extern IntPtr OpenUSB();
		[DllImport ("ASimplePlugin")]
		private static extern IntPtr CloseUSB();
		[DllImport ("ASimplePlugin")]
		private static extern IntPtr TurnLEDOn();
	[DllImport ("ASimplePlugin")]
	private static extern int CheckUSB ();
		[DllImport ("ASimplePlugin")]
		private static extern IntPtr TurnLEDOff();
		[DllImport ("ASimplePlugin")]
		private static extern long SyncPulse();
		[DllImport ("ASimplePlugin")]
		private static extern IntPtr StimPulse(float durationSeconds, float freqHz, bool doRelay);
#endif
		public bool ShouldSyncPulse = true;
		public float PulseOnSeconds;
		public float PulseOffSeconds;

		public bool isUSBOpen = false;

	#if UNITY_STANDALONE_WIN
	//u3 specific
	private U3 u3;
	double dblDriverVersion;
	LJUD.IO ioType = 0;
	LJUD.CHANNEL channel = 0;
	#endif


		//SINGLETON
		private static SyncboxControl _instance;

		public static SyncboxControl Instance{
			get{
				return _instance;
			}
		}

		void Awake(){

			if (_instance != null) {
				UnityEngine.Debug.Log("Instance already exists!");
				Destroy(transform.gameObject);
				return;
			}
			_instance = this;

		}

		// Use this for initialization
		void Start () {
			if(Config.isSyncbox){
				StartCoroutine(ConnectSyncbox());
			}
		}

	#if UNITY_STANDALONE_WIN
	IEnumerator TurnOnOff()
	{
		LJUD.eDO(u3.ljhandle, 0, 1);
		yield return new WaitForSeconds(2f);
		LJUD.eDO(u3.ljhandle, 0, 0);
		yield return null;
	}
	public void ShowErrorMessage(LabJackUDException e)
	{
		UnityEngine.Debug.Log("ERROR: " + e.ToString());


	}
	#endif

		IEnumerator ConnectSyncbox(){
		
		string connectionError = "";
			while(!isUSBOpen){
			UnityEngine.Debug.Log ("attempting to connect");
			#if !UNITY_STANDALONE_WIN
				string usbOpenFeedback = Marshal.PtrToStringAuto (OpenUSB());
				UnityEngine.Debug.Log(usbOpenFeedback);
				if(usbOpenFeedback != "didn't open USB..."){
					isUSBOpen = true;
				}
			#else
			try
			{

				u3 = new U3(LJUD.CONNECTION.USB, "0", true); // Connection through USB
				//Start by using the pin_configuration_reset IOType so that all
				//pin assignments are in the factory default condition.


			}
			catch (LabJackUDException e)
			{
				connectionError = e.ToString();
				ShowErrorMessage(e);
			}
			//   StartCoroutine("TurnOnOff");
			UnityEngine.Debug.Log("connectionerror " + connectionError);
			if (connectionError == "") {
				isUSBOpen = true;
			}
			else
			{
				exp.trialController.ConnectionText.text = "Please connect Syncbox and Restart";
			}
			#endif
				yield return 0;
			}
		ShouldSyncPulse = true;
			StartCoroutine ("CheckSyncboxConnection");
			StartCoroutine ("RunSyncPulseManual");
		yield return null;
		}

		// Update is called once per frame
		void Update () {
			GetInput ();

//		if (Input.GetKeyDown (KeyCode.A)) {
//			int ok = CheckUSB ();
//
//			UnityEngine.Debug.Log (ok.ToString());
//		}
		}

		void GetInput(){
			//use this for debugging if you'd like
		}

	IEnumerator CheckSyncboxConnection()
	{
		while (ShouldSyncPulse) {
			int syncStatus = CheckUSB ();
			UnityEngine.Debug.Log ("sync status is: " + syncStatus.ToString ());
			#if FREIBURG
			if (syncStatus == 0) {
			UnityEngine.Debug.Log ("Syncbox connected");
			} 
			#else
			if (syncStatus == 1) {
				UnityEngine.Debug.Log ("Syncbox connected");
			} 
			#endif
			else {
				isUSBOpen = false;
				UnityEngine.Debug.Log ("disconnected; initiating reconnection procedure");
				StartCoroutine (ReconnectSyncbox ());
			}
			yield return new WaitForSeconds (2f); //check every 2 seconds
			yield return 0;
		}
		yield return null;
	}

	IEnumerator ReconnectSyncbox()
	{
		//stop running coroutines
		StopCoroutine ("RunSyncPulseManual");
		StopCoroutine ("CheckSyncboxConnection");

		//close any lingering USB handles
		UnityEngine.Debug.Log(Marshal.PtrToStringAuto (CloseUSB()));

		ShouldSyncPulse = false;

		exp.trialController.TogglePause (); //pause the game
//		yield return new WaitForSeconds(1f);
		UnityEngine.Debug.Log ("attempting to reconnect");
		yield return StartCoroutine(ConnectSyncbox());
		exp.trialController.TogglePause (); //unpause the game
		yield return null;
	}

		float syncPulseDuration = 0.05f;
		float syncPulseInterval = 1.0f;
	/*
		IEnumerator RunSyncPulse(){
			Stopwatch executionStopwatch = new Stopwatch ();

			while (ShouldSyncPulse) {
				executionStopwatch.Reset();

				SyncPulse(); //executes pulse, then waits for the rest of the 1 second interval

				executionStopwatch.Start();
				long syncPulseOnTime = SyncPulse();
				LogSYNCOn(syncPulseOnTime);
				while(executionStopwatch.ElapsedMilliseconds < 1500){
					yield return 0;
				}

				executionStopwatch.Stop();

			}
		}
*/

		//WE'RE USING THIS FUNCTION
		IEnumerator RunSyncPulseManual(){
			float jitterMin = 0.1f;
			float jitterMax = syncPulseInterval - syncPulseDuration;

			Stopwatch executionStopwatch = new Stopwatch ();

			while (ShouldSyncPulse) {
				executionStopwatch.Reset();
			UnityEngine.Debug.Log ("pulse running");

				float jitter = UnityEngine.Random.Range(jitterMin, jitterMax);//syncPulseInterval - syncPulseDuration);
				yield return StartCoroutine(WaitForShortTime(jitter));

				ToggleLEDOn();
				yield return StartCoroutine(WaitForShortTime(syncPulseDuration));
				ToggleLEDOff();

				float timeToWait = (syncPulseInterval - syncPulseDuration) - jitter;
				if(timeToWait < 0){
					timeToWait = 0;
				}

				yield return StartCoroutine(WaitForShortTime(timeToWait));

				executionStopwatch.Stop();
			}
		}

		//return microseconds it took to turn on LED
		void ToggleLEDOn(){

		#if !UNITY_STANDALONE_WIN
			TurnLEDOn ();
		#else
		LJUD.eDO(u3.ljhandle, 0, 1);
		#endif
			LogSYNCOn (GameClock.SystemTime_Milliseconds);
		}

		void ToggleLEDOff(){

		#if !UNITY_STANDALONE_WIN
			TurnLEDOff();
		#else
		LJUD.eDO(u3.ljhandle, 0, 0);
		#endif
			LogSYNCOff (GameClock.SystemTime_Milliseconds);

		}

		long GetMicroseconds(long ticks){
			long microseconds = ticks / (TimeSpan.TicksPerMillisecond / 1000);
			return microseconds;
		}

		IEnumerator WaitForShortTime(float jitter){
			float currentTime = 0.0f;
			while (currentTime < jitter) {
				currentTime += Time.deltaTime;
				yield return 0;
			}

		}

		void LogSYNCOn(long time){
			if (ExperimentSettings.isLogging) {
				exp.eegLog.Log (time, exp.eegLog.GetFrameCount(), "ON"); //NOTE: NOT USING FRAME IN THE FRAME SLOT
			}
		}

		void LogSYNCOff(long time){
			if (ExperimentSettings.isLogging) {
				exp.eegLog.Log (time, exp.eegLog.GetFrameCount(), "OFF"); //NOTE: NOT USING FRAME IN THE FRAME SLOT
			}
		}

		void LogSYNCStarted(long time, float duration){
			if (ExperimentSettings.isLogging) {
				exp.eegLog.Log (time, exp.eegLog.GetFrameCount (), "SYNC PULSE STARTED" + Logger_Threading.LogTextSeparator + duration);
			}
		}

		void LogSYNCPulseInfo(long time, float timeBeforePulseSeconds){
			if (ExperimentSettings.isLogging) {
				exp.eegLog.Log (time, exp.eegLog.GetFrameCount (), "SYNC PULSE INFO" + Logger_Threading.LogTextSeparator + timeBeforePulseSeconds*1000); //log milliseconds
			}
		}

		void OnApplicationQuit(){
			UnityEngine.Debug.Log(Marshal.PtrToStringAuto (CloseUSB()));
		}

	}
