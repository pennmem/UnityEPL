using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using NetMQ;

public class RamulatorInterface : MonoBehaviour
{
	//This will be updated with warnings about the status of ramulator connectivity
	public UnityEngine.UI.Text ramulatorWarningText;
	//This will be activated when a warning needs to be displayed
	public GameObject ramulatorWarning;
	//This will be used to log messages
	public ScriptedEventReporter scriptedEventReporter;

	//how long to wait for ramulator to connect
	const int timeoutDelay = 90;
	const int unreceivedHeartbeatsToQuit = 8;

	private int unreceivedHeartbeats = 0;

	private NetMQ.Sockets.PairSocket zmqSocket;
	private const string address = "tcp://*:8889";

	void OnApplicationQuit()
	{
		if (zmqSocket != null)
			zmqSocket.Close ();
		NetMQConfig.Cleanup();
	}

	//this coroutine connects to ramulator and communicates how ramulator expects it to
	//in order to start the experiment session.  follow it up with BeginNewTrial and
	//SetState calls
	public IEnumerator BeginNewSession(int sessionNumber)
	{
		//Connect to ramulator///////////////////////////////////////////////////////////////////
		zmqSocket = new NetMQ.Sockets.PairSocket ();
		zmqSocket.Bind (address);
		//Debug.Log ("socket bound");


		yield return WaitForMessage ("CONNECTED", "Ramulated not connected.");


		//SendSessionEvent//////////////////////////////////////////////////////////////////////
		System.Collections.Generic.Dictionary<string, string> sessionData = new Dictionary<string, string>();
		sessionData.Add ("name", UnityEPL.GetExperimentName ());
		sessionData.Add ("version", Application.version);
		sessionData.Add ("subject", UnityEPL.GetParticipants () [0]);
		sessionData.Add ("session_number", sessionNumber.ToString());
		DataPoint sessionDataPoint = new DataPoint ("SESSION", DataReporter.RealWorldTime (), sessionData);
		SendMessageToRamulator (sessionDataPoint.ToJSON ());
		yield return null;


		//Begin Heartbeats///////////////////////////////////////////////////////////////////////
		InvokeRepeating ("SendHeartbeat", 0, 1);


		//SendReadyEvent////////////////////////////////////////////////////////////////////
		DataPoint ready = new DataPoint ("READY", DataReporter.RealWorldTime (), new Dictionary<string, string> ());
		SendMessageToRamulator (ready.ToJSON ());
		yield return null;


		yield return WaitForMessage ("START", "Start signal not received");


		InvokeRepeating ("ReceiveHeartbeat", 0, 1);

	}

	private IEnumerator WaitForMessage(string containingString, string errorMessage)	
	{
		ramulatorWarning.SetActive (true);
		ramulatorWarningText.text = "Waiting on Ramulator";

		string receivedMessage = "";
		float startTime = Time.time;
		while (receivedMessage == null || !receivedMessage.Contains (containingString))
		{
			zmqSocket.TryReceiveFrameString (out receivedMessage);
			if (receivedMessage != "" && receivedMessage != null)
			{
				string messageString = receivedMessage.ToString ();
				Debug.Log ("received: " + messageString);
				ReportMessage (messageString, false);
			}

			//if we have exceeded the timeout time, show warning and stop trying to connect
			if (Time.time > startTime + timeoutDelay)
			{
				ramulatorWarningText.text = errorMessage;
				Debug.LogWarning ("Timed out waiting for ramulator");
				yield break;
			}
			yield return null;
		}
		ramulatorWarning.SetActive (false);
	}

	//ramulator expects this before the beginning of a new list
	public void BeginNewTrial(int trialNumber)
	{
		if (zmqSocket == null)
			throw new Exception ("Please begin a session before beginning trials");
		System.Collections.Generic.Dictionary<string, string> sessionData = new Dictionary<string, string>();
		sessionData.Add ("trial", trialNumber.ToString());
		DataPoint sessionDataPoint = new DataPoint ("TRIAL", DataReporter.RealWorldTime (), sessionData);
		SendMessageToRamulator (sessionDataPoint.ToJSON ());
	}

	//ramulator expects this when you display words to the subject.
	//for words, stateName is "WORD"
	public void SetState(string stateName, bool stateToggle, System.Collections.Generic.Dictionary<string, string> sessionData)
	{
		sessionData.Add ("name", stateName);
		sessionData.Add ("value",  stateToggle.ToString());
		DataPoint sessionDataPoint = new DataPoint ("STATE", DataReporter.RealWorldTime (), sessionData);
		SendMessageToRamulator (sessionDataPoint.ToJSON ());
	}

	public void SendMathMessage(string problem, string response, int responseTimeMs, bool correct)
	{
		Dictionary<string, string> mathData = new Dictionary<string, string> ();
		mathData.Add ("problem", problem);
		mathData.Add ("response", response);
		mathData.Add ("response_time_ms", responseTimeMs.ToString());
		mathData.Add ("correct", correct.ToString());
		DataPoint mathDataPoint = new DataPoint ("MATH", DataReporter.RealWorldTime (), mathData);
		SendMessageToRamulator (mathDataPoint.ToJSON ());
	}


	private void SendHeartbeat()
	{
		DataPoint sessionDataPoint = new DataPoint ("HEARTBEAT", DataReporter.RealWorldTime (), null);
		SendMessageToRamulator (sessionDataPoint.ToJSON ());
	}

	private void ReceiveHeartbeat()
	{
		unreceivedHeartbeats = unreceivedHeartbeats + 1;
		Debug.Log ("Unreceived heartbeats: " + unreceivedHeartbeats.ToString ());
		if (unreceivedHeartbeats > unreceivedHeartbeatsToQuit)
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}

		string receivedMessage = "";
		float startTime = Time.time;
		zmqSocket.TryReceiveFrameString (out receivedMessage);
		if (receivedMessage != "" && receivedMessage != null)
		{
			string messageString = receivedMessage.ToString ();
			Debug.Log ("heartbeat received: " + messageString);
			ReportMessage (messageString, false);
			unreceivedHeartbeats = 0;
		}
	}

	private void SendMessageToRamulator(string message)
	{
		bool wouldNotHaveBlocked = zmqSocket.TrySendFrame(message, more: false);
		Debug.Log ("Tried to send a message: " + message + " \nWouldNotHaveBlocked: " + wouldNotHaveBlocked.ToString());
		ReportMessage (message, true);
	}

	private void ReportMessage(string message, bool sent)
	{
		Dictionary<string, string> messageDataDict = new Dictionary<string, string> ();
		messageDataDict.Add ("message", message);
		messageDataDict.Add ("sent", sent.ToString());
		scriptedEventReporter.ReportScriptedEvent("network", messageDataDict);
	}
}