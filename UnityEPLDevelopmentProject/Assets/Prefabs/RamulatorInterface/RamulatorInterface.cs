using System;
//using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class RamulatorInterface : MonoBehaviour
{
	public UnityEngine.UI.Text ramulatorWarningText;
	public GameObject ramulatorWarning;

	const float timeoutDelay = 5f;

	[DllImport ("ZMQPlugin")]
	private static extern int ZMQConnect(string hostAddress);

	[DllImport ("ZMQPlugin")]
	private static extern IntPtr ZMQReceive();

	[DllImport ("ZMQPlugin")]
	private static extern void ZMQSend(string message,int length);

	[DllImport ("ZMQPlugin")]
	private static extern void ZMQClose();

	// Use this for initialization
	void Start ()
	{
		//test
		BeginNewSession (0);
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnDisable()
	{
		ZMQClose ();
	}

	public void BeginNewSession(int sessionNumber)
	{
		//Connect to ramulator///////////////////////////////////////////////////////////////////
		string address = "tcp://*:8889";
		int connectionStatus = ZMQConnect (address);
		if (connectionStatus == 0)
			Debug.Log("CONNECTED!");
		else
			Debug.Log("CANNOT CONNECT");
		



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

		DateTime startTime = DataReporter.RealWorldTime ();

		string ramulatorMessage = "none";
		while (ramulatorMessage.Equals("none"))
		{
			ramulatorMessage = Marshal.PtrToStringAnsi (ZMQReceive ());
			if (DataReporter.RealWorldTime () > startTime.AddSeconds (timeoutDelay))
				throw new UnityException ("Ramulator didn't respond within the timeout delay.");
		}
		Debug.Log("received: " + ramulatorMessage);

		ramulatorWarning.SetActive(false);




		//Begin Heartbeats///////////////////////////////////////////////////////////////////////
		InvokeRepeating ("SendHeartbeat", 0, 1);




	}

	public void BeginNewTrial(int trialNumber)
	{
		System.Collections.Generic.Dictionary<string, string> sessionData = new Dictionary<string, string>();
		sessionData.Add ("trial", trialNumber.ToString());
		DataPoint sessionDataPoint = new DataPoint ("TRIAL", DataReporter.RealWorldTime (), sessionData);
		SendMessageToRamulator (sessionDataPoint.ToJSON ());
	}

	private void SendHeartbeat()
	{
		DataPoint sessionDataPoint = new DataPoint ("HEARTBEAT", DataReporter.RealWorldTime (), null);
		SendMessageToRamulator (sessionDataPoint.ToJSON ());
	}

	private void SetState(string stateName, bool stateToggle)
	{
		System.Collections.Generic.Dictionary<string, string> sessionData = new Dictionary<string, string>();
		sessionData.Add ("name", stateName);
		sessionData.Add ("value",  stateToggle.ToString());
		DataPoint sessionDataPoint = new DataPoint ("STATE", DataReporter.RealWorldTime (), sessionData);
		SendMessageToRamulator (sessionDataPoint.ToJSON ());
	}

	private void SendMessageToRamulator(string message)
	{
		ZMQSend (message, message.Length);
	}
}