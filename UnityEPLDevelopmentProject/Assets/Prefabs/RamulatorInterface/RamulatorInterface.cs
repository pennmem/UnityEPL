using System;
//using System.Collections;
//using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class RamulatorInterface : MonoBehaviour
{
	public UnityEngine.UI.Text ramulatorWarningText;
	public GameObject ramulatorWarning;

	public enum EventType 
	{
		SUBJECTID,
		EXPNAME,
		VERSION,
		INFO,
		CONTROL,
		DEFINE,
		SESSION,
		PRACTICE,
		TRIAL,
		PHASE,
		DISPLAYON,
		DISPLAYOFF,
		HEARTBEAT,
		ALIGNCLOCK,
		ABORT,
		SYNC,
		SYNCNP,
		SYNCED,
		READY,
		STATE,
		EXIT
	}

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
		BeginNewSession (0, 0);
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void BeginNewSession(int sessionNumber, int trialNumber)
	{
		//Connect to ramulator
		string address = "tcp://*:" + TCPServer.ConnectionPort;
		int connectionStatus = ZMQConnect (address);
		if (connectionStatus == 0)
			Debug.Log("CONNECTED!");
		else
			Debug.Log("CANNOT CONNECT");


		//SendSessionEvent(System.Convert.ToInt64(UnityEPL.MillisecondsSinceTheEpoch()), TCPServer.EventType.SESSION, UnityEPL.GetExperimentName(), Application.version, UnityEPL.GetParticipants()[0], sessionNumber);


		ramulatorWarning.SetActive (true);
		ramulatorWarningText.text = "Waiting on Ramulator";
		//!!HERE: block until I receive a start message
		ramulatorWarning.SetActive(false);


		InvokeRepeating ("SendHeartbeat", 0, 1);

		//myServer.SendTrialEvent (System.DateTime.Now.Millisecond, TCPServer.EventType.TRIAL, null, trialNumber);
	}

	void SendHeartbeat()
	{
		//SendSimpleJSONEvent(lastBeat, TCPServer.EventType.HEARTBEAT, null, intervalMS.ToString());
	}
}