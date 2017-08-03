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
		Debug.Log (UnityEPL.CountKeyEvents ());
		Debug.Log (UnityEPL.CountMouseEvents ());
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