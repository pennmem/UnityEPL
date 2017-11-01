using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[AddComponentMenu("UnityEPL/Reporters/Input Reporter")]
public class InputReporter : DataReporter
{

	public bool reportKeyStrokes = false;
	public bool reportMouseClicks = false;
	public bool reportMousePosition = false;
	private Dictionary<int, bool> keyDownStates = new Dictionary<int, bool> ();
	private Dictionary<int, bool> mouseDownStates = new Dictionary<int, bool> ();


	void Update()
	{
		if (reportMouseClicks)
			CollectMouseEvents ();
		if (reportKeyStrokes)
			CollectKeyEvents ();
		if (reportMousePosition)
			CollectMousePosition ();
	}

	void CollectMouseEvents()
	{
		int eventCount = UnityEPL.CountMouseEvents ();
		if (eventCount >= 1)
		{
			int mouseButton = UnityEPL.PopMouseButton ();
			double timestamp = UnityEPL.PopMouseTimestamp ();
			bool downState;
			mouseDownStates.TryGetValue (mouseButton, out downState);
			mouseDownStates [mouseButton] = !downState;
			Dictionary<string, string> dataDict = new Dictionary<string, string> ();
			dataDict.Add ("mouse button", mouseButton.ToString ());
			dataDict.Add ("is pressed", mouseDownStates [mouseButton].ToString());
			eventQueue.Enqueue(new DataPoint("mouse button up/down", OSXTimestampToTimestamp(timestamp), dataDict));
		}
	}

	void CollectKeyEvents()
	{
		int eventCount = UnityEPL.CountKeyEvents ();
		if (eventCount >= 1)
		{
			int keyCode = UnityEPL.PopKeyKeycode ();
			double timestamp = UnityEPL.PopKeyTimestamp ();
			bool downState;
			keyDownStates.TryGetValue (keyCode, out downState);
			keyDownStates [keyCode] = !downState;
			Dictionary<string, string> dataDict = new Dictionary<string, string> ();
			dataDict.Add ("key code", keyCode.ToString ());
			dataDict.Add ("is pressed", keyDownStates [keyCode].ToString());
			eventQueue.Enqueue(new DataPoint("key press/release", OSXTimestampToTimestamp(timestamp), dataDict));
		}
	}

	void CollectMousePosition()
	{
		Dictionary<string, string> dataDict = new Dictionary<string, string> ();
		dataDict.Add ("mouse position", Input.mousePosition.ToString());
		eventQueue.Enqueue(new DataPoint("key press/release", DataReporter.RealWorldTime(), dataDict));
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