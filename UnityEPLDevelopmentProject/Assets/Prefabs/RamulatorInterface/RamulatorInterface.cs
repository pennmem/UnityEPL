using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using NetMQ;

public class RamulatorInterface : MonoBehaviour
{
	public UnityEngine.UI.Text ramulatorWarningText;
	public GameObject ramulatorWarning;

	const int timeoutDelay = 20;

	private NetMQ.Sockets.PairSocket zmqSocket;

	private const string address = "tcp://*:8889";

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnApplicationQuit()
	{
		if (zmqSocket != null)
			zmqSocket.Close ();
		NetMQConfig.Cleanup();
	}

	public IEnumerator BeginNewSession(int sessionNumber)
	{
		//Connect to ramulator///////////////////////////////////////////////////////////////////
		zmqSocket = new NetMQ.Sockets.PairSocket ();
		zmqSocket.Bind (address);
		//Debug.Log ("socket bound");


		yield return WaitForMessage ("CONNECTED", "Ramulated not connected.");


		//Begin Heartbeats///////////////////////////////////////////////////////////////////////
		InvokeRepeating ("SendHeartbeat", 0, 1);


		//SendConnectedEvent////////////////////////////////////////////////////////////////////
		DataPoint connected = new DataPoint ("CONNECTED", DataReporter.RealWorldTime (), new Dictionary<string, string> ());
		SendMessageToRamulator (connected.ToJSON ());
		yield return null;


		//SendSessionEvent//////////////////////////////////////////////////////////////////////
		System.Collections.Generic.Dictionary<string, string> sessionData = new Dictionary<string, string>();
		sessionData.Add ("name", UnityEPL.GetExperimentName ());
		sessionData.Add ("version", Application.version);
		sessionData.Add ("subject", UnityEPL.GetParticipants () [0]);
		sessionData.Add ("session_number", sessionNumber.ToString());
		DataPoint sessionDataPoint = new DataPoint ("SESSION", DataReporter.RealWorldTime (), sessionData);
		SendMessageToRamulator (sessionDataPoint.ToJSON ());
		yield return null;


		yield return WaitForMessage ("START", "Start signal not received");





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
				Debug.Log ("received: " + receivedMessage.ToString ());
			}

			//if we have exceeded the timeout time, show warning and stop trying to connect
			if (Time.time > startTime + timeoutDelay)
			{
				ramulatorWarningText.text = errorMessage;
				Debug.LogWarning ("Timed out waiting for ramulator");
				break;
			}
			yield return null;
		}
		ramulatorWarning.SetActive (false);
	}

	public void BeginNewTrial(int trialNumber)
	{
		if (zmqSocket == null)
			throw new Exception ("Please begin a session before beginning trials");
		System.Collections.Generic.Dictionary<string, string> sessionData = new Dictionary<string, string>();
		sessionData.Add ("trial", trialNumber.ToString());
		DataPoint sessionDataPoint = new DataPoint ("TRIAL", DataReporter.RealWorldTime (), sessionData);
		SendMessageToRamulator (sessionDataPoint.ToJSON ());
	}

	public void SetState(string stateName, bool stateToggle, IronPython.Runtime.PythonDictionary extraData)
	{
		System.Collections.Generic.Dictionary<string, string> sessionData = new Dictionary<string, string>();
		sessionData.Add ("name", stateName);
		sessionData.Add ("value",  stateToggle.ToString());
		foreach (string key in extraData.Keys)
			sessionData.Add (key, extraData [key] == null ? "" : extraData [key].ToString());
		DataPoint sessionDataPoint = new DataPoint ("STATE", DataReporter.RealWorldTime (), sessionData);
		SendMessageToRamulator (sessionDataPoint.ToJSON ());
	}
		
	private void SendHeartbeat()
	{
		DataPoint sessionDataPoint = new DataPoint ("HEARTBEAT", DataReporter.RealWorldTime (), null);
		SendMessageToRamulator (sessionDataPoint.ToJSON ());
	}
		
	private void SendMessageToRamulator(string message)
	{
		bool wouldNotHaveBlocked = zmqSocket.TrySendFrame(message, more: false);
		//Debug.Log ("Tried to send a message: " + message + " \nWouldNotHaveBlocked: " + wouldNotHaveBlocked.ToString());
	}
}