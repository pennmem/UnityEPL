using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class InputReporter : DataReporter
{

	public bool reportKeyStrokes = false;
	public bool reportMouseClicks = false;
	public bool reportMousePosition = false;



	void Update()
	{
		CollectMouseEvents ();
	}

	void CollectMouseEvents()
	{
		int eventCount = UnityEPL.CountMouseEvents ();
		if (eventCount > 1)
		{
			int mouseButton = UnityEPL.PopMouseButton ();
			double timestamp = UnityEPL.PopMouseTimestamp ();
			Dictionary<string, string> dataDict = new Dictionary<string, string> ();
			dataDict.Add ("mouse button", mouseButton.ToString ());
			eventQueue.Enqueue(new DataPoint("mouse button", OSXTimestampToTimestamp(timestamp), dataDict));
		}
	}





//	private Thread pollingThread;
//
//	private volatile bool stopPolling = false;
//
//	void Awake()
//	{
//		pollingThread = new Thread(InputPolling);
//		pollingThread.Start ();
//	}
//
//	private void CheckInput()
//	{
//		Debug.Log (gamewatch.Elapsed.TotalMilliseconds);
//		if (UnityEngine.Input.GetKey(KeyCode.A) || UnityEngine.Input.GetKey(KeyCode.S))
//			UnityEngine.Debug.Log (gamewatch.Elapsed.TotalMilliseconds);
//	}
//
//	private void InputPolling()
//	{
//		while (!stopPolling)
//			CheckInput ();
//	}
//
//	void OnDestroy()
//	{
//		stopPolling = true;
//	}


}