using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this superclass implements an interface for retrieving behavioral events from a queue
public abstract class DataReporter : MonoBehaviour
{
	private static System.DateTime realWorldStartTime;

	private static bool nativePluginRunning = false;
	private static bool startTimeInitialized = false;

	protected System.Collections.Generic.Queue<DataPoint> eventQueue = new Queue<DataPoint>();

	private static double OSStartTime;
	private static float unityTimeStartTime;

	void Awake()
	{
		if (!startTimeInitialized)
		{
			realWorldStartTime = System.DateTime.UtcNow;
			unityTimeStartTime = Time.realtimeSinceStartup;
			startTimeInitialized = true;
		}
		if (!nativePluginRunning)
		{
			OSStartTime = UnityEPL.StartCocoaPlugin ();
			nativePluginRunning = true;
		}

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
		
	//changing time scale will affect this!!
	//TODO: Get the frame display time in a way unaffected by time scale?
	protected System.DateTime RealWorldFrameDisplayTime()
	{
		double secondsSinceUnityStart = Time.unscaledTime - unityTimeStartTime;
		return GetStartTime().AddSeconds(secondsSinceUnityStart);
	}

	protected System.DateTime OSXTimestampToTimestamp(double OSXTimestamp)
	{
		double secondsSinceOSStart = OSXTimestamp - OSStartTime;
		return GetStartTime().AddSeconds (secondsSinceOSStart);
	}


	public static System.DateTime RealWorldTime()
	{
		double secondsSinceUnityStart = Time.realtimeSinceStartup - unityTimeStartTime;
		return GetStartTime().AddSeconds(secondsSinceUnityStart);
	}


	public static System.DateTime GetStartTime()
	{
		return realWorldStartTime;
	}
}