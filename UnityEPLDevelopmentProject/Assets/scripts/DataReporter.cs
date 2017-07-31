using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this superclass implements an interface for retrieving behavioral events from a queue
public abstract class DataReporter : MonoBehaviour 
{

	//this real world start time timestamp is currently of unknown accuraccy.
	private static System.DateTime realWorldStartTime = System.DateTime.UtcNow;
	private static System.Diagnostics.Stopwatch gamewatch = new System.Diagnostics.Stopwatch();

	protected System.Collections.Generic.Queue<DataPoint> eventQueue = new Queue<DataPoint>();

	void Awake()
	{
		if (!gamewatch.IsRunning)
			gamewatch.Start ();
		if (QualitySettings.vSyncCount == 0)
			Debug.LogWarning ("vSync is off!  This will cause tearing, which will prevent meaningful reporting of frame-based time data.");
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

	protected System.DateTime RealWorldTime()
	{
		return realWorldStartTime.Add(gamewatch.Elapsed);
	}
}