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
		Debug.Log ("socket bound");


		//SendSessionEvent//////////////////////////////////////////////////////////////////////
		System.Collections.Generic.Dictionary<string, string> sessionData = new Dictionary<string, string>();
		sessionData.Add ("name", UnityEPL.GetExperimentName ());
		sessionData.Add ("version", Application.version);
		sessionData.Add ("subject", UnityEPL.GetParticipants () [0]);
		sessionData.Add ("session_number", sessionNumber.ToString());
		DataPoint sessionDataPoint = new DataPoint ("SESSION", DataReporter.RealWorldTime (), sessionData);
		SendMessageToRamulator (sessionDataPoint.ToJSON ());



		//Block until start received////////////////////////////////////////////////////////////
		ramulatorWarning.SetActive (true);
		ramulatorWarningText.text = "Waiting on Ramulator";
		yield return null;
		string receivedMessage;
		zmqSocket.TryReceiveFrameString(new System.TimeSpan(0, 0, timeoutDelay), out receivedMessage);
		if (receivedMessage != null)
			Debug.Log ("received: " + receivedMessage.ToString ());
		else
			Debug.Log ("Timed out waiting for Ramulator");
		ramulatorWarning.SetActive(false);



		//Begin Heartbeats///////////////////////////////////////////////////////////////////////
		InvokeRepeating ("SendHeartbeat", 0, 1);



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

	public void SetState(string stateName, bool stateToggle)
	{
		System.Collections.Generic.Dictionary<string, string> sessionData = new Dictionary<string, string>();
		sessionData.Add ("name", stateName);
		sessionData.Add ("value",  stateToggle.ToString());
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
		Debug.Log ("Tried to send a message.  WouldNotHaveBlocked: " + wouldNotHaveBlocked.ToString());
	}
}