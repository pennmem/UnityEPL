using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this superclass implements an interface for retrieving behavioral events from a queue
public abstract class DataReporter : MonoBehaviour 
{
	private static System.DateTime realWorldStartTime;

	//this stopwatch is the ultimate reference for time according to the plugin
	public static System.Diagnostics.Stopwatch gamewatch = new System.Diagnostics.Stopwatch();

	private static bool nativePluginRunning = false;
	private static bool startTimeInitialized = false;

	protected System.Collections.Generic.Queue<DataPoint> eventQueue = new Queue<DataPoint>();
	protected static double OSStartTime;

	void Awake()
	{
		if (!startTimeInitialized) 
		{
			realWorldStartTime = System.DateTime.UtcNow;
			startTimeInitialized = true;
		}
		if (!nativePluginRunning) 
		{
			OSStartTime = UnityEPL.StartCocoaPlugin ();
			nativePluginRunning = true;
		}

	
		if (!gamewatch.IsRunning)
			gamewatch.Start ();

		if (QualitySettings.vSyncCount == 0)
			Debug.LogWarning ("vSync is off!  This will cause tearing, which will prevent meaningful reporting of frame-based time data.");
	}

	void OnDestroy()
	{
		if (nativePluginRunning)
		{
			UnityEPL.StopCocoaPlugin ();
			nativePluginRunning = false;
		}
	}

	public int UnreadDataPointCount()
	{
		return eventQueue.Count;
	}

	//UnityEPL users can use this to pull data points out manually on a per-reporter basis
	public DataPoint[] ReadDataPoints(int count)
	{
		if (eventQueue.Count < count)
		{
			throw new UnityException ("Not enough data points!  Check UnreadDataPointCount first.");
		}

		DataPoint[] dataPoints = new DataPoint[count];
		for (int i = 0; i < count; i++)
		{
			dataPoints [i] = eventQueue.Dequeue ();
		}

		return dataPoints;
	}

	//this will be modified to use native OS functionality for increased accuracy
	//changing time scale will break this!!
	protected System.DateTime RealWorldFrameDisplayTime()
	{
		return realWorldStartTime.AddSeconds (Time.time);
	}

	protected System.DateTime RealWorldTimeStopwatch()
	{
		return realWorldStartTime.Add(gamewatch.Elapsed);
	}

	protected System.DateTime RealWorldTime()
	{
		return realWorldStartTime.Add (new System.TimeSpan(System.TimeSpan.TicksPerSecond * (long)Time.realtimeSinceStartup));
	}

	protected System.DateTime OSXTimestampToTimestamp(double OSXTimestamp)
	{
//		Debug.Log (OSXStartTime);
//		Debug.Log (OSXTimestamp);
		return realWorldStartTime.Add (new System.TimeSpan((long)(System.TimeSpan.TicksPerSecond * (OSXTimestamp - OSStartTime))));
	}

	public static System.DateTime GetStartTime()
	{
		return realWorldStartTime;
	}
}